using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;

//-------------------------------------------------------------------------
/// <summary>
/// A component update the Collider-Prefabs according to the current state
/// of the OTTilesSprite object (mainly its scroll-position).
/// </summary>
[ExecuteInEditMode]
public class AlphaMeshColliderUpdateOTTilesSpriteColliders : MonoBehaviour {

	public const string RUNTIME_GROUP_NODE_NAME = "AlphaMeshColliders";

	[SerializeField] protected Component mOTTilesSprite = null;
	public List<GameObject> mColliderPrefabAtTileIndex = null;
	[SerializeField] protected Vector2 mAtlasFrameSizeInPixels = Vector2.one;
	[SerializeField] protected int mTileWidth = 1;
	[SerializeField] protected int mTileHeight = 1;
	[SerializeField] protected int[][] mTilesLink = null; // indexed [x][y].
	[SerializeField] protected int[] mCurrentTiles = null; // indexed [y * mNumTilesX + x].
	[SerializeField] protected int mNumTilesX = 0;
	[SerializeField] protected int mNumTilesY = 0;
	/*[SerializeField] */protected int[] mInstantiatedTileIndices = null; // indexed [y * mNumTilesX + x]
	/*[SerializeField] */protected GameObject[] mInstantiatedTileObjects = null; // indexed [y * mNumTilesX + x]
	[SerializeField] protected GameObject mTileObjectsGroupNode = null;
	[SerializeField] protected Vector3 mTileScale = Vector3.one;
	[SerializeField] protected Vector2 mBottomLeftOffset = Vector3.zero;
	[SerializeField] protected bool mAreTileColliderPrefabsCreated = false;
	[SerializeField] protected bool mAreTileCollidersInstantiated = false;
	
	//-------------------------------------------------------------------------
	public void SetOTTilesSprite(Component otTilesSprite) {
		mOTTilesSprite = otTilesSprite;
	}
	
	//-------------------------------------------------------------------------
	void LateUpdate() {
	
#if UNITY_EDITOR
		if (Application.isEditor && !Application.isPlaying) {
			InEditorUpdate();
		}
		else {
#endif
			// Runtime Code
			GetTilePlacementInfos(mOTTilesSprite);
			UpdateTileColliders();
#if UNITY_EDITOR
		}
#endif
	}
	
#if UNITY_EDITOR
	//-------------------------------------------------------------------------
	// IN-EDITOR ONLY METHODS
	//-------------------------------------------------------------------------
	//-------------------------------------------------------------------------
	void Start() {
		if (Application.isEditor && !Application.isPlaying) {
			InEditorUpdate();
		}
	}
	
	//-------------------------------------------------------------------------
	void InEditorUpdate() {
		if (mOTTilesSprite) {
			// Create a collider-prefab for each tile of the tile-set.
			if (!mAreTileColliderPrefabsCreated) {
				DeleteAndCreateAllTileColliderPrefabs();
			}
			GetTilePlacementInfos(mOTTilesSprite);
			UpdateTileColliders();
		}
	}
	
	//-------------------------------------------------------------------------
	public void RecreateAllTileColliderPrefabs() {
		mAreTileColliderPrefabsCreated = false;
		InEditorUpdate();
	}
	
	//-------------------------------------------------------------------------
	void DeleteAndCreateAllTileColliderPrefabs() {
		GameObject groupNode = GetTileColliderPrefabGroupNode(mOTTilesSprite);
		if (groupNode != null) {
			GameObject.DestroyImmediate(groupNode);
		}
		CreateAllTileColliderPrefabs(mOTTilesSprite, out mColliderPrefabAtTileIndex, out mAtlasFrameSizeInPixels);
		mAreTileColliderPrefabsCreated = true;
	}
	
	//-------------------------------------------------------------------------
	static bool CreateAllTileColliderPrefabs(Component otTilesSprite, out List<GameObject> colliderPrefabAtTileIndex, out Vector2 atlasFrameSizeInPixels) {
		AlphaMeshCollider referenceParameterObject = otTilesSprite.GetComponent<AlphaMeshCollider>();
		return AlphaMeshCollider.CreateColliderPrefabsForAllOTContainerFrames(otTilesSprite, referenceParameterObject, out colliderPrefabAtTileIndex, out atlasFrameSizeInPixels);
	}
	
	//-------------------------------------------------------------------------
	static GameObject GetTileColliderPrefabGroupNode(Component otTilesSprite) {
		string prefabGroupNodeName = AlphaMeshCollider.GetTileColliderPrefabGroupNodeName(otTilesSprite);
		return GameObject.Find(prefabGroupNodeName);
	}
#endif
	
	//-------------------------------------------------------------------------
	// NORMAL METHODS
	//-------------------------------------------------------------------------
	//-------------------------------------------------------------------------
	bool GetTilePlacementInfos(Component otTilesSprite) {
	
		if (mTilesLink == null) {
			Type otSpriteType = otTilesSprite.GetType();
		
			// mTilesLink = otSprite._tiles  (int[][])
			FieldInfo fieldTiles = otSpriteType.GetField("_tiles", BindingFlags.NonPublic | BindingFlags.Instance);
			if (fieldTiles == null) {
				Debug.LogWarning("Failed to read '_tiles' field of the OTTilesSprite component. Seems as if a different version of Orthello is used. If you need texture- or sprite-atlas support, please consider updating your Orthello framework.");
				return false;
			}
			mTilesLink = (int[][]) fieldTiles.GetValue(otTilesSprite);
			
			// mTileWidth|mTileHeight = otSprite._tileSize.x|.y (IVector)
			FieldInfo fieldTileSize = otSpriteType.GetField("_tileSize");
			if (fieldTileSize == null) {
				Debug.LogWarning("Failed to read '_tileSize' field of the OTTilesSprite component. Seems as if a different version of Orthello is used. If you need texture- or sprite-atlas support, please consider updating your Orthello framework.");
				return false;
			}
			object tileSizeIVector = fieldTileSize.GetValue(otTilesSprite);
			// read the tileSizeIVector.x and .y fields since IVector is a custom Orthello type.
			Type iVectorType = tileSizeIVector.GetType();
			FieldInfo fieldX = iVectorType.GetField("x");
			FieldInfo fieldY = iVectorType.GetField("y");
			if (fieldX == null || fieldY == null) {
				Debug.LogWarning("Failed to read 'fieldX' or 'fieldY' field of the IVector component. Seems as if a different version of Orthello is used. If you need texture- or sprite-atlas support, please consider updating your Orthello framework.");
				return false;
			}
			mTileWidth = (int) fieldX.GetValue(tileSizeIVector);
			mTileHeight = (int) fieldY.GetValue(tileSizeIVector);
			
			SetTileScale();
		}
		
		// Note: The _tiles field of the OTTilesSprite is nulled at play/stop in the editor,
		// so we copy the tile values to mCurrentTiles which gets persisted (because it's a
		// normal one-dimensional array).
		if (mTilesLink != null) {
			CopyCurrentTiles(mTilesLink);
		}
		
		return true;
	}
	
	//-------------------------------------------------------------------------
	// Note: param tilesArray is indexed tilesArray[x][y]
	private void CopyCurrentTiles(int[][] tilesArray) {
		if (tilesArray == null)
			return;
	
		mNumTilesX = tilesArray.Length;
		mNumTilesY = tilesArray[0].Length;	
		int numTilesXY = mNumTilesX * mNumTilesY;
	
		if (mCurrentTiles == null || mCurrentTiles.Length != numTilesXY) {
			mCurrentTiles = new int [numTilesXY];
		}
		
		for (int y = 0; y < mNumTilesY; ++y) {
			for (int x = 0; x < mNumTilesX; ++x) {
				mCurrentTiles[x + y * mNumTilesX] = tilesArray[x][y]; // tilesArray is indexed tilesArray[x][y]
			}
		}
	}
	
	//-------------------------------------------------------------------------
	private bool UpdateTileColliders() {
	
		// if no colliders instantiated -> create new colliders, return
		// else if colliders unchanged -> return
		// else if wasScrolled+/-1 -> scroll
		// else arbitrarily changed -> delete old, create new colliders
	
		int dy = 0;
		
		// Instantiate tile colliders according to mCurrentTiles
		if (!AreTileColliderObjectsInstantiated()) {
			DestroyTileObjectsGroupNode();
			InstantiateAllTileColliders();
			return true;
		}
		else if (AreTilesUnchanged()) {
			return true; // nothing to do.
		}
		else if (WasScrolled(out dy)) {
			ScrollTileColliders(dy);
			return true;
		}
		else { // arbitrarily changed
			DestroyGameObject(mTileObjectsGroupNode);
			InstantiateAllTileColliders();
			return true;
		}
	}
	
	//-------------------------------------------------------------------------
	private bool AreTilesUnchanged() {
		if (mCurrentTiles == null || mCurrentTiles.Length == 0)
			return false;
	
		return AreTileRowsEqual(0, mNumTilesY, 0);
	}
	
	//-------------------------------------------------------------------------
	private bool WasScrolled(out int dy) {
	
		dy = 0;
		if (mCurrentTiles == null || mCurrentTiles.Length == 0)
			return false;
			
		bool wasScrolledDown = AreTileRowsEqual(0, mNumTilesY-1, 1);
		if (wasScrolledDown) {
			dy = -1;
			return true;
		}
		bool wasScrolledUp = AreTileRowsEqual(1, mNumTilesY, 0);
		if (wasScrolledUp) {
			dy = 1;
			return true;
		}
		return false;
	}
	
	//-------------------------------------------------------------------------
	private bool AreTileRowsEqual(int startYNew, int endYNew, int startYOld) {
		
		for (int yNew = startYNew, yOld = startYOld; yNew < endYNew; ++yNew, ++yOld) {
			if (!IsTileRowEqual(yNew, yOld)) {
				return false;
			}
		}
		return true;
	}
	
	//-------------------------------------------------------------------------
	private bool IsTileRowEqual(int yNew, int yOld) {
		
		for (int x = 0; x < mNumTilesX; ++x) {
			int newTileIndex = mCurrentTiles[x + yNew * mNumTilesX];
			int oldTileIndex = mInstantiatedTileIndices[x + yOld * mNumTilesX];
			if (newTileIndex != oldTileIndex) {
				return false;
			}
		}
		return true;
	}
	
	//-------------------------------------------------------------------------
	private bool ScrollTileColliders(int dy) {
		if (dy == 0) {
			return false;
		}
		
		int rowIndexToClear;
		int rowIndexForNewColliders;
		
		if (dy == -1) {
			rowIndexToClear = 0;
			rowIndexForNewColliders = mNumTilesY-1;
		}
		else { // dy = +1
			rowIndexToClear = mNumTilesY-1;
			rowIndexForNewColliders = 0;
		}
		ClearColliderRow(rowIndexToClear);
		MoveColliderRows(dy);
		InstantiateColliderRow(rowIndexForNewColliders, mNumTilesX);
		return true;
	}
	
	//-------------------------------------------------------------------------
	private void DestroyTileObjectsGroupNode() {
		foreach (Transform child in this.transform) {
			if (child.name.Equals(RUNTIME_GROUP_NODE_NAME)) {
				GameObject.DestroyImmediate(child.gameObject);
				return;
			}
		}
	}
	
	//-------------------------------------------------------------------------
	private void ClearColliderRow(int y) {
		
		for (int x = 0; x < mNumTilesX; ++x) {
			mInstantiatedTileIndices[x + y * mNumTilesX] = 0;
			GameObject colliderObject = mInstantiatedTileObjects[x + y * mNumTilesX];
			DestroyGameObject(colliderObject);
		}
	}
	
	//-------------------------------------------------------------------------
	private void MoveColliderRows(int dy) {
		
		int srcStartY;
		int srcEndY;
		int dstStartY;
		
		if (dy == -1) {
			// down
			srcStartY = 1;
			dstStartY = 0;
			srcEndY = mNumTilesY;
			
			// ascend row by row.
			for (int srcY = srcStartY, dstY = dstStartY; srcY < srcEndY; ++srcY, ++dstY) {
				for (int x = 0; x < mNumTilesX; ++x) {
					int dstIndex = x + dstY * mNumTilesX;
					int srcIndex = x + srcY * mNumTilesX;
					mInstantiatedTileIndices[dstIndex] = mInstantiatedTileIndices[srcIndex];
					mInstantiatedTileObjects[dstIndex] = mInstantiatedTileObjects[srcIndex];
					
					if (mInstantiatedTileObjects[dstIndex] != null) {
						Transform colliderTransform = mInstantiatedTileObjects[dstIndex].transform;
						colliderTransform.localPosition = new Vector3(colliderTransform.localPosition.x,
																	  colliderTransform.localPosition.y + dy * mTileHeight,
																	  colliderTransform.localPosition.z);
					}
				}
			}
		}
		else { // dy = +1
			// up
			srcStartY = mNumTilesY - 2;
			dstStartY = mNumTilesY - 1;
			srcEndY = 0;
			
			// descend row by row.
			for (int srcY = srcStartY, dstY = dstStartY; srcY >= srcEndY; --srcY, --dstY) {
				for (int x = 0; x < mNumTilesX; ++x) {
					int dstIndex = x + dstY * mNumTilesX;
					int srcIndex = x + srcY * mNumTilesX;
					mInstantiatedTileIndices[dstIndex] = mInstantiatedTileIndices[srcIndex];
					mInstantiatedTileObjects[dstIndex] = mInstantiatedTileObjects[srcIndex];
					
					if (mInstantiatedTileObjects[dstIndex] != null) {
						Transform colliderTransform = mInstantiatedTileObjects[dstIndex].transform;
						colliderTransform.localPosition = new Vector3(colliderTransform.localPosition.x,
																	  colliderTransform.localPosition.y + dy * mTileHeight,
																	  colliderTransform.localPosition.z);
					}
				}
			}
		}
	}
	
	//-------------------------------------------------------------------------
	bool AreTileColliderObjectsInstantiated() {
		if (mInstantiatedTileObjects != null) {
			return true;
		}
		return false;
	}
	
	//-------------------------------------------------------------------------
	private bool InstantiateAllTileColliders() {
		
		if (mCurrentTiles == null || mCurrentTiles.Length <= 0) {
			return false;
		}
		
		mInstantiatedTileIndices = new int[mNumTilesY * mNumTilesX];
		mInstantiatedTileObjects = new GameObject[mNumTilesY * mNumTilesX];
		
		mTileObjectsGroupNode = new GameObject(RUNTIME_GROUP_NODE_NAME);
		mTileObjectsGroupNode.transform.parent = mOTTilesSprite.transform;
		mTileObjectsGroupNode.transform.localPosition = Vector3.zero;
		mTileObjectsGroupNode.transform.localScale = Vector3.one;
		
		mBottomLeftOffset = new Vector2(mTileWidth * mNumTilesX, mTileHeight * mNumTilesY);
		
		// Instantiate tile colliders according to mCurrentTiles
		for (int y = 0; y < mNumTilesY; ++y) {
			InstantiateColliderRow(y, mNumTilesX);
		}
		
		mAreTileCollidersInstantiated = true;
		return true;
	}
	
	//-------------------------------------------------------------------------
	private bool InstantiateColliderRow(int y, int mNumTilesX) {
		// Instantiate a collider row.
		for (int x = 0; x < mNumTilesX; ++x) {
			int tileIndex = mCurrentTiles[x + y * mNumTilesX];
			mInstantiatedTileIndices[x + y * mNumTilesX] = tileIndex;
			if (tileIndex >= 0) {
				GameObject colliderPrefab = mColliderPrefabAtTileIndex[tileIndex];
				if (colliderPrefab != null) {
					Vector3 position = new Vector3((x + 0.5f) * mTileWidth - mBottomLeftOffset.x,
												   (y + 0.5f) * mTileHeight - mBottomLeftOffset.y, 0);
					GameObject newColliderObject = (GameObject) UnityEngine.Object.Instantiate(colliderPrefab, position, Quaternion.identity);
					newColliderObject.name = "Tile y" + y + " x" + x + " " +colliderPrefab.name;
					newColliderObject.transform.parent = mTileObjectsGroupNode.transform;
					newColliderObject.transform.localPosition = position;
					newColliderObject.transform.localScale = mTileScale;
					mInstantiatedTileObjects[x + mNumTilesX * y] = newColliderObject;
				}
			}
		}
		return true;
	}
	
	//-------------------------------------------------------------------------
	public void SetTileScale() {
		mTileScale = new Vector3(mTileWidth / mAtlasFrameSizeInPixels.x, mTileHeight / mAtlasFrameSizeInPixels.y, 1);
	}
	
	//-------------------------------------------------------------------------
	public void DestroyGameObject(GameObject gameObject) {
	
#if UNITY_EDITOR
		if (Application.isEditor && !Application.isPlaying) {
			GameObject.DestroyImmediate(gameObject);
		}
		else {
#endif
			GameObject.Destroy(gameObject);
#if UNITY_EDITOR
		}
#endif
	}
}
