using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;

//-------------------------------------------------------------------------
/// <summary>
/// Editor window for the 2D Toolkit (TK2D) specific version of the
/// AlphaMeshCollider.
/// 
/// A ColliderGenTK2DParameterStore object is used to store parameter values
/// across selection changes and closing and reopening the SpriteCollection
/// editor window.
/// The store is used as follows:
/// - after RecalculateSelectedColliders() was called -> save values to store (not in prefab yet).
/// - if collection == oldCollection && spriteIDs != oldSpriteIDs -> load from store (not from prefab)
/// - if collection != oldCollection -> load store from prefab
/// - if "Commit" was hit -> persist store to prefab
/// </summary>
public class EditorScriptAlphaMeshColliderTK2DWindow : EditorWindow {
	
	const int COLLIDER_EDIT_MODE_INT_VALUE = 3; // Note: keep up to date with tk2dEditor.SpriteCollectionEditor.TextureEditor.Mode enum.	
	
	protected bool mWasInitialized = false;
	
	GUIContent mColliderPointCountLabel = new GUIContent("Outline Vertex Count");
	GUIContent mEditorLiveUpdateLabel = new GUIContent("Editor Live Update");
	GUIContent mAlphaOpaqueThresholdLabel = new GUIContent("Alpha Opaque Threshold");
	GUIContent mConvexLabel = new GUIContent("Force Convex");
	GUIContent mFlipInsideOutsideLabel = new GUIContent("Flip Normals");
	GUIContent mAdvancedSettingsLabel =  new GUIContent("Advanced Settings");
	GUIContent mCustomImageLabel = new GUIContent("Custom Image");
	const string mCustomScaleLabel = "Custom Scale";
	const string mCustomOffsetLabel = "Custom Offset";
	GUIContent mCalculateOutlineVerticesLabel = new GUIContent("Update Collider");
	
	protected Vector2 mScrollViewVector = Vector2.zero;
	protected bool mLiveUpdate = true;
	protected bool mShowAdvanced = false;
	protected bool mShowHolesAndIslandsSection = false;
	
	protected int mPointCountSliderMax = 100;
	
	protected bool mColliderPointCountChanged = false;
	protected bool mFlipInsideOutsideChanged = false;
	protected bool mConvexChanged = false;
	protected bool mNormalizedAlphaOpaqueThresholdChanged = false;
	protected bool mCustomScaleChanged = false;
	protected bool mCustomOffsetChanged = false;
	protected bool mCustomTexChanged = false;
	
	protected bool mAllSelectedSpritesHaveSameNumColliderRegions = false;
	
	GenerateColliderTK2DHelper mAlgorithmHelper = new GenerateColliderTK2DHelper();
	ColliderGenTK2DParameterStore mParameterStore = null;
	string mParameterStoreSaveFilePath;
	SortedList<int, Component> mDifferentSprites = null;
	
	object mSpriteCollectionProxyToEdit = null;
	object mOldSpriteCollectionProxyToEdit = null;
	int[] mSpriteIDsToEdit = null;
	int[] mOldSpriteIDsToEdit = null;
	uint mRepaintCounter = 0;
	
	static string mAtlasPathToMonitorForCommit = "";
	//-------------------------------------------------------------------------
	public static string AtlasPathToMonitorForCommit {
		get {
			return mAtlasPathToMonitorForCommit;
		}
	}
	
	//-------------------------------------------------------------------------
	[MenuItem ("2D ColliderGen/2D Toolkit Specific/Show ColliderGen TK2D Window", true)]
	static bool ValidateGenerateColliderVerticesMenuEntry() {
		// no special selection criteria needed.
		return true;
    }
	//-------------------------------------------------------------------------
	[MenuItem ("2D ColliderGen/2D Toolkit Specific/Show ColliderGen TK2D Window", false, 101)]
	static void GenerateColliderVerticesMenuEntry() {
		
		// Get existing open window or if none, make a new one:
		EditorScriptAlphaMeshColliderTK2DWindow window = EditorWindow.GetWindow<EditorScriptAlphaMeshColliderTK2DWindow>();
		window.title = "ColliderGen TK2D";
    }
	
	//-------------------------------------------------------------------------
	void OnGUI() {

		AssetPostprocessorDetectTK2DCommit.TargetColliderGenTK2DWindow = this;
		
		bool isFirstOnGuiCallOfLayoutRenderPair = Event.current.type == EventType.Layout;
		
		if (isFirstOnGuiCallOfLayoutRenderPair) {
			CheckForSelectedSpriteCollectionAndSprites();
			CheckForValuesToUpdate();
		}
		
		//EditorGUIUtility.LookLikeControls(150.0f);
		
		mScrollViewVector = GUILayout.BeginScrollView(mScrollViewVector);
		
		mLiveUpdate = EditorGUILayout.Toggle(mEditorLiveUpdateLabel, mLiveUpdate);
		
		// int [3..100] max point count
		mAlgorithmHelper.MaxPointCountOfFirstEnabledRegion = GuiHelper.IntSliderGuiElement(mColliderPointCountLabel, mAlgorithmHelper.MaxPointCountOfFirstEnabledRegion, 3, mPointCountSliderMax, ref mColliderPointCountChanged);
		
		// Note: Removed since it was not intuitive enough to use.
		// float [0..max(width, height)] Accepted Distance
		//float imageMinExtent = Mathf.Min(targetObject.mTextureWidth, targetObject.mTextureHeight);
		//targetObject.mVertexReductionDistanceTolerance = EditorGUILayout.Slider("Accepted Distance", targetObject.mVertexReductionDistanceTolerance, 0.0f, imageMinExtent/2);
		
		// float [0..1] Alpha Opaque Threshold
		mAlgorithmHelper.RegionIndependentParams.AlphaOpaqueThreshold = GuiHelper.FloatSliderGuiElement(mAlphaOpaqueThresholdLabel, mAlgorithmHelper.RegionIndependentParams.AlphaOpaqueThreshold, 0.0f, 1.0f, ref mNormalizedAlphaOpaqueThresholdChanged);
		
		mAlgorithmHelper.RegionIndependentParams.Convex = GuiHelper.ToggleGuiElement(mConvexLabel, mAlgorithmHelper.RegionIndependentParams.Convex, ref mConvexChanged);
		
		mAlgorithmHelper.RegionIndependentParams.FlipInsideOutside = GuiHelper.ToggleGuiElement(mFlipInsideOutsideLabel, mAlgorithmHelper.RegionIndependentParams.FlipInsideOutside, ref mFlipInsideOutsideChanged);
		
		// Advanced settings
		mShowAdvanced = EditorGUILayout.Foldout(mShowAdvanced, mAdvancedSettingsLabel);
        if(mShowAdvanced) {
			EditorGUI.indentLevel++;
			
			mAlgorithmHelper.RegionIndependentParams.CustomTex = (Texture2D) GuiHelper.ObjectFieldGuiElement(mCustomImageLabel, mAlgorithmHelper.RegionIndependentParams.CustomTex, typeof(Texture2D), false, ref mCustomTexChanged);
			
			mAlgorithmHelper.RegionIndependentParams.CustomScale = GuiHelper.Vector2FieldGuiElement(mCustomScaleLabel, mAlgorithmHelper.RegionIndependentParams.CustomScale, ref mCustomScaleChanged);
			mAlgorithmHelper.RegionIndependentParams.CustomOffset = GuiHelper.Vector2FieldGuiElement(mCustomOffsetLabel, mAlgorithmHelper.RegionIndependentParams.CustomOffset, ref mCustomOffsetChanged);
			
			EditorGUI.indentLevel--;
		}
		
		bool haveColliderRegionEnabledChanged = false;
		bool haveColliderRegionMaxPointCountChanged = false;
		bool haveColliderRegionConvexChanged = false;
		OnInspectorGuiHolesAndIslandsSection(out haveColliderRegionEnabledChanged, out haveColliderRegionMaxPointCountChanged, out haveColliderRegionConvexChanged);
		
		if(GUILayout.Button(mCalculateOutlineVerticesLabel)) {

            RecalculateSelectedColliders();
		}
		
		GUILayout.EndScrollView();
		
		if (mLiveUpdate && mSpriteCollectionProxyToEdit != null && mSpriteIDsToEdit != null) {
			bool pointCountNeedsUpdate = (mColliderPointCountChanged /*&& (mColliderPointCount > 2)*/); // when typing 28, it would otherwise update at the first digit '2'.
			if (pointCountNeedsUpdate ||
				mFlipInsideOutsideChanged ||
				mConvexChanged ||
				mNormalizedAlphaOpaqueThresholdChanged ||
				mCustomTexChanged ||
				mCustomScaleChanged ||
				mCustomOffsetChanged ||
				haveColliderRegionEnabledChanged || haveColliderRegionMaxPointCountChanged || haveColliderRegionConvexChanged) {

                RecalculateSelectedColliders();
			}
		}
	}
	
	//-------------------------------------------------------------------------
	void OnInspectorGuiHolesAndIslandsSection(out bool haveColliderRegionEnabledChanged,
											  out bool haveColliderRegionMaxPointCountChanged,
											  out bool haveColliderRegionConvexChanged) {
		
		haveColliderRegionEnabledChanged = false;
		haveColliderRegionMaxPointCountChanged = false;
		haveColliderRegionConvexChanged = false;
		
		if (mAlgorithmHelper.ColliderRegions == null || mAlgorithmHelper.ColliderRegions.Length == 0 ||
			mAlgorithmHelper.ColliderRegionParams == null || mAlgorithmHelper.ColliderRegionParams.Length == 0) {
			
			return;
		}
		
		if (!mAllSelectedSpritesHaveSameNumColliderRegions) {
			EditorGUILayout.LabelField("Holes and Islands", "<different number of Holes/Islands>");
			return;
		}
		
		int numColliderRegions = 0;
		if (mAlgorithmHelper.ColliderRegions != null) {
			numColliderRegions = mAlgorithmHelper.ColliderRegions.Length;
		}
		
		bool[] newIsRegionEnabled = new bool [numColliderRegions];
		int[] newRegionPointCount = new int [numColliderRegions];
		bool[] newForceRegionConvex = new bool [numColliderRegions];
		
		string foldoutString = "Holes and Islands [" + mAlgorithmHelper.NumEnabledColliderRegions + "][" + mAlgorithmHelper.ActualPointCountOfAllRegions + " vertices]";
		mShowHolesAndIslandsSection = EditorGUILayout.Foldout(mShowHolesAndIslandsSection, foldoutString);
		if(mShowHolesAndIslandsSection) {
			EditorGUI.indentLevel++;
			for (int regionIndex = 0; regionIndex < numColliderRegions; ++regionIndex) {
				
				ColliderRegionData colliderRegion = mAlgorithmHelper.ColliderRegions[regionIndex];
				ColliderRegionParametersTK2D parameters = mAlgorithmHelper.ColliderRegionParams[regionIndex];
				bool isEnabled = parameters.EnableRegion;
				int maxPointCount = parameters.MaxPointCount;
				bool convex = parameters.Convex;
				
				if (regionIndex != 0) {
					EditorGUILayout.Space();
				}
				
				
				
				string regionOrIslandString = colliderRegion.mRegionIsIsland ? "Island " : "Hole ";
				regionOrIslandString += regionIndex + " [" + colliderRegion.mDetectedRegion.mPointCount + " px]";
				
				//EditorGUILayout.BeginToggleGroup(regionOrIslandString, true);
				bool newIsEnabled = EditorGUILayout.BeginToggleGroup(regionOrIslandString, isEnabled);
				//bool newIsEnabled = EditorGUILayout.Toggle(regionOrIslandString, isEnabled);
				
				EditorGUI.indentLevel++;
				
				
				// int [3..100] max point count
				int newPointCount = EditorGUILayout.IntSlider("Outline Vertex Count", maxPointCount, 3, mPointCountSliderMax);
				bool newConvex = EditorGUILayout.Toggle("Force Convex", convex);
				
				EditorGUI.indentLevel--;
				EditorGUILayout.EndToggleGroup();
				
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
			}
			
			for (int regionIndex = 0; regionIndex < numColliderRegions; ++regionIndex) {
				
				ColliderRegionParametersTK2D colliderRegionParams = mAlgorithmHelper.ColliderRegionParams[regionIndex];
				colliderRegionParams.EnableRegion = newIsRegionEnabled[regionIndex];
				colliderRegionParams.MaxPointCount = newRegionPointCount[regionIndex];
				colliderRegionParams.Convex = newForceRegionConvex[regionIndex];
			}
			
			EditorGUI.indentLevel--;
		}
	}
	
	//-------------------------------------------------------------------------
	void OnDestroy() {
		AssetPostprocessorDetectTK2DCommit.TargetColliderGenTK2DWindow = null;
	}
	
	//-------------------------------------------------------------------------
	public void OnSpriteCollectionCommit() {
		SaveParameterStoreToPrefab();
	}
	
	//-------------------------------------------------------------------------
	/// <summary>
	/// We need to periodically update the window since we cannot create hook
	/// to repaint if the selected sprites of the SpriteCollection editor
	/// window have changed.
	/// </summary>
	void Update() {
	    if (!EditorApplication.isPlaying) {
			if ((mRepaintCounter % 50) == 0) {
				Repaint();
			}
			++mRepaintCounter;
		}
	}
	
	//-------------------------------------------------------------------------
	protected void CheckForSelectedSpriteCollectionAndSprites() {
		
		mOldSpriteCollectionProxyToEdit = mSpriteCollectionProxyToEdit;
		mOldSpriteIDsToEdit = mSpriteIDsToEdit;
		
		mSpriteCollectionProxyToEdit = GetSelectedSpriteCollectionProxy();
		mSpriteIDsToEdit = GetSelectedSpriteEntries();
	}
	
	//-------------------------------------------------------------------------
	protected void CheckForValuesToUpdate() {
		if (!mWasInitialized)
			InitWithPreferencesValues();
		
		// The following actions are performed:
		// - storeNeedsReload = collection != oldCollection -> load store from prefab
		// - loadDifferentSpriteOfSameCollection = collection == oldCollection && spriteIDs != oldSpriteIDs -> load params from store (not from prefab)
		//
		// - PrepareHelperBackendForGui -> updates holes/islands data and sets up region-parameters
		
		if (mSpriteCollectionProxyToEdit != null && mSpriteIDsToEdit != null) {
			
			
			// SpriteCollection editor window is already open, sprites are selected.
			
			bool activeSpriteCollectionChanged = (mSpriteCollectionProxyToEdit != mOldSpriteCollectionProxyToEdit);
			bool spriteSelectionIDsChanged = !AreArraysEqual(mSpriteIDsToEdit, mOldSpriteIDsToEdit);
			
			bool storeNeedsReload = (mParameterStore == null || activeSpriteCollectionChanged);
			if (storeNeedsReload) {
				mParameterStore = CreateOrLoadParameterStore(ref mParameterStoreSaveFilePath);
				if (mParameterStore != null) {
					bool parametersFound = LoadValuesFromParameterStore(mParameterStore, mSpriteIDsToEdit);
					if (!parametersFound) {
						InitWithPreferencesValues();
					}
				}
				mAtlasPathToMonitorForCommit = GetAtlasPathForActiveSpriteCollection();
			}
			else {
				
				bool loadDifferentSpriteOfSameCollection = (spriteSelectionIDsChanged && !activeSpriteCollectionChanged);
				if (loadDifferentSpriteOfSameCollection) {
					bool parametersFound = LoadValuesFromParameterStore(mParameterStore, mSpriteIDsToEdit);
					if (!parametersFound) {
						InitWithPreferencesValues();
					}
				}
			}
			
			if ((activeSpriteCollectionChanged || spriteSelectionIDsChanged)) {
				
				mAllSelectedSpritesHaveSameNumColliderRegions = PrepareHelperBackendForGui(mSpriteCollectionProxyToEdit, mSpriteIDsToEdit);
			}
		}
	}
	
	//-------------------------------------------------------------------------
	protected void InitWithPreferencesValues() {
		
		this.mLiveUpdate = AlphaMeshColliderPreferences.Instance.DefaultLiveUpdate;
		int defaultPointCount = AlphaMeshColliderPreferences.Instance.DefaultColliderPointCount;
		this.mAlgorithmHelper.RegionIndependentParams.DefaultMaxPointCount = defaultPointCount;
		if (this.mAlgorithmHelper.ColliderRegionParams != null && this.mAlgorithmHelper.ColliderRegionParams.Length != 0) {
			this.mAlgorithmHelper.ColliderRegionParams[0].MaxPointCount = defaultPointCount;
		}
		
		this.mAlgorithmHelper.RegionIndependentParams.Convex = AlphaMeshColliderPreferences.Instance.DefaultConvex;
		this.mPointCountSliderMax = AlphaMeshColliderPreferences.Instance.ColliderPointCountSliderMaxValue;
		
		
		this.mAlgorithmHelper.RegionIndependentParams.AlphaOpaqueThreshold = 0.1f;
		this.mAlgorithmHelper.RegionIndependentParams.FlipInsideOutside = false;
		this.mAlgorithmHelper.RegionIndependentParams.CustomTex = null;
		this.mAlgorithmHelper.RegionIndependentParams.CustomScale = Vector2.one;
		this.mAlgorithmHelper.RegionIndependentParams.CustomOffset = Vector2.zero;
		
		mWasInitialized = true;
	}
	
	//-------------------------------------------------------------------------
	protected string GetAtlasPathForActiveSpriteCollection() {
		object spriteCollection = GetSelectedSpriteCollection();
		string prefabPath = GetSpriteCollectionPrefabFilePath(spriteCollection);
		string directory = System.IO.Path.GetDirectoryName(prefabPath);
		string atlasPath = directory + "/atlas0.png";
		return atlasPath;
	}
	
	//-------------------------------------------------------------------------
	protected object GetSelectedSpriteCollection() {
		Type spriteCollectionEditorType = Type.GetType("tk2dSpriteCollectionEditorPopup");
		if (spriteCollectionEditorType == null) {
			return null;
		}
		EditorWindow window = EditorWindow.GetWindow(spriteCollectionEditorType, false, "Sprite Collection Editor", false);
		if (window == null) {
			return null;
		}
		
		object spriteCollection = GetSpriteCollection(window);
		return spriteCollection;
	}
	
	//-------------------------------------------------------------------------
	protected object GetSelectedSpriteCollectionProxy() {
		Type spriteCollectionEditorType = Type.GetType("tk2dSpriteCollectionEditorPopup");
		if (spriteCollectionEditorType == null) {
			return null;
		}
		
		EditorWindow window = EditorWindow.GetWindow(spriteCollectionEditorType, false, "Sprite Collection Editor", false);
		if (window == null) {
			return null;
		}
		
		object spriteCollectionProxy = GetSpriteCollectionProxy(window);
		return spriteCollectionProxy;
	}
	
	//-------------------------------------------------------------------------
	protected void SaveParameterStoreToPrefab() {
		SaveParameterStoreToPrefab(mParameterStore, mParameterStoreSaveFilePath);
	}
	
	//-------------------------------------------------------------------------
	protected ColliderGenTK2DParameterStore CreateOrLoadParameterStore(ref string parameterStoreSavePrefabPath) {
		
		object spriteCollection = GetSelectedSpriteCollection();
		ColliderGenTK2DParameterStore result = EnsureParameterStorePrefabExistsForCollection(ref parameterStoreSavePrefabPath, spriteCollection);
		result.UpdateToCurrentVersionIfNecessary();
		return result;
	}
	
	//-------------------------------------------------------------------------
	protected bool LoadValuesFromParameterStore(ColliderGenTK2DParameterStore parameterStore, int[] spriteIDs) {
		if (spriteIDs.Length == 0 || parameterStore == null)
			return false;
		
		ColliderGenTK2DParametersForSprite parameters = parameterStore.GetParametersForSprite(spriteIDs[0]);
		if (parameters == null) {
			return false;
		}
		
		mAlgorithmHelper.RegionIndependentParams = parameters.mRegionIndependentParameters;
		mAlgorithmHelper.ColliderRegionParams = parameters.mColliderRegionParameters;
		
		return true;
	}
	
	//-------------------------------------------------------------------------
	protected void SaveValuesToParameterStore(ColliderGenTK2DParameterStore parameterStore, int[] spriteIDs) {
		if (spriteIDs.Length == 0 || parameterStore == null)
			return;
		
		foreach (int spriteID in spriteIDs) {
			ColliderGenTK2DParametersForSprite parameters = new ColliderGenTK2DParametersForSprite();
		
			parameters.mRegionIndependentParameters = mAlgorithmHelper.RegionIndependentParams;
			parameters.mColliderRegionParameters = mAlgorithmHelper.ColliderRegionParams;
			
			parameters.mSpriteIndex = spriteID;
			
			parameterStore.SaveParametersForSprite(spriteID, parameters);
		}
	}	
	
	//-------------------------------------------------------------------------
	void RecalculateSelectedColliders() {
		
		Type spriteCollectionEditorType = Type.GetType("tk2dSpriteCollectionEditorPopup");
		if (spriteCollectionEditorType == null) {
			return;
		}
		EditorWindow window = EditorWindow.GetWindow(spriteCollectionEditorType, false, "Sprite Collection Editor", false);
		
		object spriteCollection = GetSpriteCollection(window);
		object spriteCollectionProxy = GetSpriteCollectionProxy(window);
		mSpriteIDsToEdit = GetSelectedSpriteEntries(window);
		
		bool isSelectionInWindowOK = (mSpriteIDsToEdit != null && mSpriteIDsToEdit.Length > 0) && (spriteCollection != null);
		if (!isSelectionInWindowOK) {
			RecalculateCollidersOfSceneSelection(window);
		}
		else {
			RecalculateCollidersOf2DToolkitWindowSelection(window, spriteCollection, spriteCollectionProxy);
		}

        window.Repaint();
		SaveValuesToParameterStore(mParameterStore, mSpriteIDsToEdit);
	}

	//-------------------------------------------------------------------------
	void RecalculateCollidersOfSceneSelection(EditorWindow window) {
		
		mDifferentSprites = GetSpriteIDsAndContainerIDFromSelectedSprites();
		mSpriteIDsToEdit = new int[mDifferentSprites.Count];
		mDifferentSprites.Keys.CopyTo(mSpriteIDsToEdit, 0);
		
		LoadCollectionInSpriteCollectionEditorWindow(window, mDifferentSprites);
		SelectSpritesInSpriteCollectionEditor(window, mSpriteIDsToEdit);
		object spriteCollection = GetSpriteCollection(window);
		object spriteCollectionProxy = GetSpriteCollectionProxy(window);
		
		mSpriteCollectionProxyToEdit = spriteCollectionProxy;
		
		CheckForValuesToUpdate(); // we need to check if we need to load some parameters from the parameter store.
		
		RecalculateCollidersOf2DToolkitWindowSelection(window, spriteCollection, spriteCollectionProxy);
	}
	
	//-------------------------------------------------------------------------
	void RecalculateCollidersOf2DToolkitWindowSelection(EditorWindow window, object spriteCollection, object spriteCollectionProxy) {
		EnsureColliderTypePolyCollider(spriteCollectionProxy, mSpriteIDsToEdit);
		GenerateColliderVertices(spriteCollectionProxy, mSpriteIDsToEdit);
		
		float editorScale = 1.0f;
		PropertyInfo propertyEditorDisplayScale = null;
		object textureEditor = null;
		bool scaleReadSuccessfully = GetEditorDisplayScale(out editorScale, out propertyEditorDisplayScale, out textureEditor, window);
		
		SelectSpritesInSpriteCollectionEditor(window, mSpriteIDsToEdit);
		SetViewModeToColliderMode(window);
		
		if (scaleReadSuccessfully) {
			propertyEditorDisplayScale.SetValue(textureEditor, editorScale, null);
		}
	}
	
	//-------------------------------------------------------------------------
	object GetSpriteCollection(EditorWindow window) {
		Type spriteCollectionEditorType = window.GetType();
		FieldInfo fieldSpriteCollection = spriteCollectionEditorType.GetField("_spriteCollection", BindingFlags.Instance | BindingFlags.NonPublic);
		if (fieldSpriteCollection == null) {
			Debug.LogError("Detected a missing '_spriteCollection' member variable at the TK2D editor window class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return null;
		}
		return fieldSpriteCollection.GetValue(window);
	}
	
	//-------------------------------------------------------------------------
	object GetSpriteCollectionProxy(EditorWindow window) {
		Type spriteCollectionEditorType = window.GetType();
		FieldInfo fieldSpriteCollectionProxy = spriteCollectionEditorType.GetField("spriteCollectionProxy", BindingFlags.Instance | BindingFlags.NonPublic);
		if (fieldSpriteCollectionProxy == null) {
			Debug.LogError("Detected a missing 'spriteCollectionProxy' member variable at the TK2D editor window class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return null;
		}
		return fieldSpriteCollectionProxy.GetValue(window);
	}
	
	//-------------------------------------------------------------------------
	int[] GetSelectedSpriteEntries() {
		Type spriteCollectionEditorType = Type.GetType("tk2dSpriteCollectionEditorPopup");
		if (spriteCollectionEditorType == null) {
			return null;
		}
		
		EditorWindow window = EditorWindow.GetWindow(spriteCollectionEditorType, false, "Sprite Collection Editor", false);
		if (window == null)
			return null;
		
		return GetSelectedSpriteEntries(window);
	}
	
	//-------------------------------------------------------------------------
	int[] GetSelectedSpriteEntries(EditorWindow window) {
		Type spriteCollectionEditorType = window.GetType();
		FieldInfo fieldSelectedEntries = spriteCollectionEditorType.GetField("selectedEntries", BindingFlags.Instance | BindingFlags.NonPublic);
		if (fieldSelectedEntries == null) {
			Debug.LogError("Detected a missing 'selectedEntries' member variable at the TK2D editor window class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return null;
		}
		
		IList selectedEntries = (IList) fieldSelectedEntries.GetValue(window);
		
		int[] resultArray = new int[selectedEntries.Count];
		for (int i = 0; i < selectedEntries.Count; ++i) {
			object entry = selectedEntries[i];
			
			Type entryType = entry.GetType();
			FieldInfo fieldSpriteIndex = entryType.GetField("index");
			if (fieldSpriteIndex == null) {
				Debug.LogError("Detected a missing 'index' member variable at a TK2D sprite collection entry - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
				return null;
			}
			int spriteIndex = (int) fieldSpriteIndex.GetValue(entry);
			resultArray[i] = spriteIndex;
		}
		return resultArray;
	}
	
	//-------------------------------------------------------------------------
	void RestoreSpriteSelection(EditorWindow window, int[] spriteIDsToSelect) {
		
		// Does the following via reflection:
		//
		// foreach (object entry in window.entries)
		// {
		//   if (spriteIDsToSelect.Contains(entry.index))
		//   {
		//     entry.selected = true;
		//   }
		// }
		// window.UpdateSelection();
		
		Type spriteCollectionEditorType = window.GetType();
		FieldInfo fieldEntries = spriteCollectionEditorType.GetField("entries", BindingFlags.Instance | BindingFlags.NonPublic);
		if (fieldEntries == null) {
			Debug.LogError("Detected a missing 'entries' member variable at the TK2D editor window class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return;
		}
		IList entries = (IList) fieldEntries.GetValue(window);
		
		foreach (object entry in entries)
		{
			Type entryType = entry.GetType();
			FieldInfo fieldSpriteIndex = entryType.GetField("index");
			if (fieldSpriteIndex == null) {
				Debug.LogError("Detected a missing 'index' member variable at a TK2D sprite collection entry - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
				return;
			}
			
			int spriteIndex = (int) fieldSpriteIndex.GetValue(entry);
			bool shallSelect = (Array.IndexOf(spriteIDsToSelect, spriteIndex) != -1);
			if (shallSelect) {
				FieldInfo fieldSelected = entryType.GetField("selected");
				if (fieldSelected == null) {
					Debug.LogError("Detected a missing 'selected' member variable at a TK2D sprite collection entry - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
					return;
				}
				fieldSelected.SetValue(entry, true);
			}
		}
		
		MethodInfo methodUpdateSelection = spriteCollectionEditorType.GetMethod("UpdateSelection", BindingFlags.Instance | BindingFlags.NonPublic);
		methodUpdateSelection.Invoke(window, null);
	}
	
	//-------------------------------------------------------------------------
	void LoadCollectionInSpriteCollectionEditorWindow(EditorWindow window, SortedList<int, Component> differentSprites) {
		
		if (differentSprites.Count <= 0)
			return;
		
		// Does the following via reflection:
		// window.SetGeneratorAndSelectedSprite((tk2dSpriteCollection) spriteCollectionToDisplay, spriteIDToDisplay);
		Type spriteCollectionEditorType = window.GetType();
		MethodInfo methodSetGeneratorAndSelectedSprite = spriteCollectionEditorType.GetMethod("SetGeneratorAndSelectedSprite");
		
		int lastIndex = differentSprites.Count-1;
		int spriteIDToDisplay = differentSprites.Keys[lastIndex];
		Component sprite = differentSprites.Values[lastIndex];
		object spriteCollectionToDisplay = mAlgorithmHelper.GetTK2DSpriteCollection(sprite);
		
		object[] methodParams = new object[] { spriteCollectionToDisplay, spriteIDToDisplay };
		methodSetGeneratorAndSelectedSprite.Invoke(window, methodParams);
	}
	
	//-------------------------------------------------------------------------
	void SelectSpritesInSpriteCollectionEditor(EditorWindow window, int[] spriteIDs) {
		
		// Does the following via reflection:
		// window.SelectSpritesFromList(spriteIDs);
		Type spriteCollectionEditorType = window.GetType();
		MethodInfo methodSelectSpritesFromList = spriteCollectionEditorType.GetMethod("SelectSpritesFromList");
		
		object[] methodParams = new object[] { spriteIDs };
		methodSelectSpritesFromList.Invoke(window, methodParams);
	}
	
	//-------------------------------------------------------------------------
	void SetViewModeToColliderMode(EditorWindow window) {
		Type spriteCollectionEditorType = window.GetType();
		FieldInfo fieldSpriteView = spriteCollectionEditorType.GetField("spriteView", BindingFlags.Instance | BindingFlags.NonPublic);
		if (fieldSpriteView == null) {
			Debug.LogError("Detected a missing 'spriteView' member variable at the TK2D editor window class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return;
		}
		object spriteView = fieldSpriteView.GetValue(window);
		
		Type spriteViewType = spriteView.GetType();
		FieldInfo fieldTextureEditor = spriteViewType.GetField("textureEditor", BindingFlags.Instance | BindingFlags.NonPublic);
		if (fieldTextureEditor == null) {
			Debug.LogError("Detected a missing 'textureEditor' member variable at the TK2D editor window sprite view class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return;
		}
		object textureEditor = fieldTextureEditor.GetValue(spriteView);
		
		Type textureEditorType = textureEditor.GetType();
		FieldInfo fieldMode = textureEditorType.GetField("mode", BindingFlags.Instance | BindingFlags.NonPublic);
		if (fieldMode == null) {
			Debug.LogError("Detected a missing 'mode' member variable at the TK2D editor window texture editor class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return;
		}
		object enumValue = fieldMode.GetValue(textureEditor);
		Type enumType = enumValue.GetType();
		int oldIntValue = (int) enumValue;
		bool isInColliderMode = oldIntValue == COLLIDER_EDIT_MODE_INT_VALUE;
		if (!isInColliderMode) {
			object newEnumValue = Enum.ToObject(enumType, COLLIDER_EDIT_MODE_INT_VALUE);
			fieldMode.SetValue(textureEditor, newEnumValue);
		}
	}
	
	//-------------------------------------------------------------------------
	bool GetEditorDisplayScale(out float editorScale, out PropertyInfo propertyEditorDisplayScale, out object textureEditor, EditorWindow window) {
		
		propertyEditorDisplayScale = null;
		textureEditor = null;
		editorScale = 1.0f;
		
		Type spriteCollectionEditorType = window.GetType();
		FieldInfo fieldSpriteView = spriteCollectionEditorType.GetField("spriteView", BindingFlags.Instance | BindingFlags.NonPublic);
		if (fieldSpriteView == null) {
			Debug.LogError("Detected a missing 'spriteView' member variable at the TK2D editor window class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		object spriteView = fieldSpriteView.GetValue(window);
		
		Type spriteViewType = spriteView.GetType();
		FieldInfo fieldTextureEditor = spriteViewType.GetField("textureEditor", BindingFlags.Instance | BindingFlags.NonPublic);
		if (fieldTextureEditor == null) {
			Debug.LogError("Detected a missing 'textureEditor' member variable at the TK2D editor window sprite view class - Is your 2D Toolkit package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		textureEditor = fieldTextureEditor.GetValue(spriteView);
		
		Type textureEditorType = textureEditor.GetType();
		PropertyInfo propertySpriteCollection = textureEditorType.GetProperty("SpriteCollection", BindingFlags.Instance | BindingFlags.NonPublic);
		object spriteCollection = propertySpriteCollection.GetValue(textureEditor, null);
		if (spriteCollection != null) {
			propertyEditorDisplayScale = textureEditorType.GetProperty("editorDisplayScale", BindingFlags.Instance | BindingFlags.NonPublic);
			editorScale = (float) propertyEditorDisplayScale.GetValue(textureEditor, null);
			return true;
		}
		return false;
	}
	
	//-------------------------------------------------------------------------
	SortedList<int, Component> GetSpriteIDsAndContainerIDFromSelectedSprites() {
		
		object spriteCollectionToDisplay = 0;
		SortedList<int, Component> differentSprites = new SortedList<int, Component>(); // SortedList<spriteID, tk2dSprite>
		
		for (int index = Selection.gameObjects.Length - 1; index >= 0; --index) {
			Component tk2dSpriteObject = Selection.gameObjects[index].GetComponent("tk2dSprite");
			if (tk2dSpriteObject != null) {
				spriteCollectionToDisplay = mAlgorithmHelper.GetTK2DSpriteCollection(tk2dSpriteObject);
				break;
			}
		}
		
		foreach (GameObject gameObj in Selection.gameObjects) {
			Component tk2dSpriteObject = gameObj.GetComponent("tk2dSprite");
			if (tk2dSpriteObject != null) {
				int spriteID = mAlgorithmHelper.GetSpriteID(tk2dSpriteObject);
				object collection = mAlgorithmHelper.GetTK2DSpriteCollection(tk2dSpriteObject);
				
				if (collection == spriteCollectionToDisplay && !differentSprites.ContainsKey(spriteID)) {
					differentSprites.Add(spriteID, tk2dSpriteObject);
				}
			}
		}
		return differentSprites;
	}
	
	//-------------------------------------------------------------------------
	void EnsureColliderTypePolyCollider(object spriteCollection, int[] spriteIDs) {
		
		bool wasSuccessful = mAlgorithmHelper.EnsureColliderTypePolyCollider(spriteCollection, spriteIDs);
		if (!wasSuccessful) {
			Debug.LogError("Error: EnsureColliderTypePolyCollider failed.");
		}
	}
	
	//-------------------------------------------------------------------------
	void GenerateColliderVertices(object spriteCollection, int[] spriteIDs) {
		
		mAlgorithmHelper.GenerateColliderVertices(spriteCollection, spriteIDs);
	}
	
	//-------------------------------------------------------------------------
	bool PrepareHelperBackendForGui(object spriteCollection, int[] spriteIDs) {
		
		bool allHaveSameNumberOfColliderRegions;
		mAlgorithmHelper.PrepareIslandsForGui(out allHaveSameNumberOfColliderRegions, spriteCollection, spriteIDs);
		return allHaveSameNumberOfColliderRegions;
	}
	
	//-------------------------------------------------------------------------
	bool AreArraysEqual<T>(T[] a, T[] b) {
	    return AreArraysEqual(a, b, EqualityComparer<T>.Default);
	}
	
	//-------------------------------------------------------------------------
	bool AreArraysEqual<T>(T[] a, T[] b, IEqualityComparer<T> comparer) {
		if (a == null || b == null) {
			return false;
		}
		if(a.Length != b.Length) {
	        return false;
	    }
	    for(int i = 0; i < a.Length; i++) {
	        if(!comparer.Equals(a[i], b[i])) {
	            return false;
	        }
	    }
	    return true;
	}
	
	//-------------------------------------------------------------------------
	// Parameter Store Part
	//-------------------------------------------------------------------------
	//-------------------------------------------------------------------------
	public static ColliderGenTK2DParameterStore EnsureParameterStorePrefabExistsForCollection(ref string parameterStoreSavePrefabPath, object spriteCollection) {
		
		ColliderGenTK2DParameterStore parameterStoreObject = null;
		parameterStoreSavePrefabPath = GetParameterStorePrefabFilePath(spriteCollection);
		string prefabDir = System.IO.Path.GetDirectoryName(parameterStoreSavePrefabPath);
		
		System.IO.FileInfo fileInfo = new System.IO.FileInfo(prefabDir);
		if (!fileInfo.Directory.Exists) {
			Debug.LogError("Directory '" + prefabDir + "' for creating the ColliderGenTK2DParameterStore prefab does not exist.");
			return null;
		}
		
		parameterStoreObject = UnityEditor.AssetDatabase.LoadAssetAtPath(parameterStoreSavePrefabPath, typeof(ColliderGenTK2DParameterStore)) as ColliderGenTK2DParameterStore;
		// Does not exist yet - create
		if (parameterStoreObject == null)
		{	
			GameObject go = new GameObject();
			go.AddComponent<ColliderGenTK2DParameterStore>();
#if (UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4)
			UnityEngine.Object p = EditorUtility.CreateEmptyPrefab(targetParameterStorePrefabPath);
			EditorUtility.ReplacePrefab(go, p);
#else
			UnityEngine.Object p = UnityEditor.PrefabUtility.CreateEmptyPrefab(parameterStoreSavePrefabPath);
			PrefabUtility.ReplacePrefab(go, p);
#endif
			GameObject.DestroyImmediate(go);
			AssetDatabase.SaveAssets();

			parameterStoreObject = UnityEditor.AssetDatabase.LoadAssetAtPath(parameterStoreSavePrefabPath, typeof(ColliderGenTK2DParameterStore)) as ColliderGenTK2DParameterStore;
		}
		return parameterStoreObject;
	}
	
	//-------------------------------------------------------------------------
	public static void SaveParameterStoreToPrefab(ColliderGenTK2DParameterStore parameterStore, string targetParameterStorePrefabPath) {
		
		GameObject go = new GameObject();
		go.AddComponent<ColliderGenTK2DParameterStore>();
		ColliderGenTK2DParameterStore emptyComponent = go.GetComponent<ColliderGenTK2DParameterStore>();
		
		//EditorUtility.CopySerialized(parameterStore, emptyComponent);
		emptyComponent.CopyFrom(parameterStore);
		
#if (UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4)
		UnityEngine.Object p = EditorUtility.CreateEmptyPrefab(targetParameterStorePrefabPath);
		EditorUtility.ReplacePrefab(go, p);
#else
		UnityEngine.Object p = UnityEditor.PrefabUtility.CreateEmptyPrefab(targetParameterStorePrefabPath);
		PrefabUtility.ReplacePrefab(go, p);
#endif
		GameObject.DestroyImmediate(go);
		AssetDatabase.SaveAssets();
	}
	
	//-------------------------------------------------------------------------
	public static string GetParameterStorePrefabFilePath(object spriteCollection) {
		string prefabPath = GetSpriteCollectionPrefabFilePath(spriteCollection);
		string prefabDir = System.IO.Path.GetDirectoryName(prefabPath);
		string storePrefabPath = prefabDir + "/ColliderGenParameters.prefab";
		return storePrefabPath;
	}
	
	//-------------------------------------------------------------------------
	public static string GetSpriteCollectionPrefabFilePath(object spriteCollection) {
		
		// taken from tk2dSpriteCollectionBuilder class begin
		string path = UnityEditor.AssetDatabase.GetAssetPath((UnityEngine.Object) spriteCollection);
		string subDirName = System.IO.Path.GetDirectoryName(path.Substring(7));
		if (subDirName.Length > 0) subDirName += "/";

		string dataDirFullPath = Application.dataPath + "/" + subDirName + System.IO.Path.GetFileNameWithoutExtension(path) + " Data";
		string dataDirName = "Assets/" + dataDirFullPath.Substring( Application.dataPath.Length + 1 ) + "/";
		
		string prefabObjectPath = "";
		
		// changed for reflection begin
		// reads spriteCollection.spriteCollection via reflection.
		Type spriteCollectionType = spriteCollection.GetType();
		FieldInfo fieldSpriteCollection = spriteCollectionType.GetField("spriteCollection");
		object spriteCollectionData = fieldSpriteCollection.GetValue(spriteCollection);
		
		string spriteCollectionName = ((MonoBehaviour)spriteCollection).name;
		
		if (spriteCollectionData != null) // changed for reflection end
			prefabObjectPath = UnityEditor.AssetDatabase.GetAssetPath((UnityEngine.Object) spriteCollectionData);
		else
			prefabObjectPath = dataDirName + spriteCollectionName + ".prefab";
		return prefabObjectPath;
		// taken from tk2dSpriteCollectionBuilder class end
	}
}
