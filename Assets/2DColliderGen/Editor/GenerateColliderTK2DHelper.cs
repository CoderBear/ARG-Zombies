using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

//-------------------------------------------------------------------------
/// <summary>
/// Class that provides the collider mesh generation functionality for
/// 2D Toolkit (TK2D) sprites.
/// </summary>
public class GenerateColliderTK2DHelper {
	
	protected const int POLYGON_COLLIDER_TYPE_INT_VALUE = 4; // NOTE: keep up to date with the ColliderType enum in tk2dSpriteCollection.cs.
	
	protected PolygonOutlineFromImageFrontend mOutlineAlgorithm = null;
	protected IslandDetector mIslandDetector = null;
    protected IslandDetector.Region[] mIslands = null;
    protected IslandDetector.Region[] mSeaRegions = null;

	protected ColliderRegionData[] mColliderRegions = null;

	Texture2D mMainTex = null;
	bool [,]mBinaryImage = null;
	
	[SerializeField] protected RegionIndependentParametersTK2D mRegionIndependentParameters = new RegionIndependentParametersTK2D();
	[SerializeField] protected ColliderRegionParametersTK2D[] mColliderRegionParameters = null;

	// Setters and Getters
	public int MaxPointCountOfFirstEnabledRegion {
		get {
			ColliderRegionParametersTK2D firstRegionParams = this.FirstActiveColliderRegionParameters;
			if (firstRegionParams == null) {
				return 0;
			}
			return firstRegionParams.MaxPointCount;
		}
		set {
			ColliderRegionParametersTK2D firstRegionParams = this.FirstActiveColliderRegionParameters;
			if (firstRegionParams == null) {
				return;
			}
			firstRegionParams.MaxPointCount = value;
		}
	}
	
	public int ActualPointCountOfAllRegions {
		get {
			int pointCount = 0;
			if (this.ColliderRegions != null && this.ColliderRegions.Length != 0) {
				foreach (ColliderRegionData colliderRegion in this.ColliderRegions) {
					if (colliderRegion.mReducedOutlineVertices != null) {
						pointCount += colliderRegion.mReducedOutlineVertices.Count;
					}
				}
			}
			else if (this.NumEnabledColliderRegions > 0) {
				pointCount = this.ColliderRegionsTotalMaxPointCount;
			}
			else {
				pointCount = 0;
			}
			return pointCount;
		}
	}
	
	public ColliderRegionParametersTK2D FirstActiveColliderRegionParameters {
		get {
			if (mColliderRegionParameters == null) {
				return null;
			}
			foreach (ColliderRegionParametersTK2D regionParameters in mColliderRegionParameters) {
				if (regionParameters.EnableRegion) {
					return regionParameters;
				}
			}
			return null;
		}
	}
	
	public int ColliderRegionsTotalMaxPointCount {
		get {
			if (mColliderRegionParameters == null) {
				return 0;
			}
			int totalCount = 0;
			foreach (ColliderRegionParametersTK2D regionParameters in mColliderRegionParameters) {
				if (regionParameters.EnableRegion) {
					totalCount += regionParameters.MaxPointCount;
				}
			}
			return totalCount;
		}
	}
	
	public int NumEnabledColliderRegions {
		get {
			if (mColliderRegionParameters == null) {
				return 0;
			}
			int totalCount = 0;
			foreach (ColliderRegionParametersTK2D regionParameters in mColliderRegionParameters) {
				if (regionParameters.EnableRegion) {
					++totalCount;
				}
			}
			return totalCount;
		}
	}
	
	public RegionIndependentParametersTK2D RegionIndependentParams {
		get {
			return mRegionIndependentParameters;
		}
		set {
			mRegionIndependentParameters = value;
		}
	}
	
	public ColliderRegionParametersTK2D[] ColliderRegionParams {
		get {
			return mColliderRegionParameters;
		}
		set {
			mColliderRegionParameters = value;
		}
	}
	
	public ColliderRegionData[] ColliderRegions {
		get {
			return mColliderRegions;
		}
		set {
			mColliderRegions = value;
		}
	}
	
	protected Texture2D UsedTexture {
		get {
			return mRegionIndependentParameters.CustomTex != null ? mRegionIndependentParameters.CustomTex : mMainTex;
		}
	}
	
	//-------------------------------------------------------------------------
	public int GetSpriteID(Component tk2dSpriteComponent) {
		Type componentType = tk2dSpriteComponent.GetType();
		FieldInfo fieldSpriteId = componentType.GetField("_spriteId", BindingFlags.Instance | BindingFlags.NonPublic);
		if (fieldSpriteId == null) {
			Debug.LogError("Detected a missing '_spriteId' member variable at the tk2dSpriteComponent class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return 0;
		}
		return (int) fieldSpriteId.GetValue(tk2dSpriteComponent);
	}
	
	//-------------------------------------------------------------------------
	public bool EnsureColliderTypePolyCollider(object spriteCollectionProxy, int[] spriteIDs) {
		bool wereAllSuccessful = true;
		
		foreach (int spriteID in spriteIDs) {
			object spriteCollectionDefinition = GetTK2DSpriteCollectionDefinition(spriteCollectionProxy, spriteID);
			if (spriteCollectionDefinition == null) {
				// last error is already set in GetTK2DSpriteCollectionDefinition() above.
				wereAllSuccessful = false;
				continue;
			}
			Type spriteCollectionDefinitionType = spriteCollectionDefinition.GetType();
			FieldInfo fieldColliderType = spriteCollectionDefinitionType.GetField("colliderType");
			if (fieldColliderType == null) {
				Debug.LogError("Detected a missing 'colliderType' member variable at the tk2dSpriteCollectionDefinition class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
				return false;
			}
			object enumValue = fieldColliderType.GetValue(spriteCollectionDefinition);
			Type enumType = enumValue.GetType();
			object newEnumValue = Enum.ToObject(enumType, POLYGON_COLLIDER_TYPE_INT_VALUE);
			fieldColliderType.SetValue(spriteCollectionDefinition, newEnumValue);
		}
				
		return wereAllSuccessful;
	}
	
	//-------------------------------------------------------------------------
	public bool PrepareIslandsForGui(out bool allHaveSameNumberOfColliderRegions, object spriteCollection, int[] spriteIDs) {
		
		allHaveSameNumberOfColliderRegions = true;
		int commonNumColliderRegions = -1;
		bool wereAllSuccessful = true;
		
		foreach (int spriteID in spriteIDs) {
			
			object spriteCollectionDefinition = null;
			int numColliderRegions = 0;
			if (!SetupBinaryImageAndIslands(out numColliderRegions, out spriteCollectionDefinition, spriteCollection, spriteID)) {
				wereAllSuccessful = false;
				allHaveSameNumberOfColliderRegions = false;
				continue;
			}
			if (commonNumColliderRegions == -1) {
				commonNumColliderRegions = numColliderRegions;
			}
			if (numColliderRegions != commonNumColliderRegions) {
				allHaveSameNumberOfColliderRegions = false;
			}
			
			CalculateUnreducedOutline();
			
			int numNonEmptyRegions = ReduceOutline();
			if (numNonEmptyRegions == 0) {
				wereAllSuccessful = false;
				allHaveSameNumberOfColliderRegions = false;
				continue;
			}
		}
		return wereAllSuccessful;
	}
	
	//-------------------------------------------------------------------------
	public bool GenerateColliderVertices(object spriteCollection, int[] spriteIDs) {
		bool wereAllSuccessful = true;
		
		foreach (int spriteID in spriteIDs) {
			
			object spriteCollectionDefinition = null;
			int discardedOutParamInt;
			if (!SetupBinaryImageAndIslands(out discardedOutParamInt, out spriteCollectionDefinition, spriteCollection, spriteID)) {
				wereAllSuccessful = false;
				continue;
			}
			
			CalculateUnreducedOutline();
			
			int numNonEmptyRegions = ReduceOutline();
			
			if (numNonEmptyRegions == 0) {
				wereAllSuccessful = false;
				continue;
			}
			
			bool successfullyWritten = WriteColliderDataToSpriteCollection(spriteCollectionDefinition, spriteCollection, spriteID, numNonEmptyRegions);
			if (!successfullyWritten) {
				wereAllSuccessful = false;
				continue;
			}
		}
		
		return wereAllSuccessful;
	}
	
	//-------------------------------------------------------------------------
	public bool CalculateUnreducedOutline() {
		bool ccwVertexOrder = !mRegionIndependentParameters.FlipInsideOutside; // reverse vertex order is the outside-order because of the -1 y-scale.
        if (mOutlineAlgorithm.XScale * mOutlineAlgorithm.YScale > 0)
        {
			ccwVertexOrder = !ccwVertexOrder;
		}
		CalculateUnreducedOutlineForAllColliderRegions(ref mColliderRegions, ref mOutlineAlgorithm, mRegionIndependentParameters, mColliderRegionParameters, mBinaryImage, ccwVertexOrder);
		return true;
	}
	
	//-------------------------------------------------------------------------
	public int ReduceOutline() {
		int numNonEmptyRegions = ReduceOutlineForAllColliderRegions(ref mColliderRegions, ref mOutlineAlgorithm, mRegionIndependentParameters, mColliderRegionParameters);
		return numNonEmptyRegions;
	}
	
	//-------------------------------------------------------------------------
	public bool SetupBinaryImageAndIslands(out int numColliderRegions, out object spriteCollectionDefinition, object spriteCollection, int spriteID) {
		
		numColliderRegions = 0;
		spriteCollectionDefinition = null;
		
		mMainTex = GetTextureRef(spriteCollection, spriteID);
		if (mMainTex == null) {
			
			//Debug.LogError("No sprite texture found at sprite '" + tk2dSpriteComponent.name + "' with spriteID " + spriteID + ".");
			Debug.LogError("No sprite texture found at sprite with spriteID " + spriteID + ".");
			return false;
		}
		
		spriteCollectionDefinition = GetTK2DSpriteCollectionDefinition(spriteCollection, spriteID);
		if (spriteCollectionDefinition == null) {
			// last error is already set in GetTK2DSpriteCollectionDefinition() above.
			return false;
		}
		
		if (mOutlineAlgorithm == null) {
			mOutlineAlgorithm = new PolygonOutlineFromImageFrontend();
		}
		
		int regionXOffset = 0;
		int regionYOffset = 0;
		int regionWidth = UsedTexture.width;
		int regionHeight = UsedTexture.height;
		bool isRegionUsed = false;
		if (mRegionIndependentParameters.CustomTex == null) {
			isRegionUsed = ReadRegionParameters(out regionXOffset, out regionYOffset, out regionWidth, out regionHeight, spriteCollectionDefinition);
			regionYOffset = mMainTex.height - regionYOffset - regionHeight;		
		}
		
		mOutlineAlgorithm.BinaryAlphaThresholdImageFromTexture(out mBinaryImage, UsedTexture, mRegionIndependentParameters.AlphaOpaqueThreshold,
															   isRegionUsed, regionXOffset, regionYOffset, regionWidth, regionHeight);
		
		bool anyIslandsFound = CalculateIslandStartingPoints(mBinaryImage, out mIslands, out mSeaRegions);
        if (!anyIslandsFound) {
            return false;
        }
		
		AlphaMeshCollider.SetupColliderRegions(out mColliderRegions, mIslands, mSeaRegions);
		numColliderRegions = mColliderRegions.Length;
		SetupColliderRegionParameters(ref mColliderRegionParameters, mRegionIndependentParameters.DefaultMaxPointCount, mIslands, mSeaRegions);
		return true;
	}
	
	//-------------------------------------------------------------------------
	public bool WriteColliderDataToSpriteCollection(object spriteCollectionDefinition, object spriteCollection, int spriteID, int numNonEmptyRegions) {
		
		IEnumerable colliderIslandsArray = PrepareColliderIslands(spriteCollectionDefinition, numNonEmptyRegions);
		int regionIndex = 0;
		foreach (object island in colliderIslandsArray) {
			Type spriteColliderIslandType = island.GetType();
			FieldInfo fieldPoints = spriteColliderIslandType.GetField("points");
			if (fieldPoints == null) {
				Debug.LogError("Detected a missing 'colliderType' member variable at the tk2dSpriteCollectionDefinition class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
				return false;
			}
			while (mColliderRegions[regionIndex].mReducedOutlineVertices == null || mColliderRegions[regionIndex].mReducedOutlineVertices.Count == 0) {
				++regionIndex;
			}
			fieldPoints.SetValue(island, mColliderRegions[regionIndex].mReducedOutlineVertices.ToArray()); // TODO: by now we only support one island.
			++regionIndex;
		}
		return true;
	}
	
	//-------------------------------------------------------------------------
	public static void SetupColliderRegionParameters(ref ColliderRegionParametersTK2D[] colliderRegionParameters, int defaultMaxPointCount,
													 IslandDetector.Region[] islands, IslandDetector.Region[] seaRegions) {
		
		int numColliderRegions = islands.Length + seaRegions.Length;
		
		bool shallResetRegionParameters = (colliderRegionParameters == null || numColliderRegions != colliderRegionParameters.Length); // TODO!! check when to throw parameters away!
		if (shallResetRegionParameters) {
			
			colliderRegionParameters = new ColliderRegionParametersTK2D[numColliderRegions];
			int colliderRegionIndex = 0;
			
			// Note: We enable the first island region only. All other island- and all sea-regions are initially disabled.
			for (int islandIndex = 0; islandIndex < islands.Length; ++islandIndex) {
				
				ColliderRegionParametersTK2D newParameters = new ColliderRegionParametersTK2D();
				if (islandIndex == 0) {
					newParameters.EnableRegion = true;
				}
				else {
					newParameters.EnableRegion = false;
				}
				newParameters.MaxPointCount = defaultMaxPointCount;
				colliderRegionParameters[colliderRegionIndex++] = newParameters;
			}
			for (int seaRegionIndex = 0; seaRegionIndex < seaRegions.Length; ++seaRegionIndex) {

				ColliderRegionParametersTK2D newParameters = new ColliderRegionParametersTK2D();
				newParameters.EnableRegion = false;
				newParameters.MaxPointCount = defaultMaxPointCount;
				colliderRegionParameters[colliderRegionIndex++] = newParameters;
			}
		}
		else {
			for (int count = 0; count < colliderRegionParameters.Length; ++count) {
				colliderRegionParameters[count].RegionUpdateCalculationNeeded = true;
			}
		}
	}
	
	//-------------------------------------------------------------------------
	public static void CalculateUnreducedOutlineForAllColliderRegions(ref ColliderRegionData[] colliderRegions, ref PolygonOutlineFromImageFrontend outlineAlgorithm,
																	  RegionIndependentParametersTK2D regionIndependentParameters,
																	  ColliderRegionParametersTK2D[] colliderRegionParameters, bool [,] binaryImage,
																	  bool ccwVertexOrder) {
		
		Vector3 customOffset = regionIndependentParameters.CustomOffset;
		Vector3 customScale = regionIndependentParameters.CustomScale;
		
		for (int count = 0; count < colliderRegions.Length; ++count) {
        
			if (colliderRegionParameters[count].EnableRegion) {
				// Calculate polygon bounds
	            outlineAlgorithm.VertexReductionDistanceTolerance = regionIndependentParameters.VertexReductionDistanceTolerance;
			    outlineAlgorithm.MaxPointCount = colliderRegionParameters[count].MaxPointCount;
				
				// TODO: replace this with a joint-convex hull implementation, just a workaround for now.
				bool allRegionsConvex = regionIndependentParameters.Convex;
				outlineAlgorithm.Convex = allRegionsConvex ? true : colliderRegionParameters[count].Convex;
			    //outlineAlgorithm.Convex = colliderRegionParameters[count].Convex;
			    
				outlineAlgorithm.XOffsetNormalized = 0.0f + customOffset.x + (0.5f - (0.5f * customScale.x));
	            outlineAlgorithm.YOffsetNormalized = 1.0f - customOffset.y - (0.5f - (0.5f * customScale.y));
				outlineAlgorithm.XScale = 1.0f * customScale.x;
	            outlineAlgorithm.YScale = -1.0f * customScale.y;
			    bool outputVerticesInNormalizedSpace = false;
				
				bool regionVertexOrder = ccwVertexOrder;
				if (!colliderRegions[count].mRegionIsIsland) {
					regionVertexOrder = !regionVertexOrder;
				}
	
				colliderRegions[count].mOutlineVertexOrderIsCCW = regionVertexOrder;
	            outlineAlgorithm.UnreducedOutlineFromBinaryImage(out colliderRegions[count].mIntermediateOutlineVertices, binaryImage, colliderRegions[count].mDetectedRegion.mPointAtBorder, colliderRegions[count].mRegionIsIsland, outputVerticesInNormalizedSpace, regionVertexOrder);
			}
			else {
				colliderRegions[count].mIntermediateOutlineVertices = null;
				colliderRegions[count].mResultVertices = null;
				colliderRegions[count].mResultTriangleIndices = null;
			}
        }
	}
	
	//-------------------------------------------------------------------------
	public static int ReduceOutlineForAllColliderRegions(ref ColliderRegionData[] colliderRegions, ref PolygonOutlineFromImageFrontend outlineAlgorithm,
														  RegionIndependentParametersTK2D regionIndependentParameters,
														  ColliderRegionParametersTK2D[] colliderRegionParameters) {
		
		int numRegionsWithData = 0;
		if (colliderRegions == null || colliderRegions.Length == 0 || outlineAlgorithm == null) {
			Debug.LogError("Error: Unexpected state in ReduceOutlineForAllColliderRegions(): colliderRegions is empty or null or outlineAlgorithm is null!");
			return 0;
		}
		
		for (int count = 0; count < colliderRegions.Length; ++count) {
			
			if (colliderRegions[count].mIntermediateOutlineVertices == null) {
				colliderRegions[count].mResultVertices = null;
				colliderRegions[count].mResultTriangleIndices = null;
				continue;
			}
			if (colliderRegions[count].mReducedOutlineVertices != null && !colliderRegionParameters[count].RegionUpdateCalculationNeeded) {
				continue;
			}
			
			outlineAlgorithm.VertexReductionDistanceTolerance = regionIndependentParameters.VertexReductionDistanceTolerance;
			outlineAlgorithm.MaxPointCount = colliderRegionParameters[count].MaxPointCount;
			// TODO: replace this with a joint-convex hull implementation, just a workaround for now.
			bool allRegionsConvex = regionIndependentParameters.Convex;
			outlineAlgorithm.Convex = allRegionsConvex ? true : colliderRegionParameters[count].Convex;
			
			colliderRegions[count].mReducedOutlineVertices = outlineAlgorithm.ReduceOutline(colliderRegions[count].mIntermediateOutlineVertices, colliderRegions[count].mOutlineVertexOrderIsCCW);
			colliderRegionParameters[count].RegionUpdateCalculationNeeded = false;
			++numRegionsWithData;
		}
		return numRegionsWithData;
	}
	
	//-------------------------------------------------------------------------
    /// <returns>True if at least one island is found, false otherwise.</returns>
    protected bool CalculateIslandStartingPoints(bool[,] binaryImage, out IslandDetector.Region[] islands, out IslandDetector.Region[] seaRegions) {
		int[,] islandClassificationImage = null;
		islands = null;
		seaRegions = null;
		
		mIslandDetector = new IslandDetector();
		mIslandDetector.DetectIslandsFromBinaryImage(binaryImage, out islandClassificationImage, out islands, out seaRegions);
        
        return (islands.Length > 0);
		}
	
	//-------------------------------------------------------------------------
	protected bool ReadRegionParameters(out int regionXOffset, out int regionYOffset, out int regionWidth, out int regionHeight,
										object spriteCollectionDefinition) {
		regionXOffset = 0;
		regionYOffset = 0;
		regionWidth = 0;
		regionHeight = 0;
		
		Type spriteCollectionDefinitionType = spriteCollectionDefinition.GetType();
		FieldInfo fieldExtractRegion = spriteCollectionDefinitionType.GetField("extractRegion");
		if (fieldExtractRegion == null) {
			Debug.LogError("Detected a missing 'extractRegion' member variable at the tk2dSpriteCollectionDefinition class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		bool extractRegion = (bool) fieldExtractRegion.GetValue(spriteCollectionDefinition);
		
		FieldInfo fieldRegionX = spriteCollectionDefinitionType.GetField("regionX");
		FieldInfo fieldRegionY = spriteCollectionDefinitionType.GetField("regionY");
		FieldInfo fieldRegionH = spriteCollectionDefinitionType.GetField("regionH");
		FieldInfo fieldRegionW = spriteCollectionDefinitionType.GetField("regionW");
		if (fieldRegionX == null || fieldRegionY == null || fieldRegionH == null || fieldRegionW == null) {
			Debug.LogError("Detected a missing 'fieldRegionX/Y' or 'fieldRegionW/H' member variable at the tk2dSpriteCollectionDefinition class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		
		regionXOffset = (int) fieldRegionX.GetValue(spriteCollectionDefinition);
		regionYOffset = (int) fieldRegionY.GetValue(spriteCollectionDefinition);
		regionWidth   = (int) fieldRegionW.GetValue(spriteCollectionDefinition);
		regionHeight  = (int) fieldRegionH.GetValue(spriteCollectionDefinition);
		return extractRegion;
	}
	
	//-------------------------------------------------------------------------
	protected IList PrepareColliderIslands(object spriteCollectionDefinition, int targetNumIslands) {
		
		Type spriteCollectionDefinitionType = spriteCollectionDefinition.GetType();
		FieldInfo fieldPolyColliderIslands = spriteCollectionDefinitionType.GetField("polyColliderIslands");
		if (fieldPolyColliderIslands == null) {
			Debug.LogError("Detected a missing 'polyColliderIslands' member variable at the tk2dSpriteCollectionDefinition class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return null;
		}
		IList colliderIslandsArray = (IList) fieldPolyColliderIslands.GetValue(spriteCollectionDefinition);
		
		int currentNumIslands = 0;
		Type colliderIslandsArrayType = null;
		Type colliderIslandType = null;
		if (colliderIslandsArray == null) {
			colliderIslandType = Type.GetType("tk2dSpriteColliderIsland");
			IList tempArray = Array.CreateInstance(colliderIslandType, 0);
			colliderIslandsArrayType = tempArray.GetType();
			currentNumIslands = 0;
		}
		else {
			colliderIslandsArrayType = colliderIslandsArray.GetType();
			colliderIslandType = colliderIslandsArrayType.GetElementType();
			currentNumIslands = colliderIslandsArray.Count;
		}
		
		
		if (currentNumIslands != targetNumIslands) {
			colliderIslandsArray = Array.CreateInstance(colliderIslandType, targetNumIslands);
			for (int index = 0; index < targetNumIslands; ++index) {
				colliderIslandsArray[index] = Activator.CreateInstance(colliderIslandType);
			}
			fieldPolyColliderIslands.SetValue(spriteCollectionDefinition, colliderIslandsArray);
		}
		
		foreach (object island in colliderIslandsArray) {
			FieldInfo fieldConnected = colliderIslandType.GetField("connected");
			if (fieldConnected == null) {
				Debug.LogError("Detected a missing 'connected' member variable at TK2D's collider island class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
				continue;
			}
			bool trueValue = true;
			fieldConnected.SetValue(island, trueValue);
			// island.points is set later anyway.
		}
		return colliderIslandsArray;
	}
	
	//-------------------------------------------------------------------------
	public object GetTK2DSpriteCollection(Component tk2dSpriteComponent) {
		
		string spriteCollectionGUID = GetSpriteCollectionGUID(tk2dSpriteComponent);
		if (spriteCollectionGUID == null)
			return null;
		
		string path = AssetDatabase.GUIDToAssetPath(spriteCollectionGUID);
		object spriteCollection = AssetDatabase.LoadAssetAtPath(path, typeof(MonoBehaviour));
		if (spriteCollection == null) {
			Debug.LogError("Failed to load sprite collection at path " + path + ".");
			return null;
		}		
		return spriteCollection;
	}
	
	//-------------------------------------------------------------------------
	object GetTK2DSpriteCollectionDefinition(object spriteCollection, int spriteID) {
		
		// Actually does this:
		// tk2dSpriteCollectionDefinition[] collectionDefArray = spriteCollection.textureParams;
		// tk2dSpriteCollectionDefinition collectionDef = collectionDefArray[spriteID];
		
		Type spriteCollectionType = spriteCollection.GetType();
		FieldInfo fieldTextureParams = spriteCollectionType.GetField("textureParams"); // NOTE: the name textureParams is a bit misleading.
		if (fieldTextureParams == null) {
			Debug.LogError("Detected a missing 'textureParams' member variable at TK2D's sprite collection class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return null;
		}
		
		IEnumerable textureParams = (IEnumerable) fieldTextureParams.GetValue(spriteCollection);
		object spriteCollectionDef = null;
		int index = 0;
		foreach (object currentSpriteCollectionDef in textureParams) {
			if (index++ == spriteID) {
				spriteCollectionDef = currentSpriteCollectionDef;
				break;
			}
		}
		
		if (spriteCollectionDef == null) {
			Debug.LogError("No sprite collection definition found at spriteID " + spriteID + ".");
		}
		
		return spriteCollectionDef;
	}
	
	//-------------------------------------------------------------------------
	string GetSpriteCollectionGUID(Component tk2dSpriteComponent) {
		// Actually does this:
		// tk2dSpriteCollectionData spriteCollectionData = tk2dSpriteComponent.collection
		Type componentType = tk2dSpriteComponent.GetType().BaseType;
        FieldInfo fieldCollection = componentType.GetField("collection", BindingFlags.Instance | BindingFlags.NonPublic);
		if (fieldCollection == null) {
			Debug.LogError("Detected a missing 'collection' member variable at the tk2dSpriteComponent class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return null;
		}		
		object spriteCollectionData = fieldCollection.GetValue(tk2dSpriteComponent);
		if (spriteCollectionData == null) {
			Debug.LogError("No sprite collection data found at sprite '" + tk2dSpriteComponent.name + "'.");
			return null;
		}
		// Actually does this:
		// tk2dSpriteDefinition[] spriteDefinitions = spriteCollection.spriteDefinitions
		Type spriteCollectionDataType = spriteCollectionData.GetType();
		FieldInfo fieldSpriteCollectionGUID = spriteCollectionDataType.GetField("spriteCollectionGUID");
		if (fieldSpriteCollectionGUID == null) {
			Debug.LogError("Detected a missing 'spriteCollectionGUID' member variable at the spriteCollectionData class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return null;
		}
		
		string spriteCollectionGUID = (string) fieldSpriteCollectionGUID.GetValue(spriteCollectionData);
		return spriteCollectionGUID;
	}
	
	//-------------------------------------------------------------------------
	Texture2D GetTextureRef(object spriteCollection, int spriteID) {
		
		Texture2D texture = null;
		Type spriteCollectionType = spriteCollection.GetType();
		// first we test for the textureRefs (older version of TK2D), simpler to access.
		FieldInfo fieldTextureRefs = spriteCollectionType.GetField("textureRefs");
		if (fieldTextureRefs != null) {
			IEnumerable textureRefs = (IEnumerable) fieldTextureRefs.GetValue(spriteCollection);
			
			int index = 0;
			foreach (Texture2D currentTexture in textureRefs) {
				if (index++ == spriteID) {
					texture = currentTexture;
					break;
				}
			}
			return texture;
		}
		else {
			// now we test for the sprite collection definition (new version of TK2D, 'textureRefs' member is gone).
			object spriteCollectionDefinition = GetTK2DSpriteCollectionDefinition(spriteCollection, spriteID);
			if (spriteCollectionDefinition == null) {
				// last error is already set in GetTK2DSpriteCollectionDefinition() above.
				return null;
			}
			
			Type spriteCollectionDefinitionType = spriteCollectionDefinition.GetType();
			FieldInfo fieldTexture = spriteCollectionDefinitionType.GetField("texture");
			if (fieldTexture == null) {
				Debug.LogError("Detected a missing 'texture' member variable at the tk2dSpriteCollectionDefinition class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
				return null;
			}
			texture = (Texture2D) fieldTexture.GetValue(spriteCollectionDefinition);
		}
		return texture;
	}
}
