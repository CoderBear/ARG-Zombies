using UnityEngine;


/// <summary>
/// The parameter set at a single sprite.
/// </summary>
[System.Serializable]
public class ColliderGenTK2DParametersForSprite /*: MonoBehaviour*/ {
	
	private const int PARAMETER_NOT_USED_ANYMORE = -1;
	private const float LATEST_VERSION_ID = 1.0f;
	private const float VERSION_ID_BEFORE_PARAMETER_GROUPS = 0.5f;
	
	public int mSpriteIndex;
	
	// START OF OLD PARAMETERS - NOT USED ANYMORE - NOW MOVED INTO A SEPARATE RegionIndependentParameters CLASS
	public int mOutlineVertexCount = PARAMETER_NOT_USED_ANYMORE;
	public float mAlphaOpaqueThreshold;
	public bool mForceConvex;
	public bool mFlipNormals;
	public Texture2D mCustomTexture;
	public Vector2 mCustomScale;
	public Vector2 mCustomOffset;
	// END OF OLD PARAMETERS - NOT USED ANYMORE - NOW MOVED INTO A SEPARATE RegionIndependentParameters CLASS
	
	public ColliderRegionParametersTK2D[] mColliderRegionParameters = null;
	public RegionIndependentParametersTK2D mRegionIndependentParameters = null;
	
	public float mVersionID = LATEST_VERSION_ID;
	
	// METHODS
	//-------------------------------------------------------------------------
	/// Default Constructor.
	public ColliderGenTK2DParametersForSprite() {
	}
	
	//-------------------------------------------------------------------------
	/// Copy Constructor - creates a deep copy of the src object.
	public ColliderGenTK2DParametersForSprite(ColliderGenTK2DParametersForSprite src) {
		mSpriteIndex = src.mSpriteIndex;
	
		mOutlineVertexCount = src.mOutlineVertexCount;
		// other old unused parameters skipped.
		
		// deep copy of the following two member variables
		if (src.mRegionIndependentParameters != null) {
			mRegionIndependentParameters = new RegionIndependentParametersTK2D(src.mRegionIndependentParameters);
		}
		else {
			mRegionIndependentParameters = null;
		}
		
		if (src.mColliderRegionParameters != null) {
			mColliderRegionParameters = new ColliderRegionParametersTK2D[src.mColliderRegionParameters.Length];
			for (int index = 0; index < src.mColliderRegionParameters.Length; ++index) {
				mColliderRegionParameters[index] = new ColliderRegionParametersTK2D(src.mColliderRegionParameters[index]);
			}
		}
		else {
			src.mColliderRegionParameters = null;
		}
	
		mVersionID = src.mVersionID;
	}
	
	//-------------------------------------------------------------------------
	public void UpdateToCurrentVersionIfNecessary() {
		
		float currentVersionID = mVersionID;
		
		if (mOutlineVertexCount != PARAMETER_NOT_USED_ANYMORE) {
			currentVersionID = VERSION_ID_BEFORE_PARAMETER_GROUPS;
		}
		
		if (currentVersionID < LATEST_VERSION_ID) {
			UpdateFromVersion(currentVersionID);
		}
		
		mVersionID = LATEST_VERSION_ID;
	}
	
	//-------------------------------------------------------------------------
	public void UpdateFromVersion(float versionID) {
		if (versionID <= VERSION_ID_BEFORE_PARAMETER_GROUPS) {
			CopyPreParameterGroupParameters();
		}
		
	}
	
	//-------------------------------------------------------------------------
	protected void CopyPreParameterGroupParameters() {
		
		if (mOutlineVertexCount != PARAMETER_NOT_USED_ANYMORE) {
			
			mRegionIndependentParameters.DefaultMaxPointCount = mOutlineVertexCount;
			if (mColliderRegionParameters != null && mColliderRegionParameters.Length != 0) {
				mColliderRegionParameters[0].MaxPointCount = mOutlineVertexCount;
			}
			
			mRegionIndependentParameters.AlphaOpaqueThreshold = mAlphaOpaqueThreshold;
			mRegionIndependentParameters.Convex = mForceConvex;
			mRegionIndependentParameters.FlipInsideOutside = mFlipNormals;
			mRegionIndependentParameters.CustomTex = mCustomTexture;
			mRegionIndependentParameters.CustomScale = mCustomScale;
			mRegionIndependentParameters.CustomOffset = mCustomOffset;
			
			mOutlineVertexCount = PARAMETER_NOT_USED_ANYMORE; // mark it as done.
		}
	}
}