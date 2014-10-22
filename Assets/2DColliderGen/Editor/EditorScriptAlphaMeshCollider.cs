#if !(UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
#define UNITY_4_3_AND_LATER
#endif

#if (UNITY_2_6 || UNITY_2_6_1 || UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4)
#define ONLY_SINGLE_SELECTION_SUPPORTED_IN_INSPECTOR
#endif

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

//-------------------------------------------------------------------------
/// <summary>
/// Editor class for the AlphaMeshCollider component.
/// </summary>
[CustomEditor(typeof(AlphaMeshCollider))]
[CanEditMultipleObjects]
public class EditorScriptAlphaMeshCollider : Editor {
	
	protected float mOldAlphaThreshold = 0;
	protected int mOldTargetColliderTypeEnumIndex = 0;
	protected bool mOldFlipNormals = false;
	protected bool mFlipHorizontalChanged = false;
	protected bool mFlipVerticalChanged = false;
	protected bool mOldConvex = false;
	protected int mOldPointCount = 0;
	protected float mOldThickness = 0;
	protected float mOldDistanceTolerance = 0;
	protected float mOldCustomRotation = 0.0f;
	protected Vector2 mOldCustomScale = Vector2.one;
	protected Vector3 mOldCustomOffset = Vector3.zero;
	protected bool mOldIsCustomAtlasRegionUsed = false;
	public float mOldCustomAtlasFrameRotation = 0.0f;
		
	protected bool mLiveUpdate = true;
	protected bool mShowAdvanced = false;
	protected bool mShowTextureRegionSection = false;
	protected bool mShowHolesAndIslandsSection = false;
	protected Texture2D mOldCustomTex = null;
	protected int mPointCountSliderMax = 100;
	
	SerializedProperty targetLiveUpdate;
	SerializedProperty targetAlphaOpaqueThreshold;
	SerializedProperty targetFlipNormals;
	SerializedProperty targetConvex;
	//SerializedProperty targetMaxPointCount;
	SerializedProperty targetVertexReductionDistanceTolerance;
	SerializedProperty targetThickness;
	SerializedProperty targetHasOTSpriteComponent;
	SerializedProperty targetHasOTTilesSpriteComponent;
	SerializedProperty targetTargetColliderType;
	SerializedProperty targetCopyOTSpriteFlipping;
	SerializedProperty targetCustomRotation;
	SerializedProperty targetCustomScale;
	SerializedProperty targetCustomOffset;
	
	SerializedProperty targetIsCustomAtlasRegionUsed;
	SerializedProperty targetCustomAtlasFrameRotation;
	
	//-------------------------------------------------------------------------
	enum OTTilesSpriteSelectionStatus {
		NONE = 0,
		MIXED,
		ALL
	}
	
	//-------------------------------------------------------------------------
	class DuplicatePermittingIntComparer : IComparer<int> {
		
		public int Compare(int x, int y) {
			if (x > y)
				return -1;
			else
				return 1;
		}
	}
	
	//-------------------------------------------------------------------------
	[MenuItem ("Component/Physics/Alpha MeshCollider", false, 51)]
	static void ComponentPhysicsAlphaMeshCollider() {
		
		foreach (GameObject gameObj in Selection.gameObjects) {
			AlphaMeshCollider alphaCollider = gameObj.GetComponent<AlphaMeshCollider>();
			if (alphaCollider == null) {
				alphaCollider = gameObj.AddComponent<AlphaMeshCollider>();
			}
		}
    }
	//-------------------------------------------------------------------------
	// Validation function for the function above.
	[MenuItem ("Component/Physics/Alpha MeshCollider", true)]
	static bool ValidateComponentPhysicsAlphaMeshCollider() {
		
		if (Selection.gameObjects.Length == 0) {
			return false;
		}
		else {
			foreach (GameObject gameObj in Selection.gameObjects) {
				object tk2dSprite = gameObj.GetComponent("tk2dBaseSprite");
				if (tk2dSprite != null) {
					return false;
				}
			}
			return true; // no tk2dSprite selected
		}
    }
	
	
	//-------------------------------------------------------------------------
	[MenuItem ("2D ColliderGen/Add AlphaMeshCollider", false, 10)]
	static void ColliderGenAddAlphaMeshCollider() {
		
		foreach (GameObject gameObj in Selection.gameObjects) {
			AlphaMeshCollider alphaCollider = gameObj.GetComponent<AlphaMeshCollider>();
			if (alphaCollider == null) {
				alphaCollider = gameObj.AddComponent<AlphaMeshCollider>();
			}
		}
    }
	//-------------------------------------------------------------------------
	// Validation function for the function above.
	[MenuItem ("2D ColliderGen/Add AlphaMeshCollider", true)]
	static bool ValidateColliderGenAddAlphaMeshCollider() {
		
		if (Selection.gameObjects.Length == 0) {
			return false;
		}
		else {
			foreach (GameObject gameObj in Selection.gameObjects) {
				Component tk2dSprite = gameObj.GetComponent("tk2dBaseSprite");
				if (tk2dSprite != null) {
					return false;
				}
			}
			return true; // no tk2dSprite selected
		}
    }
	
	//-------------------------------------------------------------------------
	[MenuItem ("2D ColliderGen/Select AlphaMeshCollider Children", false, 11)]
	static void ColliderGenSelectChildAlphaMeshColliders() {
		
		SelectChildAlphaMeshColliders(Selection.gameObjects);
    }
	//-------------------------------------------------------------------------
	// Validation function for the function above.
	[MenuItem ("2D ColliderGen/Remove AlphaMeshCollider Components", true)]
	static bool ValidateColliderGenRemoveColliderAndGenerator() {
		
		return (Selection.gameObjects.Length > 0);
    }

	//-------------------------------------------------------------------------
	[MenuItem ("2D ColliderGen/Remove AlphaMeshCollider Components", false, 11)]
	static void ColliderGenRemoveColliderAndGenerator() {
		
		RemoveColliderAndGenerator(Selection.gameObjects);
    }
	//-------------------------------------------------------------------------
	// Validation function for the function above.
	[MenuItem ("2D ColliderGen/Select AlphaMeshCollider Children", true)]
	static bool ValidateColliderGenSelectChildAlphaMeshColliders() {
		
		return (Selection.gameObjects.Length > 0);
    }
	
	//-------------------------------------------------------------------------
	[MenuItem ("2D ColliderGen/SmoothMoves Specific/Add AlphaMeshColliders To BoneAnimation", false, 100)]
	static void ColliderGenAddAlphaMeshColliderToAllBones() {
		
		foreach (GameObject gameObj in Selection.gameObjects) {
			Component boneAnimObject = gameObj.GetComponent("BoneAnimation");
			if (boneAnimObject != null) {
				AddCollidersToBoneAnimationTree(gameObj.transform);
			}
		}
		
		SelectChildAlphaMeshColliders(Selection.gameObjects);
    }
	//-------------------------------------------------------------------------
	// Validation function for the function above.
	[MenuItem ("2D ColliderGen/SmoothMoves Specific/Add AlphaMeshColliders To BoneAnimation", true)]
	static bool ValidateColliderGenAddAlphaMeshColliderToAllBones() {
		
		foreach (GameObject gameObj in Selection.gameObjects) {
			Component boneAnimObject = gameObj.GetComponent("BoneAnimation");
			if (boneAnimObject != null) {
				return true;
			}
		}
		return false; // no BoneAnimation component found.
    }
	
	//-------------------------------------------------------------------------
	[MenuItem ("2D ColliderGen/Orthello Specific/Add AlphaMeshColliders To OTTileMap", false, 101)]
	static void ColliderGenAddAlphaMeshColliderToTileMap() {
		
		foreach (GameObject gameObj in Selection.gameObjects) {
			Component tileMapObject = gameObj.GetComponent("OTTileMap");
			if (tileMapObject != null) {
				AddCollidersToOTTileMap(gameObj.transform, tileMapObject);
			}
		}
    }
	//-------------------------------------------------------------------------
	// Validation function for the function above.
	[MenuItem ("2D ColliderGen/Orthello Specific/Add AlphaMeshColliders To OTTileMap", true)]
	static bool ValidateColliderGenAddAlphaMeshColliderToTileMap() {
		
		foreach (GameObject gameObj in Selection.gameObjects) {
			Component tileMapObject = gameObj.GetComponent("OTTileMap");
			if (tileMapObject != null) {
				return true;
			}
		}
		return false; // no OTTileMap component found.
    }
	
	//-------------------------------------------------------------------------
	void OnEnable() {
        // Setup the SerializedProperties
		
		targetLiveUpdate = serializedObject.FindProperty("mRegionIndependentParameters.mLiveUpdate");
		targetAlphaOpaqueThreshold = serializedObject.FindProperty("mRegionIndependentParameters.mAlphaOpaqueThreshold");
		targetFlipNormals = serializedObject.FindProperty("mRegionIndependentParameters.mFlipInsideOutside");
		targetConvex = serializedObject.FindProperty("mRegionIndependentParameters.mConvex");
		//targetMaxPointCount = serializedObject.FindProperty("mMaxPointCount");
		targetVertexReductionDistanceTolerance = serializedObject.FindProperty("mRegionIndependentParameters.mVertexReductionDistanceTolerance");
		targetThickness = serializedObject.FindProperty("mRegionIndependentParameters.mThickness");
#if UNITY_4_3_AND_LATER
		targetTargetColliderType = serializedObject.FindProperty("mRegionIndependentParameters.mTargetColliderType");
#endif
		targetHasOTSpriteComponent = serializedObject.FindProperty("mHasOTSpriteComponent");
		targetHasOTTilesSpriteComponent = serializedObject.FindProperty("mHasOTTilesSpriteComponent");
		targetCopyOTSpriteFlipping = serializedObject.FindProperty("mRegionIndependentParameters.mCopyOTSpriteFlipping");
		targetCustomRotation = serializedObject.FindProperty("mRegionIndependentParameters.mCustomRotation");
		targetCustomScale = serializedObject.FindProperty("mRegionIndependentParameters.mCustomScale");
		targetCustomOffset = serializedObject.FindProperty("mRegionIndependentParameters.mCustomOffset");
		
		targetIsCustomAtlasRegionUsed = serializedObject.FindProperty("mRegionIndependentParameters.mIsCustomAtlasRegionUsed");
		targetCustomAtlasFrameRotation = serializedObject.FindProperty("mRegionIndependentParameters.mCustomAtlasFrameRotation");
		
		mPointCountSliderMax = AlphaMeshColliderPreferences.Instance.ColliderPointCountSliderMaxValue;
    }
	
    //-------------------------------------------------------------------------
	// Newer multi-selection version.
	public override void OnInspectorGUI() {
		
		//EditorGUIUtility.LookLikeInspector();
		
		// Update the serializedProperty - needed in the beginning of OnInspectorGUI.
		serializedObject.Update();
		
		mOldAlphaThreshold = targetAlphaOpaqueThreshold.floatValue;
#if UNITY_4_3_AND_LATER
		mOldTargetColliderTypeEnumIndex = targetTargetColliderType.enumValueIndex;
#endif
		mOldFlipNormals = targetFlipNormals.boolValue;
		mOldConvex = targetConvex.boolValue;
		mOldThickness = targetThickness.floatValue;
		mOldDistanceTolerance = targetVertexReductionDistanceTolerance.floatValue;
		mOldCustomRotation = targetCustomRotation.floatValue;
		mOldCustomScale = targetCustomScale.vector2Value;
		mOldCustomOffset = targetCustomOffset.vector3Value;
		mOldIsCustomAtlasRegionUsed = targetIsCustomAtlasRegionUsed.boolValue;
		mOldCustomAtlasFrameRotation = targetCustomAtlasFrameRotation.floatValue;

		Texture2D usedTexture = null;
		bool usedTextureIsCustomTex = false;
		bool areUsedTexturesDifferent = false;
		bool areOutputDirectoriesDifferent = false;
		bool areOutputFilenamesDifferent = false;
		bool areGroupSuffixesDifferent = false;
		float imageMinExtent = 128;
		string commonOutputDirectoryPath = null;
		string commonOutputFilename = null;
		string commonGroupSuffix = null;
		Texture2D commonCustomTexture = null;
		bool areCustomTexturesDifferent = false;
		Vector2 commonCustomAtlasFramePositionInPixels = Vector2.zero;
		Vector2 commonCustomAtlasFrameSizeInPixels = Vector2.zero;
		bool isCommonCustomAtlasFramePositionInPixelsDifferent = false;
		bool isCommonCustomAtlasFrameSizeInPixelsDifferent = false;
		bool hasCommonCustomAtlasFramePositionInPixelsChanged = false;
		bool hasCommonCustomAtlasFrameSizeInPixelsChanged = false;
		int numObjectsWithSmoothMovesBoneAnimationParent = 0;
		bool allHaveSmoothMovesBoneAnimationParent = false;
		bool commonApplySmoothMovesScaleAnim = false;
		bool isApplySmoothMovesScaleAnimDifferent = false;
		bool hasCommonApplySmoothMovesScaleAnimChanged = false;
		bool haveColliderRegionEnabledChanged = false;
		bool haveColliderRegionMaxPointCountChanged = false;
		bool haveColliderRegionConvexChanged = false;
		int commonFirstEnabledMaxPointCount = 0;
		bool isFirstEnabledMaxPointCountDifferent = false;
		bool hasFirstEnabledMaxPointCountChanged = false;		

		bool hasDifferentNumColliderFrames = false;
		int commonNumColliderFrames = 1;
		int commonActiveColliderFrame = 1;
		
		bool isAtlas = false;
		bool canReloadAnyCollider = false;
		bool canRecalculateAnyCollider = false;
		bool anyColliderHasMultipleFrames = false;
		bool anyColliderWithoutActiveIslandsOrHoles = false; // if we have no active islands/holes currently in any of the selected objects OR have no island ticked in the inspector menu.
		OTTilesSpriteSelectionStatus otTilesSpriteSelectionStatus = OTTilesSpriteSelectionStatus.NONE;
		
		Object[] targetObjects = serializedObject.targetObjects;
		SortedDictionary<int, AlphaMeshCollider> sortedTargets = SortAlphaMeshColliders(targetObjects);
		areUsedTexturesDifferent = AreUsedTexturesDifferent(sortedTargets);

		bool buttonRecalculateTileCollidersPressed = false;
		bool buttonRecalculateColliderPressed = false;
		bool buttonReloadColliderPressed = false;
		bool buttonReloadAllCollidersAllFramesPressed = false;
		bool buttonRecalculateAllCollidersAllFramesPressed = false;
		bool buttonDecreaseActiveFrameIndexPressed = false;
		bool buttonIncreaseActiveFrameIndexPressed = false;
		bool buttonDisplayAllFramesPressed = false;
		
		AlphaMeshCollider targetObject;
		for (int targetIndex = 0; targetIndex != targetObjects.Length; ++targetIndex) {
			targetObject = (AlphaMeshCollider) targetObjects[targetIndex];
			Texture2D currentTexture = targetObject.UsedTexture;
			usedTextureIsCustomTex = (targetObject.CustomTex != null);
			isAtlas = targetObject.mIsAtlasUsed;
			int textureWidth = targetObject.UsedTexture != null ? targetObject.UsedTexture.width : 0;
			int textureHeight = targetObject.UsedTexture != null ? targetObject.UsedTexture.height : 0;
			
			imageMinExtent = Mathf.Min(imageMinExtent, Mathf.Min(textureWidth, textureHeight));
			
			if (targetObject.CanReloadCollider)
				canReloadAnyCollider = true;
			if (targetObject.CanRecalculateCollider)
				canRecalculateAnyCollider = true;
			if (targetObject.HasMultipleColliderFrames)
				anyColliderHasMultipleFrames = true;
			
			// set commonOutputDirectoryPath and areOutputDirectoriesDifferent
			if (commonOutputDirectoryPath != null && !commonOutputDirectoryPath.Equals(targetObject.ColliderMeshDirectory)) {
				areOutputDirectoriesDifferent = true;
			}
			else {
				commonOutputDirectoryPath = targetObject.ColliderMeshDirectory;
			}
			
			// set commonGroupSuffix and areGroupSuffixesDifferent
			if (commonGroupSuffix != null && !commonGroupSuffix.Equals(targetObject.GroupSuffix)) {
				areGroupSuffixesDifferent = true;
			}
			else {
				commonGroupSuffix = targetObject.GroupSuffix;
			}
			
			// set commonOutputFilename and areOutputFilenamesDifferent
			if (commonOutputFilename != null && !commonOutputFilename.Equals(targetObject.mColliderMeshFilename)) {
				areOutputFilenamesDifferent = true;
			}
			else {
				commonOutputFilename = targetObject.mColliderMeshFilename;
			}
			
			// set commonCustomTexture and areCustomTexturesDifferent
			if (commonCustomTexture != null && commonCustomTexture != targetObject.CustomTex) {
				areCustomTexturesDifferent = true;
			}
			else {
				commonCustomTexture = targetObject.CustomTex;
			}
			
			if (!usedTexture) {
				usedTexture = currentTexture;
			}
			
			// set commonCustomAtlasFramePositionInPixels
			if (targetIndex != 0 && commonCustomAtlasFramePositionInPixels != targetObject.CustomAtlasFramePositionInPixels) {
				isCommonCustomAtlasFramePositionInPixelsDifferent = true;
			}
			else {
				commonCustomAtlasFramePositionInPixels = targetObject.CustomAtlasFramePositionInPixels;
			}
			// set commonCustomAtlasFrameSizeInPixels
			if (targetIndex != 0 && commonCustomAtlasFrameSizeInPixels != targetObject.CustomAtlasFrameSizeInPixels) {
				isCommonCustomAtlasFrameSizeInPixelsDifferent = true;
			}
			else {
				commonCustomAtlasFrameSizeInPixels = targetObject.CustomAtlasFrameSizeInPixels;
			}
			
			// set numObjectsWithSmoothMovesBoneAnimationParent
			if (targetObject.HasSmoothMovesBoneAnimationParent) {
				++numObjectsWithSmoothMovesBoneAnimationParent;
			}
			
			// set commonApplySmoothMovesScaleAnim
			if (targetIndex != 0 && commonApplySmoothMovesScaleAnim != targetObject.ApplySmoothMovesScaleAnim) {
				isApplySmoothMovesScaleAnimDifferent = true;
			}
			else {
				commonApplySmoothMovesScaleAnim = targetObject.ApplySmoothMovesScaleAnim;
			}
			
			if (targetIndex != 0 &&  commonFirstEnabledMaxPointCount != targetObject.MaxPointCountOfFirstEnabledRegion) {
				isFirstEnabledMaxPointCountDifferent = true;
			}
			else {
				commonFirstEnabledMaxPointCount = targetObject.MaxPointCountOfFirstEnabledRegion;
			}

			if (targetIndex != 0 && commonNumColliderFrames != targetObject.NumColliderFrames) {
				hasDifferentNumColliderFrames = true;
			}
			else {
				commonNumColliderFrames = targetObject.NumColliderFrames;
				commonActiveColliderFrame = targetObject.ActiveColliderIndex; // we don't really care about a uniform active collider frame index, as long as all selected objects have the same amount of frames.
			}
		}
		
		// set otTilesSpriteSelectionStatus
		if (targetHasOTTilesSpriteComponent.hasMultipleDifferentValues) {
			otTilesSpriteSelectionStatus = OTTilesSpriteSelectionStatus.MIXED;
		}
		else if (targetHasOTTilesSpriteComponent.boolValue == true) {
			otTilesSpriteSelectionStatus = OTTilesSpriteSelectionStatus.ALL;
		}
		else {
			otTilesSpriteSelectionStatus = OTTilesSpriteSelectionStatus.NONE;
		}
		
		// set allHaveSmoothMovesBoneAnimationParent
		if (numObjectsWithSmoothMovesBoneAnimationParent == targetObjects.Length) {
			allHaveSmoothMovesBoneAnimationParent = true;
		}
		else {
			allHaveSmoothMovesBoneAnimationParent = false;
		}
		
		PrepareColliderRegionsIfOldVersion(sortedTargets);
		
		if (!areUsedTexturesDifferent) {
			
			if (usedTexture == null) {
				EditorGUILayout.LabelField("No Texture Image", "Set Advanced/Custom Image");
			}
			else {
				Rect textureImageRect = GUILayoutUtility.GetRect(50, 50);
				if (usedTextureIsCustomTex) {
					EditorGUI.LabelField(textureImageRect, new GUIContent("Custom Image"), new GUIContent(usedTexture));
				}
				else {
					if (otTilesSpriteSelectionStatus == OTTilesSpriteSelectionStatus.ALL) {
						EditorGUI.LabelField(textureImageRect, new GUIContent("OTTilesSprite"), new GUIContent(usedTexture));
					}
					else if (isAtlas) {
						EditorGUI.LabelField(textureImageRect, new GUIContent("Atlas / SpriteSheet"), new GUIContent(usedTexture));
					}
					else {
						EditorGUI.LabelField(textureImageRect, new GUIContent("Texture Image"), new GUIContent(usedTexture));
					}
				}
				EditorGUILayout.LabelField("Texture Width x Height: ", usedTexture.width.ToString() + " x " + usedTexture.height.ToString());
			}
		}
		else {
			EditorGUILayout.LabelField("Texture Image", "<different textures selected>");
		}
		
		if (canRecalculateAnyCollider) {
			if (otTilesSpriteSelectionStatus == OTTilesSpriteSelectionStatus.NONE) {
				EditorGUILayout.PropertyField(targetLiveUpdate, new GUIContent("Editor Live Update"));
			}
			
			// float [0..1] Alpha Opaque Threshold
			EditorGUILayout.Slider(targetAlphaOpaqueThreshold, 0.0f, 1.0f, "Alpha Opaque Threshold");
			
			// int [3..100] max point count
			//EditorGUILayout.IntSlider(targetMaxPointCount, 3, mPointCountSliderMax, "Outline Vertex Count");
			
			if (!isFirstEnabledMaxPointCountDifferent) {
				bool noIslandOrHoleActive = (commonFirstEnabledMaxPointCount == 0);
				string outlineVertexCountLabelString = noIslandOrHoleActive ? "No Holes or Islands Active" : "Outline Vertex Count";

				int newFirstEnabledMaxPointCount = EditorGUILayout.IntSlider(outlineVertexCountLabelString, commonFirstEnabledMaxPointCount, 3, mPointCountSliderMax);
				hasFirstEnabledMaxPointCountChanged = newFirstEnabledMaxPointCount != commonFirstEnabledMaxPointCount;
				if (hasFirstEnabledMaxPointCountChanged) {
					for (int targetIndex = 0; targetIndex < targetObjects.Length; ++targetIndex) {
						targetObject = (AlphaMeshCollider) targetObjects[targetIndex];
						targetObject.MaxPointCountOfFirstEnabledRegion = newFirstEnabledMaxPointCount;
					}
				}
			}
			else {
				EditorGUILayout.LabelField("Outline Vertex Count", "<different values>");
			}
			
			// removed, since it was not too intuitive to use.
			// float [0..0.3] Accepted Distance
			//EditorGUILayout.Slider(targetVertexReductionDistanceTolerance, 0.0f, 0.3f, "Accepted Distance");
			
			// float thickness
			EditorGUILayout.PropertyField(targetThickness, new GUIContent("Z-Thickness"));
		}
		
		if (canReloadAnyCollider || canRecalculateAnyCollider) {
#if UNITY_4_3_AND_LATER
			// collider type enum
			EditorGUILayout.PropertyField(targetTargetColliderType, new GUIContent("Collider Type"));
#endif

			// copy OT sprite flipping
			bool showFlipProperties = true;
			
			if (!targetHasOTSpriteComponent.hasMultipleDifferentValues &&
				targetHasOTSpriteComponent.boolValue == true) {
				
				//targetObject.mCopyOTSpriteFlipping = EditorGUILayout.Toggle("Copy OTSprite Flipping", targetObject.mCopyOTSpriteFlipping);
				EditorGUILayout.PropertyField(targetCopyOTSpriteFlipping, new GUIContent("Copy OTSprite Flipping"));
				
				if (targetCopyOTSpriteFlipping.boolValue == true) {
					showFlipProperties = false;
				}
			}
			
			if (showFlipProperties) {
				bool areFlipHorizontalDifferent = false;
				bool areFlipVerticalDifferent = false;
				mFlipHorizontalChanged = false;
				mFlipVerticalChanged = false;
				
				targetObject = (AlphaMeshCollider) targetObjects[0];
				bool flipHorizontal = targetObject.FlipHorizontal;
				bool flipVertical = targetObject.FlipVertical;
				
				for (int targetIndex = 1; targetIndex < targetObjects.Length; ++targetIndex) {
					targetObject = (AlphaMeshCollider) targetObjects[targetIndex];
					bool currentFlipHorizontal = targetObject.FlipHorizontal;
					bool currentFlipVertical = targetObject.FlipVertical;
					if (currentFlipHorizontal != flipHorizontal)
						areFlipHorizontalDifferent = true;
					if (currentFlipVertical != flipVertical)
						areFlipVerticalDifferent = true;
				}
				
				// flip horizontal
				bool newFlipHorizontal = false;
				if (!areFlipHorizontalDifferent) {
					newFlipHorizontal = EditorGUILayout.Toggle("Flip Horizontal", flipHorizontal);
					
					if (newFlipHorizontal != flipHorizontal) {
						mFlipHorizontalChanged = true;
						
						for (int targetIndex = 0; targetIndex < targetObjects.Length; ++targetIndex) {
							targetObject = (AlphaMeshCollider) targetObjects[targetIndex];
							targetObject.FlipHorizontal = newFlipHorizontal;
						}
					}
				}
				else {
					EditorGUILayout.LabelField("Flip Horizontal", "<different values>");
				}
				
				// flip vertical
				bool newFlipVertical = false;
				if (!areFlipVerticalDifferent) {
					newFlipVertical = EditorGUILayout.Toggle("Flip Vertical", flipVertical);
					
					if (newFlipVertical != flipVertical) {
						mFlipVerticalChanged = true;
					
						for (int targetIndex = 0; targetIndex < targetObjects.Length; ++targetIndex) {
							targetObject = (AlphaMeshCollider) targetObjects[targetIndex];
							targetObject.FlipVertical = newFlipVertical;
						}
					}
				}
				else {
					EditorGUILayout.LabelField("Flip Vertical", "<different values>");
				}
			}
			
			EditorGUILayout.PropertyField(targetConvex, new GUIContent("Force Convex"));
			
			EditorGUILayout.PropertyField(targetFlipNormals, new GUIContent("Flip Normals"));
			
			if (allHaveSmoothMovesBoneAnimationParent) {
				
				if (!isApplySmoothMovesScaleAnimDifferent) {
					bool newApplySmoothMovesScaleAnim = EditorGUILayout.Toggle("SmoothMoves Scale Anim", commonApplySmoothMovesScaleAnim);
					hasCommonApplySmoothMovesScaleAnimChanged = newApplySmoothMovesScaleAnim != commonApplySmoothMovesScaleAnim;
					if (hasCommonApplySmoothMovesScaleAnimChanged) {
						for (int targetIndex = 0; targetIndex < targetObjects.Length; ++targetIndex) {
							targetObject = (AlphaMeshCollider) targetObjects[targetIndex];
							targetObject.ApplySmoothMovesScaleAnim  = newApplySmoothMovesScaleAnim;
						}
					}
				}
				else {
					EditorGUILayout.LabelField("SmoothMoves Scale Anim", "<different values>");
				}
			}
		}
		
		// output directory
		string newOutputDirectoryPath = null;
		if (!areOutputDirectoriesDifferent) {
			newOutputDirectoryPath = EditorGUILayout.TextField("Output Directory", commonOutputDirectoryPath);
		}
		else {
			newOutputDirectoryPath = EditorGUILayout.TextField("Output Directory", "<different values>");
		}
		if (!newOutputDirectoryPath.Equals(commonOutputDirectoryPath) && !newOutputDirectoryPath.Equals("<different values>")) {
			for (int targetIndex = 0; targetIndex < targetObjects.Length; ++targetIndex) {
				targetObject = (AlphaMeshCollider) targetObjects[targetIndex];
				targetObject.ColliderMeshDirectory = newOutputDirectoryPath;
			}
		}
		
		// group suffix
		string newGroupSuffix = null;
		if (!areGroupSuffixesDifferent) {
			newGroupSuffix = EditorGUILayout.TextField("Group Suffix", commonGroupSuffix);
		}
		else {
			newGroupSuffix = EditorGUILayout.TextField("Group Suffix", "<different values>");
		}
		if (!newGroupSuffix.Equals(commonGroupSuffix) && !newGroupSuffix.Equals("<different values>")) {
			for (int targetIndex = 0; targetIndex < targetObjects.Length; ++targetIndex) {
				targetObject = (AlphaMeshCollider) targetObjects[targetIndex];
				targetObject.GroupSuffix = newGroupSuffix;
			}
		}
		
		if (otTilesSpriteSelectionStatus == OTTilesSpriteSelectionStatus.NONE) {
			// output filename (read-only)
			if (!areOutputFilenamesDifferent) {
				EditorGUILayout.TextField("Output Filename", commonOutputFilename);
			}
			else {
				EditorGUILayout.TextField("Output Filename", "<different values>");
			}
		}
		
		// Advanced settings
		mShowAdvanced = EditorGUILayout.Foldout(mShowAdvanced, "Advanced Settings");
        if(mShowAdvanced) {
			EditorGUI.indentLevel++;
			
			Texture2D newCustomTexture = null;
			if (!areCustomTexturesDifferent) {
				newCustomTexture = (Texture2D) EditorGUILayout.ObjectField("Custom Image", commonCustomTexture, typeof(Texture2D), false);
				if (newCustomTexture != commonCustomTexture) {

					for (int targetIndex = 0; targetIndex < targetObjects.Length; ++targetIndex) {
						targetObject = (AlphaMeshCollider) targetObjects[targetIndex];
						targetObject.CustomTex = newCustomTexture;
					}
					
					sortedTargets = SortAlphaMeshColliders(targetObjects); // path hash values are outdated - recalculate them!
					areUsedTexturesDifferent = AreUsedTexturesDifferent(sortedTargets);
					
					ReloadOrRecalculateSelectedColliders(sortedTargets);
				}
			}
			else {
				EditorGUILayout.LabelField("Custom Image", "<different images selected>");
			}

			EditorGUILayout.Slider(targetCustomRotation, 0.0f, 360.0f, new GUIContent("Custom Rotation"));
			EditorGUI.indentLevel++; // provide some space for the vector foldout triangles
			EditorGUILayout.PropertyField(targetCustomScale, new GUIContent("Custom Scale"), true);
			EditorGUILayout.PropertyField(targetCustomOffset, new GUIContent("Custom Offset"), true);
			EditorGUI.indentLevel--;
			
			EditorGUI.indentLevel--;
		}
		
		// custom texture region part
		if (otTilesSpriteSelectionStatus == OTTilesSpriteSelectionStatus.NONE) {
			mShowTextureRegionSection = EditorGUILayout.Foldout(mShowTextureRegionSection, "Custom Texture Region");
			if(mShowTextureRegionSection) {
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(targetIsCustomAtlasRegionUsed, new GUIContent("Use Custom Region"));
				
				bool showCustomAtlasRegionParams = false;
				if (targetIsCustomAtlasRegionUsed.boolValue == true) {
					showCustomAtlasRegionParams = true;
				}
				if (showCustomAtlasRegionParams) {
					
					if (!isCommonCustomAtlasFramePositionInPixelsDifferent) {
						Vector2 newCommonCustomAtlasFramePositionInPixels = EditorGUILayout.Vector2Field("Position", commonCustomAtlasFramePositionInPixels);
						hasCommonCustomAtlasFramePositionInPixelsChanged = newCommonCustomAtlasFramePositionInPixels != commonCustomAtlasFramePositionInPixels;
						if (hasCommonCustomAtlasFramePositionInPixelsChanged) {
							for (int targetIndex = 0; targetIndex < targetObjects.Length; ++targetIndex) {
								targetObject = (AlphaMeshCollider) targetObjects[targetIndex];
								targetObject.CustomAtlasFramePositionInPixels  = newCommonCustomAtlasFramePositionInPixels;
							}
						}
					}
					else {
						EditorGUILayout.LabelField("Position", "<different values>");
					}
					
					if (!isCommonCustomAtlasFrameSizeInPixelsDifferent) {
						Vector2 newCommonCustomAtlasFrameSizeInPixels = EditorGUILayout.Vector2Field("Size", commonCustomAtlasFrameSizeInPixels);
						hasCommonCustomAtlasFrameSizeInPixelsChanged = newCommonCustomAtlasFrameSizeInPixels != commonCustomAtlasFrameSizeInPixels;
						if (hasCommonCustomAtlasFrameSizeInPixelsChanged) {
							for (int targetIndex = 0; targetIndex < targetObjects.Length; ++targetIndex) {
								targetObject = (AlphaMeshCollider) targetObjects[targetIndex];
								targetObject.CustomAtlasFrameSizeInPixels  = newCommonCustomAtlasFrameSizeInPixels;
							}
						}
					}
					else {
						EditorGUILayout.LabelField("Size", "<different values>");
					}
				}
				EditorGUI.indentLevel--;
			}
		}
		
		// holes and islands part
		if (!areUsedTexturesDifferent && otTilesSpriteSelectionStatus == OTTilesSpriteSelectionStatus.NONE) {
			
			OnInspectorGuiHolesAndIslandsSection(out haveColliderRegionEnabledChanged, out haveColliderRegionMaxPointCountChanged, out haveColliderRegionConvexChanged, targetObjects);
		}

		// check if we now have no islands/holes selected
		for (int targetIndex = 0; targetIndex != targetObjects.Length; ++targetIndex) {
			targetObject = (AlphaMeshCollider) targetObjects[targetIndex];
			if (targetObject.NumEnabledColliderRegions == 0) {
				anyColliderWithoutActiveIslandsOrHoles = true;
			}
		}
		
		// Apply changes to the serializedProperty.
        serializedObject.ApplyModifiedProperties();
		//mLiveUpdate = sortedTargets.Values[0].RegionIndependentParams.LiveUpdate;
		SortedDictionary<int, AlphaMeshCollider>.Enumerator firstTargetEnumerator = sortedTargets.GetEnumerator();
		firstTargetEnumerator.MoveNext();
		AlphaMeshCollider firstTarget = firstTargetEnumerator.Current.Value;
		mLiveUpdate = firstTarget.RegionIndependentParams.LiveUpdate;
		if (otTilesSpriteSelectionStatus != OTTilesSpriteSelectionStatus.NONE) {
			mLiveUpdate = false;
		}
		
		EditorGUILayout.BeginHorizontal();
		if (otTilesSpriteSelectionStatus == OTTilesSpriteSelectionStatus.ALL) {
			if (GUILayout.Button("Recalculate Tile Colliders")) {

				buttonRecalculateTileCollidersPressed = true;
			}
		}
		else {
			if (canReloadAnyCollider)
			{
				if (GUILayout.Button("Reload Collider")) {
		
					buttonReloadColliderPressed = true;
				}
			}
			if (canRecalculateAnyCollider) {
				if (GUILayout.Button("Recalculate Collider")) {

					buttonRecalculateColliderPressed = true;
				}
			}
		}
		EditorGUILayout.EndHorizontal();

		int desiredActiveColliderIndexPlus1 = 1;
		bool desiredActiveColliderIndexHasChanged = false;
		if (anyColliderHasMultipleFrames) {
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Reload All Frames")) {
				
				buttonReloadAllCollidersAllFramesPressed = true;
			}
			if (GUILayout.Button("Recalculate All Frames")) {
				
				buttonRecalculateAllCollidersAllFramesPressed = true;
			}
			EditorGUILayout.EndHorizontal();


			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("<", GUILayout.MaxWidth(25))) {
				buttonDecreaseActiveFrameIndexPressed = true;
			}
			if (!hasDifferentNumColliderFrames) {
				desiredActiveColliderIndexPlus1 = EditorGUILayout.IntField(commonActiveColliderFrame+1, GUILayout.MaxWidth(60));
				if (desiredActiveColliderIndexPlus1 > commonNumColliderFrames) {
					desiredActiveColliderIndexPlus1 = commonNumColliderFrames;
				}
				if (desiredActiveColliderIndexPlus1 < 1) {
					desiredActiveColliderIndexPlus1 = 1;
				}
				if (desiredActiveColliderIndexPlus1-1 != commonActiveColliderFrame) {
					desiredActiveColliderIndexHasChanged = true;
				}
			}
			//int newPointCount = EditorGUILayout.IntSlider(20, 0, 40);
			if (GUILayout.Button(">", GUILayout.MaxWidth(25))) {
				
				buttonIncreaseActiveFrameIndexPressed = true;
			}
			if (GUILayout.Button("Show All")) {
				
				buttonDisplayAllFramesPressed = true;
			}
			EditorGUILayout.EndHorizontal();
		}
		
		if (mLiveUpdate) {
			
			bool isCompleteRecalculationRequired = false;
			bool isRecalculationFromPreviousResultRequired = false;
			bool isSingleFrameRewriteRequired = false;
			bool isAllFrameRewriteRequired = false;
			
			//bool pointCountNeedsUpdate = ((mOldPointCount != targetMaxPointCount.intValue) && (targetMaxPointCount.intValue > 2)); // when typing 28, it would otherwise update at the first digit '2'.
			bool pointCountNeedsUpdate = hasFirstEnabledMaxPointCountChanged || haveColliderRegionMaxPointCountChanged;

			if (pointCountNeedsUpdate ||
				haveColliderRegionConvexChanged ||
				mOldConvex != targetConvex.boolValue ||
				mOldThickness != targetThickness.floatValue ||
				mOldDistanceTolerance != targetVertexReductionDistanceTolerance.floatValue) {
			
				isRecalculationFromPreviousResultRequired = true;
			}
			
			if (mOldConvex != targetConvex.boolValue) {
				isCompleteRecalculationRequired = true; // TODO: this part is not efficient, we should set the property instead of the value directly and then act based on it.
			}
			
			if (mOldAlphaThreshold != targetAlphaOpaqueThreshold.floatValue ||
				mOldFlipNormals != targetFlipNormals.boolValue ||
				hasCommonApplySmoothMovesScaleAnimChanged) {
			
				isCompleteRecalculationRequired = true;
			}
			if (mOldIsCustomAtlasRegionUsed != targetIsCustomAtlasRegionUsed.boolValue || 
				hasCommonCustomAtlasFramePositionInPixelsChanged ||
				hasCommonCustomAtlasFrameSizeInPixelsChanged ||
				mOldCustomAtlasFrameRotation != targetCustomAtlasFrameRotation.floatValue) {
			
				isCompleteRecalculationRequired = true;
			}

			if (haveColliderRegionEnabledChanged) {
				
				isCompleteRecalculationRequired = true;
			}
			
			if (mOldCustomRotation != targetCustomRotation.floatValue ||
				mOldCustomScale != targetCustomScale.vector2Value ||
				mOldCustomOffset != targetCustomOffset.vector3Value) {

				isSingleFrameRewriteRequired = true;
			}

#if UNITY_4_3_AND_LATER
			if (mOldTargetColliderTypeEnumIndex != targetTargetColliderType.enumValueIndex) {

				isAllFrameRewriteRequired = true;
			}
#endif

			if (!anyColliderWithoutActiveIslandsOrHoles) {
				if (isCompleteRecalculationRequired) {
					RecalculateSelectedColliders(sortedTargets);
				}
				else if (isRecalculationFromPreviousResultRequired) {
					RecalculateSelectedCollidersFromPreviousResult(sortedTargets);
				}
				else if (isSingleFrameRewriteRequired) {
					RewriteSingleFrameSelectedColliders(sortedTargets);
				}
			}
			if (isAllFrameRewriteRequired) {
				RewriteAllFramesSelectedColliders(sortedTargets);
			}
		}

		if (buttonRecalculateTileCollidersPressed) {
			RecalculateOTTilesSpriteColliders(sortedTargets);
		}
		if (buttonReloadColliderPressed) {
			ReloadSelectedColliders(sortedTargets);
		}
		if (buttonRecalculateColliderPressed) {
			RecalculateSelectedColliders(sortedTargets);
		}
		if (buttonReloadAllCollidersAllFramesPressed) {
			ReloadAllFramesAtSelectedColliders(sortedTargets);
		}
		if (buttonRecalculateAllCollidersAllFramesPressed) {
			RecalculateAllFramesAtSelectedColliders(sortedTargets);
		}

		if (buttonDecreaseActiveFrameIndexPressed) {
			DecreaseActiveColliderFrameIndex(sortedTargets);
		}
		if (buttonIncreaseActiveFrameIndexPressed) {
			IncreaseActiveColliderFrameIndex(sortedTargets);
		}
		if (desiredActiveColliderIndexHasChanged) {
			SetActiveColliderFrameIndex(sortedTargets, desiredActiveColliderIndexPlus1-1);
		}
		if (buttonDisplayAllFramesPressed) {
			DisplayAllColliderFrames(sortedTargets);
		}
		
		if (GUI.changed) {
			foreach (Object target in targetObjects) {
            	EditorUtility.SetDirty(target);
			}
		}
		
		//EditorGUIUtility.LookLikeControls();
	}
	
	//-------------------------------------------------------------------------
	void OnInspectorGuiHolesAndIslandsSection(out bool haveColliderRegionEnabledChanged,
											  out bool haveColliderRegionMaxPointCountChanged,
											  out bool haveColliderRegionConvexChanged,
											  Object[] targetObjects) {
		
		haveColliderRegionEnabledChanged = false;
		haveColliderRegionMaxPointCountChanged = false;
		haveColliderRegionConvexChanged = false;
		
		AlphaMeshCollider firstObject = (AlphaMeshCollider) targetObjects[0];
		
		int numColliderRegions = 0;
		if (firstObject.ColliderRegions != null) {
			numColliderRegions = firstObject.ColliderRegions.Length;
		}
		
		bool[] newIsRegionEnabled = new bool [numColliderRegions];
		int[] newRegionPointCount = new int [numColliderRegions];
		bool[] newForceRegionConvex = new bool [numColliderRegions];
		
		string foldoutString = "Holes and Islands [" + firstObject.NumEnabledColliderRegions + "][" + firstObject.ActualPointCountOfAllRegions + " vertices]";
		mShowHolesAndIslandsSection = EditorGUILayout.Foldout(mShowHolesAndIslandsSection, foldoutString);

		if(mShowHolesAndIslandsSection) {
			EditorGUI.indentLevel++;

			int islandIndex = 0;
			int seaRegionIndex = 0;

			for (int regionIndex = 0; regionIndex < numColliderRegions; ++regionIndex) {
				
				ColliderRegionData colliderRegion = firstObject.ColliderRegions[regionIndex];
				bool isIslandRegion = colliderRegion.mRegionIsIsland;

				AlphaMeshCollider.ColliderRegionParameters parameters = isIslandRegion ? firstObject.IslandRegionParams[islandIndex] : firstObject.SeaRegionParams[seaRegionIndex];
				bool isEnabled = parameters.EnableRegion;
				int maxPointCount = parameters.MaxPointCount;
				bool convex = parameters.Convex;
				
				if (regionIndex != 0) {
					EditorGUILayout.Space();
				}
				string regionOrIslandString = isIslandRegion ? "Island " + islandIndex : "Hole " + seaRegionIndex;
				regionOrIslandString += " [" + colliderRegion.mDetectedRegion.mPointCount + " px]";
				//bool newIsEnabled = EditorGUILayout.BeginToggleGroup(regionOrIslandString, isEnabled);
				bool newIsEnabled = EditorGUILayout.Toggle(regionOrIslandString, isEnabled);
				EditorGUI.indentLevel++;
				
				// int [3..100] max point count
				int newPointCount = EditorGUILayout.IntSlider("Outline Vertex Count", maxPointCount, 3, mPointCountSliderMax);
				bool newConvex = EditorGUILayout.Toggle("Force Convex", convex);
				
				EditorGUI.indentLevel--;
				//EditorGUILayout.EndToggleGroup();
				
				bool hasEnabledChanged = newIsEnabled != isEnabled;
				bool hasPountCountChanged = newPointCount != maxPointCount;
				bool hasConvexChanged = newConvex != convex;
				if (hasEnabledChanged) {
					haveColliderRegionEnabledChanged = true;
				}
				if (hasPountCountChanged) {
					haveColliderRegionMaxPointCountChanged = true;
				}
				if (hasConvexChanged) {
					haveColliderRegionConvexChanged = true;
				}
				
				newIsRegionEnabled[regionIndex] = newIsEnabled;
				newRegionPointCount[regionIndex] = newPointCount;
				newForceRegionConvex[regionIndex] = newConvex;

				if (isIslandRegion) {
					++islandIndex;
				}
				else {
					++seaRegionIndex;
				}
			}


			for (int targetIndex = 0; targetIndex != targetObjects.Length; ++targetIndex) {

				AlphaMeshCollider targetObject = (AlphaMeshCollider) targetObjects[targetIndex];
				islandIndex = 0;
				seaRegionIndex = 0;

				for (int regionIndex = 0; regionIndex < numColliderRegions; ++regionIndex) {

					ColliderRegionData colliderRegion = firstObject.ColliderRegions[regionIndex];
					bool isIslandRegion = colliderRegion.mRegionIsIsland;

					AlphaMeshCollider.ColliderRegionParameters colliderRegionParams = isIslandRegion ? targetObject.IslandRegionParams[islandIndex] : targetObject.SeaRegionParams[seaRegionIndex];
					colliderRegionParams.EnableRegion = newIsRegionEnabled[regionIndex];
					colliderRegionParams.MaxPointCount = newRegionPointCount[regionIndex];
					colliderRegionParams.Convex = newForceRegionConvex[regionIndex];

					if (isIslandRegion) {
						++islandIndex;
					}
					else {
						++seaRegionIndex;
					}
				}
			}
			
			EditorGUI.indentLevel--;
		}
	}
	
	//-------------------------------------------------------------------------
	static void SelectChildAlphaMeshColliders(GameObject[] gameObjects) {
		
		List<GameObject> newSelectionList = new List<GameObject>();
		
		foreach (GameObject gameObj in gameObjects) {
			
			AddAlphaMeshCollidersOfTreeToList(gameObj.transform, ref newSelectionList);
		}
		
		GameObject[] newSelection = newSelectionList.ToArray();
		Selection.objects = newSelection;
	}
	
	//-------------------------------------------------------------------------
	static void RemoveColliderAndGenerator(GameObject[] gameObjects) {
		foreach (GameObject gameObj in gameObjects) {
			
			// AlphaMeshCollider component
			AlphaMeshCollider[] alphaMeshColliderComponents = gameObj.GetComponents<AlphaMeshCollider>();
			List<Transform> colliderNodes = new List<Transform>();
			if (alphaMeshColliderComponents != null) {
				foreach (AlphaMeshCollider component in alphaMeshColliderComponents) {
					colliderNodes.Add(component.TargetNodeToAttachMeshCollider);
					DestroyImmediate(component);
				}
			}
			// secondary components
			AlphaMeshColliderSmoothMovesRestore[] restoreComponents = gameObj.GetComponents<AlphaMeshColliderSmoothMovesRestore>();
			if (restoreComponents != null) {
				foreach (AlphaMeshColliderSmoothMovesRestore restoreComponent in restoreComponents) {
					DestroyImmediate(restoreComponent);
				}
			}
			AlphaMeshColliderUpdateOTTilesSpriteColliders[] updateComponents = gameObj.GetComponents<AlphaMeshColliderUpdateOTTilesSpriteColliders>();
			if (updateComponents != null) {
				foreach (AlphaMeshColliderUpdateOTTilesSpriteColliders updateComponent in updateComponents) {
					DestroyImmediate(updateComponent);
				}
			}
			AlphaMeshColliderCopyColliderEnabled[] copyComponents = gameObj.GetComponents<AlphaMeshColliderCopyColliderEnabled>();
			if (copyComponents != null) {
				foreach (AlphaMeshColliderCopyColliderEnabled copyComponent in copyComponents) {
					DestroyImmediate(copyComponent);
				}
			}
			// MeshCollider components
			MeshCollider[] meshColliderComponents = gameObj.GetComponents<MeshCollider>();
			if (meshColliderComponents != null) {
				foreach (MeshCollider meshColliderComponent in meshColliderComponents) {
					DestroyImmediate(meshColliderComponent);
				}
			}
			// Potentially a MeshCollider at a child node (used for SmoothMoves scale animation)
			foreach (Transform colliderNode in colliderNodes) {
				meshColliderComponents = colliderNode.GetComponents<MeshCollider>();
				foreach (MeshCollider meshColliderComponent in meshColliderComponents) {
					DestroyImmediate(meshColliderComponent);
				}
			}
#if UNITY_4_3_AND_LATER
			// PolygonCollider2D component
			PolygonCollider2D[] polygonColliderComponents = gameObj.GetComponents<PolygonCollider2D>();
			foreach (PolygonCollider2D polygonColliderComponent in polygonColliderComponents) {
				DestroyImmediate(polygonColliderComponent);
			}
			// Potentially a PolygonCollider2D at a child node (used for SmoothMoves scale animation)
			foreach (Transform colliderNode in colliderNodes) {
				polygonColliderComponents = colliderNode.GetComponents<PolygonCollider2D>();
				foreach (PolygonCollider2D polygonColliderComponent in polygonColliderComponents) {
					DestroyImmediate(polygonColliderComponent);
				}
			}

			// RuntimeAnimatedColliderSwitch component
			RuntimeAnimatedColliderSwitch[] runtimeAnimatedColliderSwitchComponents = gameObj.GetComponents<RuntimeAnimatedColliderSwitch>();
			foreach (RuntimeAnimatedColliderSwitch switchComponent in runtimeAnimatedColliderSwitchComponents) {
				DestroyImmediate(switchComponent);
			}
			foreach (Transform colliderNode in colliderNodes) {
				runtimeAnimatedColliderSwitchComponents = colliderNode.GetComponents<RuntimeAnimatedColliderSwitch>();
				foreach (RuntimeAnimatedColliderSwitch switchComponent in runtimeAnimatedColliderSwitchComponents) {
					DestroyImmediate(switchComponent);
				}
			}
#endif
			
			// AlphaMeshColliders child gameobject node
			foreach (Transform child in gameObj.transform) {
				if (child.name.Equals("AlphaMeshColliders")) {
					GameObject.DestroyImmediate(child.gameObject);
				}
			}
		}
	}
	
	//-------------------------------------------------------------------------
	static void AddAlphaMeshCollidersOfTreeToList(Transform node, ref List<GameObject> resultList) {
		
		AlphaMeshCollider alphaCollider = node.GetComponent<AlphaMeshCollider>();
		if (alphaCollider != null) {
			resultList.Add(node.gameObject);
		}
		
		foreach (Transform child in node) {
			AddAlphaMeshCollidersOfTreeToList(child, ref resultList);
		}
	}
	
	//-------------------------------------------------------------------------
	static void AddCollidersToBoneAnimationTree(Transform node) {
		foreach (Transform child in node) {
			
			if (!child.name.EndsWith("_Sprite")) {
				AlphaMeshCollider collider = child.GetComponent<AlphaMeshCollider>();
				if (collider == null) {
					collider = child.gameObject.AddComponent<AlphaMeshCollider>();
				}
			}
			
			AddCollidersToBoneAnimationTree(child);
		}
	}
	
	//-------------------------------------------------------------------------
	static void AddCollidersToOTTileMap(Transform tileMapNode, Component otTileMap) {
		
		// OTTileMapLayer[]  otTileMap.layers
		System.Type otTileMapType = otTileMap.GetType();
		FieldInfo fieldLayers = otTileMapType.GetField("layers");
		if (fieldLayers == null) {
			Debug.LogError("Detected a missing 'layers' member variable at OTTileMap component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return;
		}

		// add a GameObject node named "AlphaMeshColliders"
		GameObject collidersNode = new GameObject("AlphaMeshColliders");
		collidersNode.transform.parent = tileMapNode;
		collidersNode.transform.localPosition = Vector3.zero;
		collidersNode.transform.localScale = Vector3.one;
		
		IEnumerable layersArray = (IEnumerable) fieldLayers.GetValue(otTileMap);
		int layerIndex = 0;
		foreach (object otTileMapLayer in layersArray) {
		
			System.Type otTileMapLayerType = otTileMapLayer.GetType();
			FieldInfo fieldName = otTileMapLayerType.GetField("name");
			if (fieldName == null) {
				Debug.LogError("Detected a missing 'name' member variable at OTTileMapLayer component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
				return;
			}
			string layerName = (string) fieldName.GetValue(otTileMapLayer);
			// add a GameObject node for each tilemap layer.
			GameObject layerNode = new GameObject(layerName);
			layerNode.transform.parent = collidersNode.transform;
			layerNode.transform.localPosition = Vector3.zero;
			layerNode.transform.localScale = Vector3.one;
			
			addColliderGameObjectsForOTTileMapLayer(layerNode.transform, otTileMap, otTileMapLayer, layerIndex);
			++layerIndex;
		}
	}
	
	//-------------------------------------------------------------------------
	static void addColliderGameObjectsForOTTileMapLayer(Transform layerNode, Component otTileMap, object otTileMapLayer, int layerIndex) {
	
		// read tileMapSize = OTTileMap.mapSize (UnityEngine.Vector2)
		System.Type otTileMapType = otTileMap.GetType();
		FieldInfo fieldMapSize = otTileMapType.GetField("mapSize");
		if (fieldMapSize == null) {
			Debug.LogError("Detected a missing 'mapSize' member variable at OTTileMap component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return;
		}
		Vector2 tileMapSize = (UnityEngine.Vector2) fieldMapSize.GetValue(otTileMap);
		int tileMapWidth = (int) tileMapSize.x;
		int tileMapHeight = (int) tileMapSize.y;
		// read mapTileSize = OTTileMap.mapTileSize (UnityEngine.Vector2)
		FieldInfo fieldMapTileSize = otTileMapType.GetField("mapTileSize");
		if (fieldMapTileSize == null) {
			Debug.LogError("Detected a missing 'mapTileSize' member variable at OTTileMap component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return;
		}
		Vector2 mapTileSize = (UnityEngine.Vector2) fieldMapTileSize.GetValue(otTileMap);
		Vector3 mapTileScale = new Vector3(1.0f / tileMapSize.x, 1.0f / tileMapSize.y, 1.0f / tileMapSize.x);
		
		System.Collections.Generic.Dictionary<int, object> tileSetAtTileIndex = new System.Collections.Generic.Dictionary<int, object>();
		
	
		Vector2 bottomLeftTileOffset = new Vector2(-0.5f, -0.5f);
		
		// read tileIndices = otTileMapLayer.tiles (int[])
		System.Type otTileMapLayerType = otTileMapLayer.GetType();
		FieldInfo fieldTiles = otTileMapLayerType.GetField("tiles");
		if (fieldTiles == null) {
			Debug.LogError("Detected a missing 'tiles' member variable at OTTileMapLayer component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return;
		}
		int[] tileIndices = (int[]) fieldTiles.GetValue(otTileMapLayer);
		System.Collections.Generic.Dictionary<int, Transform> groupNodeForTileIndex = new System.Collections.Generic.Dictionary<int, Transform>();
		Transform tileGroupNode = null;
		
		object tileSet = null;
		
		for (int y = 0; y < tileMapHeight; ++y) {
			for (int x = 0; x < tileMapWidth; ++x) {
				int tileIndex = tileIndices[y * tileMapWidth + x];
				if (tileIndex != 0) {
				
					if (groupNodeForTileIndex.ContainsKey(tileIndex)) {
						tileGroupNode = groupNodeForTileIndex[tileIndex];
						tileSet = tileSetAtTileIndex[tileIndex];
					}
					else {
						// create a group node
						GameObject newTileGroup = new GameObject("Tile Type " + tileIndex);
						newTileGroup.transform.parent = layerNode;
						newTileGroup.transform.localPosition = Vector3.zero;
						newTileGroup.transform.localScale = Vector3.one;
						tileGroupNode = newTileGroup.transform;
						groupNodeForTileIndex[tileIndex] = tileGroupNode;
						// get tileset for tile index
						tileSet = AlphaMeshCollider.GetOTTileSetForTileIndex(otTileMap, tileIndex);
						tileSetAtTileIndex[tileIndex] = tileSet;
					}
					// read tileSet.tileSize (Vector2)
					System.Type otTileSetType = tileSet.GetType();
					FieldInfo fieldTileSize = otTileSetType.GetField("tileSize");
					if (fieldTileSize == null) {
						Debug.LogError("Detected a missing 'tileSize' member variable at OTTileSet class - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
						return;
					}
					Vector2 tileSize = (UnityEngine.Vector2) fieldTileSize.GetValue(tileSet);
					Vector3 tileScale = new Vector3(mapTileScale.x / mapTileSize.x * tileSize.x, mapTileScale.y / mapTileSize.y * tileSize.y, mapTileScale.z);
					Vector2 tileCenterOffset = new Vector3(tileScale.x * 0.5f, tileScale.x * 0.5f);
				
					// add a GameObject for each enabled tile with name "tile y x"
					GameObject alphaMeshColliderNode = new GameObject("tile " + y + " " + x);
					alphaMeshColliderNode.transform.parent = tileGroupNode;
					AlphaMeshCollider alphaMeshColliderComponent = alphaMeshColliderNode.AddComponent<AlphaMeshCollider>();
					alphaMeshColliderComponent.SetOTTileMap(otTileMap, layerIndex, x, y, tileMapWidth);
					
					// set the position of the tile collider according to its (x,y) pos in the map.
					alphaMeshColliderNode.transform.localPosition = new Vector3(x * mapTileScale.x + bottomLeftTileOffset.x + tileCenterOffset.x, (tileMapSize.y - 1 - y) * mapTileScale.y + bottomLeftTileOffset.y + tileCenterOffset.y, 0.0f);
					alphaMeshColliderNode.transform.localScale = tileScale;
				}
			}
		}
	}
	
	//-------------------------------------------------------------------------
	static void AddAlphaMeshColliderToOTTilesSprite(Transform tilesSpriteNode, Component otTilesSprite) {
		
		AlphaMeshCollider alphaMeshColliderComponent = tilesSpriteNode.GetComponent<AlphaMeshCollider>();
		if (alphaMeshColliderComponent == null) {
			alphaMeshColliderComponent = tilesSpriteNode.gameObject.AddComponent<AlphaMeshCollider>();
			alphaMeshColliderComponent.SetOTTilesSprite(otTilesSprite);
		}
	}
	
	//-------------------------------------------------------------------------
	void ReloadSelectedColliders(SortedDictionary<int, AlphaMeshCollider> sortedTargets) {
		foreach (KeyValuePair<int, AlphaMeshCollider> pair in sortedTargets) {
			AlphaMeshCollider target = pair.Value;
			if (target.CanReloadCollider) {
				target.ReloadCollider();
			}
		}
	}
	
	//-------------------------------------------------------------------------
	void ReloadOrRecalculateSelectedColliders(SortedDictionary<int, AlphaMeshCollider> sortedTargets) {
		int lastHash = int.MinValue;
		foreach (KeyValuePair<int, AlphaMeshCollider> pair in sortedTargets) {
			int hash = pair.Key;
			AlphaMeshCollider target = pair.Value;
			if (hash != lastHash) {
				AlphaMeshColliderRegistry.Instance.ReloadOrRecalculateColliderAndUpdateSimilar(target); // if found, just load it, if not, recalculate and update all others.
			}
			lastHash = hash;
		}
	}
	
	//-------------------------------------------------------------------------
	void PrepareColliderIslandsAtSelectedColliders(SortedDictionary<int, AlphaMeshCollider> sortedTargets) {
		int lastHash = int.MinValue;
		foreach (KeyValuePair<int, AlphaMeshCollider> pair in sortedTargets) {
			int hash = pair.Key;
			AlphaMeshCollider target = pair.Value;
			if (hash != lastHash) {
				target.PrepareColliderIslandsForGui();
			}
			lastHash = hash;
		}
	}
	
	//-------------------------------------------------------------------------
	void PrepareColliderRegionsIfOldVersion(SortedDictionary<int, AlphaMeshCollider> sortedTargets) {
		
		int lastHash = int.MinValue;
		foreach (KeyValuePair<int, AlphaMeshCollider> pair in sortedTargets) {
			int hash = pair.Key;
			AlphaMeshCollider target = pair.Value;
			if (hash != lastHash) {
				if (target.IslandRegionParams == null || target.IslandRegionParams.Length == 0) {
					target.PrepareColliderIslandsForGui();
				}
			}
			lastHash = hash;
		}
	}

	//-------------------------------------------------------------------------
	void ReloadAllFramesAtSelectedColliders(SortedDictionary<int, AlphaMeshCollider> sortedTargets) {
		int lastHash = int.MinValue;
		foreach (KeyValuePair<int, AlphaMeshCollider> pair in sortedTargets) {
			int hash = pair.Key;
			AlphaMeshCollider target = pair.Value;
			if (hash != lastHash) {
				AlphaMeshColliderRegistry.Instance.ReloadAllFramesAtColliderAndUpdateSimilar(target);
			}
			lastHash = hash;
		}
	}

	//-------------------------------------------------------------------------
	void RecalculateAllFramesAtSelectedColliders(SortedDictionary<int, AlphaMeshCollider> sortedTargets) {
		int lastHash = int.MinValue;
		foreach (KeyValuePair<int, AlphaMeshCollider> pair in sortedTargets) {
			int hash = pair.Key;
			AlphaMeshCollider target = pair.Value;
			if (hash != lastHash) {
				AlphaMeshColliderRegistry.Instance.RecalculateAllFramesAtColliderAndUpdateSimilar(target);
			}
			lastHash = hash;
		}
	}
	
	//-------------------------------------------------------------------------
	void RecalculateSelectedColliders(SortedDictionary<int, AlphaMeshCollider> sortedTargets) {
		int lastHash = int.MinValue;
		foreach (KeyValuePair<int, AlphaMeshCollider> pair in sortedTargets) {
			int hash = pair.Key;
			AlphaMeshCollider target = pair.Value;
			if (hash != lastHash) {
				AlphaMeshColliderRegistry.Instance.RecalculateColliderAndUpdateSimilar(target);
			}
			lastHash = hash;
		}
	}
	
	//-------------------------------------------------------------------------
	void RecalculateSelectedCollidersFromPreviousResult(SortedDictionary<int, AlphaMeshCollider> sortedTargets) {
		int lastHash = int.MinValue;
		foreach (KeyValuePair<int, AlphaMeshCollider> pair in sortedTargets) {
			int hash = pair.Key;
			AlphaMeshCollider target = pair.Value;
			if (hash != lastHash) {
				AlphaMeshColliderRegistry.Instance.RecalculateColliderFromPreviousResultAndUpdateSimilar(target);
			}
			lastHash = hash;
		}
	}
	
	//-------------------------------------------------------------------------
	void RewriteSingleFrameSelectedColliders(SortedDictionary<int, AlphaMeshCollider> sortedTargets) {
		int lastHash = int.MinValue;
		foreach (KeyValuePair<int, AlphaMeshCollider> pair in sortedTargets) {
			int hash = pair.Key;
			AlphaMeshCollider target = pair.Value;
			if (hash != lastHash) {
				AlphaMeshColliderRegistry.Instance.RewriteAndReloadColliderAndUpdateSimilar(target);
			}
			lastHash = hash;
		}
	}

	//-------------------------------------------------------------------------
	void RewriteAllFramesSelectedColliders(SortedDictionary<int, AlphaMeshCollider> sortedTargets) {
		int lastHash = int.MinValue;
		foreach (KeyValuePair<int, AlphaMeshCollider> pair in sortedTargets) {
			int hash = pair.Key;
			AlphaMeshCollider target = pair.Value;
			if (hash != lastHash) {
				AlphaMeshColliderRegistry.Instance.RewriteAndReloadAllFramesAndUpdateSimilar(target);
			}
			lastHash = hash;
		}
	}


	//-------------------------------------------------------------------------
	void DecreaseActiveColliderFrameIndex(SortedDictionary<int, AlphaMeshCollider> sortedTargets) {
		foreach (KeyValuePair<int, AlphaMeshCollider> pair in sortedTargets) {
			AlphaMeshCollider target = pair.Value;
			target.DecreaseActiveColliderFrameIndex();
		}
	}
	
	//-------------------------------------------------------------------------
	void IncreaseActiveColliderFrameIndex(SortedDictionary<int, AlphaMeshCollider> sortedTargets) {
		foreach (KeyValuePair<int, AlphaMeshCollider> pair in sortedTargets) {
			AlphaMeshCollider target = pair.Value;
			target.IncreaseActiveColliderFrameIndex();
		}
	}
	//-------------------------------------------------------------------------
	void SetActiveColliderFrameIndex(SortedDictionary<int, AlphaMeshCollider> sortedTargets, int index) {
		foreach (KeyValuePair<int, AlphaMeshCollider> pair in sortedTargets) {
			AlphaMeshCollider target = pair.Value;
			target.SetActiveColliderFrameIndex(index);
		}
	}
	//-------------------------------------------------------------------------
	void DisplayAllColliderFrames(SortedDictionary<int, AlphaMeshCollider> sortedTargets) {
		foreach (KeyValuePair<int, AlphaMeshCollider> pair in sortedTargets) {
			AlphaMeshCollider target = pair.Value;
			target.EnableAllColliderFrames();
		}
	}
	
	//-------------------------------------------------------------------------
	void RecalculateOTTilesSpriteColliders(SortedDictionary<int, AlphaMeshCollider> sortedTargets) {
		foreach (KeyValuePair<int, AlphaMeshCollider> pair in sortedTargets) {
			AlphaMeshCollider target = pair.Value;
			target.RecalculateCollidersForOTTilesSprite();
		}
	}
	
	//-------------------------------------------------------------------------
	SortedDictionary<int, AlphaMeshCollider> SortAlphaMeshColliders(Object[] unsortedAlphaMeshColliders) {
		SortedDictionary<int, AlphaMeshCollider> resultList = new SortedDictionary<int, AlphaMeshCollider>(new DuplicatePermittingIntComparer());
		
		foreach (AlphaMeshCollider alphaMeshCollider in unsortedAlphaMeshColliders) {
			int textureHash = alphaMeshCollider.FirstFrameFullColliderMeshPath().GetHashCode();
			resultList.Add(textureHash, alphaMeshCollider);
		}
		
		return resultList;
	}
	
	//-------------------------------------------------------------------------
	bool AreUsedTexturesDifferent(SortedDictionary<int, AlphaMeshCollider> sortedTargets) {
		
		Texture firstTexture = null;
		foreach (KeyValuePair<int, AlphaMeshCollider> pair in sortedTargets) {
			AlphaMeshCollider target = pair.Value;
			Texture texture = target.UsedTexture;
			if (firstTexture == null) {
				firstTexture = texture;
			}
			if (texture != firstTexture) {
				return true;
			}
		}
		return false;
	}
}
