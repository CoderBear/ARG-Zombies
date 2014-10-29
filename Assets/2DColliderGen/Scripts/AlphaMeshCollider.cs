#if !(UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
#define UNITY_4_3_AND_LATER
#endif


#if !(UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5)
#define UNITY_4_AND_LATER
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Reflection;

#if UNITY_EDITOR

using UnityEditor;

//-------------------------------------------------------------------------
/// <summary>
/// A component to generate a MeshCollider from an image with alpha channel.
/// </summary>
[ExecuteInEditMode]
public class AlphaMeshCollider : MonoBehaviour {
	
	public enum InitState {
		NotChecked = 0,
		Yes = 1,
		No = 2
	}
	public enum TargetColliderType {
		MeshCollider = 0,
		PolygonCollider2D = 1
	}
	private const int PARAMETER_NOT_USED_ANYMORE = -1;
	private const float OLD_PARAMETERS_CONVERTED = -888; // will be assigned to mRegionIndependentParameters.CustomRotation as a conversion-done value.
	
	public static string DEFAULT_OUTPUT_DIR = "Assets/Colliders/Generated";
	public static string DEFAULT_TILE_COLLIDER_PREFAB_OUTPUT_DIR = DEFAULT_OUTPUT_DIR + "/TileColliderPrefabs";
	public static string PREFAB_GAMEOBJECT_NAME_PREFIX = "Collider_";
	
	// START OF OLD PARAMETERS - NOT USED ANYMORE - NOW MOVED INTO A SEPARATE RegionIndependentParameters CLASS
	[SerializeField] private bool mLiveUpdate = true;
	[SerializeField] private float mAlphaOpaqueThreshold = 0.1f;
	[SerializeField] private float mVertexReductionDistanceTolerance = 0.0f;
	[SerializeField] private int mMaxPointCount = PARAMETER_NOT_USED_ANYMORE; // Don't use this parameter for anything! It is kept for backwards-compatibility only, to read the parameter from saved scenes.
	[SerializeField] private int mDefaultMaxPointCount = 20;
	[SerializeField] private float mThickness = 1.0f;
	[SerializeField] private bool mFlipHorizontal = false;
	[SerializeField] private bool mFlipVertical = false;
	[SerializeField] private bool mConvex = false;
	[SerializeField] private bool mFlipInsideOutside = false;

	[SerializeField] private float   mCustomRotation = 0.0f;
	[SerializeField] private Vector2 mCustomScale = Vector2.one;
	[SerializeField] private Vector3 mCustomOffset = Vector3.zero;
	
	[SerializeField] private bool mCopyOTSpriteFlipping = true;
	[SerializeField] private bool mCopySmoothMovesSpriteDimensions = true;
	[SerializeField] private Texture2D mCustomTex;
	
	[SerializeField] private bool mIsCustomAtlasRegionUsed = false;
	[SerializeField] private string mCustomAtlasFrameTitle = null;
	/// mCustomAtlasFramePositionInPixels describes the offset of the top-left corner of the sub-texture from the top-left origin of the currently used texture
	[SerializeField] private Vector2 mCustomAtlasFramePositionInPixels = Vector2.zero;
	[SerializeField] private Vector2 mCustomAtlasFrameSizeInPixels = Vector2.zero;
	[SerializeField] private float mCustomAtlasFrameRotation = 0.0f;
	
	[SerializeField] private bool mApplySmoothMovesScaleAnim = true;
	// END OF OLD PARAMETERS - NOT USED ANYMORE - NOW MOVED INTO A SEPARATE RegionIndependentParameters CLASS
	
	
	[SerializeField] protected string mGroupSuffix = ""; // an optional suffix to append to the filename (before the extension) to distinguish groups of the same sprite with different parameters.
	public string mColliderMeshFilename = "";  // the filename of the mesh without the directory but including the extension. E.g. "Island2_446_flipped_h.dae"
	
	
	[SerializeField] protected bool mWasInitialized = false;
	public Texture2D mMainTex = null;
	[SerializeField] protected string mColliderMeshDirectory = ""; // only the directory without the filename. Without a trailing slash. E.g. "Assets/Colliders/Generated"
	
	public bool mIsAtlasUsed = false;
	public int mAtlasFrameIndex = 0;
	public string mAtlasFrameTitle = null;
	/// mAtlasFramePositionInPixels describes the offset of the top-left corner of the sub-texture from the top-left origin of the atlasTex
	public Vector2 mAtlasFramePositionInPixels = Vector2.zero;
	public Vector2 mAtlasFrameSizeInPixels = Vector2.zero;
	public float mAtlasFrameRotation = 0.0f;
	[SerializeField] protected Vector2 mOutlineScale = Vector2.one;
	[SerializeField] protected Vector3 mOutlineOffset = Vector3.zero;

	public bool mInactiveBaseImageIsAtlas = false;
	public int mInactiveBaseImageWidth = 0;
	public int mInactiveBaseImageHeight = 0;
	public Vector2 mInactiveBaseImageOutlineScale = Vector2.one;
	public Vector3 mInactiveBaseImageOutlineOffset = Vector3.zero;
	
	
	protected bool mCurrentFlipHorizontal = false;
	protected bool mCurrentFlipVertical = false;
	
	public bool mHasOTSpriteComponent = false;
	protected Component mOTSpriteComponent = null; // read via reflection, therefore of type 'Component' instead of 'OTSprite'.
	public bool mHasSmoothMovesSpriteComponent = false;
	protected Component mSmoothMovesSpriteComponent = null; // read via reflection, therefore of type 'Component'.
	protected InitState mHasSmoothMovesBoneAnimationParent = InitState.NotChecked;
	protected string mFullSmoothMovesNodeString = null; // e.g. "Root/Torso/ArmLeft/Weapon"
	public bool mIsSmoothMovesNodeWithoutSprite = false;
	public bool mHasSmoothMovesAnimBoneColliderComponent = false;
	protected Component mSmoothMovesAnimBoneColliderComponent = null; // read via reflection, therefore of type 'Component'.
	protected Component mSmoothMovesBoneAnimation = null; // read via reflection, therefore of type 'Component'.
	protected string mFullSmoothMovesAssemblyName = "SmoothMoves_Runtime, Version=1.10.1.0, Culture=neutral, PublicKeyToken=null";
	protected Type mSmoothMovesAtlasType = null;
	protected Type mSmoothMovesBoneAnimationDataType = null;
	protected bool mHasTK2DSpriteComponent = false;
	protected Component mTK2DSpriteComponent = null; // read via reflection, therefore of type 'Component'.

#if UNITY_4_3_AND_LATER
	public bool mHasUnity43SpriteRendererComponent = false;
	protected UnityEngine.SpriteRenderer mUnity43SpriteRendererComponent = null;
	public bool mHasUnity43SpriteAnimatorComponent = false;
	protected UnityEngine.Animator mUnity43AnimatorComponent = null;
#endif

    public IslandDetector mIslandDetector = null;
    


	public bool [,] mBinaryImage = null; // NOT USED ANYMORE - NOW MOVED INTO A SEPARATE GeneratedColliderData CLASS
	[SerializeField] protected ColliderRegionData[] mColliderRegions = null; // NOT USED ANYMORE - NOW MOVED INTO A SEPARATE GeneratedColliderData CLASS
	public PolygonOutlineFromImageFrontend mOutlineAlgorithm = null; // NOT USED ANYMORE - NOW MOVED INTO A SEPARATE GeneratedColliderData CLASS

	/// Used to deal with multiple sprite frames, where it will be set to the frame-index. In normal cases, this is always 0.
	[SerializeField] protected int mActiveColliderIndex = 0;
    
	public GeneratedColliderData[] mGeneratedColliderData = new GeneratedColliderData[1] { new GeneratedColliderData() };

	//-------------------------------------------------------------------------
	/// <summary>
	/// Nested class to group parameters that are needed for creating and
	/// switching between multiple colliders based on sprite animation frames.
	/// Note: The runtime switching part is covered in the class
	/// RuntimeAnimatedColliderSwitch, AlphaMeshCollider will only generate the
	/// necessary colliders.
	/// </summary>
	[System.Serializable]
	public class AnimatedColliderParameters {
		[SerializeField] protected bool mIsColliderAnimationUsed = false;
#if UNITY_4_3_AND_LATER
		[SerializeField] protected Sprite[] mSpriteFrames = null;
		[SerializeField] protected PolygonCollider2D[] mPolygonCollidersToSwitch = null;
#endif
		[SerializeField] protected MeshCollider[] mMeshCollidersToSwitch = null;
		[SerializeField] protected string[] mColliderIDStrings = null;


		// Setters and Getters
#if UNITY_4_3_AND_LATER
		public Sprite[] SpriteFrames {
			get {
				return mSpriteFrames;
			}
			set {
				mSpriteFrames = value;
				mIsColliderAnimationUsed = (value != null);
			}
		}
		public PolygonCollider2D[] PolygonCollidersToSwitch {
			get {
				return mPolygonCollidersToSwitch;
			}
			set {
				mPolygonCollidersToSwitch = value;
			}
		}
#endif
		public MeshCollider[] MeshCollidersToSwitch {
			get {
				return mMeshCollidersToSwitch;
			}
			set {
				mMeshCollidersToSwitch = value;
			}
		}
		public string[] ColliderIDStrings {
			get {
				return mColliderIDStrings;
			}
			set {
				mColliderIDStrings = value;
			}
		}
		public bool IsColliderAnimationUsed {
			get {
				return mIsColliderAnimationUsed;
			}
		}
	}
	[SerializeField] protected AnimatedColliderParameters mAnimatedColliderParameters = new AnimatedColliderParameters();

	//-------------------------------------------------------------------------
	/// <summary>
	/// Nested class to group general (shared for all regions) parameters.
	/// </summary>
	[System.Serializable]
	public class RegionIndependentParameters : RegionIndependentParametersBase {

		//[SerializeField] protected bool mCreate2DPolygonCollider = false;
		//[SerializeField] protected bool mCreateMeshCollider = true; // initial behaviour is the old one, for easier backwards compatibility.
		[SerializeField] protected TargetColliderType mTargetColliderType = TargetColliderType.MeshCollider;
		[SerializeField] protected bool mCopyOTSpriteFlipping = true;
		[SerializeField] protected bool mCopySmoothMovesSpriteDimensions = true;
		[SerializeField] protected bool mApplySmoothMovesScaleAnim = true;

		// The following parameters are used mainly for processing sprite animations.
		// They are set to point to the respective sprite frames and target colliders
		// to generate a collider for each frame.
		[SerializeField] protected int mNumCollidersNeeded = 1;
#if UNITY_4_3_AND_LATER
		[SerializeField] protected Sprite mCustomUnity43Sprite;
#endif

		// Setters and Getters
		public TargetColliderType TargetColliderType {
			get {
				return mTargetColliderType;
			}
			set {
				if (value != mTargetColliderType) {
					mUpdateCalculationNeeded = true;
				}
				mTargetColliderType = value;
			}
		}
		public bool CopyOTSpriteFlipping {
			get {
				return mCopyOTSpriteFlipping;
			}
			set {
				if (value != mCopyOTSpriteFlipping) {
					mUpdateCalculationNeeded = true;
				}
				mCopyOTSpriteFlipping = value;
			}
		}
		public bool CopySmoothMovesSpriteDimensions {
			get {
				return mCopySmoothMovesSpriteDimensions;
			}
			set {
				if (value != mCopySmoothMovesSpriteDimensions) {
					mUpdateCalculationNeeded = true;
				}
				mCopySmoothMovesSpriteDimensions = value;
			}
		}
		public bool ApplySmoothMovesScaleAnim {
			get {
				return mApplySmoothMovesScaleAnim;
			}
			set {
				if (value != mApplySmoothMovesScaleAnim) {
					mUpdateCalculationNeeded = true;
				}
				mApplySmoothMovesScaleAnim = value;
			}
		}

		public int NumCollidersNeeded {
			get {
				return mNumCollidersNeeded;
			}
			set {
				mNumCollidersNeeded = value;
			}
		}

#if UNITY_4_3_AND_LATER
		public Sprite CustomUnity43Sprite {
			get {
				return mCustomUnity43Sprite;
			}
			set {
				mCustomUnity43Sprite = value; // Note: for now we don't want to update when CustomUnity43Sprite has changed, since we only modify this property from inside the AlphaMeshCollider
			}
		}
#endif
	}
	[SerializeField] protected RegionIndependentParameters mRegionIndependentParameters = new RegionIndependentParameters();
	//-------------------------------------------------------------------------
	/// <summary>
	/// Nested class to group general (shared for all regions) parameters.
	/// </summary>
	[System.Serializable]
	public class ColliderRegionParameters : ColliderRegionParametersBase {
		// No additional parameters needed for now.
	}
	[SerializeField] protected ColliderRegionParameters[] mColliderRegionParameters = null; // - NOT USED ANYMORE - now replaced by mIslandRegionParameters and mSeaRegionParameters
	[SerializeField] protected ColliderRegionParameters[] mIslandRegionParameters = null;
	[SerializeField] protected ColliderRegionParameters[] mSeaRegionParameters = null;
	
	// OTTileMap support
	protected bool mHasOTTileMapComponent = false;
	protected Component mOTTileMapComponent = null; // read via reflection, therefore of type 'Component'.
	public int mOTTileMapLayerIndex = 0;
	public int mOTTileMapMapPosX = 0;
	public int mOTTileMapMapPosY = 0;
	public int mOTTileMapWidth = 0;
	
	// OTTilesSprite support
	public bool mHasOTTilesSpriteComponent = false;
	protected Component mOTTilesSpriteComponent = null; // read via reflection, therefore of type 'Component'.
	
	
	// Setters and Getters
	public MeshCollider TargetMeshColliderComponent {
		get {
			if (mAnimatedColliderParameters.MeshCollidersToSwitch != null && mAnimatedColliderParameters.MeshCollidersToSwitch.Length >= mActiveColliderIndex) {
				return mAnimatedColliderParameters.MeshCollidersToSwitch[mActiveColliderIndex];
			}
			else {
				return this.GetComponent<MeshCollider>();
			}
		}
	}
#if UNITY_4_3_AND_LATER
	public PolygonCollider2D TargetPolygonCollider2DComponent {
		get {
			if (mAnimatedColliderParameters.PolygonCollidersToSwitch != null && mAnimatedColliderParameters.PolygonCollidersToSwitch.Length >= mActiveColliderIndex) {
				return mAnimatedColliderParameters.PolygonCollidersToSwitch[mActiveColliderIndex];
			}
			else {
				return this.GetComponent<PolygonCollider2D>();
			}
		}
	}
#endif

	public int MaxPointCountOfFirstEnabledRegion {
		get {
			ColliderRegionParameters firstRegionParams = this.FirstActiveColliderRegionParameters;
			if (firstRegionParams == null) {
				return 0;
			}
			return firstRegionParams.MaxPointCount;
		}
		set {
			ColliderRegionParameters firstRegionParams = this.FirstActiveColliderRegionParameters;
			if (firstRegionParams == null) {
				return;
			}
			firstRegionParams.MaxPointCount = value;
		}
	}
	
	public int ActualPointCountOfAllRegions {
		get {
			int pointCount = 0;
			Mesh[] colliderMeshes = this.ColliderMeshes;
			Mesh firstMesh = null;
			if (colliderMeshes != null && colliderMeshes.Length > 0) {
				firstMesh = colliderMeshes[0];
			}
			if (firstMesh &&
				(firstMesh.triangles != null) &&
				(firstMesh.triangles.Length > 0)) {
				
				pointCount = firstMesh.triangles.Length / 6;
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
	
	public ColliderRegionParameters FirstActiveColliderRegionParameters {
		get {
			if (mIslandRegionParameters == null) {
				if (mIslandRegionParameters == null) {
					return null;
				}
			}
			foreach (ColliderRegionParameters regionParameters in mIslandRegionParameters) {
				if (regionParameters.EnableRegion) {
					return regionParameters;
				}
			}
			foreach (ColliderRegionParameters regionParameters in mSeaRegionParameters) {
				if (regionParameters.EnableRegion) {
					return regionParameters;
				}
			}
			return null;
		}
	}
	
	public int ColliderRegionsTotalMaxPointCount {
		get {
			if (mIslandRegionParameters == null) {
				return 0;
			}
			int totalCount = 0;
			foreach (ColliderRegionParameters regionParameters in mIslandRegionParameters) {
				if (regionParameters.EnableRegion) {
					totalCount += regionParameters.MaxPointCount;
				}
			}
			foreach (ColliderRegionParameters regionParameters in mSeaRegionParameters) {
				if (regionParameters.EnableRegion) {
					totalCount += regionParameters.MaxPointCount;
				}
			}
			return totalCount;
		}
	}
	
	public int NumEnabledColliderRegions {
		get {
			if (mIslandRegionParameters == null) {
				return 0;
			}
			int totalCount = 0;
			foreach (ColliderRegionParameters regionParameters in mIslandRegionParameters) {
				if (regionParameters.EnableRegion) {
					++totalCount;
				}
			}
			foreach (ColliderRegionParameters regionParameters in mSeaRegionParameters) {
				if (regionParameters.EnableRegion) {
					++totalCount;
				}
			}
			return totalCount;
		}
	}
	
	public Vector2 CustomAtlasFramePositionInPixels {
		get {
			return mRegionIndependentParameters.CustomAtlasFramePositionInPixels;
		}
		set {
			if (UsedTexture == null) {
				mRegionIndependentParameters.CustomAtlasFramePositionInPixels = Vector2.zero;
			}
			else {
				mRegionIndependentParameters.CustomAtlasFramePositionInPixels = value;
				ClampCustomAtlasFramePositionAndSize();
			}
		}
	}
	
	public Vector2 CustomAtlasFrameSizeInPixels {
		get {
			return mRegionIndependentParameters.CustomAtlasFrameSizeInPixels;
		}
		set {
			if (UsedTexture == null) {
				mRegionIndependentParameters.CustomAtlasFrameSizeInPixels = Vector2.zero;
			}
			else {
				mRegionIndependentParameters.CustomAtlasFrameSizeInPixels = value;
				ClampCustomAtlasFramePositionAndSize();
			}
		}
	}
	
	public bool FlipHorizontal {
	    get { return this.mRegionIndependentParameters.FlipHorizontal; }
	    set { this.mRegionIndependentParameters.FlipHorizontal = value; } // don't call UpdateColliderMeshFilename() here, otherwise the filename does not fit the calculated collider for the registry!
	}
	public bool FlipVertical {
	    get { return this.mRegionIndependentParameters.FlipVertical; }
	    set { this.mRegionIndependentParameters.FlipVertical = value; } // don't call UpdateColliderMeshFilename() here, otherwise the filename does not fit the calculated collider for the registry!
	}
	
	public bool HasSmoothMovesBoneAnimationParent {
		get { return mHasSmoothMovesBoneAnimationParent == InitState.Yes; }
	}
	
	public string ColliderMeshDirectory {
	    get { return this.mColliderMeshDirectory; }
	    set {
			string directory =  value;
			if (directory == null || directory.Equals("")) {
				directory = DEFAULT_OUTPUT_DIR;
			}
			
			char[] charsToRemove = {'/', '\\', ' '};
			this.mColliderMeshDirectory = directory.TrimEnd(charsToRemove);
		}
	}
	
	public string GroupSuffix {
		get { return this.mGroupSuffix; }
	    set { this.mGroupSuffix = value; UpdateColliderMeshFilename(); }
	}
	
	public bool CanRecalculateCollider {
		get { return (UsedTexture != null && !mColliderMeshFilename.Equals("")); }
	}
	
	public bool CanReloadCollider {
		get {
			return (!mColliderMeshFilename.Equals("") && !mHasOTTilesSpriteComponent);
		}
	}
	
	public bool CanRewriteCollider {
		get {
			if (mRegionIndependentParameters.TargetColliderType != TargetColliderType.MeshCollider) {
				return false;
			}
			return (!mColliderMeshFilename.Equals("") && !mHasOTTilesSpriteComponent);
		}
	}

	public int ActiveColliderIndex {
		get { return mActiveColliderIndex; }
	}

	public int NumColliderFrames {
		get { return mRegionIndependentParameters.NumCollidersNeeded; }
	}

	public bool HasMultipleColliderFrames {
		get {
			return (mRegionIndependentParameters.NumCollidersNeeded > 1);
		}
	}

	public Texture2D UsedTexture
	{
	    get {
			if (mRegionIndependentParameters.CustomTex)
				return mRegionIndependentParameters.CustomTex;
			else
				return mMainTex;
		}
	}
	
	public Texture2D CustomTex {
		get { return this.mRegionIndependentParameters.CustomTex; }
		set { this.mRegionIndependentParameters.CustomTex = value; UpdateColliderMeshFilename(); }
	}
	
	public PolygonOutlineFromImageFrontend OutlineAlgorithm {
		get { return mGeneratedColliderData[mActiveColliderIndex].mOutlineAlgorithm; }
		set { mGeneratedColliderData[mActiveColliderIndex].mOutlineAlgorithm = value; }
	}
	
	public RegionIndependentParameters RegionIndependentParams {
		get {
			return mRegionIndependentParameters;
		}
		set {
			mRegionIndependentParameters = value;
		}
	}
	
	public ColliderRegionParameters[] IslandRegionParams {
		get {
			return mIslandRegionParameters;
		}
		set {
			mIslandRegionParameters = value;
		}
	}
	public ColliderRegionParameters[] SeaRegionParams {
		get {
			return mSeaRegionParameters;
		}
		set {
			mSeaRegionParameters = value;
		}
	}

	public GeneratedColliderData[] GeneratedColliderData {
		get {
			return mGeneratedColliderData;
		}
		set {
			mGeneratedColliderData = value;
		}
	}
	
	public ColliderRegionData[] ColliderRegions {
		get {
			return mGeneratedColliderData[mActiveColliderIndex].mColliderRegions;
		}
		set {
			mGeneratedColliderData[mActiveColliderIndex].mColliderRegions = value;
		}
	}
	
	public Mesh[] ColliderMeshes {
		get {
			List<Mesh> meshes = new List<Mesh>();
			if (mRegionIndependentParameters.TargetColliderType == TargetColliderType.MeshCollider) {
				MeshCollider[] colliders = TargetNodeToAttachMeshCollider.GetComponents<MeshCollider>();
				if (colliders == null) {
					return null;
				}
				for (int count = 0; count < colliders.Length; ++count) {
					meshes.Add(colliders[count].sharedMesh);
				}
				return meshes.ToArray();
			}
			else {
				return null;
			}
		}
		set {
			if (mRegionIndependentParameters.TargetColliderType == TargetColliderType.MeshCollider) {

				MeshCollider[] colliders = EnsureHasMeshColliderComponents(value.Length);

				for (int count = 0; count < colliders.Length; ++count) {
					colliders[count].sharedMesh = null;
					colliders[count].sharedMesh = value[count];
				}
			}
		}
	}

	public Vector2 AtlasFramePositionInPixels {
		get {
			if (mRegionIndependentParameters.IsCustomAtlasRegionUsed) {
				return mRegionIndependentParameters.CustomAtlasFramePositionInPixels;
			}
			else {
				return mAtlasFramePositionInPixels;
			}
		}
	}
	public Vector2 AtlasFrameSizeInPixels {
		get {
			if (mRegionIndependentParameters.IsCustomAtlasRegionUsed) {
				return mRegionIndependentParameters.CustomAtlasFrameSizeInPixels;
			}
			else {
				return mAtlasFrameSizeInPixels;
			}
		}
	}
	public float AtlasFrameRotation {
		get {
			if (mRegionIndependentParameters.IsCustomAtlasRegionUsed) {
				return mRegionIndependentParameters.CustomAtlasFrameRotation;
			}
			else {
				return mAtlasFrameRotation;
			}
		}
	}
	
	public Transform TargetNodeToAttachMeshCollider {
		get {
			if (this.HasSmoothMovesBoneAnimationParent && mRegionIndependentParameters.ApplySmoothMovesScaleAnim) {
				Transform targetNode = this.transform.Find(this.name + "_Sprite"); // attach the MeshCollider to the 
				if (targetNode == null) {
					Debug.Log("Unable to attach MeshCollider to '" + this.name + "_Sprite' child GameObject (GameObject not found)! Attaching MeshCollider to the parent instead.");
					return this.transform;
				}
				else {
					return targetNode;
				}
			}
			else {
				return this.transform;
			}
		}
	}
	
	public string TargetNodeNameToAttachMeshCollider {
		get {
			if (this.HasSmoothMovesBoneAnimationParent && mRegionIndependentParameters.ApplySmoothMovesScaleAnim) {
				Transform targetNode = this.transform.Find(this.name + "_Sprite"); // attach the MeshCollider to the 
				if (targetNode == null) {
					return this.name;
				}
				else {
					return targetNode.name;
				}
			}
			else {
				return this.name;
			}
		}
	}
	
	public bool ApplySmoothMovesScaleAnim {
		get { return mRegionIndependentParameters.ApplySmoothMovesScaleAnim; }
		set {
			bool oldApplySmoothMovesScaleAnim = mRegionIndependentParameters.ApplySmoothMovesScaleAnim;
			if (value != oldApplySmoothMovesScaleAnim) {
				if (this.HasSmoothMovesBoneAnimationParent) {
					MeshCollider oldMeshCollider = this.TargetNodeToAttachMeshCollider.GetComponent<MeshCollider>();
					if (oldMeshCollider) {
						DestroyImmediate(oldMeshCollider);
					}
				}
			}
			mRegionIndependentParameters.ApplySmoothMovesScaleAnim = value;
		}
	}
	
	//-------------------------------------------------------------------------
	protected void ClampCustomAtlasFramePositionAndSize() {
		int x  = (int) mRegionIndependentParameters.CustomAtlasFramePositionInPixels.x;
		int y  = (int) mRegionIndependentParameters.CustomAtlasFramePositionInPixels.y;
		x = Mathf.Clamp(x, 0, UsedTexture.width - 1);
		y = Mathf.Clamp(y, 0, UsedTexture.height - 1);
		mRegionIndependentParameters.CustomAtlasFramePositionInPixels = new Vector2(x, y);
		
		int width = (int) mRegionIndependentParameters.CustomAtlasFrameSizeInPixels.x;
		int height = (int) mRegionIndependentParameters.CustomAtlasFrameSizeInPixels.y;
		width = (int) Mathf.Clamp(width, 0, UsedTexture.width - mRegionIndependentParameters.CustomAtlasFramePositionInPixels.x);
		height = (int) Mathf.Clamp(height, 0, UsedTexture.height - mRegionIndependentParameters.CustomAtlasFramePositionInPixels.y);
		mRegionIndependentParameters.CustomAtlasFrameSizeInPixels = new Vector2(width, height);
	}
	
	//-------------------------------------------------------------------------
	public void SetCustomAtlasRegion(string customAtlasFrameTitle, Vector2 customAtlasFramePositionInPixels, Vector2 customAtlasFrameSizeInPixels, float customAtlasFrameRotation) {
		mRegionIndependentParameters.IsCustomAtlasRegionUsed = true;
		mRegionIndependentParameters.CustomAtlasFrameTitle = customAtlasFrameTitle;
		mRegionIndependentParameters.CustomAtlasFramePositionInPixels = customAtlasFramePositionInPixels;
		mRegionIndependentParameters.CustomAtlasFrameSizeInPixels = customAtlasFrameSizeInPixels;
		mRegionIndependentParameters.CustomAtlasFrameRotation = customAtlasFrameRotation;
		
		UpdateColliderMeshFilename();
	}
	
	//-------------------------------------------------------------------------
	public void ClearCustomAtlasRegion() {
		mRegionIndependentParameters.IsCustomAtlasRegionUsed = false;
		mRegionIndependentParameters.CustomAtlasFrameTitle = null;
		mRegionIndependentParameters.CustomAtlasFramePositionInPixels = Vector2.zero;
		mRegionIndependentParameters.CustomAtlasFrameSizeInPixels = Vector2.zero;
		mRegionIndependentParameters.CustomAtlasFrameRotation = 0;
		
		UpdateColliderMeshFilename();
	}
	
	//-------------------------------------------------------------------------
	public void SetOTTileMap(Component otTileMapComponent, int layerIndex, int mapPosX, int mapPosY, int mapWidth) {
		mHasOTTileMapComponent = true;
		mOTTileMapComponent = otTileMapComponent;
		mOTTileMapLayerIndex = layerIndex;
		mOTTileMapMapPosX = mapPosX;
		mOTTileMapMapPosY = mapPosY;
		mOTTileMapWidth =  mapWidth;
	}
	
	//-------------------------------------------------------------------------
	void RemoveOTTileMap() {
		mHasOTTileMapComponent = false;
		mOTTileMapComponent = null;
		mOTTileMapLayerIndex = 0;
		mOTTileMapMapPosX = 0;
		mOTTileMapMapPosY = 0;
		mOTTileMapWidth = 0;
	}
	
	//-------------------------------------------------------------------------
	public void SetOTTilesSprite(Component otTilesSpriteComponent) {
		mHasOTTilesSpriteComponent = true;
		mOTTilesSpriteComponent = otTilesSpriteComponent;
	}
	
	//-------------------------------------------------------------------------
	void RemoveOTTilesSprite() {
		mHasOTTilesSpriteComponent = false;
		mOTTilesSpriteComponent = null;
	}
	
	//-------------------------------------------------------------------------
	// Use this for initialization - we use this script from the editor only
	void Update() {
		
		if (!Application.isEditor || Application.isPlaying)
			return;
		
		UpdateAlphaMeshCollider();
	}
	
	//-------------------------------------------------------------------------
	public void PrepareColliderIslandsForGui() {

		this.UpdateAlphaMeshCollider();
	}
	
	//-------------------------------------------------------------------------
	public void UpdateAlphaMeshCollider() {
		
		UpdateFromOldVersionForBackwardsCompatibility();
		
		if (!mWasInitialized)
			InitWithPreferencesValues();

#if UNITY_4_3_AND_LATER
		CheckForUnity43SpriteComponent(out mHasUnity43SpriteRendererComponent, out mUnity43SpriteRendererComponent);

		if (mHasUnity43SpriteRendererComponent) {
			CheckForUnity43SpriteAnimatorComponent(out mHasUnity43SpriteAnimatorComponent, out mUnity43AnimatorComponent);
		}
#endif
		
		CheckForOTTilesSpriteComponent(out mHasOTTilesSpriteComponent, out mOTTilesSpriteComponent);
		if (!mHasOTTilesSpriteComponent) {
			// OTSprite is a base-class of OTTilesSprite, so we should not check this again here.
			CheckForOTSpriteComponent(out mHasOTSpriteComponent, out mOTSpriteComponent);
		}
		CheckForSmoothMovesSpriteComponent(out mHasSmoothMovesSpriteComponent, out mSmoothMovesSpriteComponent);
		if (mHasSmoothMovesBoneAnimationParent == InitState.NotChecked) { // new part for SmoothMoves v2.x
			CheckForSmoothMovesBoneAnimationParent(out mHasSmoothMovesBoneAnimationParent, out mSmoothMovesBoneAnimation, out mFullSmoothMovesNodeString);
		}
		
		if (mHasOTSpriteComponent && mRegionIndependentParameters.CopyOTSpriteFlipping) {
			GetOTSpriteFlipParameters(mOTSpriteComponent, out mRegionIndependentParameters.mFlipHorizontal, out mRegionIndependentParameters.mFlipVertical);
		}
		
		if (UsedTexture == null) {
			InitTextureParams();
		}
		
		if (mColliderMeshDirectory.Equals("")) {
			mColliderMeshDirectory = DEFAULT_OUTPUT_DIR;
		}
		if (mColliderMeshFilename.Equals("")) {
			UpdateColliderMeshFilename();
		}
		
		
		bool hasChangedFlipState = (mCurrentFlipHorizontal != mRegionIndependentParameters.FlipHorizontal) || (mCurrentFlipVertical != mRegionIndependentParameters.FlipVertical);
		if (hasChangedFlipState) {
			UpdateColliderMeshFilename();
			mCurrentFlipHorizontal = mRegionIndependentParameters.FlipHorizontal;
			mCurrentFlipVertical = mRegionIndependentParameters.FlipVertical;
		}
		
		if (mHasOTSpriteComponent) {
			EnsureOTSpriteCustomPhysicsMode(mOTSpriteComponent);
		}
		if (mHasSmoothMovesAnimBoneColliderComponent || mHasSmoothMovesBoneAnimationParent == InitState.Yes) {
			EnsureSmoothMovesBoneAnimHasRestoreComponent(mSmoothMovesBoneAnimation);
		}
		if (mHasOTTilesSpriteComponent) {
			EnsureOTTilesSpriteHasUpdateComponent(mOTTilesSpriteComponent);
		}
#if UNITY_4_3_AND_LATER
		if (mHasUnity43SpriteAnimatorComponent) {
			EnsureHasUnity43AnimatedColliderComponents();
			EnsureDataIsPreparedForMultipleColliders();
		}
#endif
		
		MeshCollider meshCollider = TargetNodeToAttachMeshCollider.GetComponent<MeshCollider>();
		bool isMeshColliderMissing = ((mRegionIndependentParameters.TargetColliderType == TargetColliderType.MeshCollider) && (meshCollider == null || meshCollider.sharedMesh == null));
#if UNITY_4_3_AND_LATER
		PolygonCollider2D polygonCollider = TargetNodeToAttachMeshCollider.GetComponent<PolygonCollider2D>();
		bool isPolygonColliderMissing = ((mRegionIndependentParameters.TargetColliderType == TargetColliderType.PolygonCollider2D) && (polygonCollider == null));
#else
		bool isPolygonColliderMissing = false;
#endif
		
		if (isMeshColliderMissing || isPolygonColliderMissing || hasChangedFlipState) {
			
			AlphaMeshColliderRegistry.Instance.ReloadOrRecalculateSingleCollider(this);
		}
	}

	
#if UNITY_4_3_AND_LATER	
	//-------------------------------------------------------------------------
	protected PolygonCollider2D[] EnsureHasPolygonCollider2DComponents(int numComponentsRequired) {

		PolygonCollider2D[] colliders = TargetNodeToAttachMeshCollider.GetComponents<PolygonCollider2D>();
		if (colliders == null || colliders.Length == 0) {
			RemoveExistingMeshColliderComponents(); // unfortunately we can't have both, it would produce an error.
			SwitchRigidbodyToRigidbody2D();
			colliders = AddEmptyPolygonCollider2DComponents(numComponentsRequired);
		}
		else if (colliders.Length != numComponentsRequired) {
			if (colliders.Length > numComponentsRequired) {
				RemoveExistingPolygonCollider2DComponents();
				colliders = AddEmptyPolygonCollider2DComponents(numComponentsRequired);
			}
			else {
				AddEmptyPolygonCollider2DComponents(numComponentsRequired - colliders.Length);
				colliders = TargetNodeToAttachMeshCollider.GetComponents<PolygonCollider2D>();
			}
		}
		mAnimatedColliderParameters.PolygonCollidersToSwitch = colliders;
		return colliders;
	}
#endif

	//-------------------------------------------------------------------------
	protected MeshCollider[] EnsureHasMeshColliderComponents(int numComponentsRequired) {
		MeshCollider[] colliders = TargetNodeToAttachMeshCollider.GetComponents<MeshCollider>();
		if (colliders == null || colliders.Length == 0) {
#if UNITY_4_3_AND_LATER	
			RemoveExistingPolygonCollider2DComponents(); // unfortunately we can't have both, it would produce an error.
			SwitchRigidbody2DToRigidbody();
#endif
			colliders = AddEmptyMeshColliderComponents(numComponentsRequired);
		}
		else if (colliders.Length != numComponentsRequired) {
			if (colliders.Length > numComponentsRequired) {
				RemoveExistingMeshColliderComponents();
				colliders = AddEmptyMeshColliderComponents(numComponentsRequired);
			}
			else {
				AddEmptyMeshColliderComponents(numComponentsRequired - colliders.Length);
				colliders = TargetNodeToAttachMeshCollider.GetComponents<MeshCollider>();
			}
		}
		mAnimatedColliderParameters.MeshCollidersToSwitch = colliders;
		return colliders;
	}

	//-------------------------------------------------------------------------
	protected void EnsureHasNoColliderComponent() {
#if UNITY_4_3_AND_LATER	
		RemoveExistingPolygonCollider2DComponents();
#endif
		RemoveExistingMeshColliderComponents();
	}
	
#if UNITY_4_3_AND_LATER	
	//-------------------------------------------------------------------------
	protected bool SwitchRigidbody2DToRigidbody() {
		Rigidbody2D rigidbody2D = TargetNodeToAttachMeshCollider.GetComponent<Rigidbody2D>();
		if (rigidbody2D == null) {
			return false;
		}
		float mass = rigidbody2D.mass;
		float drag = rigidbody2D.drag;
		float angularDrag = rigidbody2D.angularDrag;
		bool useGravity = (rigidbody2D.gravityScale > 0);
		bool isKinematic = rigidbody2D.isKinematic;
		RigidbodyInterpolation2D interpolation2D = rigidbody2D.interpolation;
		CollisionDetectionMode2D collisionDetectionMode2D = rigidbody2D.collisionDetectionMode;

		DestroyImmediate(rigidbody2D);
		Rigidbody newRigidbody = TargetNodeToAttachMeshCollider.gameObject.AddComponent<Rigidbody>();
		newRigidbody.mass = mass;
		newRigidbody.drag = drag;
		newRigidbody.angularDrag = angularDrag;
		newRigidbody.useGravity = useGravity;
		newRigidbody.isKinematic = isKinematic;

		switch (interpolation2D) {
		case RigidbodyInterpolation2D.Interpolate:
			newRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
			break;
		case RigidbodyInterpolation2D.Extrapolate:
			newRigidbody.interpolation = RigidbodyInterpolation.Extrapolate;
			break;
		case RigidbodyInterpolation2D.None:
		default:
			newRigidbody.interpolation = RigidbodyInterpolation.None;
			break;
		}

		switch (collisionDetectionMode2D) {
		case CollisionDetectionMode2D.Continuous:
			newRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
			break;
		case CollisionDetectionMode2D.None:
		default:
			newRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete; // Note: there is no proper mapping for CollisionDetectionMode2D.None, so we'll keep it at 'Discrete'.
			break;
		}
		return true;
	}

	//-------------------------------------------------------------------------
	protected bool SwitchRigidbodyToRigidbody2D() {
		Rigidbody rigidbody = TargetNodeToAttachMeshCollider.GetComponent<Rigidbody>();
		if (rigidbody == null) {
			return false;
		}
		float mass = rigidbody.mass;
		float drag = rigidbody.drag;
		float angularDrag = rigidbody.angularDrag;
		bool useGravity = rigidbody.useGravity;
		bool isKinematic = rigidbody.isKinematic;
		RigidbodyInterpolation interpolation = rigidbody.interpolation;
		CollisionDetectionMode collisionDetectionMode = rigidbody.collisionDetectionMode;
		
		DestroyImmediate(rigidbody);
		Rigidbody2D newRigidbody = TargetNodeToAttachMeshCollider.gameObject.AddComponent<Rigidbody2D>();
		newRigidbody.mass = mass;
		newRigidbody.drag = drag;
		newRigidbody.angularDrag = angularDrag;
		newRigidbody.gravityScale = useGravity ? 1.0f : 0.0f;
		newRigidbody.isKinematic = isKinematic;
		
		switch (interpolation) {
		case RigidbodyInterpolation.Interpolate:
			newRigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
			break; 
		case RigidbodyInterpolation.Extrapolate:
			newRigidbody.interpolation = RigidbodyInterpolation2D.Extrapolate;
			break;
		case RigidbodyInterpolation.None:
		default:
			newRigidbody.interpolation = RigidbodyInterpolation2D.None;
			break;
		}
		
		switch (collisionDetectionMode) {
		case CollisionDetectionMode.Continuous:
		case CollisionDetectionMode.Discrete: // Note: there is no proper mapping for CollisionDetectionMode2D.None, so we'll keep it at 'Continuous' in all cases.
		default:
			newRigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
			break;
		}
		return true;
	}

	//-------------------------------------------------------------------------
	protected PolygonCollider2D[] AddEmptyPolygonCollider2DComponents(int numComponentsToAdd) {

		// Note: we have to intermediately clear the meshFilter.sharedMesh,
		// otherwise we get the warning message "Compute mesh inertia tensor
		// failed for one of the actor's mesh shapes" because the flat
		// sprite-quad is used for volume calculations.
		Transform targetNode = TargetNodeToAttachMeshCollider;
		
		MeshFilter meshFilter = null;
		Mesh mesh = null;
		Rigidbody rigidbody = targetNode.GetComponent<Rigidbody>();
		if (rigidbody != null) {
			meshFilter = targetNode.GetComponent<MeshFilter>();
			if (meshFilter != null) {
				mesh = meshFilter.sharedMesh;
				meshFilter.sharedMesh = null;
			}
		}
		
		List<PolygonCollider2D> resultColliders = new List<PolygonCollider2D>();
		for (int count = 0; count < numComponentsToAdd; ++count) {
			PolygonCollider2D newCollider = targetNode.gameObject.AddComponent<PolygonCollider2D>();
			resultColliders.Add(newCollider);
		}
		
		if (rigidbody != null && meshFilter != null) {
			meshFilter.sharedMesh = mesh;
		}
		return resultColliders.ToArray();
	}
#endif
	
	//-------------------------------------------------------------------------
	protected MeshCollider[] AddEmptyMeshColliderComponents(int numComponentsToAdd) {
		
		// Note: we have to intermediately clear the meshFilter.sharedMesh,
		// otherwise we get the warning message "Compute mesh inertia tensor
		// failed for one of the actor's mesh shapes" because the flat
		// sprite-quad is used for volume calculations.
		Transform targetNode = TargetNodeToAttachMeshCollider;
		
		MeshFilter meshFilter = null;
		Mesh mesh = null;
		Rigidbody rigidbody = targetNode.GetComponent<Rigidbody>();
		if (rigidbody != null) {
			meshFilter = targetNode.GetComponent<MeshFilter>();
			if (meshFilter != null) {
				mesh = meshFilter.sharedMesh;
				meshFilter.sharedMesh = null;
			}
		}
		
		List<MeshCollider> resultColliders = new List<MeshCollider>();
		for (int count = 0; count < numComponentsToAdd; ++count) {
			MeshCollider newCollider = targetNode.gameObject.AddComponent<MeshCollider>();
			resultColliders.Add(newCollider);
		}
		
		if (rigidbody != null && meshFilter != null) {
			meshFilter.sharedMesh = mesh;
		}
		return resultColliders.ToArray();
	}
	
	//-------------------------------------------------------------------------
	protected void InitWithPreferencesValues() {
		
		this.ColliderMeshDirectory = AlphaMeshColliderPreferences.Instance.DefaultColliderDirectory;
		this.mRegionIndependentParameters.LiveUpdate = AlphaMeshColliderPreferences.Instance.DefaultLiveUpdate;
		this.mRegionIndependentParameters.DefaultMaxPointCount = AlphaMeshColliderPreferences.Instance.DefaultColliderPointCount;
		this.mRegionIndependentParameters.Convex = AlphaMeshColliderPreferences.Instance.DefaultConvex;
		this.mRegionIndependentParameters.Thickness = AlphaMeshColliderPreferences.Instance.DefaultAbsoluteColliderThickness;
#if UNITY_4_3_AND_LATER
		this.mRegionIndependentParameters.TargetColliderType = AlphaMeshColliderPreferences.Instance.DefaultTargetColliderType;
#endif	
		mWasInitialized = true;
	}
	
	//-------------------------------------------------------------------------
	/// <returns>
	/// The collider mesh path including the directory and the filename plus extension.
	/// E.g. "Assets/Colliders/Generated/Island2_446_flipped_h.dae".
	/// </returns>
	public string ActiveFrameFullColliderMeshPath() {
		return mColliderMeshDirectory + "/" + mColliderMeshFilename;
	}

	//-------------------------------------------------------------------------
	/// <returns>
	/// The collider mesh path including the directory and the filename plus extension.
	/// E.g. "Assets/Colliders/Generated/Island2_446_flipped_h.dae".
	/// </returns>
	public string FirstFrameFullColliderMeshPath() {
		return mColliderMeshDirectory + "/" + GetFirstFrameColliderMeshFilename();
	}

	//-------------------------------------------------------------------------
	public void SetActiveColliderFrameIndex(int colliderFrameIndex) {
		mActiveColliderIndex = colliderFrameIndex;
		
#if UNITY_4_3_AND_LATER
		if (mAnimatedColliderParameters.IsColliderAnimationUsed) {
			if (mHasUnity43SpriteRendererComponent && mHasUnity43SpriteAnimatorComponent) {
				SetUnity43SpriteRendererToActiveFrameIndexSprite(ref mUnity43SpriteRendererComponent, mAnimatedColliderParameters, mActiveColliderIndex);
			}

			InitTextureParams(); // we need to update the filename according to the sprite's name
			UpdateColliderMeshFilename();

			EnableOnlyActiveColliderFrame();
		}
#endif
	}

	//-------------------------------------------------------------------------
	public void DecreaseActiveColliderFrameIndex() {
		int targetIndex = mActiveColliderIndex - 1;
		if (targetIndex < 0) {
			targetIndex = 0;
		}
		SetActiveColliderFrameIndex(targetIndex);
	}

	//-------------------------------------------------------------------------
	public void IncreaseActiveColliderFrameIndex() {
		int numFrames = this.NumColliderFrames;
		int targetIndex = mActiveColliderIndex + 1;
		if (targetIndex >= numFrames) {
			targetIndex = numFrames-1;
		}
		SetActiveColliderFrameIndex(targetIndex);
	}

	//-------------------------------------------------------------------------
	public void EnableAllColliderFrames() {
#if UNITY_4_3_AND_LATER
		if (mAnimatedColliderParameters.PolygonCollidersToSwitch != null) {
			for (int index = 0; index < mAnimatedColliderParameters.PolygonCollidersToSwitch.Length; ++index) {
				mAnimatedColliderParameters.PolygonCollidersToSwitch[index].enabled = true;
			}
		}
		if (mAnimatedColliderParameters.MeshCollidersToSwitch != null) {
			for (int index = 0; index < mAnimatedColliderParameters.MeshCollidersToSwitch.Length; ++index) {
				mAnimatedColliderParameters.MeshCollidersToSwitch[index].enabled = true;
			}
		}
#endif
	}

	//-------------------------------------------------------------------------
	public void EnableOnlyActiveColliderFrame() {

#if UNITY_4_3_AND_LATER
		if (mAnimatedColliderParameters.PolygonCollidersToSwitch != null) {
			for (int index = 0; index < mAnimatedColliderParameters.PolygonCollidersToSwitch.Length; ++index) {
				if (index != mActiveColliderIndex) {
					mAnimatedColliderParameters.PolygonCollidersToSwitch[index].enabled = false;
				}
				else {
					mAnimatedColliderParameters.PolygonCollidersToSwitch[index].enabled = true;
				}
			}
		}
		if (mAnimatedColliderParameters.MeshCollidersToSwitch != null) {
			for (int index = 0; index < mAnimatedColliderParameters.MeshCollidersToSwitch.Length; ++index) {
				if (index != mActiveColliderIndex) {
					mAnimatedColliderParameters.MeshCollidersToSwitch[index].enabled = false;
				}
				else {
					mAnimatedColliderParameters.MeshCollidersToSwitch[index].enabled = true;
				}
			}
		}
#endif
	}

	//-------------------------------------------------------------------------
	public void ReloadAllColliderFrames() {
		
		UpdateFromOldVersionForBackwardsCompatibility();
		
		int numFrames = mGeneratedColliderData.Length;
		bool cancelled = false;
		for (int colliderFrameIndex = 0; colliderFrameIndex < numFrames && !cancelled; ++colliderFrameIndex) {
			this.SetActiveColliderFrameIndex(colliderFrameIndex);

			string title = String.Format("Reloading Collider Frames - {0}", this.name);
			string message = String.Format("Reloading Frame {0} of {1}", colliderFrameIndex+1, numFrames);
			cancelled = EditorUtility.DisplayCancelableProgressBar(title, message, colliderFrameIndex / ((float)numFrames));

			try {
				ReloadCollider();
			}
			catch (Exception exc) {
				Debug.LogError(String.Format("Error: Caught exception while reloading {0}, collider frame {1} of {2}: {3}", this.name, colliderFrameIndex+1, numFrames, exc.Message));
			}

			if (cancelled) {
				Debug.Log(String.Format("Cancelled at Frame {0} of {1}", colliderFrameIndex+1, numFrames));
			}
		}
		EditorUtility.ClearProgressBar();
		this.SetActiveColliderFrameIndex(0);
	}

	//-------------------------------------------------------------------------
	public void RewriteAndReloadAllColliderFrames() {
		UpdateFromOldVersionForBackwardsCompatibility();
		
		int numFrames = mGeneratedColliderData.Length;
		bool cancelled = false;
		for (int colliderFrameIndex = 0; colliderFrameIndex < numFrames && !cancelled; ++colliderFrameIndex) {
			this.SetActiveColliderFrameIndex(colliderFrameIndex);

			string title = String.Format("Recalculating Collider Frames - {0}", this.name);
			string message = String.Format("Recalculating Frame {0} of {1}", colliderFrameIndex+1, numFrames);
			cancelled = EditorUtility.DisplayCancelableProgressBar(title, message, colliderFrameIndex / ((float)numFrames));

			try {
				RewriteAndReloadCollider();
			}
			catch (Exception exc) {
				Debug.LogError(String.Format("Error: Caught exception while rewriting {0}, collider frame {1} of {2}: {3}", this.name, colliderFrameIndex+1, numFrames, exc.Message));
			}
			
			if (cancelled) {
				Debug.Log(String.Format("Cancelled at Frame {0} of {1}", colliderFrameIndex+1, numFrames));
			}
		}
		EditorUtility.ClearProgressBar();
		this.SetActiveColliderFrameIndex(0);
	}

	//-------------------------------------------------------------------------
	public void RecalculateAllColliderFrames() {
		
		UpdateFromOldVersionForBackwardsCompatibility();

		int numFrames = mGeneratedColliderData.Length;
		bool cancelled = false;
		for (int colliderFrameIndex = 0; colliderFrameIndex < numFrames && !cancelled; ++colliderFrameIndex) {
			this.SetActiveColliderFrameIndex(colliderFrameIndex);

			string title = String.Format("Recalculating Collider Frames - {0}", this.name);
			string message = String.Format("Recalculating Frame {0} of {1}", colliderFrameIndex+1, numFrames);
			cancelled = EditorUtility.DisplayCancelableProgressBar(title, message, colliderFrameIndex / ((float)numFrames));

			try {
				bool succesfullyGenerated = GenerateAndStoreColliderMesh();
				if (!succesfullyGenerated) {
					SetEmptyCollider();
				}
				else {
					UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.Default);
					if (!LoadAlreadyGeneratedCollider()) {
						if (!LoadAlreadyGeneratedCollider()) {
							Debug.LogError("Unable to load the generated Collider Mesh '" + ActiveFrameFullColliderMeshPath() + "'!");
						}
					}
				}
			}
			catch (Exception exc) {
				Debug.LogError(String.Format("Error: Caught exception while recalculating {0}, collider frame {1} of {2}: {3}", this.name, colliderFrameIndex+1, numFrames, exc.Message));
			}

			if (cancelled) {
				Debug.Log(String.Format("Cancelled at Frame {0} of {1}", colliderFrameIndex+1, numFrames));
			}
		}
		EditorUtility.ClearProgressBar();
		this.SetActiveColliderFrameIndex(0);
	}
	
	//-------------------------------------------------------------------------
	public void RecalculateCollider() {
		
		UpdateFromOldVersionForBackwardsCompatibility();
		
		bool successfullyGenerated = GenerateAndStoreColliderMesh();
		if (!successfullyGenerated) {
			SetEmptyCollider();
		}
		else {
			UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.Default);
			if (!LoadAlreadyGeneratedCollider()) {
				if (!LoadAlreadyGeneratedCollider()) {
					Debug.LogError("Unable to load the generated Collider Mesh '" + ActiveFrameFullColliderMeshPath() + "'!");
				}
			}
		}
	}
	
	//-------------------------------------------------------------------------
	public void RecalculateColliderFromPreviousResult() {

		UpdateFromOldVersionForBackwardsCompatibility();

		if (mGeneratedColliderData[mActiveColliderIndex].mColliderRegions == null ||
		    mGeneratedColliderData[mActiveColliderIndex].mColliderRegions.Length == 0) {

			if (!GenerateUnreducedColliderMesh(ref mGeneratedColliderData[mActiveColliderIndex]) ) {
				Debug.LogError("Unable to generate the collider mesh '" + ActiveFrameFullColliderMeshPath() + "'!");
				return;
			}
		}

		bool storedSuccessfully = ReduceAndStoreColliderMesh(ref mGeneratedColliderData[mActiveColliderIndex]);
		if (!storedSuccessfully) {
			SetEmptyCollider();
		}
		else {
			UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.Default);
			
			if (!LoadAlreadyGeneratedCollider()) {
				if (!LoadAlreadyGeneratedCollider()) {
					Debug.LogError("Unable to load the generated Collider Mesh '" + ActiveFrameFullColliderMeshPath() + "'!");
				}
			}
		}
	}
	
	//-------------------------------------------------------------------------
	public void RewriteAndReloadCollider() {
		if (this.CanRewriteCollider) {
			GeneratedColliderData generatedColliderData = mGeneratedColliderData[mActiveColliderIndex];
			ExportMeshToFile(generatedColliderData);
			UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.Default);
		}
		
		if (!LoadAlreadyGeneratedCollider()) {
			if (!LoadAlreadyGeneratedCollider()) {
				Debug.LogError("Unable to load the generated Collider Mesh '" + ActiveFrameFullColliderMeshPath() + "'!");
			}
		}
	}
	
	//-------------------------------------------------------------------------
	static public void ReloadAllSimilarCollidersInScene(string colliderMeshPathToReload) {
#if UNITY_4_AND_LATER
		object[] alphaMeshColliders = GameObject.FindObjectsOfType(typeof(AlphaMeshCollider));
#else
		object[] alphaMeshColliders = GameObject.FindSceneObjectsOfType(typeof(AlphaMeshCollider));
#endif
		foreach (AlphaMeshCollider collider in alphaMeshColliders)
		{
			if (collider.FirstFrameFullColliderMeshPath().Equals(colliderMeshPathToReload)) {
				collider.ReloadCollider();
			}
		}
	}

	//-------------------------------------------------------------------------
	public void ReloadCollider() {
		bool alreadyGenerated = LoadAlreadyGeneratedCollider();
		if (!alreadyGenerated) {
			UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.Default);
			
			if (!LoadAlreadyGeneratedCollider()) {
				Debug.LogError("Unable to load the Collider Mesh '" + ActiveFrameFullColliderMeshPath() + "'!");
			}
		}
	}

	//-------------------------------------------------------------------------
	bool SetEmptyCollider() {
		bool wereAllSuccessful = true;
		CorrectColliderTypeToParameters();
#if UNITY_4_3_AND_LATER	
		if ((mRegionIndependentParameters.TargetColliderType == TargetColliderType.PolygonCollider2D)) {
			if (!SetEmpty2DPolygonCollider()) {
				wereAllSuccessful = false;
			}
		}
		else { // (mRegionIndependentParameters.TargetColliderType == TargetColliderType.MeshCollider)
#endif
			if (!SetEmptyMeshCollider()) {
				wereAllSuccessful = false;
			}
#if UNITY_4_3_AND_LATER
		}
#endif
		return wereAllSuccessful;
	}
	
	//-------------------------------------------------------------------------
	bool LoadAlreadyGeneratedCollider() {

		bool wereAllSuccessful = true;
		CorrectColliderTypeToParameters();
#if UNITY_4_3_AND_LATER	
		if ((mRegionIndependentParameters.TargetColliderType == TargetColliderType.PolygonCollider2D)) {
			if (!LoadAlreadyGeneratedColliderTo2DPolygonCollider()) {
				wereAllSuccessful = false;
			}
		}
		else { // (mRegionIndependentParameters.TargetColliderType == TargetColliderType.MeshCollider)
#endif
			if (!LoadAlreadyGeneratedColliderToMeshCollider()) {
				wereAllSuccessful = false;
			}
#if UNITY_4_3_AND_LATER
		}
#endif
		return wereAllSuccessful;
	}
	
#if UNITY_4_3_AND_LATER
	//-------------------------------------------------------------------------
	bool SetEmpty2DPolygonCollider() {
		PolygonCollider2D collider = this.TargetPolygonCollider2DComponent;
		collider.pathCount = 0;
		//collider.SetPath(0, null);
		return true;
	}

	//-------------------------------------------------------------------------
	bool LoadAlreadyGeneratedColliderTo2DPolygonCollider() {

		PolygonCollider2D collider = this.TargetPolygonCollider2DComponent;

		int numActiveIslands = 0;
		ColliderRegionData[] colliderRegions = mGeneratedColliderData[mActiveColliderIndex].mColliderRegions;
		for (int count = 0; count < colliderRegions.Length; ++count) {
			if (colliderRegions[count].mReducedOutlineVertices != null && colliderRegions[count].mReducedOutlineVertices.Count != 0) {
				++numActiveIslands;
			}
		}
		if (collider.pathCount != numActiveIslands) {
			collider.pathCount = numActiveIslands;
		}

		int activeIslandIndex = 0;
		for (int count = 0; count < colliderRegions.Length; ++count) {

			if (colliderRegions[count].mReducedOutlineVertices != null && colliderRegions[count].mReducedOutlineVertices.Count != 0) {

				Vector2[] transformedPolygon = new Vector2[colliderRegions[count].mReducedOutlineVertices.Count];
				TransformReducedOutline(colliderRegions[count].mReducedOutlineVertices, transformedPolygon);

				collider.SetPath(activeIslandIndex++, transformedPolygon);
			}
		}
		return true;
	}
#endif

	//-------------------------------------------------------------------------
	bool SetEmptyMeshCollider() {
		Mesh emptyColliderMesh = null;
		
		MeshCollider collider = this.TargetMeshColliderComponent;
		if (collider == null) {
			EnsureHasMeshColliderComponents(mRegionIndependentParameters.NumCollidersNeeded);
			collider = this.TargetMeshColliderComponent;
			if (collider == null) {
				Debug.LogError("Error: Failed to add MeshCollider component!");
				return false;
			}
		}
		
		collider.sharedMesh = null;
		collider.sharedMesh = emptyColliderMesh;
		
		return true;
	}

	//-------------------------------------------------------------------------
	bool LoadAlreadyGeneratedColliderToMeshCollider() {

		Mesh loadedColliderMesh = (Mesh) UnityEditor.AssetDatabase.LoadAssetAtPath(ActiveFrameFullColliderMeshPath(), typeof(Mesh));
		if (loadedColliderMesh == null)
			return false; // unable to load the collider mesh.
		
		MeshCollider collider = this.TargetMeshColliderComponent;
		if (collider == null) {
			EnsureHasMeshColliderComponents(mRegionIndependentParameters.NumCollidersNeeded);
			collider = this.TargetMeshColliderComponent;
			if (collider == null) {
				Debug.LogError("Error: Failed to add MeshCollider component!");
				return false;
			}
		}

		collider.sharedMesh = null;
		collider.sharedMesh = loadedColliderMesh;
		
		return true;
	}

	//-------------------------------------------------------------------------
	public void CorrectColliderTypeToParameters() {
#if UNITY_4_3_AND_LATER	
		if (mRegionIndependentParameters.TargetColliderType == TargetColliderType.PolygonCollider2D) {
			EnsureHasPolygonCollider2DComponents(mRegionIndependentParameters.NumCollidersNeeded);
		}
		else if (mRegionIndependentParameters.TargetColliderType == TargetColliderType.MeshCollider) {
#endif
			EnsureHasMeshColliderComponents(mRegionIndependentParameters.NumCollidersNeeded);
#if UNITY_4_3_AND_LATER
		}
		else {
			EnsureHasNoColliderComponent(); // won't happen, but the code is there.
		}
#endif
	}

#if UNITY_4_3_AND_LATER
	//-------------------------------------------------------------------------
	public bool ReassignCollidersIfNeeded() {

		this.ReloadAllColliderFrames();
		return true;
		/*if (mRegionIndependentParameters.TargetColliderType == TargetColliderType.PolygonCollider2D) {
			return LoadAlreadyGeneratedColliderTo2DPolygonCollider();
		}
		else {
			return false;
		}*/
	}

	//-------------------------------------------------------------------------
	public void RemoveExistingPolygonCollider2DComponents() {
		PolygonCollider2D[] colliders = TargetNodeToAttachMeshCollider.GetComponents<PolygonCollider2D>();
		if (colliders != null) {
			for (int count = 0; count < colliders.Length; ++count) {
				DestroyImmediate(colliders[count]);
			}
		}
		mAnimatedColliderParameters.PolygonCollidersToSwitch = null;
	}
#endif
	
	//-------------------------------------------------------------------------
	public void RemoveExistingMeshColliderComponents() {
		MeshCollider[] colliders = TargetNodeToAttachMeshCollider.GetComponents<MeshCollider>();
		if (colliders != null) {
			for (int count = 0; count < colliders.Length; ++count) {
				DestroyImmediate(colliders[count]);
			}
		}
		mAnimatedColliderParameters.MeshCollidersToSwitch = null;
	}
	
	//-------------------------------------------------------------------------
	public void UpdateColliderMeshFilename() {
		mColliderMeshFilename = GetColliderMeshFilename();
	}
	
	//-------------------------------------------------------------------------
	void CheckForOTSpriteComponent(out bool hasOTSpriteComponent, out Component otSpriteComponent) {
		otSpriteComponent = this.GetComponent("OTSprite");
		if (otSpriteComponent != null) {
			hasOTSpriteComponent = true;
		}
		else {
			hasOTSpriteComponent = false;
		}
	}
	
	//-------------------------------------------------------------------------
	void CheckForOTTilesSpriteComponent(out bool hasOTTilesSpriteComponent, out Component otTilesSpriteComponent) {
		otTilesSpriteComponent = this.GetComponent("OTTilesSprite");
		if (otTilesSpriteComponent != null) {
			hasOTTilesSpriteComponent = true;
		}
		else {
			hasOTTilesSpriteComponent = false;
		}
	}
	
	//-------------------------------------------------------------------------
	void CheckForSmoothMovesSpriteComponent(out bool hasSmoothMovesSpriteComponent, out Component smoothMovesSpriteComponent) {
		//smoothMovesSpriteComponent = this.GetComponent("Sprite"); // this seems to cause problems, potentially clashing with SpriteManager2's Sprite class.
		smoothMovesSpriteComponent = null;
		Component[] allComponents = this.GetComponents(typeof(Component));
		foreach (Component currentComponent in allComponents) {
			string typeName = currentComponent.GetType().Name;
			string typeNameSpace = currentComponent.GetType().Namespace;
			if (typeName.Equals("Sprite") && typeNameSpace.Equals("SmoothMoves")) {
				smoothMovesSpriteComponent = currentComponent;
				break;
			}
		}
		
		if (smoothMovesSpriteComponent != null) {
			Type spriteComponentType = smoothMovesSpriteComponent.GetType();
			FieldInfo fieldTextureGUID = spriteComponentType.GetField("textureGUID");
			FieldInfo fieldAtlas = spriteComponentType.GetField("atlas");
			FieldInfo fieldPivotOffsetOverride = spriteComponentType.GetField("pivotOffsetOverride");
			
			if (fieldTextureGUID != null &&
				fieldAtlas != null &&
				fieldPivotOffsetOverride != null) {
				
				hasSmoothMovesSpriteComponent = true;
			}
			else {
				hasSmoothMovesSpriteComponent = false;
			}
		}
		else {
			hasSmoothMovesSpriteComponent = false;
		}
	}
	
	//-------------------------------------------------------------------------
	void CheckForSmoothMovesBoneAnimationParent(out InitState hasSmoothMovesBoneAnimationParent,
												out Component smoothMovesBoneAnimationParent,
												out string fullNodeString) {
		string tempNodeString = "";
		smoothMovesBoneAnimationParent = FindBoneAnimationParent(this.transform, ref tempNodeString);
		if (smoothMovesBoneAnimationParent != null) {
			hasSmoothMovesBoneAnimationParent = InitState.Yes;
			fullNodeString = tempNodeString;
			return;
		}
		else {
			hasSmoothMovesBoneAnimationParent = InitState.No;
			fullNodeString = null;
			return;
		}
	}
	
	//-------------------------------------------------------------------------
	void CheckForSmoothMovesAnimBoneColliderComponent(out bool hasSmoothMovesAnimBoneColliderComponent,
													  out Component smoothMovesAnimBoneColliderComponent,
													  out Component smoothMovesBoneAnimation,
													  out string nodeHierarchyString) {
		
		smoothMovesAnimBoneColliderComponent = this.GetComponent("AnimationBoneCollider");
		
		if (smoothMovesAnimBoneColliderComponent != null) {
			Type componentType = smoothMovesAnimBoneColliderComponent.GetType();
			FieldInfo fieldBoneAnimation = componentType.GetField("_boneAnimation", BindingFlags.NonPublic | BindingFlags.Instance);
			
			if (fieldBoneAnimation != null) {
				string tempNodeString = "";
				smoothMovesBoneAnimation = FindBoneAnimationParent(smoothMovesAnimBoneColliderComponent.transform, ref tempNodeString);
				nodeHierarchyString = tempNodeString;
				
				if (smoothMovesBoneAnimation != null) {
					hasSmoothMovesAnimBoneColliderComponent = true;
					return;
				}
			}
		}
		smoothMovesAnimBoneColliderComponent = null;
		smoothMovesBoneAnimation = null;
		hasSmoothMovesAnimBoneColliderComponent = false;
		nodeHierarchyString = null;
	}
	
	//-------------------------------------------------------------------------
	void CheckForTK2DSpriteComponent(out bool hasTK2DSpriteComponent, out Component tk2dSpriteComponent) {
		tk2dSpriteComponent = this.GetComponent("tk2dSprite");
		
		if (tk2dSpriteComponent != null) {
			Type componentType = tk2dSpriteComponent.GetType();
			FieldInfo fieldSpriteId = componentType.GetField("_spriteId", BindingFlags.Instance | BindingFlags.NonPublic);
			
			if (fieldSpriteId != null) {
				hasTK2DSpriteComponent = true;
				return;
			}
		}
		
		hasTK2DSpriteComponent = false;
		tk2dSpriteComponent = null;
	}
	
	//-------------------------------------------------------------------------
	static Component FindBoneAnimationParent(Transform searchStartNode, ref string nodeHierarchyString) {
		Transform boneNode = searchStartNode;
		nodeHierarchyString = "";
		Component result = boneNode.GetComponent("BoneAnimation");
		
		while (result == null && boneNode.parent != null) {
			
			if (nodeHierarchyString == "")
				nodeHierarchyString = boneNode.name;
			else
				nodeHierarchyString = boneNode.name + "/" + nodeHierarchyString;
			
			boneNode = boneNode.parent;
			result = boneNode.GetComponent("BoneAnimation");
		}
		
		if (result == null) {
			nodeHierarchyString = null;
		}
		return result;
	}
	
	//-------------------------------------------------------------------------
	static bool EnsureOTSpriteCustomPhysicsMode(Component otSpriteComponent) {
		Type otSpriteType = otSpriteComponent.GetType();
		
		FieldInfo fieldPhysics = otSpriteType.GetField("_physics");
		if (fieldPhysics == null) {
			Debug.LogError("Detected a missing '_physics' member variable at an OTSprite component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		object enumValue = fieldPhysics.GetValue(otSpriteComponent);
		Type enumType = enumValue.GetType();
		FieldInfo enumValueCustomPhysics = enumType.GetField("Custom");
		if (enumValueCustomPhysics == null) {
			Debug.LogError("Detected a missing 'Custom' member variable at an OTSprite component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		int customPhysicsIntValue = (int)enumValueCustomPhysics.GetValue(enumType);
		int oldPhysicsIntValue = (int) enumValue;
		
		bool wasCustomPhysicsBefore = (oldPhysicsIntValue == customPhysicsIntValue);
		if (!wasCustomPhysicsBefore) {
			object newEnumValue = Enum.ToObject(enumType, customPhysicsIntValue);
			fieldPhysics.SetValue(otSpriteComponent, newEnumValue);
		}
		return true;
	}
	
	//-------------------------------------------------------------------------
	static public bool CreateColliderPrefabsForAllOTContainerFrames(Component otSprite, AlphaMeshCollider referenceParameters, out List<GameObject> colliderPrefabAtTileIndex, out Vector2 atlasFrameSizeInPixels) {
	
		colliderPrefabAtTileIndex = new List<GameObject>();
		atlasFrameSizeInPixels = Vector2.one;
		
		object otSpriteContainer;
		Texture2D texture;
		GetSpriteContainerAndTextureOfOTSprite(otSprite, out otSpriteContainer, out texture);
		string groupNodeName = GetTileColliderPrefabGroupNodeName(texture.name);
		
		// add a GameObject node to group the collider prefabs
		GameObject collidersNode = new GameObject(groupNodeName);
		collidersNode.transform.parent = null;
		collidersNode.transform.localPosition = Vector3.zero;
		collidersNode.transform.localScale = Vector3.one;
#if UNITY_4_AND_LATER
		collidersNode.SetActive(true);
#else
		collidersNode.active = true; // we need to keep this root-node active in order to find it via GameObject.Find().
#endif
		
		// framesArray = otSpriteContainer.frames
		Type containerType = otSpriteContainer.GetType();
		FieldInfo fieldFrames = containerType.BaseType.GetField("frames", BindingFlags.NonPublic | BindingFlags.Instance);
		if (fieldFrames == null) {
			Debug.LogWarning("Failed to read frames field of the OTContainer component. Seems as if a different version of Orthello is used. If you need texture- or sprite-atlas support, please consider updating your Orthello framework.");
			return false;
		}
		IEnumerable framesArray = (IEnumerable) fieldFrames.GetValue(otSpriteContainer);
		bool areAllSuccessful = true;
		Vector2 frameSize;
		foreach (object frame in framesArray) {
			
			int tileIndex;
			GameObject colliderForFrame = CreateColliderPrefabForOTContainerFrame(referenceParameters, out tileIndex, out frameSize, collidersNode.transform, frame, texture);
			if (colliderForFrame == null) {
				areAllSuccessful = false;
			}
			else {
				while (colliderPrefabAtTileIndex.Count < tileIndex + 1) {
					colliderPrefabAtTileIndex.Add(null);
				}
				colliderPrefabAtTileIndex[tileIndex] = colliderForFrame;
				atlasFrameSizeInPixels = frameSize;
			}
		}
		return areAllSuccessful;
	}
	
	
	
	//-------------------------------------------------------------------------
	static public string GetTileColliderPrefabGroupNodeName(Component otSprite) {
		
		object otSpriteContainer;
		Texture2D texture;
		GetSpriteContainerAndTextureOfOTSprite(otSprite, out otSpriteContainer, out texture);
		if (texture == null) {
			return null;
		}
		return GetTileColliderPrefabGroupNodeName(texture.name);
	}
	
	//-------------------------------------------------------------------------
	static public string GetTileColliderPrefabGroupNodeName(string textureName) {
		
		return "AlphaMeshColliders Tileset " + textureName;
	}
	
	//-------------------------------------------------------------------------
	static protected bool GetSpriteContainerAndTextureOfOTSprite(Component otSprite, out object otSpriteContainer, out Texture2D texture) {
		
		otSpriteContainer = null;
		texture = null;
		
		Type otSpriteType = otSprite.GetType();
	
		// otSpriteContainer = otSprite._spriteContainer
		FieldInfo fieldSpriteContainer = otSpriteType.GetField("_spriteContainer");
		if (fieldSpriteContainer == null) {
			Debug.LogWarning("Failed to read _spriteContainer field of the OTSprite component. Seems as if a different version of Orthello is used. If you need texture- or sprite-atlas support, please consider updating your Orthello framework.");
			return false;
		}
		otSpriteContainer = fieldSpriteContainer.GetValue(otSprite);
		if (otSpriteContainer == null) {
			Debug.LogWarning("Unexpected state: _spriteContainer field of the OTSprite component is null. Seems as if a different version of Orthello is used. If you need texture- or sprite-atlas support, please consider updating your Orthello framework.");
			return false;
		}
		Type containerType = otSpriteContainer.GetType();
		// texture = otSpriteContainer._texture
		FieldInfo fieldTexture = containerType.GetField("_texture", BindingFlags.NonPublic | BindingFlags.Instance);
		if (fieldTexture == null) {
			Debug.LogWarning("Failed to read '_texture' field of the OTContainer component. Seems as if a different version of Orthello is used. If you need texture- or sprite-atlas support, please consider updating your Orthello framework.");
			return false;
		}
		texture = (Texture2D) fieldTexture.GetValue(otSpriteContainer);
		if (texture == null) {
			Debug.LogWarning("Unexpected state: '_texture' field of the OTContainer component is null. Seems as if a different version of Orthello is used. If you need texture- or sprite-atlas support, please consider updating your Orthello framework.");
			return false;
		}
		return true;
	}
	
	//-------------------------------------------------------------------------
	static public GameObject CreateColliderPrefabForOTContainerFrame(AlphaMeshCollider referenceParameters, out int tileIndex, out Vector2 resultFrameSize, Transform parentNode, object otContainerFrame, Texture2D texture) {
		
		tileIndex = 0;
		resultFrameSize = Vector2.one;
		
		Type containerFrameType = otContainerFrame.GetType();
		// tileIndex = frame.index
		FieldInfo fieldIndex = containerFrameType.GetField("index");
		if (fieldIndex == null) {
			Debug.LogError("Detected a missing 'index' member variable at an OTContainer.Frame object - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return null;
		}
		tileIndex = (int) fieldIndex.GetValue(otContainerFrame);
					
		// string nameString = frame.name
		FieldInfo fieldName = containerFrameType.GetField("name");
		if (fieldName == null) {
			Debug.LogError("Detected a missing 'name' member variable at an OTContainer.Frame object - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return null;
		}
		string name = (string) fieldName.GetValue(otContainerFrame);
		// float rotation = frame.rotation
		FieldInfo fieldRotation = containerFrameType.GetField("rotation");
		float rotation = 0;
		if (fieldRotation == null) {
			Debug.LogWarning("Detected a missing 'rotation' member variable at an OTContainer.Frame object - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
		}
		else {
			rotation = (float) fieldRotation.GetValue(otContainerFrame);
		}
		
		// Vector2[] uvCoords = frame.uv
		FieldInfo fieldUV = containerFrameType.GetField("uv");
		if (fieldUV == null) {
			Debug.LogError("Detected a missing 'uv' member variable at an OTContainer.Frame object - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return null;
		}
		Vector2[] uvCoords = (Vector2[]) fieldUV.GetValue(otContainerFrame);
		Vector2 framePosition;
		GetSizeAndPositionFromOrthelloUVCoords(out framePosition, out resultFrameSize, uvCoords, texture.width, texture.height);
		
		GameObject colliderObject = CreateAlphaMeshColliderGameObject(parentNode, PREFAB_GAMEOBJECT_NAME_PREFIX + name, name, texture, framePosition, resultFrameSize, rotation);
		AlphaMeshCollider alphaMeshColliderComponent = colliderObject.GetComponent<AlphaMeshCollider>();
		if (referenceParameters) {
			alphaMeshColliderComponent.mRegionIndependentParameters.AlphaOpaqueThreshold = referenceParameters.mRegionIndependentParameters.AlphaOpaqueThreshold;
			alphaMeshColliderComponent.mRegionIndependentParameters.DefaultMaxPointCount = referenceParameters.mRegionIndependentParameters.DefaultMaxPointCount;
			alphaMeshColliderComponent.mRegionIndependentParameters.Thickness = referenceParameters.mRegionIndependentParameters.Thickness;
			alphaMeshColliderComponent.mRegionIndependentParameters.FlipHorizontal = referenceParameters.mRegionIndependentParameters.FlipHorizontal;
			alphaMeshColliderComponent.mRegionIndependentParameters.FlipVertical = referenceParameters.mRegionIndependentParameters.FlipVertical;
			alphaMeshColliderComponent.mRegionIndependentParameters.Convex = referenceParameters.mRegionIndependentParameters.Convex;
			alphaMeshColliderComponent.mRegionIndependentParameters.FlipInsideOutside = referenceParameters.mRegionIndependentParameters.FlipInsideOutside;
			alphaMeshColliderComponent.mColliderMeshDirectory = referenceParameters.mColliderMeshDirectory;
			alphaMeshColliderComponent.mGroupSuffix = referenceParameters.mGroupSuffix;
			alphaMeshColliderComponent.mRegionIndependentParameters.CustomRotation = referenceParameters.mRegionIndependentParameters.CustomRotation;
			alphaMeshColliderComponent.mRegionIndependentParameters.CustomScale = referenceParameters.mRegionIndependentParameters.CustomScale;
			alphaMeshColliderComponent.mRegionIndependentParameters.CustomOffset = referenceParameters.mRegionIndependentParameters.CustomOffset;
			alphaMeshColliderComponent.mWasInitialized = true;
		}
		else {
			Debug.Log("NULL reference params??"); // debug, remove!
		}
		alphaMeshColliderComponent.UpdateAlphaMeshCollider();
#if UNITY_4_AND_LATER
		colliderObject.SetActive(true);
#else
		colliderObject.active = false;
#endif
		
		return colliderObject;
	}
	
	
	//-------------------------------------------------------------------------
	static GameObject CreateAlphaMeshColliderGameObject(Transform parentNode, string gameObjectName, string customRegionName, Texture2D texture, Vector2 framePositionInPixels, Vector2 frameSizeInPixels, float frameRotation) {
		
		GameObject colliderNode = new GameObject(gameObjectName);
		colliderNode.transform.parent = parentNode;
		colliderNode.transform.localPosition = Vector3.zero;
		colliderNode.transform.localScale = Vector3.one;
		
		AlphaMeshCollider alphaMeshCollider = colliderNode.AddComponent<AlphaMeshCollider>();
		alphaMeshCollider.CustomTex = texture;
		alphaMeshCollider.SetCustomAtlasRegion(customRegionName, framePositionInPixels, frameSizeInPixels, frameRotation);
		return colliderNode;
	}
	
	//-------------------------------------------------------------------------
	public static string GetOTContainerFrameColliderPrefabFilePath(string atlasName, string frameName) {
		return DEFAULT_TILE_COLLIDER_PREFAB_OUTPUT_DIR + "/" + atlasName + "/" + frameName + ".prefab";
	}
	
	//-------------------------------------------------------------------------
	static void GetOTSpriteFlipParameters(Component otSpriteComponent, out bool flipHorizontal, out bool flipVertical) {
		Type otSpriteType = otSpriteComponent.GetType();
		
		FieldInfo fieldFlipHorizontal = otSpriteType.GetField("_flipHorizontal");
		FieldInfo fieldFlipVertical = otSpriteType.GetField("_flipVertical");
		if (fieldFlipHorizontal == null || fieldFlipVertical == null) {
			Debug.LogError("Detected a missing '_flipHorizontal' or '_flipVertical' member variable at an OTSprite component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			flipHorizontal = false;
			flipVertical = false;
			return;
		}
		
		flipHorizontal = (bool) fieldFlipHorizontal.GetValue(otSpriteComponent);
		flipVertical = (bool) fieldFlipVertical.GetValue(otSpriteComponent);
	}
	
	//-------------------------------------------------------------------------
	static void GetSmoothMovesSpriteDimensions(Component smoothMovesSpriteComponent, out Vector2 customScale, out Vector3 customOffset) {
		Type spriteType = smoothMovesSpriteComponent.GetType();
		
		FieldInfo fieldSize = spriteType.GetField("size");
		FieldInfo fieldBottomLeft = spriteType.GetField("_bottomLeft");
		if (fieldSize == null || fieldBottomLeft == null) {
			Debug.LogError("Detected a missing 'size' or '_bottomLeft' member variable at an OTSprite component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			customScale = Vector2.one;
			customOffset = Vector3.zero;
			return;
		}
		
		customScale = (Vector2) fieldSize.GetValue(smoothMovesSpriteComponent);
		Vector2 offset2D = (Vector2) fieldBottomLeft.GetValue(smoothMovesSpriteComponent);
		customOffset = new Vector3(offset2D.x + (customScale.x / 2), offset2D.y + (customScale.y / 2), 0);
	}
	
	//-------------------------------------------------------------------------
	void InitTextureParams() {
		bool mHasNoTextureAtGameObjectButSomewhereElse = (mHasSmoothMovesAnimBoneColliderComponent || (mHasSmoothMovesBoneAnimationParent == InitState.Yes && !mIsSmoothMovesNodeWithoutSprite));
		if (!mHasNoTextureAtGameObjectButSomewhereElse) {
			if (this.renderer && this.renderer.sharedMaterial) {
				mMainTex = (Texture2D) this.renderer.sharedMaterial.mainTexture;
			}
			else {
				mMainTex = null;
			}
		}

		mRegionIndependentParameters.NumCollidersNeeded = 1;
		mOutlineScale = Vector2.one;
		mOutlineOffset = Vector3.zero;
		
		mIsAtlasUsed = false;
		
		mInactiveBaseImageIsAtlas = false;
		mInactiveBaseImageWidth = 100;
		mInactiveBaseImageHeight = 100;
		if (mMainTex != null) {
			mInactiveBaseImageWidth = mMainTex.width;
			mInactiveBaseImageHeight = mMainTex.height;
		}
		
		mInactiveBaseImageOutlineScale = Vector2.one;
		mInactiveBaseImageOutlineOffset = Vector2.zero;
		
		if (mRegionIndependentParameters.CustomTex == null) {
			ReadNormalImageParametersFromComponents();
		}
		else {
			ReadCustomImageParametersFromComponents();
		}
		
		// set mCustomAtlasFrame.. params accordingly so that they have nice default values when enabling custom texture region extraction
		if (mRegionIndependentParameters.CustomAtlasFrameSizeInPixels == Vector2.zero) {
			if (mIsAtlasUsed) {
				mRegionIndependentParameters.CustomAtlasFramePositionInPixels = mAtlasFramePositionInPixels;
				mRegionIndependentParameters.CustomAtlasFrameSizeInPixels = mAtlasFrameSizeInPixels;
				mRegionIndependentParameters.CustomAtlasFrameRotation = mAtlasFrameRotation;
			}
			else {
				mRegionIndependentParameters.CustomAtlasFramePositionInPixels = Vector3.zero;
				mRegionIndependentParameters.CustomAtlasFrameSizeInPixels = new Vector2(mInactiveBaseImageWidth, mInactiveBaseImageHeight);
				mRegionIndependentParameters.CustomAtlasFrameRotation = 0.0f;
			}
		}
	}

	//-------------------------------------------------------------------------
	void ReadNormalImageParametersFromComponents() {
#if UNITY_4_3_AND_LATER
		if (mHasUnity43SpriteRendererComponent) {
			mIsAtlasUsed = ReadUnity43SpriteParams(mUnity43SpriteRendererComponent,  out mMainTex, out mAtlasFrameTitle, out mAtlasFrameIndex, out mAtlasFramePositionInPixels, out mAtlasFrameSizeInPixels, out mAtlasFrameRotation, out mOutlineScale, out mOutlineOffset);
		}
		if (mHasUnity43SpriteAnimatorComponent) {
			ReadUnity43SpriteAnimatorParams(mUnity43AnimatorComponent, ref mAnimatedColliderParameters, ref mRegionIndependentParameters);
		}
#endif
		if (mHasOTTileMapComponent) {
			mIsAtlasUsed = true;
			ReadOTTileMapParams(mOTTileMapComponent, mOTTileMapLayerIndex, mOTTileMapMapPosX, mOTTileMapMapPosY, out mMainTex, out mAtlasFrameIndex, out mAtlasFramePositionInPixels, out mAtlasFrameSizeInPixels, out mAtlasFrameRotation);
		}
		if (mHasOTSpriteComponent) {
			mIsAtlasUsed = ReadOTSpriteContainerParams(mOTSpriteComponent, out mAtlasFrameIndex, out mAtlasFramePositionInPixels, out mAtlasFrameSizeInPixels, out mAtlasFrameRotation);
		}
		if (mHasSmoothMovesSpriteComponent) {
			if (mRegionIndependentParameters.CopySmoothMovesSpriteDimensions) {
				GetSmoothMovesSpriteDimensions(mSmoothMovesSpriteComponent, out mOutlineScale, out mOutlineOffset);
			}
			mIsAtlasUsed = ReadSmoothMovesSpriteAtlasParams(mSmoothMovesSpriteComponent, UsedTexture, out mAtlasFrameIndex, out mAtlasFrameTitle, out mAtlasFramePositionInPixels, out mAtlasFrameSizeInPixels, out mAtlasFrameRotation);
		}
		if (mHasSmoothMovesAnimBoneColliderComponent) {
			mIsAtlasUsed = ReadSmoothMovesAnimatedSpriteAtlasParams(mFullSmoothMovesNodeString, mSmoothMovesBoneAnimation, out mMainTex, out mAtlasFrameTitle, out mAtlasFrameIndex, out mAtlasFramePositionInPixels, out mAtlasFrameSizeInPixels, out mAtlasFrameRotation, out mOutlineScale, out mOutlineOffset);
		}
		// TODO: get rid of the above code-branch, remove the old mHasSmoothMovesAnimBoneColliderComponent part.
		if (mHasSmoothMovesBoneAnimationParent == InitState.Yes) {
			mIsAtlasUsed = ReadSmoothMovesAnimatedSpriteAtlasParams(mFullSmoothMovesNodeString, mSmoothMovesBoneAnimation, out mMainTex, out mAtlasFrameTitle, out mAtlasFrameIndex, out mAtlasFramePositionInPixels, out mAtlasFrameSizeInPixels, out mAtlasFrameRotation, out mOutlineScale, out mOutlineOffset);
			if (!mIsAtlasUsed)
				mIsSmoothMovesNodeWithoutSprite = true;
			else
				mIsSmoothMovesNodeWithoutSprite = false;
		}
	}
	
	//-------------------------------------------------------------------------
	void ReadCustomImageParametersFromComponents() {
		int discardOutInt = 0;
		Texture2D discardOutTexture = null;
		Vector2 discardOutVector;
		string discardOutString;
		Vector2 frameSize = Vector2.zero;
		float frameRotation = 0.0f;
		
		
		if (mHasOTSpriteComponent) {
			mInactiveBaseImageIsAtlas = ReadOTSpriteContainerParams(mOTSpriteComponent, out discardOutInt, out discardOutVector, out frameSize, out frameRotation);
		}
		if (mHasSmoothMovesSpriteComponent) {
			if (mRegionIndependentParameters.CopySmoothMovesSpriteDimensions) {
				GetSmoothMovesSpriteDimensions(mSmoothMovesSpriteComponent, out mInactiveBaseImageOutlineScale, out mInactiveBaseImageOutlineOffset);
				mOutlineOffset = mInactiveBaseImageOutlineOffset;
			}
			mInactiveBaseImageIsAtlas = ReadSmoothMovesSpriteAtlasParams(mSmoothMovesSpriteComponent, mMainTex, out discardOutInt, out discardOutString, out discardOutVector, out frameSize, out frameRotation);
		}
		if (mHasSmoothMovesAnimBoneColliderComponent) {
			mInactiveBaseImageIsAtlas = ReadSmoothMovesAnimatedSpriteAtlasParams(mFullSmoothMovesNodeString, mSmoothMovesBoneAnimation, out discardOutTexture, out discardOutString, out discardOutInt, out discardOutVector, out frameSize, out frameRotation, out mInactiveBaseImageOutlineScale, out mInactiveBaseImageOutlineOffset);
			mOutlineOffset = mInactiveBaseImageOutlineOffset;
		}
		if (mHasSmoothMovesBoneAnimationParent == InitState.Yes) {
			mInactiveBaseImageIsAtlas = ReadSmoothMovesAnimatedSpriteAtlasParams(mFullSmoothMovesNodeString, mSmoothMovesBoneAnimation, out discardOutTexture, out discardOutString, out discardOutInt, out discardOutVector, out frameSize, out frameRotation, out mInactiveBaseImageOutlineScale, out mInactiveBaseImageOutlineOffset);
			mOutlineOffset = mInactiveBaseImageOutlineOffset;
		}
		
		if (mInactiveBaseImageIsAtlas) {
			bool isRotated90Degrees = frameRotation == 90.0f || frameRotation == 270.0f || frameRotation == -90.0f;
			if (!isRotated90Degrees) {
				mInactiveBaseImageWidth = (int) frameSize.x;
				mInactiveBaseImageHeight = (int) frameSize.y;
			}
			else {
				mInactiveBaseImageWidth = (int) frameSize.y;
				mInactiveBaseImageHeight = (int) frameSize.x;
			}
		}
	}
	
	//-------------------------------------------------------------------------
	bool ReadOTSpriteContainerParams(object otSprite, out int atlasFrameIndex, out Vector2 framePositionInPixels, out Vector2 frameSizeInPixels, out float frameRotation) {
		framePositionInPixels = frameSizeInPixels = Vector2.zero;
		frameRotation = 0.0f;
		atlasFrameIndex = 0;
		
		// Check if we use a texture atlas instead of a normal image.
		Type otSpriteType = otSprite.GetType();
		
		FieldInfo fieldSpriteContainer = otSpriteType.GetField("_spriteContainer");
		FieldInfo fieldframeIndex = otSpriteType.GetField("_frameIndex");
		if (fieldSpriteContainer == null || fieldframeIndex == null) {
			Debug.LogWarning("Failed to read _spriteContainer or _frameIndex field of the OTSprite component. Seems as if a different version of Orthello is used. If you need texture- or sprite-atlas support, please consider updating your Orthello framework.");
			return false;
		}
		System.Object otSpriteContainer = fieldSpriteContainer.GetValue(otSprite);
		atlasFrameIndex = (int)fieldframeIndex.GetValue(otSprite);
		
		if (otSpriteContainer != null) {
			// we have a texture atlas or sprite sheet attached.
			Type containerType = otSpriteContainer.GetType();
			FieldInfo fieldAtlasData = containerType.GetField("atlasData");
			FieldInfo fieldFramesXY = containerType.GetField("_framesXY");
			if (fieldAtlasData != null) {
				return ReadOTSpriteAtlasParams(otSpriteContainer, atlasFrameIndex, out framePositionInPixels, out frameSizeInPixels, out frameRotation);
			}
			else if (fieldFramesXY != null) {
				ReadOTSpriteSheetParams(otSpriteContainer, atlasFrameIndex, out framePositionInPixels, out frameSizeInPixels, out frameRotation);
			}
			else {
				Debug.LogWarning("_spriteContainer of OTSprite is neither of type OTSpriteContainer nor OTSpriteAtlas (neither 'atlasData' nor '_framesXY' members were found). Seems as if a different version of Orthello is used. If you need texture- or sprite-atlas support, please consider updating your Orthello framework.");
				return false;
			}
			
			return true;
		}
		return false;
	}
	
	//-------------------------------------------------------------------------
	bool ReadOTTileMapParams(object otTileMap, int otTileMapLayerIndex, int otTileMapMapPosX, int otTileMapMapPosY, out Texture2D mainTex, out int atlasFrameIndex, out Vector2 framePositionInPixels, out Vector2 frameSizeInPixels, out float frameRotation) {
		framePositionInPixels = frameSizeInPixels = Vector2.zero;
		frameRotation = 0.0f;
		atlasFrameIndex = 0;
		mainTex = null;
		
		System.Type otTileMapType = otTileMap.GetType();
		FieldInfo fieldLayers = otTileMapType.GetField("layers");
		if (fieldLayers == null) {
			Debug.LogError("Detected a missing 'layers' member variable at OTTileMap component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		Array layersArray = (Array) fieldLayers.GetValue(otTileMap);
		if (otTileMapLayerIndex >= layersArray.Length) {
			Debug.LogError("Error: found a layer index that is larger than the OTTileMap.layers array - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		object otTileMapLayer = layersArray.GetValue(otTileMapLayerIndex);
		System.Type otTileMapLayerType = otTileMapLayer.GetType();
		FieldInfo fieldTiles = otTileMapLayerType.GetField("tiles");
		if (fieldTiles == null) {
			Debug.LogError("Detected a missing 'tiles' member variable at OTTileMapLayer class - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		int[] tileIndices = (int[]) fieldTiles.GetValue(otTileMapLayer);
		int tileIndex = tileIndices[otTileMapMapPosY * mOTTileMapWidth +  otTileMapMapPosX];
		atlasFrameIndex = tileIndex;
		
		object tileSet = GetOTTileSetForTileIndex(otTileMap, tileIndex);
		
		// read OTTileSet.image (Texture)
		System.Type tileSetType = tileSet.GetType();
		FieldInfo fieldImage = tileSetType.GetField("image");
		if (fieldImage == null) {
			Debug.LogError("Detected a missing 'image' member variable at OTTileSet class - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		Texture2D texture = (Texture2D) fieldImage.GetValue(tileSet);
		mainTex = texture;
		
		// Own version of OTTileMap::GetUV(int tile), returns an array of 4 uv vectors.
		Vector2[] uvs = GetOTTileMapUVCoords(otTileMap, tileSet, tileIndex);
		GetSizeAndPositionFromOrthelloUVCoords(out framePositionInPixels, out frameSizeInPixels, uvs, texture.width, texture.height);
		
		// read OTTileMap.layers[0].rotation (int[])
		FieldInfo fieldRotation = otTileMapLayerType.GetField("rotation");
		if (fieldRotation == null) {
			// OK. This parameter is only present in newer Orthello versions.
		}
		else {
		    // we directly set the GameObject's transform eulerAngles value, since the object has nothing else attached.
			int[] rotationValues = (int[]) fieldRotation.GetValue(otTileMapLayer);
			int rotation = rotationValues[otTileMapMapPosY * mOTTileMapWidth +  otTileMapMapPosX];
			this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y, rotation);
		}
		return true;
	}
	
	//-------------------------------------------------------------------------
	public static void GetSizeAndPositionFromOrthelloUVCoords(out Vector2 position, out Vector2 size, Vector2[] uvCoords, int textureWidth, int textureHeight) {
		float normalizedX = uvCoords[0].x;
		float normalizedY = 1.0f - uvCoords[0].y;
		float normalizedWidth = uvCoords[2].x - uvCoords[0].x;
		float normalizedHeight = uvCoords[0].y - uvCoords[2].y;
		
		size = new Vector2(normalizedWidth * textureWidth, normalizedHeight * textureHeight);
		position = new Vector2(Mathf.Floor(normalizedX * textureWidth),
							   Mathf.Clamp(Mathf.Floor(normalizedY * textureHeight), 0, textureHeight-1));
	}
	
	//-------------------------------------------------------------------------
	// Note: In this method we manually search through the tile sets.
	//       It is needed because the tileSetLookup member variable is cleared
	//       when we would like to read from it.
	public static object GetOTTileSetForTileIndex(object otTileMap, int tileIndex) {
		System.Type otTileMapType = otTileMap.GetType();
		FieldInfo fieldTileSets = otTileMapType.GetField("tileSets");
		if (fieldTileSets == null) {
			Debug.LogError("Detected a missing 'tileSets' member variable at OTTileMap component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		Array tileSets = (Array) fieldTileSets.GetValue(otTileMap);
		int tileSetIndex = 0;
		object tileSet = null;
		for ( ; tileSetIndex < tileSets.Length; ++tileSetIndex) {
			object otTileSet = tileSets.GetValue(tileSetIndex);
			System.Type otTileSetType = otTileSet.GetType();
			FieldInfo fieldFirstGid = otTileSetType.GetField("firstGid"); // int
			FieldInfo fieldTilesXY = otTileSetType.GetField("tilesXY"); // Vector2
			int firstGid = (int) fieldFirstGid.GetValue(otTileSet);
			Vector2 tilesXY = (Vector2) fieldTilesXY.GetValue(otTileSet);
			int numTilesInSet = (int)(tilesXY.x * tilesXY.y);
			if ((firstGid <= tileIndex) && (tileIndex < firstGid + numTilesInSet)) {
				tileSet = tileSets.GetValue(tileSetIndex);
				return tileSet;
			}
		}
		return null;
	}
	
	//-------------------------------------------------------------------------
	// Note: This is a functional copy of OTTileMap.GetUV(int tile).
	//       It is needed because the tileSetLookup member variable is cleared
	//       when we would like to read from it, leading to an exception thrown
	//       in the GetUV() method.
	Vector2[] GetOTTileMapUVCoords(object otTileMap, object tileSet, int tileIndex) {
		int tile = tileIndex;
		
		// The following code does this through reflection:
        // int ty = (int)Mathf.Floor((float)(tile-ts.firstGid) / ts.tilesXY.x);
        // int tx = (tile-ts.firstGid+1) - (int)((float)ty * ts.tilesXY.x) - 1;
		System.Type otTileSetType = tileSet.GetType();
		FieldInfo fieldFirstGid = otTileSetType.GetField("firstGid"); // int
		FieldInfo fieldTilesXY = otTileSetType.GetField("tilesXY"); // Vector2
		int tsFirstGid = (int) fieldFirstGid.GetValue(tileSet);
		Vector2 tsTilesXY = (Vector2) fieldTilesXY.GetValue(tileSet);
		int ty = (int)Mathf.Floor((float)(tile-tsFirstGid) / tsTilesXY.x);
		int tx = (tile-tsFirstGid+1) - (int)((float)ty * tsTilesXY.x) - 1;
		
		// The following code does this through reflection:
		// float ux = (1f / ts.imageSize.x);
        // float uy = (1f / ts.imageSize.y);
        // float usx = ux *  ts.tileSize.x;
        // float usy = uy *  ts.tileSize.y;		
		FieldInfo fieldImageSize = otTileSetType.GetField("imageSize"); // Vector2
		FieldInfo fieldTileSize = otTileSetType.GetField("tileSize"); // Vector2
		if (fieldImageSize == null || fieldTileSize == null) {
			Debug.LogError("Detected a missing 'fieldImageSize' or 'fieldTileSize' member variable at OTTileSet class - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return null;
		}
		Vector2 tsImageSize = (Vector2) fieldImageSize.GetValue(tileSet);
		Vector2 tsTileSize = (Vector2) fieldTileSize.GetValue(tileSet);
		float ux = (1f / tsImageSize.x);
        float uy = (1f / tsImageSize.y);
        float usx = ux *  tsTileSize.x;
        float usy = uy *  tsTileSize.y;
        
        // float utx = (ux * tx); // this was a comment in the original code part.
		// The following code does this through reflection:
        //float utx = (ux * ts.margin)+(tx * usx);
		//if (tx>0)utx+=(tx * ts.spacing * ux);
		//
        //float uty = (uy * ts.margin)+(ty * usy);
		//if (ty>0)uty+=(ty * ts.spacing * uy);
		FieldInfo fieldMargin = otTileSetType.GetField("margin"); // int
		FieldInfo fieldSpacing = otTileSetType.GetField("spacing"); // int
		int tsMargin = 0;
		int tsSpacing = 0;
		if (fieldMargin == null || fieldSpacing == null) {
			Debug.LogError("Detected a missing 'fieldMargin' or 'fieldSpacing' member variable at OTTileSet class - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			// this is a non-fatal error, we continue with margin and spacing of 0.
		}
		else {
			tsMargin = (int) fieldMargin.GetValue(tileSet);
			tsSpacing = (int) fieldSpacing.GetValue(tileSet);
		}
		
		float utx = (ux * tsMargin)+(tx * usx);
		if (tx>0)utx+=(tx * tsSpacing * ux);
		float uty = (uy * tsMargin)+(ty * usy);
		if (ty>0)uty+=(ty * tsSpacing * uy);
		
		// Read otTileMap.reduceBleeding field.
		System.Type otTileMapType = otTileMap.GetType();
		FieldInfo fieldReduceBleeding = otTileMapType.GetField("reduceBleeding");
		bool tileMapReduceBleeding = true;
		if (fieldReduceBleeding == null) {
			Debug.LogError("Detected a missing 'reduceBleeding' member variable at OTTileMap component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			// this is a non-fatal error, we continue with reduceBleeding set to true.
		}
		else {
			tileMapReduceBleeding = (bool) fieldReduceBleeding.GetValue(otTileMap);
		}
		
		
		// create a tiny fraction (uv size / 25 )
		// that will be removed from the UV coords
		// to reduce bleeding.
		int dv = 25;
        float dx = usx / dv;
        float dy = usy / dv;
		if (!tileMapReduceBleeding)
		{
			dx = 0; dy = 0;
		}
		
		return new Vector2[] { 
            new Vector2(utx + dx,1 - uty - dy ), new Vector2(utx + usx - dx,1 - uty - dy), 
            new Vector2(utx + usx - dx ,1- uty - usy + dy), new Vector2(utx + dx,1 - uty - usy + dy) 
        };
	}
	
	//-------------------------------------------------------------------------
	bool ReadSmoothMovesSpriteAtlasParams(object sprite, Texture2D atlasTexture, out int frameIndex, out string frameTitle, out Vector2 framePositionInPixels, out Vector2 frameSizeInPixels, out float frameRotation) {
		bool isAtlasUsed = false;
		frameIndex = 0;
		frameTitle = null;
		framePositionInPixels = Vector2.zero;
		frameSizeInPixels = Vector2.zero;
		frameRotation = 0;
		
		Type spriteType = sprite.GetType();
		FieldInfo fieldTextureIndex = spriteType.GetField("_textureIndex");
		FieldInfo fieldAtlas = spriteType.GetField("atlas");
		object atlas = null;
		if (fieldTextureIndex == null || fieldAtlas == null) {
			Debug.LogError("Detected a missing '_textureIndex' or 'atlas' member variable at an OTSprite component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
		}
		else {
			// member found - it can still be set to null if no atlas is used, though.
			atlas = fieldAtlas.GetValue(sprite);
		}
		
		if (atlas != null) {
			frameIndex = (int) fieldTextureIndex.GetValue(sprite);
			frameTitle = GetTextureNameAtAtlasFrameIndex(atlas, frameIndex);
			
			Type atlasType = atlas.GetType();
			FieldInfo fieldUVs = atlasType.GetField("uvs");
			if (fieldUVs == null) {
				Debug.LogError("Detected a missing 'uvs' member variable at an altas of an OTSprite component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			}
			else {
				List<UnityEngine.Rect> uvList = (List<UnityEngine.Rect>) fieldUVs.GetValue(atlas);
				float normalizedX = uvList[mAtlasFrameIndex].x;
				float normalizedY = 1.0f - uvList[mAtlasFrameIndex].y;
				float normalizedWidth = uvList[mAtlasFrameIndex].width;
				float normalizedHeight = uvList[mAtlasFrameIndex].height;
				
				frameSizeInPixels = new Vector2(normalizedWidth * atlasTexture.width, normalizedHeight * atlasTexture.height);
				framePositionInPixels = new Vector2(normalizedX * atlasTexture.width,
													Mathf.Clamp((normalizedY * atlasTexture.height) - frameSizeInPixels.y, 0, atlasTexture.height-1));
				
				frameRotation = 0;
				isAtlasUsed = true;
			}
		}
		return isAtlasUsed;
	}
	
	//-------------------------------------------------------------------------
	string GetTextureNameAtAtlasFrameIndex(object atlas, int frameIndex) {
		
		if (frameIndex < 0) {
			return null;
		}
		Type atlasType = atlas.GetType();
		FieldInfo fieldTextureNames = atlasType.GetField("textureNames");
		if (fieldTextureNames == null) {
			Debug.LogError("Detected a missing 'textureNames' member variable at an altas of an OTSprite component - Is your Orthello package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return null;
		}
		IEnumerable textureNamesList = (IEnumerable) fieldTextureNames.GetValue(atlas);
		if (textureNamesList == null) {
			return null;
		}
			
		int index = 0;
		foreach (string textureName in textureNamesList) {
			if (index++ == frameIndex) {
				return textureName;
			}
		}
		return null;
	}
	
	//-------------------------------------------------------------------------
	void EnsureSmoothMovesBoneAnimHasRestoreComponent(Component smoothMovesBoneAnimation) {
		AlphaMeshColliderSmoothMovesRestore restoreComponent = smoothMovesBoneAnimation.GetComponent<AlphaMeshColliderSmoothMovesRestore>();
		if (restoreComponent == null) {
			smoothMovesBoneAnimation.gameObject.AddComponent<AlphaMeshColliderSmoothMovesRestore>();
		}
	}

#if UNITY_4_3_AND_LATER
	//-------------------------------------------------------------------------
	void EnsureHasUnity43AnimatedColliderComponents() {

		// Add the RuntimeAnimatedColliderSwitch component
		RuntimeAnimatedColliderSwitch switchComponent = this.GetComponent<RuntimeAnimatedColliderSwitch>();
		if (switchComponent == null) {
			switchComponent = this.gameObject.AddComponent<RuntimeAnimatedColliderSwitch>();
			if (switchComponent == null) {
				Debug.LogError("Error: Failed to add RuntimeAnimatedColliderSwitch component! Aborting animated collider generation.");
				return;
			}
		}
		switchComponent.ColliderIDStrings = mAnimatedColliderParameters.ColliderIDStrings;

		// Now make sure we have exactly the required amount of MeshCollider or PolygonCollider2D components
		int numCollidersNeeded = mRegionIndependentParameters.NumCollidersNeeded;
		if (mRegionIndependentParameters.TargetColliderType == TargetColliderType.MeshCollider) {
			mAnimatedColliderParameters.PolygonCollidersToSwitch = null;
			if (mAnimatedColliderParameters.MeshCollidersToSwitch == null || mAnimatedColliderParameters.MeshCollidersToSwitch.Length != numCollidersNeeded) {
				EnsureHasMeshColliderComponents(numCollidersNeeded);
			}
		}
		else if (mRegionIndependentParameters.TargetColliderType == TargetColliderType.PolygonCollider2D) {
			mAnimatedColliderParameters.MeshCollidersToSwitch = null;
			if (mAnimatedColliderParameters.PolygonCollidersToSwitch == null || mAnimatedColliderParameters.PolygonCollidersToSwitch.Length != numCollidersNeeded) {
				EnsureHasPolygonCollider2DComponents(numCollidersNeeded);
			}
		}
		switchComponent.PolygonCollidersToSwitch = mAnimatedColliderParameters.PolygonCollidersToSwitch;
		switchComponent.MeshCollidersToSwitch = mAnimatedColliderParameters.MeshCollidersToSwitch;
	}

	//-------------------------------------------------------------------------
	void EnsureDataIsPreparedForMultipleColliders() {
		int oldSize = mGeneratedColliderData.Length;
		int desiredSize = mRegionIndependentParameters.NumCollidersNeeded;
		if (oldSize != desiredSize) {
			Array.Resize(ref mGeneratedColliderData, desiredSize);
			for (int newIndexToInit = oldSize; newIndexToInit < desiredSize; ++newIndexToInit) {
				mGeneratedColliderData[newIndexToInit] = new GeneratedColliderData();
			}
		}
	}
#endif
	
	//-------------------------------------------------------------------------
	void EnsureOTTilesSpriteHasUpdateComponent(Component otTilesSprite) {
		AlphaMeshColliderUpdateOTTilesSpriteColliders updateComponent = otTilesSprite.GetComponent<AlphaMeshColliderUpdateOTTilesSpriteColliders>();
		if (updateComponent == null) {
			updateComponent = otTilesSprite.gameObject.AddComponent<AlphaMeshColliderUpdateOTTilesSpriteColliders>();
			updateComponent.SetOTTilesSprite(otTilesSprite);
		}
	}
	
	//-------------------------------------------------------------------------
	public void RecalculateCollidersForOTTilesSprite() {
		AlphaMeshColliderUpdateOTTilesSpriteColliders updateComponent = this.GetComponent<AlphaMeshColliderUpdateOTTilesSpriteColliders>();
		if (updateComponent != null) {
			updateComponent.RecreateAllTileColliderPrefabs();
		}
	}
	
	//-------------------------------------------------------------------------
	bool ReadSmoothMovesAnimatedSpriteAtlasParams(string fullTargetBoneName, Component smoothMovesBoneAnimation,
									      		  out Texture2D atlasImage, out string frameTitle, out int frameIndex, out Vector2 framePositionInPixels, out Vector2 frameSizeInPixels, out float frameRotation,
												  out Vector2 customScale, out Vector3 customOffset) {
		
		int boneIndex = 0;
		atlasImage = null;
		frameTitle = null;
		frameIndex = 0;
		framePositionInPixels = frameSizeInPixels = customOffset = Vector2.zero;
		frameRotation = 0.0f;
		customScale = Vector3.one;
		
		Type boneAnimType = smoothMovesBoneAnimation.GetType();
		mFullSmoothMovesAssemblyName = boneAnimType.Assembly.FullName;
		
		object animationData = GetSmoothMovesAnimationData(smoothMovesBoneAnimation);
		
		Type animationDataType = animationData.GetType();
		
		FieldInfo fieldBoneTransformPathsList = animationDataType.GetField("boneTransformPaths");
		if (fieldBoneTransformPathsList == null) {
			Debug.LogError("Detected a missing 'boneTransformPaths' member variable at SmoothMoves BoneAnimation component - Is your SmoothMoves package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		IEnumerable boneTransformPathsList = (IEnumerable) fieldBoneTransformPathsList.GetValue(animationData);
		int index = 0;
		foreach (string fullBoneName in boneTransformPathsList) {
			
			if (fullBoneName.Equals(fullTargetBoneName)) {
				boneIndex = index;
				break;
			}
			
			++index;
		}
		
		string shortBoneName = System.IO.Path.GetFileName(fullTargetBoneName);
		frameTitle = shortBoneName;
		
		float importScale = 1.0f;
		if (!mRegionIndependentParameters.ApplySmoothMovesScaleAnim) {
			FieldInfo fieldImportScale = animationDataType.GetField("importScale");
			if (fieldImportScale != null) {
				importScale = (float) fieldImportScale.GetValue(animationData);
			}
		}
		
		FieldInfo fieldBoneSourceArray = boneAnimType.GetField("mBoneSource");
		if (fieldBoneSourceArray == null) {
			Debug.LogError("Detected a missing 'mBoneSource' member variable at SmoothMoves BoneAnimation component - Is your SmoothMoves package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		IEnumerable boneSourceArray = (IEnumerable) fieldBoneSourceArray.GetValue(smoothMovesBoneAnimation);
		// the following lines actually do this:
		//   object boneSource = boneSourceArray[boneIndex];
		object boneSource = null;
		index = 0;
		foreach (object currentBoneSource in boneSourceArray) {
			if (index++ == boneIndex) {
				boneSource = currentBoneSource;
				break;
			}
		}
		
		Type boneSourceType = boneSource.GetType();
		FieldInfo fieldMaterialIndex = boneSourceType.GetField("materialIndex");
		if (fieldMaterialIndex == null) {
			Debug.LogError("Detected a missing 'materialIndex' member variable at SmoothMoves BoneAnimation component - Is your SmoothMoves package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		int materialIndex = (int) fieldMaterialIndex.GetValue(boneSource);
		if (materialIndex < 0) {
            // This branch is entered at a SmoothMoves node without a sprite (transform bone only).
            atlasImage = null;
            frameIndex = 0;
            framePositionInPixels = Vector2.zero;
            frameSizeInPixels = Vector2.zero;
            frameRotation = 0.0f;
            customScale = Vector3.one;
            customOffset = Vector2.zero;
			return false;
		}
		FieldInfo fieldBoneQuad = boneSourceType.GetField("boneQuad");
		if (fieldBoneQuad == null) {
			Debug.LogError("Detected a missing 'boneQuad' member variable at SmoothMoves BoneAnimation component - Is your SmoothMoves package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		object boneQuad = fieldBoneQuad.GetValue(boneSource);
		Type boneQuadType = boneQuad.GetType();
		FieldInfo fieldVertexIndices = boneQuadType.GetField("vertexIndices");
		if (fieldVertexIndices == null) {
			Debug.LogError("Detected a missing 'vertexIndices' member variable at SmoothMoves BoneAnimation component - Is your SmoothMoves package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		IEnumerable vertexIndices = (IEnumerable) fieldVertexIndices.GetValue(boneQuad);
		// the following lines actually do this:
		//   int uvIndexBottomLeft = vertexIndices[2];
		//   int uvIndexTopRight = vertexIndices[3];
		int uvIndexBottomLeft = 0;
		int uvIndexTopRight = 0;
		index = 0;
		foreach (int vertexIndex in vertexIndices) {
			if (index == 2) {
				uvIndexBottomLeft = vertexIndex;
			}
			else if (index == 3) {
				uvIndexTopRight = vertexIndex;
				break; // we are done after index 3.
			}
			++index;
		}
		
		FieldInfo fieldMaterialsArray = boneAnimType.GetField("mMaterials");
		if (fieldMaterialsArray == null) {
			Debug.LogError("Detected a missing 'mMaterials' member variable at SmoothMoves BoneAnimation component - Is your SmoothMoves package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		IEnumerable materialsArray = (IEnumerable) fieldMaterialsArray.GetValue(smoothMovesBoneAnimation);
		
		index = 0;
		UnityEngine.Material targetMaterial = null;
		foreach (UnityEngine.Material material in materialsArray) {
			if (index++ == materialIndex) {
				targetMaterial = material;
				break;
			}
		}
		
		atlasImage = (Texture2D) targetMaterial.mainTexture;
		
		FieldInfo fieldUVs = boneAnimType.GetField("mUVs");
		if (fieldUVs == null) {
			Debug.LogError("Detected a missing 'mUVs' member variable at SmoothMoves BoneAnimation component - Is your SmoothMoves package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		Vector2[] uvs = (Vector2[]) fieldUVs.GetValue(smoothMovesBoneAnimation);
		
		
		Vector2 textureSize = new Vector2(atlasImage.width, atlasImage.height);
		Vector2 customRegionScale = uvs[uvIndexTopRight] - uvs[uvIndexBottomLeft];
		if (customRegionScale.x == 0.0f || customRegionScale.y == 0.0f) {
			// At some nodes without sprites we have a zero-size dummy boneSource.
			atlasImage = null;
			frameIndex = 0;
			framePositionInPixels = frameSizeInPixels = customOffset = Vector2.zero;
			frameRotation = 0.0f;
			customScale = Vector3.one;
			return false;
		}
		
		customRegionScale.Scale(textureSize);
		
		frameSizeInPixels = customRegionScale;
		float normalizedX = uvs[uvIndexBottomLeft].x;
		float normalizedY = 1.0f - uvs[uvIndexBottomLeft].y;
		framePositionInPixels = new Vector2(normalizedX * textureSize.x,
											Mathf.Clamp((normalizedY * textureSize.y) - frameSizeInPixels.y, 0, textureSize.y-1));
		
		FieldInfo fieldVertices = boneAnimType.GetField("mVertices");
		if (fieldVertices == null) {
			Debug.LogError("Detected a missing 'mVertices' member variable at SmoothMoves BoneAnimation component - Is your SmoothMoves package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return false;
		}
		Vector3[] vertices = (Vector3[]) fieldVertices.GetValue(smoothMovesBoneAnimation);
		customScale = new Vector2(vertices[uvIndexTopRight].x - vertices[uvIndexBottomLeft].x,
								  vertices[uvIndexTopRight].y - vertices[uvIndexBottomLeft].y);
		customOffset = new Vector3(customScale.x * 0.5f, customScale.y * 0.5f, 0) + vertices[uvIndexBottomLeft];
		
		customScale *= importScale;
		customOffset *= importScale;
		
		frameRotation = 0.0f;
		//GetSmoothMovesAtlasPivotOffset(atlasImage, frameSizeInPixels, uvs[uvIndexBottomLeft], uvs[uvIndexTopRight], out frameIndex, out customOffset);
		//customOffset *= importScale;
		return true;
	}
	
	//-------------------------------------------------------------------------
	object GetSmoothMovesAnimationData(Component smoothMovesBoneAnimation) {
		Type boneAnimType = smoothMovesBoneAnimation.GetType();
		FieldInfo fieldBoneAnimationData = boneAnimType.GetField("animationData");
		if (fieldBoneAnimationData == null) {
			return GetSmoothMovesAnimationDataFromGUID(smoothMovesBoneAnimation);
		}
		object animationData = fieldBoneAnimationData.GetValue(smoothMovesBoneAnimation);
		if (animationData == null) {
			// newer version of SmoothMoves (v2.2.0 and up)
			return GetSmoothMovesAnimationDataFromGUID(smoothMovesBoneAnimation);
		}
		else {
			return animationData;
		}
	}
	
	//-------------------------------------------------------------------------
	object GetSmoothMovesAnimationDataFromGUID(Component smoothMovesBoneAnimation) {
		Type boneAnimType = smoothMovesBoneAnimation.GetType();
		FieldInfo fieldAnimationDataGUID = boneAnimType.GetField("animationDataGUID");
		if (fieldAnimationDataGUID == null) {
			Debug.LogError("Detected a missing 'animationDataGUID' member variable at SmoothMoves BoneAnimation component - Is your SmoothMoves package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return null;
		}
		string animationDataGUID = (string) fieldAnimationDataGUID.GetValue(smoothMovesBoneAnimation);
		if (string.IsNullOrEmpty(animationDataGUID)) {
			Debug.LogError("animationDataGUID member variable at SmoothMoves BoneAnimation component contains an empty string - Is your SmoothMoves package up to date? 2D ColliderGen might probably not work correctly with this version.");
			return null;
		}
		else {
			string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(animationDataGUID);
			if (assetPath == "") {
				Debug.LogError("No animation data object found in AssetDatabase for BoneAnimation.animationDataGUID - Is your SmoothMoves package up to date? 2D ColliderGen might probably not work correctly with this version.");
				return null;
			}
			
			if (mSmoothMovesBoneAnimationDataType == null) {
				mSmoothMovesBoneAnimationDataType = Type.GetType("SmoothMoves.BoneAnimationData, " + mFullSmoothMovesAssemblyName);
				if (mSmoothMovesBoneAnimationDataType == null) {
					mSmoothMovesBoneAnimationDataType = Type.GetType("SmoothMoves.BoneAnimationData, SmoothMoves_Runtime, Version=2.0.0.0, Culture=neutral, PublicKeyToken=null");
					if (mSmoothMovesBoneAnimationDataType == null) {
						mSmoothMovesBoneAnimationDataType = Type.GetType("SmoothMoves.TextureAtlas, SmoothMoves_Runtime, Version=2.2.0.0, Culture=neutral, PublicKeyToken=null");
					}
				}
				if (mSmoothMovesBoneAnimationDataType == null) {
					Debug.LogError("Unable to query SmoothMoves.BoneAnimationData type - Is your SmoothMoves package up to date? 2D ColliderGen might probably not work correctly with this version.");
					return null;
				}
				}
			
			UnityEngine.Object loadedBoneAnimationDataObject = UnityEditor.AssetDatabase.LoadAssetAtPath(assetPath, mSmoothMovesBoneAnimationDataType);
			if (loadedBoneAnimationDataObject == null) {
				Debug.LogError("Unable to query SmoothMoves.BoneAnimationData type - Is your SmoothMoves package up to date? 2D ColliderGen might probably not work correctly with this version.");
				return null;
			}
			else {
				return loadedBoneAnimationDataObject;
			}
		}
	}
	
	//-------------------------------------------------------------------------
	/*bool GetSmoothMovesAtlasPivotOffset(Texture2D atlasTexture, Vector2 frameSize, Vector2 uvBottomLeft, Vector2 uvTopRight, out int frameIndex, out Vector3 pivotOffset) {
		string texturePath = UnityEditor.AssetDatabase.GetAssetPath(atlasTexture);
		
		string atlasDescriptionPath = System.IO.Path.GetDirectoryName(texturePath) + "/" + System.IO.Path.GetFileNameWithoutExtension(texturePath) + ".asset";
		if (!System.IO.File.Exists(atlasDescriptionPath)) {
			pivotOffset = Vector3.zero;
			frameIndex = 0;
			return false;
		}
		
		mSmoothMovesAtlasType = Type.GetType("SmoothMoves.TextureAtlas, " + mFullSmoothMovesAssemblyName);
		if (mSmoothMovesAtlasType == null) {
			mSmoothMovesAtlasType = Type.GetType("SmoothMoves.TextureAtlas, SmoothMoves_Runtime, Version=1.10.1.0, Culture=neutral, PublicKeyToken=null");
			if (mSmoothMovesAtlasType == null) {
				mSmoothMovesAtlasType = Type.GetType("SmoothMoves.TextureAtlas, SmoothMoves_Runtime, Version=1.9.7.0, Culture=neutral, PublicKeyToken=null");
			}
		}
		
		UnityEngine.Object loadedAtlasObject = UnityEditor.AssetDatabase.LoadAssetAtPath(atlasDescriptionPath, mSmoothMovesAtlasType);
		if (loadedAtlasObject == null) {
			pivotOffset = Vector3.zero;
			frameIndex = 0;
			return false;
		}
		
		FieldInfo fieldUVs = mSmoothMovesAtlasType.GetField("uvs");
		if (fieldUVs == null) {
			Debug.LogError("Detected a missing 'mUVs' member variable at SmoothMoves BoneAnimation component - Is your SmoothMoves package up to date? 2D ColliderGen might probably not work correctly with this version.");
			frameIndex = 0;
			pivotOffset = Vector3.zero;
			return false;
		}
		IEnumerable uvsList = (IEnumerable) fieldUVs.GetValue(loadedAtlasObject);
		frameIndex = 0;
		int index = 0;
		foreach (Rect uvRect in uvsList) {
			if (Mathf.Approximately(uvRect.xMin, uvBottomLeft.x) &&
				Mathf.Approximately(uvRect.yMin, uvBottomLeft.y) &&
				Mathf.Approximately(uvRect.xMax, uvTopRight.x) &&
				Mathf.Approximately(uvRect.yMax, uvTopRight.y)) {
				
				frameIndex = index;
				break;
			}
			++index;
		}
		
		FieldInfo fieldPivotOffsetsArray = mSmoothMovesAtlasType.GetField("defaultPivotOffsets");
		if (fieldPivotOffsetsArray == null) {
			Debug.LogError("Detected a missing 'defaultPivotOffsets' member variable at SmoothMoves BoneAnimation component - Is your SmoothMoves package up to date? 2D ColliderGen might probably not work correctly with this version.");
			pivotOffset = Vector3.zero;
			return false;
		}
		IEnumerable pivotOffsetsArray = (IEnumerable) fieldPivotOffsetsArray.GetValue(loadedAtlasObject);
		index = 0;
		Vector2 normalizedOffset = Vector2.zero;
		foreach (Vector2 offset in pivotOffsetsArray) {
			if (index == frameIndex) {
				normalizedOffset = offset;
				break;
			}
			++index;
		}
		
		pivotOffset = new Vector3(-normalizedOffset.x * frameSize.x, -normalizedOffset.y * frameSize.y, 0);
		return true;
	}*/

	//-------------------------------------------------------------------------
	bool ReadOTSpriteAtlasParams(System.Object otSpriteContainer, int frameIndex, out Vector2 framePositionInPixels, out Vector2 frameSizeInPixels, out float frameRotation) {
		FieldInfo fieldAtlasData = otSpriteContainer.GetType().GetField("atlasData");
		if (fieldAtlasData == null) {
			Debug.LogWarning("Failed to access 'atlasData' member of the sprite atlas. Seems as if a different version of Orthello is used. If you need texture- or sprite-atlas support, please consider updating your Orthello framework.");
			framePositionInPixels = frameSizeInPixels = Vector2.zero;
			frameRotation = 0.0f;
		}
		Array atlasDataArray = (Array) fieldAtlasData.GetValue(otSpriteContainer);
		if (atlasDataArray == null) { // unlikely
			Debug.LogWarning("Failed to access 'atlasData' member of the sprite atlas as an array. Seems as if a different version of Orthello is used. If you need texture- or sprite-atlas support, please consider updating your Orthello framework.");
			framePositionInPixels = frameSizeInPixels = Vector2.zero;
			frameRotation = 0.0f;
			return false;
		}
   		System.Object atlasFrameData = atlasDataArray.GetValue(frameIndex);
		return GetOTAtlasDataFrameDimensions(atlasFrameData, out framePositionInPixels, out frameSizeInPixels, out frameRotation);
	}
	
	//-------------------------------------------------------------------------
	bool GetOTAtlasDataFrameDimensions(System.Object frameOTAtlasData, out Vector2 positionInPixels, out Vector2 sizeInPixels, out float rotation) {
		Type otAtlasDataType = frameOTAtlasData.GetType();
		
		FieldInfo fieldPosition = otAtlasDataType.GetField("position");
		FieldInfo fieldSize = otAtlasDataType.GetField("size");
		FieldInfo fieldRotated = otAtlasDataType.GetField("rotated");
		if (fieldPosition == null || fieldSize == null || fieldRotated == null) {
			Debug.LogWarning("Failed to read 'position' or 'size' or 'rotated' member(s) of OTSprite's sprite atlas frame. Seems as if a different version of Orthello is used. If you need texture- or sprite-atlas support, please consider updating your Orthello framework.");
			positionInPixels = sizeInPixels = Vector2.zero;
			rotation = 0.0f;
			return false;
		}
		positionInPixels = (Vector2) fieldPosition.GetValue(frameOTAtlasData);
		bool isRotated90DegreesCW = (bool) fieldRotated.GetValue(frameOTAtlasData);
		rotation = isRotated90DegreesCW ? 270.0f : 0.0f;
		sizeInPixels = (Vector2) fieldSize.GetValue(frameOTAtlasData);
		if (rotation != 0.0f && rotation != 180.0f) {
			sizeInPixels = new Vector2(sizeInPixels.y, sizeInPixels.x); // swap x and y.
		}
		return true;
	}

	//-------------------------------------------------------------------------
	bool ReadOTSpriteSheetParams(System.Object otSpriteContainer, int frameIndex, out Vector2 framePositionInPixels, out Vector2 frameSizeInPixels, out float frameRotation) {
		Type containerType = otSpriteContainer.GetType();
		FieldInfo fieldFramesXY = containerType.GetField("_framesXY");
		FieldInfo fieldFrameSize = containerType.GetField("_frameSize");
		if (fieldFramesXY == null || fieldFrameSize == null) {
			Debug.LogWarning("Failed to read '_framesXY' or '_frameSize' member(s) of OTSprite's sprite sheet. Seems as if a different version of Orthello is used. If you need texture- or sprite-atlas support, please consider updating your Orthello framework.");
			framePositionInPixels = frameSizeInPixels = Vector2.zero;
			frameRotation = 0.0f;
			return false;
		}
		Vector2 framesXY = (Vector2) fieldFramesXY.GetValue(otSpriteContainer);
		Vector2 frameSize = (Vector2) fieldFrameSize.GetValue(otSpriteContainer);
		int framesPerRow = (int) framesXY.x;
		
		int xIndex = frameIndex % framesPerRow;
		int yIndex = frameIndex / framesPerRow;
		
		framePositionInPixels = new Vector2(xIndex * frameSize.x, yIndex * frameSize.y);
		frameSizeInPixels = frameSize;
		frameRotation = 0.0f; // never has any rotation
		return true;
	}

#if UNITY_4_3_AND_LATER
	//-------------------------------------------------------------------------
	void CheckForUnity43SpriteComponent(out bool hasUnity43SpriteRendererComponent, out UnityEngine.SpriteRenderer unity43SpriteRendererComponent) {
		unity43SpriteRendererComponent = this.gameObject.GetComponent<UnityEngine.SpriteRenderer>();
		hasUnity43SpriteRendererComponent = (unity43SpriteRendererComponent != null);
	}

	//-------------------------------------------------------------------------
	void CheckForUnity43SpriteAnimatorComponent(out bool hasUnity43SpriteAnimatorComponent, out UnityEngine.Animator animatorComponent) {
		animatorComponent = this.gameObject.GetComponent<UnityEngine.Animator>();
		hasUnity43SpriteAnimatorComponent = (animatorComponent != null);
	}

	//-------------------------------------------------------------------------
	bool ReadUnity43SpriteParams(UnityEngine.SpriteRenderer unity43SpriteRenderer, out Texture2D texture, out string atlasFrameTitle, out int atlasFrameIndex, out Vector2 framePositionInPixels, out Vector2 frameSizeInPixels, out float frameRotation, out Vector2 outlineScale, out Vector3 outlineOffset) {

		texture = null;
		atlasFrameTitle = "";
		atlasFrameIndex = 0;
		framePositionInPixels = frameSizeInPixels = Vector2.zero;
		frameRotation = 0.0f;
		outlineScale = Vector2.one;
		outlineOffset = Vector3.zero;

		UnityEngine.Sprite sprite = unity43SpriteRenderer.sprite;
		if (sprite == null) {
			return false;
		}

		texture = sprite.texture;
		atlasFrameTitle = sprite.name;
		UnityEngine.Rect rect = sprite.rect;
		float yPositionBottomTopInvertedOrigin = texture.height - rect.y - rect.height;
		framePositionInPixels = new Vector2(Mathf.Clamp(rect.x, 0, texture.width-2), // -2 because we want a minimum region size of 1x1 pixels.
		                                    Mathf.Clamp(yPositionBottomTopInvertedOrigin, 0, texture.height-2));

		float remainingWidth = texture.width - framePositionInPixels.x;
		float remainingHeight = texture.height - framePositionInPixels.y; // when counting pixels from the top it's exactly <height> pixels space below.
		frameSizeInPixels = new Vector2(Mathf.Clamp(rect.width, 1, remainingWidth),
		                                Mathf.Clamp(rect.height, 1, remainingHeight));

		outlineScale = new Vector2(sprite.bounds.size.x, sprite.bounds.size.y);
	
		if (sprite.packed) {
			frameRotation = (sprite.packingRotation == SpritePackingRotation.Any) ? 270.0f : 0.0f;
		}

		float outlineOffsetX = 0.0f;
		float outlineOffsetY = 0.0f;
		// get pivot alignment
		if (sprite.bounds.center.x > 0) { // right
			outlineOffsetX = (outlineScale.x / 2);
		}
		else if (sprite.bounds.center.x < 0) { // left
			outlineOffsetX = -(outlineScale.x / 2);
		}
		//else: center, offset = 0.

		if (sprite.bounds.center.y > 0) { // top
			outlineOffsetY = (outlineScale.y / 2);
		}
		else if (sprite.bounds.center.y < 0) { // bottom
			outlineOffsetY = -(outlineScale.y / 2);
		}
		// else: center, offset = 0.
		outlineOffset = new Vector2(outlineOffsetX, outlineOffsetY);

		return true;
	}

	//-------------------------------------------------------------------------
	bool ReadUnity43SpriteAnimatorParams(UnityEngine.Animator unity43Animator, ref AnimatedColliderParameters animatedColliderParams, ref RegionIndependentParameters regionIndependentParams) {
		
		UnityEditorInternal.AnimatorController controller = (UnityEditorInternal.AnimatorController) unity43Animator.runtimeAnimatorController;
		if (!controller) {
			regionIndependentParams.NumCollidersNeeded = 1;
			return false;
		}
		// Note: we have to maintain the order to keep the spriteRef and spriteID indices linked.
		// Thus we don't use a HashSet for duplicate prevention but a list.Contains() call.
		List<string> spriteIDs = new List<string>();
		List<UnityEngine.Sprite> spriteFrames = new List<Sprite>();

		AnimationClip[] clips = AnimationUtility.GetAnimationClips(this.gameObject);
		for (int index = 0; index < clips.Length; ++index) {
			AnimationClip clip = clips[index];

			EditorCurveBinding[] bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
			if (bindings != null) {
				for (int bindingIndex = 0; bindingIndex < bindings.Length; ++bindingIndex) {
					EditorCurveBinding binding = bindings[bindingIndex];
					if (binding.isPPtrCurve) {
						ObjectReferenceKeyframe[] references = AnimationUtility.GetObjectReferenceCurve(clip, binding);
						for (int refIndex = 0; refIndex < references.Length; ++refIndex) {
							ObjectReferenceKeyframe reference = references[refIndex];
							UnityEngine.Sprite spriteRef = (UnityEngine.Sprite) reference.value;
							if (spriteRef != null) {
								string spriteID = spriteRef.name;
								if (!spriteIDs.Contains(spriteID)) {
									spriteFrames.Add(spriteRef);
									spriteIDs.Add(spriteID);
								}
							}
						}
					}
				}
			}
		}
		animatedColliderParams.SpriteFrames =  spriteFrames.ToArray();
		animatedColliderParams.ColliderIDStrings =  spriteIDs.ToArray();
		regionIndependentParams.NumCollidersNeeded = animatedColliderParams.ColliderIDStrings.Length;
		return true;
	}

	//-------------------------------------------------------------------------
	bool SetUnity43SpriteRendererToActiveFrameIndexSprite(ref UnityEngine.SpriteRenderer unity43SpriteRenderer, AnimatedColliderParameters animatedColliderParams, int spriteIndex) {
		if (animatedColliderParams == null || animatedColliderParams.SpriteFrames == null || animatedColliderParams.SpriteFrames.Length == 0) {
			return false;
		}
		unity43SpriteRenderer.sprite = animatedColliderParams.SpriteFrames[spriteIndex];
		return true;
	}
#endif
	
	//-------------------------------------------------------------------------
	bool GenerateAndStoreColliderMesh() {
		bool unreducedColliderGeneratedSuccessfully = GenerateUnreducedColliderMesh(ref mGeneratedColliderData[mActiveColliderIndex]);
		if (!unreducedColliderGeneratedSuccessfully) {
			return false;
		}
		
		bool storedSuccessfully = ReduceAndStoreColliderMesh(ref mGeneratedColliderData[mActiveColliderIndex]);
		if (!storedSuccessfully) {
			return false;
		}
		return true;
	}
	
	//-------------------------------------------------------------------------
	bool GenerateUnreducedColliderMesh(ref GeneratedColliderData generatedColliderData) {
		
		UpdateFromOldVersionForBackwardsCompatibility();

		// just in case the texture has changed.
		InitTextureParams();

#if UNITY_4_3_AND_LATER
		if (mHasUnity43SpriteAnimatorComponent) {
			EnsureHasUnity43AnimatedColliderComponents();
			EnsureDataIsPreparedForMultipleColliders();
		}
#endif
		
		if (mHasSmoothMovesAnimBoneColliderComponent || mHasSmoothMovesBoneAnimationParent == InitState.Yes) {
			EnsureSmoothMovesBoneAnimHasRestoreComponent(mSmoothMovesBoneAnimation);
		}
		
		if (UsedTexture == null) {
			return false;
		}
		
		UpdateColliderMeshFilename();

		if (generatedColliderData.mOutlineAlgorithm == null) {
			generatedColliderData.mOutlineAlgorithm = new PolygonOutlineFromImageFrontend();
		}
		PolygonOutlineFromImageFrontend outlineAlgorithm = generatedColliderData.mOutlineAlgorithm;
		
		bool useImageRegion = false;
		int regionX = 0;
		int regionY = 0;
		int regionWidth = UsedTexture.width;
		int regionHeight = UsedTexture.height;
		
		float unroundedRegionX = 0.0f;
		float unroundedRegionY = 0.0f;
		float unroundedWidth = 1.0f;
		float unroundedHeight = 1.0f;
		
		float pixelCutawayLeft = 0.0f;
		float pixelCutawayRight = 0.0f;
		float pixelCutawayBottom = 0.0f;
		float pixelCutawayTop = 0.0f;
		
		if (mRegionIndependentParameters.IsCustomAtlasRegionUsed || mIsAtlasUsed) {
			
			useImageRegion = true;
			if (mRegionIndependentParameters.IsCustomAtlasRegionUsed) {
				
				unroundedRegionX = mRegionIndependentParameters.CustomAtlasFramePositionInPixels.x;
				unroundedRegionY = mRegionIndependentParameters.CustomAtlasFramePositionInPixels.y;
				unroundedWidth = mRegionIndependentParameters.CustomAtlasFrameSizeInPixels.x;
				unroundedHeight = mRegionIndependentParameters.CustomAtlasFrameSizeInPixels.y;
			}
			else if (mIsAtlasUsed) { // mRegionIndependentParameters.IsCustomAtlasRegionUsed has priority over mIsAtlasUsed.
				
				unroundedRegionX = mAtlasFramePositionInPixels.x;
				unroundedRegionY = mAtlasFramePositionInPixels.y;
				unroundedWidth = mAtlasFrameSizeInPixels.x;
				unroundedHeight = mAtlasFrameSizeInPixels.y;
			}
		
			regionX = Mathf.FloorToInt(unroundedRegionX);
			regionY = Mathf.FloorToInt(unroundedRegionY);
			float unroundedEndX = unroundedRegionX + unroundedWidth;
			float unroundedEndY = unroundedRegionY + unroundedHeight;
			int endX = Mathf.CeilToInt(unroundedEndX);
			int endY = Mathf.CeilToInt(unroundedEndY);
			regionWidth =  endX - regionX;
			regionHeight = endY - regionY;
			
			pixelCutawayLeft = unroundedRegionX - regionX;
			pixelCutawayRight = endX - unroundedEndX;
			pixelCutawayBottom = endY - unroundedEndY;
			pixelCutawayTop = unroundedRegionY - regionY;
		}

		if (regionWidth == 0 || regionHeight == 0) {
			Debug.LogError("Error: Encountered image width or height of 0. Stopping collider generation.");
			return false;
		}
		
		bool wasSuccessful = outlineAlgorithm.BinaryAlphaThresholdImageFromTexture(out generatedColliderData.mBinaryImage, UsedTexture, mRegionIndependentParameters.AlphaOpaqueThreshold,
															   											  useImageRegion, regionX, regionY, regionWidth, regionHeight);
		if (!wasSuccessful) {
			Debug.LogError(outlineAlgorithm.LastError);
			return false;
		}

		IslandDetector.Region[] islands;
		IslandDetector.Region[] seaRegions;
		/*bool anyIslandsFound =*/ CalculateIslandStartingPoints(generatedColliderData.mBinaryImage, out islands, out seaRegions);
        /*if (!anyIslandsFound) { // we now tolerate if there is no island pixel, one could still enable the single sea-region.
            return false;
        }*/
		
		SetupColliderRegions(out generatedColliderData.mColliderRegions, islands, seaRegions);

		CopyOldColliderRegionParametersForBackwardsCompatibility();
		SetupColliderRegionParameters(ref mIslandRegionParameters, ref mSeaRegionParameters, mRegionIndependentParameters.DefaultMaxPointCount, islands, seaRegions);
		CopyOldPointCountParameterToFirstIslandForBackwardsCompatibility(ref mIslandRegionParameters, ref mMaxPointCount);
		
		bool ccwVertexOrder = IsOutlineInCCWOrderNecessary();
		outlineAlgorithm.RegionPixelCutawayLeft = pixelCutawayLeft;
		outlineAlgorithm.RegionPixelCutawayRight = pixelCutawayRight;
		outlineAlgorithm.RegionPixelCutawayBottom = pixelCutawayBottom;
		outlineAlgorithm.RegionPixelCutawayTop = pixelCutawayTop;
		outlineAlgorithm.NormalizeResultToCutRegion = true;
			
		CalculateUnreducedOutlineForAllColliderRegions(ref generatedColliderData.mColliderRegions, ref outlineAlgorithm, mRegionIndependentParameters, mIslandRegionParameters, mSeaRegionParameters, generatedColliderData.mBinaryImage, ccwVertexOrder);
        return true;
	}
	
	//-------------------------------------------------------------------------
	bool IsOutlineInCCWOrderNecessary() {
		bool ccwVertexOrder = !mRegionIndependentParameters.FlipInsideOutside;
		if (ScaleRequiresReverseVertexOrder()) {
			ccwVertexOrder = !ccwVertexOrder; // scaled -1 -> flip inside out, the vertex order changes when mirrored.
		}
		return ccwVertexOrder;
	}
	
	//-------------------------------------------------------------------------
	bool ScaleRequiresReverseVertexOrder() {
		float scaleX = GetOutputScaleX(false);
		float scaleY = GetOutputScaleY();
		if ((scaleX * scaleY) < 0.0f) { // scaleX is positive when normal (not using the collada x-scale inversion above).
			return true;
		}
		else {
			return false;
		}
	}
	
	//-------------------------------------------------------------------------
    /// <returns>True if at least one island is found, false otherwise.</returns>
	bool CalculateIslandStartingPoints(bool [,] binaryImage, out IslandDetector.Region[] islands, out IslandDetector.Region[] seaRegions) {
		int[,] islandClassificationImage = null;
		islands = null;
		seaRegions = null;
		
		mIslandDetector = new IslandDetector();
		mIslandDetector.DetectIslandsFromBinaryImage(binaryImage, out islandClassificationImage, out islands, out seaRegions);

        return (islands.Length > 0);
	}
	
	//-------------------------------------------------------------------------
	bool ReduceAndStoreColliderMesh(ref GeneratedColliderData generatedColliderData) {
		
		UpdateFromOldVersionForBackwardsCompatibility();
		
		if (generatedColliderData.mColliderRegions == null || generatedColliderData.mColliderRegions.Length == 0) {
			return false; // needs to be calculated before calling this method!
		}

		if (generatedColliderData.mOutlineAlgorithm == null) {
			generatedColliderData.mOutlineAlgorithm = new PolygonOutlineFromImageFrontend();
		}
		PolygonOutlineFromImageFrontend outlineAlgorithm = generatedColliderData.mOutlineAlgorithm;

		bool needsTriangleFenceUpdate = this.mRegionIndependentParameters.HasThicknessChanged;
		this.mRegionIndependentParameters.HasThicknessChanged = false;

		ColliderRegionData[] colliderRegions = generatedColliderData.mColliderRegions;
		int islandIndex = 0;
		int seaRegionIndex = 0;
		bool anyRegionWithVerticesFound = false;
		for (int count = 0; count < colliderRegions.Length; ++count) {

			ColliderRegionData region = colliderRegions[count];

			bool isIslandRegion = region.mRegionIsIsland;
			ColliderRegionParameters regionParameters = isIslandRegion ? mIslandRegionParameters[islandIndex] : mSeaRegionParameters[seaRegionIndex];
			if (isIslandRegion) {
				++islandIndex;
			}
			else {
				++seaRegionIndex;
			}

			if (region.mIntermediateOutlineVertices == null) {
				region.mResultVertices = null;
				region.mResultTriangleIndices = null;
				continue;
			}
			anyRegionWithVerticesFound = true;
			
			bool needsRegionReduction = true;
			if (region.mReducedOutlineVertices != null && region.mReducedOutlineVertices.Count != 0 &&
			    !regionParameters.RegionUpdateCalculationNeeded) {
				needsRegionReduction = false;
			}
			
			outlineAlgorithm.VertexReductionDistanceTolerance = this.mRegionIndependentParameters.VertexReductionDistanceTolerance;
			outlineAlgorithm.MaxPointCount = regionParameters.MaxPointCount;
			// TODO: replace this with a joint-convex hull implementation, just a workaround for now.
			bool allRegionsConvex = mRegionIndependentParameters.Convex;
			outlineAlgorithm.Convex = allRegionsConvex ? true : regionParameters.Convex;
			outlineAlgorithm.XOffsetNormalized = -0.5f;//-this.transform.localScale.x / 2.0f;
			outlineAlgorithm.YOffsetNormalized = -0.5f;//-this.transform.localScale.y / 2.0f;
			outlineAlgorithm.Thickness = this.mRegionIndependentParameters.Thickness;
			
			if (needsRegionReduction) {
				region.mReducedOutlineVertices = outlineAlgorithm.ReduceOutline(region.mIntermediateOutlineVertices, region.mOutlineVertexOrderIsCCW);
			}
			regionParameters.RegionUpdateCalculationNeeded = false;
        
			if (needsRegionReduction || needsTriangleFenceUpdate) {
				Vector3[] vertices;
				int[] triangleIndices;
			
				outlineAlgorithm.TriangleFenceFromOutline(out vertices, out triangleIndices, region.mReducedOutlineVertices, false);
			
				region.mResultVertices = vertices;
				region.mResultTriangleIndices = triangleIndices;
			}
		}

		if (!anyRegionWithVerticesFound) {
			return false; // we cannot/don't want to store a collada file with no vertices and import it back in, unfortunately.
		}

		bool needsToWriteMeshColliderFile = true;
#if UNITY_4_3_AND_LATER
		needsToWriteMeshColliderFile = (mRegionIndependentParameters.TargetColliderType == TargetColliderType.MeshCollider);
#endif
		if (needsToWriteMeshColliderFile) {
			return ExportMeshToFile(generatedColliderData);
		}
		else {
			return true; // done already
		}
	}
	
	//-------------------------------------------------------------------------
	bool ExportMeshToFile(GeneratedColliderData generatedColliderData) {
		
		ColladaExporter colladaWriter = new ColladaExporter();
		ColladaExporter.GeometryNode rootGeometryNode = new ColladaExporter.GeometryNode();
		
		Vector3[] jointVertices = null;
		int[] jointIndices = null;
		JoinVertexGroups(generatedColliderData.mColliderRegions, out jointVertices, out jointIndices);

		if (jointVertices == null || jointVertices.Length == 0) {
			CreateDummyTriangleToCreateAValidColladaFile(out jointVertices, out jointIndices);
		}
			
		rootGeometryNode.mName = "Collider";
		rootGeometryNode.mAreVerticesLeftHanded = true;
		rootGeometryNode.mVertices = jointVertices;
		rootGeometryNode.mTriangleIndices = jointIndices;
		rootGeometryNode.mGenerateNormals = true;

		float scaleX = GetOutputScaleX(true);
		float scaleY = GetOutputScaleY();
		
		colladaWriter.mVertexScaleAfterInitialRotation.x = scaleX; // the mesh is imported in a way that we end up correct this way.
		colladaWriter.mVertexScaleAfterInitialRotation.y = scaleY;
		colladaWriter.mVertexScaleAfterSecondRotation = Vector3.one;
		
		float atlasFrameRotation = mAtlasFrameRotation;
		if (mRegionIndependentParameters.CustomTex != null) {
			colladaWriter.mVertexScaleAfterInitialRotation.Scale(GetCustomImageScale());
			atlasFrameRotation = 0.0f;
		}
		
		// In order to rotate well, we need to compensate for the gameobject's
		// transform.scale that is applied automatically after all of our transforms.
		Vector3 automaticallyAppliedScale = this.transform.localScale;
		Vector3 rotationCompensationScaleBefore = new Vector3(automaticallyAppliedScale.x, automaticallyAppliedScale.y, 1.0f);
		Vector3 rotationCompensationScaleAfter = new Vector3(1.0f / automaticallyAppliedScale.x, 1.0f / automaticallyAppliedScale.y, 1.0f);
		colladaWriter.mVertexScaleAfterInitialRotation.Scale(rotationCompensationScaleBefore);
		colladaWriter.mVertexScaleAfterSecondRotation.Scale(rotationCompensationScaleAfter);
		
		colladaWriter.mVertexOffset.x = -mOutlineOffset.x -mRegionIndependentParameters.CustomOffset.x;
		colladaWriter.mVertexOffset.y = mOutlineOffset.y + mRegionIndependentParameters.CustomOffset.y;
		colladaWriter.mVertexOffset.z = -mOutlineOffset.z -mRegionIndependentParameters.CustomOffset.z;
		colladaWriter.mVertexTransformationCenter = new Vector3(0, 0, 0);
		colladaWriter.mVertexInitialRotationQuaternion = Quaternion.Euler(0, 0, -atlasFrameRotation);
		colladaWriter.mVertexSecondRotationQuaternion = Quaternion.Euler(0, 0,  -mRegionIndependentParameters.CustomRotation);
		
		System.IO.Directory.CreateDirectory(mColliderMeshDirectory);
		colladaWriter.ExportTriangleMeshToFile(ActiveFrameFullColliderMeshPath(), rootGeometryNode);
		return true;
	}

	//-------------------------------------------------------------------------
	static void CreateDummyTriangleToCreateAValidColladaFile(out Vector3[] jointVertices, out int[] jointIndices) {
		jointVertices = new Vector3[3];
		jointIndices = new int[3];

		jointVertices[0] = new Vector3(0, 0, 0);
		jointVertices[1] = new Vector3(1, 0, 0);
		jointVertices[2] = new Vector3(1, 1, 0);
		jointIndices[0] = 0;
		jointIndices[1] = 1;
		jointIndices[2] = 2;
	}
	
	//-------------------------------------------------------------------------
	static bool JoinVertexGroups(ColliderRegionData[] regions, out Vector3[] jointVertices, out int[] jointIndices) {
		
		int numVertices = 0;
		int numIndices = 0;
		for (int count = 0; count < regions.Length; ++count) {
		
			if (regions[count].mResultVertices == null || regions[count].mResultTriangleIndices == null) {
				continue;
			}
			numVertices += regions[count].mResultVertices.Length;
			numIndices += regions[count].mResultTriangleIndices.Length;
		}
		
		jointVertices = new Vector3[numVertices];
		jointIndices = new int[numIndices];
		int jointVertexIndex = 0;
		int jointIndexIndex = 0;
		
		int indexOffset = 0;
		for (int regionIndex = 0; regionIndex < regions.Length; ++regionIndex) {
		
			if (regions[regionIndex].mResultVertices == null || regions[regionIndex].mResultTriangleIndices == null) {
				continue;
			}
			
			Vector3[] regionVertices = regions[regionIndex].mResultVertices;
			int[] regionIndices = regions[regionIndex].mResultTriangleIndices;
			
			for (int regionVertexIndex = 0; regionVertexIndex < regionVertices.Length; ++regionVertexIndex) {
				jointVertices[jointVertexIndex++] = regionVertices[regionVertexIndex];
			}
			for (int regionIndexIndex = 0; regionIndexIndex < regionIndices.Length; ++regionIndexIndex) {
				jointIndices[jointIndexIndex++] = regionIndices[regionIndexIndex] + indexOffset;
			}
			
			indexOffset += regionVertices.Length;
		}
		
		return true;
	}

	//-------------------------------------------------------------------------
	/// <summary>
	/// Rotates and scales inputVertices and writes the result to the
	/// corresponding output array transformedVertices.
	/// </summary>
	/// <param name="inputVertices">Input vertices.</param>
	/// <param name="transformedVertices">Transformed vertices. Has to be of the same size as inputVertices.</param>
	void TransformReducedOutline(List<Vector2> inputVertices, Vector2[] transformedVertices) {

		// Order of vertex transformation is:
		// 1) rotated by initialRotationQuaternion around transformationCenter
		// 2) scaled by mVertexScaleAfterInitialRotation
		// 3) rotated by mVertexSecondRotationQuaternion around mVertexTransformationCenter
		// 4) scaled by mVertexScaleAfterSecondRotation
		// 3) translated by mVertexOffset

		float scaleX = GetOutputScaleX(false);
		float scaleY = GetOutputScaleY();
		
		Vector3 scaleAfterInitialRotation = new Vector3(scaleX, scaleY, 1);
		Vector3 scaleAfterSecondRotation = Vector3.one;
		
		float atlasFrameRotation = mAtlasFrameRotation;
		if (mRegionIndependentParameters.CustomTex != null) {
			scaleAfterInitialRotation.Scale(GetCustomImageScale());
			atlasFrameRotation = 0.0f;
		}

		// In order to rotate well, we need to compensate for the gameobject's
		// transform.scale that is applied automatically after all of our transforms.
		Vector3 automaticallyAppliedScale = this.transform.localScale;
		Vector3 rotationCompensationScaleBefore = new Vector3(automaticallyAppliedScale.x, automaticallyAppliedScale.y, 1.0f);
		Vector3 rotationCompensationScaleAfter = new Vector3(1.0f / automaticallyAppliedScale.x, 1.0f / automaticallyAppliedScale.y, 1.0f);
		scaleAfterInitialRotation.Scale(rotationCompensationScaleBefore);
		scaleAfterSecondRotation.Scale(rotationCompensationScaleAfter);

		Vector3 offset = new Vector3(mOutlineOffset.x + mRegionIndependentParameters.CustomOffset.x,
		                             mOutlineOffset.y + mRegionIndependentParameters.CustomOffset.y,
		                             mOutlineOffset.z + mRegionIndependentParameters.CustomOffset.z);
		Vector3 transformationCenter = new Vector3(0, 0, 0);
		Quaternion initialRotationQuaternion = Quaternion.Euler(0, 0, atlasFrameRotation);
		Quaternion secondRotationQuaternion = Quaternion.Euler(0, 0,  mRegionIndependentParameters.CustomRotation);

		for (int index = 0; index < inputVertices.Count; ++index) {
			Vector3 inputVertex = new Vector3(inputVertices[index].x, inputVertices[index].y, 0);
			Vector3 transformedVertex = inputVertex - transformationCenter;
			// rotate initially
			transformedVertex = initialRotationQuaternion * transformedVertex;
			// scale
			transformedVertex.Scale(scaleAfterInitialRotation);
			// rotate a second time
			transformedVertex = secondRotationQuaternion * transformedVertex;
			// scale 
			transformedVertex.Scale(scaleAfterSecondRotation);
			// translate
			transformedVertex += offset;
			transformedVertex += transformationCenter;
			
			// apply zLeftHandedMultiplier
			transformedVertices[index] = new Vector2(transformedVertex.x, transformedVertex.y);
		}
	}
	
	//-------------------------------------------------------------------------
	float GetOutputScaleX(bool invertForColladaFile) {

		float scaleX = mRegionIndependentParameters.FlipHorizontal ? -1.0f : 1.0f;
		if (invertForColladaFile) {
			scaleX = -scaleX; // inverted when exporting to a collada file.
		}
		scaleX *= mOutlineScale.x * mRegionIndependentParameters.CustomScale.x;
		return scaleX;
	}

	//-------------------------------------------------------------------------
	float GetOutputScaleY() {
		float scaleY = mRegionIndependentParameters.FlipVertical ? -1.0f : 1.0f;
		scaleY *= mOutlineScale.y * mRegionIndependentParameters.CustomScale.y;
		return scaleY;
	}
	
	//-------------------------------------------------------------------------
	/// <returns>
	/// A scale vector to compensate for the game-object's transform.scale
	/// value in case of a custom image.
	/// </returns>
	Vector3 GetCustomImageScale() {
		
		float baseImageWidth = mInactiveBaseImageWidth;   // takes an atlas into account already
		float baseImageHeight = mInactiveBaseImageHeight; // same here
		
		float customImageWidth = mRegionIndependentParameters.CustomTex.width;
		float customImageHeight = mRegionIndependentParameters.CustomTex.height;
		if (mRegionIndependentParameters.IsCustomAtlasRegionUsed) {
			customImageWidth = mRegionIndependentParameters.CustomAtlasFrameSizeInPixels.x;
			customImageHeight = mRegionIndependentParameters.CustomAtlasFrameSizeInPixels.y;
		}
		
		// OTSprite collider has the size 1x1 units at a full image.
		if (mHasOTSpriteComponent) {
			return new Vector3(customImageWidth / baseImageWidth, customImageHeight / baseImageHeight, 1.0f);
		}
		// SmoothMoves sprite collider has the size 1x1 * <outlineScale> units at a full image.
		else if (mHasSmoothMovesSpriteComponent) {
			return new Vector3(customImageWidth / baseImageWidth * mInactiveBaseImageOutlineScale.x, customImageHeight / baseImageHeight * mInactiveBaseImageOutlineScale.y, 1.0f);
		}
		else if (mHasSmoothMovesAnimBoneColliderComponent) {
			return new Vector3(customImageWidth / baseImageWidth * mInactiveBaseImageOutlineScale.x, customImageHeight / baseImageHeight * mInactiveBaseImageOutlineScale.y, 1.0f);
		}
		else {
			// nothing at all
			return new Vector3(customImageWidth / baseImageWidth, customImageHeight / baseImageHeight, 1.0f);
		}
	}
	
	//-------------------------------------------------------------------------
	/// <returns>
	/// The name of the file to be generated.
	/// Follows the form:
	/// "TextureName[_AtlasIndex][_c]_PathHash[_FlipSuffix][groupSuffix].dae"
	/// or    "Atlas_[FrameTitle][_c]_PathHash[_FlipSuffix][groupSuffix].dae".
	/// E.g.: "Island2_FBAAACD3_flipped_h.dae" or "TexAtlas_12_FBA64CD3.dae"
	/// PathHash is added to the name to prevent name-collisions that could
	/// occur if the texture's name was used without considering its full path,
	/// such as "dir1/main.png" colliding with "dir2/main.png".
	/// </returns>
	string GetColliderMeshFilename() {
		if (mMainTex == null) {
			if (this.renderer && this.renderer.sharedMaterial) {
				mMainTex = (Texture2D) this.renderer.sharedMaterial.mainTexture;
			}
		}
		
		if (UsedTexture == null) {
			return "";
		}

		Texture2D targetTexture = UsedTexture;

		string nameString = "";
		if (mRegionIndependentParameters.IsCustomAtlasRegionUsed) {
			nameString = targetTexture.name + "_" + mRegionIndependentParameters.CustomAtlasFrameTitle;
		}
		else {
			if (!mIsAtlasUsed) {
				nameString = targetTexture.name;
			}
			else {
				if (!string.IsNullOrEmpty(mAtlasFrameTitle)) {
					nameString = "Atlas_" + mAtlasFrameIndex.ToString() + "_" + mAtlasFrameTitle;
				}
				else {
					nameString = targetTexture.name + "_" + mAtlasFrameIndex.ToString();
				}
			}
		}
		
		string customString = "";
		if (mRegionIndependentParameters.CustomTex != null) {
			customString = "_c";
		}
		
		string flipSuffix = "";
		if (mRegionIndependentParameters.FlipHorizontal || mRegionIndependentParameters.FlipVertical) {
			flipSuffix += "_flipped_";
			if (mRegionIndependentParameters.FlipHorizontal) {
				flipSuffix += "h";
			}
			if (mRegionIndependentParameters.FlipVertical) {
				flipSuffix += "v";
			}
		}
		string uniqueHashID = "_" + GetHashStringForTexturePath(targetTexture);
		string name = nameString + customString + uniqueHashID + flipSuffix + mGroupSuffix + ".dae";
		return name;
	}

	//-------------------------------------------------------------------------
	/// <summary>
	/// Same as the method GetColliderMeshFilename but instead of using the
	/// collider animation frame it uses the first frame.
	/// </summary>
	/// <returns>The collider mesh filename of the first animation frame.</returns>
	string GetFirstFrameColliderMeshFilename() {

#if !(UNITY_4_3_AND_LATER)
		return GetColliderMeshFilename();
#else
		if (!HasMultipleColliderFrames) {
			return GetColliderMeshFilename();
		}

		Sprite firstSpriteFrame = mAnimatedColliderParameters.SpriteFrames[0];
		Texture2D targetTexture = firstSpriteFrame.texture;

		string nameString = firstSpriteFrame.name;

		string flipSuffix = "";
		if (mRegionIndependentParameters.FlipHorizontal || mRegionIndependentParameters.FlipVertical) {
			flipSuffix += "_flipped_";
			if (mRegionIndependentParameters.FlipHorizontal) {
				flipSuffix += "h";
			}
			if (mRegionIndependentParameters.FlipVertical) {
				flipSuffix += "v";
			}
		}
		string uniqueHashID = "_" + GetHashStringForTexturePath(targetTexture);
		string name = nameString + uniqueHashID + flipSuffix + mGroupSuffix + ".dae";
		return name;

#endif // UNITY_4_3_AND_LATER
	}
	
	//-------------------------------------------------------------------------
	public static int GetHashForTexturePath(Texture2D texture) {
		string texturePath = UnityEditor.AssetDatabase.GetAssetPath(texture);
		return texturePath.GetHashCode();
	}
	
	//-------------------------------------------------------------------------
	public static string GetHashStringForTexturePath(Texture2D texture) {
		int hash = GetHashForTexturePath(texture);
		return hash.ToString("X8");
	}
	
	//-------------------------------------------------------------------------
	public static void SetupColliderRegions(out ColliderRegionData[] colliderRegions, IslandDetector.Region[] islands, IslandDetector.Region[] seaRegions) {
		
		int numColliderRegions = islands.Length + seaRegions.Length;
		colliderRegions = new ColliderRegionData[numColliderRegions];
		int colliderRegionIndex = 0;
		foreach (IslandDetector.Region islandRegion in islands) {
			ColliderRegionData newRegion = new ColliderRegionData();
			newRegion.mDetectedRegion = islandRegion;
			newRegion.mRegionIsIsland = true;
			colliderRegions[colliderRegionIndex++] = newRegion;
		}
		foreach (IslandDetector.Region seaRegion in seaRegions) {
			ColliderRegionData newRegion = new ColliderRegionData();
			newRegion.mDetectedRegion = seaRegion;
			newRegion.mRegionIsIsland = false;
			colliderRegions[colliderRegionIndex++] = newRegion;
		}
	}
	
	//-------------------------------------------------------------------------
	public static void SetupColliderRegionParameters(ref ColliderRegionParameters[] islandRegionParameters, ref ColliderRegionParameters[] seaRegionParameters,
	                                                 int defaultMaxPointCount,
													 IslandDetector.Region[] islands, IslandDetector.Region[] seaRegions) {

		// Islands
		int oldIslandRegionParametersLength = 0;
		if (islandRegionParameters == null) {			
			islandRegionParameters = new ColliderRegionParameters[islands.Length];
		}
		else {
			oldIslandRegionParametersLength = islandRegionParameters.Length;
			if (islands.Length > oldIslandRegionParametersLength) {
				System.Array.Resize(ref islandRegionParameters, islands.Length);
			}
		}

		// set RegionUpdateCalculationNeeded at old entries
		for (int existingIndex = 0; existingIndex < oldIslandRegionParametersLength; ++existingIndex) {
			islandRegionParameters[existingIndex].RegionUpdateCalculationNeeded = true;
		}
		// add new ColliderRegionParameters at new ones
		for (int newIndex = oldIslandRegionParametersLength; newIndex < islands.Length; ++newIndex) {
		
			// Note: We enable the first island region only. All other island- and all sea-regions are initially disabled.
			ColliderRegionParameters newParameters = new ColliderRegionParameters();
			if (newIndex == 0) {
				newParameters.EnableRegion = true;
			}
			else {
				newParameters.EnableRegion = false;
			}
			newParameters.MaxPointCount = defaultMaxPointCount;
			islandRegionParameters[newIndex] = newParameters;
		}

		// Sea regions
		int oldSeaRegionParametersLength = 0;
		if (seaRegionParameters == null) {			
			seaRegionParameters = new ColliderRegionParameters[seaRegions.Length];
		}
		else {
			oldSeaRegionParametersLength = seaRegionParameters.Length;
			if (seaRegions.Length > oldSeaRegionParametersLength) {
				System.Array.Resize(ref seaRegionParameters, seaRegions.Length);
			}
		}
		// set RegionUpdateCalculationNeeded at old entries
		for (int existingIndex = 0; existingIndex < oldSeaRegionParametersLength; ++existingIndex) {
			seaRegionParameters[existingIndex].RegionUpdateCalculationNeeded = true;
		}
		// add new ColliderRegionParameters at new ones
		for (int seaRegionIndex = oldSeaRegionParametersLength; seaRegionIndex < seaRegions.Length; ++seaRegionIndex) {
			
			ColliderRegionParameters newParameters = new ColliderRegionParameters();
			newParameters.EnableRegion = false;
			newParameters.MaxPointCount = defaultMaxPointCount;
			seaRegionParameters[seaRegionIndex] = newParameters;
		}
	}
	
	//-------------------------------------------------------------------------
	// Compatibility glue-code: to read the old mMaxPointCount value and sets the first island's newParameters.MaxPointCount value.
	public static void CopyOldPointCountParameterToFirstIslandForBackwardsCompatibility(ref ColliderRegionParameters[] colliderRegionParameters,
																						ref int deprecatedOldPointCountParameter) {
		
		if (deprecatedOldPointCountParameter != PARAMETER_NOT_USED_ANYMORE) {
			colliderRegionParameters[0].MaxPointCount = deprecatedOldPointCountParameter;
			deprecatedOldPointCountParameter = PARAMETER_NOT_USED_ANYMORE;
		}
	}

	//-------------------------------------------------------------------------
	public void UpdateFromOldVersionForBackwardsCompatibility() {

		if (NeedsToCopyRegionIndependentParametersForBackwardsCompatibility()) {
			CopyParametersToRegionIndependentParametersForBackwardsCompatibility(ref this.mRegionIndependentParameters, this);
		}

		if (NeedsToCopyColliderRegionsForBackwardsCompatibility()) {
			CopyColliderRegionsForBackwardsCompatibility();
		}

		CopyOldColliderRegionParametersForBackwardsCompatibility();
	}

	//-------------------------------------------------------------------------
	public bool NeedsToCopyRegionIndependentParametersForBackwardsCompatibility() {
		return this.mCustomRotation != OLD_PARAMETERS_CONVERTED;
	}

	//-------------------------------------------------------------------------
	// Compatibility glue-code: read all old parameters and set the new RegionIndependentParameters pendants accordingly.
	public static void CopyParametersToRegionIndependentParametersForBackwardsCompatibility(ref RegionIndependentParameters colliderParameters,
																   						    AlphaMeshCollider oldReference) {
		
		colliderParameters.LiveUpdate = oldReference.mLiveUpdate;
		colliderParameters.AlphaOpaqueThreshold = oldReference.mAlphaOpaqueThreshold;
		colliderParameters.VertexReductionDistanceTolerance = oldReference.mVertexReductionDistanceTolerance;
		colliderParameters.DefaultMaxPointCount = oldReference.mDefaultMaxPointCount;
		colliderParameters.Thickness = oldReference.mThickness;
		colliderParameters.FlipHorizontal = oldReference.mFlipHorizontal;
		colliderParameters.FlipVertical = oldReference.mFlipVertical;
		colliderParameters.Convex = oldReference.mConvex;
		colliderParameters.FlipInsideOutside = oldReference.mFlipInsideOutside;
		
		colliderParameters.CustomRotation = oldReference.mCustomRotation;
		colliderParameters.CustomScale = oldReference.mCustomScale;
		colliderParameters.CustomOffset = oldReference.mCustomOffset;
		
		colliderParameters.CopyOTSpriteFlipping = oldReference.mCopyOTSpriteFlipping;
		colliderParameters.CopySmoothMovesSpriteDimensions = oldReference.mCopySmoothMovesSpriteDimensions;
		colliderParameters.CustomTex = oldReference.mCustomTex;
		
		colliderParameters.IsCustomAtlasRegionUsed = oldReference.mIsCustomAtlasRegionUsed;
		colliderParameters.CustomAtlasFrameTitle = oldReference.mCustomAtlasFrameTitle;
		colliderParameters.CustomAtlasFramePositionInPixels = oldReference.mCustomAtlasFramePositionInPixels;
		colliderParameters.CustomAtlasFrameSizeInPixels = oldReference.mCustomAtlasFrameSizeInPixels;
		colliderParameters.CustomAtlasFrameRotation = oldReference.mCustomAtlasFrameRotation;
		
		colliderParameters.ApplySmoothMovesScaleAnim = oldReference.mApplySmoothMovesScaleAnim;
		
		oldReference.mCustomRotation = OLD_PARAMETERS_CONVERTED;
	}

	//-------------------------------------------------------------------------
	public void CopyOldColliderRegionParametersForBackwardsCompatibility() {
		if (mColliderRegionParameters != null) {
			
			if (mColliderRegionParameters.Length > 0) {
				// Note: index [0] since we take the island/sea regions of the first frame, because this was the old version's only frame.
				int numIslands = mGeneratedColliderData[0].NumIslandRegions;
				int numSeaRegions = mGeneratedColliderData[0].NumSeaRegions;
				ColliderRegionData[] colliderRegions = mGeneratedColliderData[0].mColliderRegions;
				
				mIslandRegionParameters = new ColliderRegionParameters[numIslands];
				mSeaRegionParameters = new ColliderRegionParameters[numSeaRegions];
				int islandIndex = 0;
				int seaRegionIndex = 0;
				for (int regionIndex = 0; regionIndex < colliderRegions.Length; ++regionIndex) {
					ColliderRegionData region = colliderRegions[regionIndex];
					if (region.mRegionIsIsland) {
						mIslandRegionParameters[islandIndex++] = mColliderRegionParameters[regionIndex];
					}
					else {
						mSeaRegionParameters[seaRegionIndex++] = mColliderRegionParameters[regionIndex];
					}
				}
			}
			mColliderRegionParameters = null;
		}
	}

	//-------------------------------------------------------------------------
	public bool NeedsToCopyColliderRegionsForBackwardsCompatibility() {
		return (mColliderRegions != null && mColliderRegions.Length > 0); // the deprecated member var will be set to null after the update.
	}

	//-------------------------------------------------------------------------
	public void CopyColliderRegionsForBackwardsCompatibility() {
		mGeneratedColliderData[0].mOutlineAlgorithm = this.mOutlineAlgorithm;
		mGeneratedColliderData[0].mBinaryImage = this.mBinaryImage;
		mGeneratedColliderData[0].mColliderRegions = this.mColliderRegions;

		this.mOutlineAlgorithm = null;
		this.mBinaryImage = null;
		this.mColliderRegions = null;
	}
	
	//-------------------------------------------------------------------------
	public static void CalculateUnreducedOutlineForAllColliderRegions(ref ColliderRegionData[] colliderRegions, ref PolygonOutlineFromImageFrontend outlineAlgorithm,
																	  RegionIndependentParameters regionIndependentParameters,
	                                                                  ColliderRegionParameters[] islandRegionParameters, ColliderRegionParameters[] seaRegionParameters,
	                                                                  bool [,] binaryImage,
																	  bool ccwVertexOrder) {

		int islandIndex = 0;
		int seaRegionIndex = 0;
		for (int count = 0; count < colliderRegions.Length; ++count) {
        
			bool isIslandRegion = colliderRegions[count].mRegionIsIsland;
			ColliderRegionParameters regionParameters = isIslandRegion ? islandRegionParameters[islandIndex] : seaRegionParameters[seaRegionIndex];

			if (regionParameters.EnableRegion) {
				// Calculate polygon bounds
	            outlineAlgorithm.VertexReductionDistanceTolerance = regionIndependentParameters.VertexReductionDistanceTolerance;
				outlineAlgorithm.MaxPointCount = regionParameters.MaxPointCount;
				
				bool allRegionsConvex = regionIndependentParameters.Convex;
				outlineAlgorithm.Convex = allRegionsConvex ? true : regionParameters.Convex;
			    //outlineAlgorithm.Convex = colliderRegionParameters[count].Convex;
			    
				outlineAlgorithm.XOffsetNormalized = -0.5f;//-this.transform.localScale.x / 2.0f;
			    outlineAlgorithm.YOffsetNormalized = -0.5f;//-this.transform.localScale.y / 2.0f;
			    bool outputInNormalizedSpace = true;

				bool regionVertexOrder = ccwVertexOrder;
				if (!colliderRegions[count].mRegionIsIsland) {
					regionVertexOrder = !regionVertexOrder;
				}
	
				colliderRegions[count].mOutlineVertexOrderIsCCW = regionVertexOrder;
	            outlineAlgorithm.UnreducedOutlineFromBinaryImage(out colliderRegions[count].mIntermediateOutlineVertices, binaryImage, colliderRegions[count].mDetectedRegion.mPointAtBorder, colliderRegions[count].mRegionIsIsland, outputInNormalizedSpace, regionVertexOrder);
			}
			else {
				colliderRegions[count].mIntermediateOutlineVertices = null;
				colliderRegions[count].mResultVertices = null;
				colliderRegions[count].mResultTriangleIndices = null;
			}

			if (isIslandRegion) {
				++islandIndex;
			}
			else {
				++seaRegionIndex;
			}
        }
	}
	
	//-------------------------------------------------------------------------
	static void LogAttributesOfObject(object target, int childLevels) {
		LogAttributesOfObject(target, childLevels, "");
	}
	
	//-------------------------------------------------------------------------
	static void LogAttributesOfObject(object target, int childLevels, string indent) {
		
		string childIndent = indent + new string(' ', 4);
		
		Type targetType = target.GetType();
		PropertyInfo[] properties = targetType.GetProperties();
		foreach (PropertyInfo propertyInfo in properties) {
			Debug.Log(indent + "prop found: " + propertyInfo.Name);
		}
		FieldInfo[] fields = targetType.GetFields();
		foreach (FieldInfo fieldInfo in fields) {
			Debug.Log(indent + "field found: " + fieldInfo.Name + "=" + fieldInfo.GetValue(target).ToString());
			Debug.Log(indent + "{ child begin------");
			if (childLevels > 0) {
				object obj = fieldInfo.GetValue(target);
				LogAttributesOfObject(obj, childLevels -1, childIndent);
			}
			Debug.Log(indent + "} child end ------");
		}
		MethodInfo[] methods = targetType.GetMethods();
		foreach (MethodInfo methodInfo in methods) {
			Debug.Log(indent + "method found: " + methodInfo.ToString());
		}
	}
}

#endif // #if UNITY_EDITOR
