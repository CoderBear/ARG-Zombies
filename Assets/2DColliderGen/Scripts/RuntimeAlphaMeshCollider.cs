using UnityEngine;
using System.Collections.Generic;

//-------------------------------------------------------------------------
/// <summary>
/// A component to generate a MeshCollider from an image with alpha channel
/// at runtime.
/// 
/// NOTE: This is experimental code - don't expect it to be perfect or
/// anything close yet.
/// 
/// TODO: Parallelization / coroutine.
/// </summary>
public class RuntimeAlphaMeshCollider : MonoBehaviour {
	
	protected int mUpdateCounter = 0;
	
	public bool mUseBinaryImageInsteadOfTexture = false; ///< When set to true, the mUsedTexture is ignored and the mBinaryImage attribute is used directly.
	public bool mOutputColliderInNormalizedSpace = true;
	public Texture2D mUsedTexture = null;
	public bool [,] mBinaryImage = null; ///< If you want to set the collider-image directly, set mUseBinaryImageInsteadOfTexture=true and fill this attribute accordingly.
	public float mAlphaOpaqueThreshold = 0.1f;
	public int mMaxNumberOfIslands = 10;
	public int mMinPixelCountToIncludeIsland = 200;
	public float mColliderThickness = 2.0f;
	
	public float mVertexReductionDistanceTolerance = 0.0f;
	public int mMaxPointCountPerIsland = 20;
	
	protected PolygonOutlineFromImageFrontend mOutlineAlgorithm = new PolygonOutlineFromImageFrontend();
	protected IslandDetector mIslandDetector = new IslandDetector();
	
	protected IslandDetector.Region[] mIslands = null;
    protected IslandDetector.Region[] mSeaRegions = null;
	protected List<List<Vector2> > mOutlineVerticesAtIsland = new List<List<Vector2> >();

	//-------------------------------------------------------------------------
	void Start() {
		
		this.gameObject.AddComponent<MeshCollider>();
		
		if (mUsedTexture == null) {
			mUsedTexture = (Texture2D) this.renderer.sharedMaterial.mainTexture;
		}
		
		UpdateMeshCollider();
	}
	
	//-------------------------------------------------------------------------
	void Update() {
		
	}
	
	//-------------------------------------------------------------------------
	/// <summary>
	/// Updates the mesh collider. Call this method from your code accordingly.
	/// </summary>
	/// <returns>
	/// The alpha mesh collider to texture.
	/// </returns>
	public bool UpdateMeshCollider() {
		
		if (!mUseBinaryImageInsteadOfTexture) {
	        bool wasSuccessful = mOutlineAlgorithm.BinaryAlphaThresholdImageFromTexture(out mBinaryImage, mUsedTexture, mAlphaOpaqueThreshold, false, 0, 0, 0, 0);
			if (!wasSuccessful) {
				Debug.LogError(mOutlineAlgorithm.LastError);
				return false;
			}
		}
		
		bool anyIslandsFound = CalculateIslandStartingPoints(mBinaryImage, out mIslands, out mSeaRegions);
        if (!anyIslandsFound) {
			Debug.LogError("Error: No opaque pixel (and thus no island region) has been found in the texture image - is your mAlphaOpaqueThreshold parameter too high?. Stopping collider generation.");
            return false;
        }
		
		mOutlineAlgorithm.VertexReductionDistanceTolerance = mVertexReductionDistanceTolerance;
		mOutlineAlgorithm.MaxPointCount = mMaxPointCountPerIsland;
		mOutlineAlgorithm.Convex = false;
		mOutlineAlgorithm.XOffsetNormalized = -0.5f;
		mOutlineAlgorithm.YOffsetNormalized = -0.5f;
		mOutlineAlgorithm.Thickness = mColliderThickness;
		
		bool anyIslandVerticesAdded = CalculateOutlineForColliderIslands(out mOutlineVerticesAtIsland, mIslands, mBinaryImage);
		if (!anyIslandVerticesAdded) {
			Debug.LogError("Error: No island vertices added in CalculateUnreducedOutlineForColliderIslands - is your mMinPixelCountToIncludeIsland parameter too low (currently set to " + mMinPixelCountToIncludeIsland + ")?. Stopping collider generation.");
            return false;
        }
		
		Vector3[] fenceVertices;
		int[] fenceTriangleIndices;
		bool isFenceCalculatedSuccessfully = CalculateTriangleFence(out fenceVertices, out fenceTriangleIndices, mOutlineVerticesAtIsland);
		if (!isFenceCalculatedSuccessfully) {
			Debug.LogError("Error: Failed to create triangle fence from the outline vertices. Stopping collider generation.");
            return false;
		}
		
		bool isMeshColliderSuccessfullySet = UpdateMeshCollider(fenceVertices, fenceTriangleIndices);
		if (!isMeshColliderSuccessfullySet) {
			Debug.LogError("Error: Failed to update the mesh collider. Stopping collider generation.");
            return false;
		}
		return true;
	}
	
	//-------------------------------------------------------------------------
    /// <returns>True if at least one island is found, false otherwise.</returns>
	bool CalculateIslandStartingPoints(bool [,] binaryImage, out IslandDetector.Region[] islands, out IslandDetector.Region[] seaRegions) {
		
		int[,] islandClassificationImage = null;
		islands = null;
		seaRegions = null;
		
		mIslandDetector.DetectIslandsFromBinaryImage(binaryImage, out islandClassificationImage, out islands, out seaRegions);

        return (islands.Length > 0);
	}
	
	//-------------------------------------------------------------------------
	bool CalculateOutlineForColliderIslands(out List<List<Vector2> > outlineVerticesAtIsland, IslandDetector.Region[] islands, bool [,] binaryImage) {
		
		outlineVerticesAtIsland = new List<List<Vector2> >();
		
		for (int islandIndex = 0; islandIndex < islands.Length; ++islandIndex) {
			
			IslandDetector.Region island = islands[islandIndex];
			
			if (islandIndex >= mMaxNumberOfIslands || island.mPointCount < mMinPixelCountToIncludeIsland) {
				break; // islands are sorted by size already, only smaller islands follow.
			}
			else {
				List<Vector2> unreducedOutlineVertices;
	            mOutlineAlgorithm.UnreducedOutlineFromBinaryImage(out unreducedOutlineVertices, binaryImage, island.mPointAtBorder, true, mOutputColliderInNormalizedSpace, true);
				
				List<Vector2> reducedVertices = mOutlineAlgorithm.ReduceOutline(unreducedOutlineVertices, true);
				outlineVerticesAtIsland.Add(reducedVertices);
			}
        }
		return outlineVerticesAtIsland.Count > 0;
	}
	
	//-------------------------------------------------------------------------
	bool CalculateTriangleFence(out Vector3[] jointVertices, out int[] jointTriangleIndices, List<List<Vector2> > outlineVerticesAtIsland) {
		
		List<Vector3[]> islandVertices = new List<Vector3[]>();
		List<int[]> islandTriangleIndices = new List<int[]>();
		
		for (int islandIndex = 0; islandIndex < outlineVerticesAtIsland.Count; ++islandIndex) {
		
			Vector3[] vertices;
			int[] triangleIndices;
			mOutlineAlgorithm.TriangleFenceFromOutline(out vertices, out triangleIndices, outlineVerticesAtIsland[islandIndex], false);
			islandVertices.Add(vertices);
			islandTriangleIndices.Add(triangleIndices);
		}
		
		JoinVertexGroups(out jointVertices, out jointTriangleIndices, islandVertices, islandTriangleIndices);
		return true;
	}
	
	//-------------------------------------------------------------------------
	bool JoinVertexGroups(out Vector3[] jointVertices, out int[] jointIndices, List<Vector3[]> islandVertices, List<int[]> islandTriangleIndices) {
		
		int numVertices = 0;
		int numIndices = 0;
		int numIslands = islandVertices.Count;
		for (int islandIndex = 0; islandIndex < numIslands; ++islandIndex) {
		
			if (islandVertices[islandIndex] == null || islandTriangleIndices[islandIndex] == null) {
				continue;
			}
			numVertices += islandVertices[islandIndex].Length;
			numIndices += islandTriangleIndices[islandIndex].Length;
		}
		
		jointVertices = new Vector3[numVertices];
		jointIndices = new int[numIndices];
		int jointVertexIndex = 0;
		int jointIndexIndex = 0;
		
		int indexOffset = 0;
		for (int islandIndex = 0; islandIndex < numIslands; ++islandIndex) {
		
			if (islandVertices[islandIndex] == null || islandTriangleIndices[islandIndex] == null) {
				continue;
			}
			
			Vector3[] regionVertices = islandVertices[islandIndex];
			int[] regionIndices = islandTriangleIndices[islandIndex];
			
			for (int regionVertexIndex = 0; regionVertexIndex < regionVertices.Length; ++regionVertexIndex) {
				jointVertices[jointVertexIndex++] = regionVertices[regionVertexIndex];
			}
			for (int islandIndexIndex = 0; islandIndexIndex < regionIndices.Length; ++islandIndexIndex) {
				jointIndices[jointIndexIndex++] = regionIndices[islandIndexIndex] + indexOffset;
			}
			
			indexOffset += regionVertices.Length;
		}
		
		return true;
	}
	
	//-------------------------------------------------------------------------
	bool UpdateMeshCollider(Vector3[] vertices, int[] triangleIndices) {
		
		MeshCollider meshCollider = this.GetComponent<MeshCollider>();
		if (meshCollider == null) {
			this.gameObject.AddComponent<MeshCollider>();
			meshCollider = this.GetComponent<MeshCollider>();
		}
		Mesh colliderMesh = new Mesh();
		colliderMesh.vertices = vertices;
		colliderMesh.triangles = triangleIndices;
		colliderMesh.RecalculateBounds();
		
		meshCollider.sharedMesh = null;
		meshCollider.sharedMesh = colliderMesh;
		return true;
	}
}
