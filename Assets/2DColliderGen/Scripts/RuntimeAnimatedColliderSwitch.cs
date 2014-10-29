#if !(UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
#define UNITY_4_3_AND_LATER
#endif


#if !(UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5)
#define UNITY_4_AND_LATER
#endif

#if UNITY_4_3_AND_LATER

using UnityEngine;
using System.Collections;

public class RuntimeAnimatedColliderSwitch : MonoBehaviour {

	protected enum ColliderMode {
		NONE,
		POLYGON_COLLIDER_2D,
		MESH_COLLIDER
	}

	protected SpriteRenderer mSpriteRenderer;
	[SerializeField] protected PolygonCollider2D[] mPolygonCollidersToSwitch;
	[SerializeField] protected MeshCollider[] mMeshCollidersToSwitch;
	[SerializeField] protected string[] mColliderIDStrings;
	public int mActiveColliderIndex = 0;
	public PolygonCollider2D mActivePolygonCollider = null;
	public MeshCollider mActiveMeshCollider = null;
	protected ColliderMode mColliderMode = ColliderMode.NONE;

	// Setters and Getters
	public PolygonCollider2D[] PolygonCollidersToSwitch {
		get {
			return mPolygonCollidersToSwitch;
		}
		set {
			mPolygonCollidersToSwitch = value;
			if (value != null && value.Length > 0) {
				mColliderMode = ColliderMode.POLYGON_COLLIDER_2D;
			}
		}
	}
	public MeshCollider[] MeshCollidersToSwitch {
		get {
			return mMeshCollidersToSwitch;
		}
		set {
			mMeshCollidersToSwitch = value;
			if (value != null && value.Length > 0) {
				mColliderMode = ColliderMode.MESH_COLLIDER;
			}
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

	//-------------------------------------------------------------------------
	void Awake() {
		mSpriteRenderer = this.GetComponent<SpriteRenderer>();

		if (mPolygonCollidersToSwitch != null) {
			for (int index = 0; index < mPolygonCollidersToSwitch.Length; ++index) {
				if (index != mActiveColliderIndex) {
					mPolygonCollidersToSwitch[index].enabled = false;
				}
				else {
					mPolygonCollidersToSwitch[index].enabled = true;
				}
			}
		}
		if (mMeshCollidersToSwitch != null) {
			for (int index = 0; index < mMeshCollidersToSwitch.Length; ++index) {
				if (index != mActiveColliderIndex) {
					mMeshCollidersToSwitch[index].enabled = false;
				}
				else {
					mMeshCollidersToSwitch[index].enabled = true;
				}
			}
		}

		if (mColliderMode == ColliderMode.NONE) {
			if (mPolygonCollidersToSwitch != null && mPolygonCollidersToSwitch.Length > 0)
				mColliderMode = ColliderMode.POLYGON_COLLIDER_2D;
			else if (mMeshCollidersToSwitch != null && mMeshCollidersToSwitch.Length > 0)
				mColliderMode = ColliderMode.MESH_COLLIDER;
		}
	}
	
	//-------------------------------------------------------------------------
	void LateUpdate () {
		if (mSpriteRenderer == null || mSpriteRenderer.sprite == null || (mPolygonCollidersToSwitch.Length == 0 && mMeshCollidersToSwitch.Length == 0)) {
			return;
		}

		if ((mActivePolygonCollider == null && mActiveMeshCollider == null) ||
		    !mSpriteRenderer.sprite.name.Equals(mColliderIDStrings[mActiveColliderIndex])) {

			if (mColliderMode == ColliderMode.POLYGON_COLLIDER_2D)
				SwitchPolygonCollider();
			else if (mColliderMode == ColliderMode.MESH_COLLIDER)
				SwitchMeshCollider();
		}
	}

	//-------------------------------------------------------------------------
	bool SwitchPolygonCollider() {

		string spriteName = mSpriteRenderer.sprite.name;

		bool wasSuitableColliderFound = true;
		int startIndex = mActiveColliderIndex;
		while (!spriteName.Equals(mColliderIDStrings[mActiveColliderIndex])) {
			mActiveColliderIndex = (mActiveColliderIndex+1) % mPolygonCollidersToSwitch.Length;
			if (mActiveColliderIndex == startIndex) {
				wasSuitableColliderFound = false;
				break;
			}
		}
		if (wasSuitableColliderFound) {
			// disable last active, activate new one
			if (mActivePolygonCollider != null) {
				mActivePolygonCollider.enabled = false;
			}

			mActivePolygonCollider = mPolygonCollidersToSwitch[mActiveColliderIndex];
			mActivePolygonCollider.enabled = true;
		}

		return wasSuitableColliderFound;
	}

	//-------------------------------------------------------------------------
	bool SwitchMeshCollider() {
		
		string spriteName = mSpriteRenderer.sprite.name;
		
		bool wasSuitableColliderFound = true;
		int startIndex = mActiveColliderIndex;
		while (!spriteName.Equals(mColliderIDStrings[mActiveColliderIndex])) {
			mActiveColliderIndex = (mActiveColliderIndex+1) % mMeshCollidersToSwitch.Length;
			if (mActiveColliderIndex == startIndex) {
				wasSuitableColliderFound = false;
				break;
			}
		}
		if (wasSuitableColliderFound) {
			// disable last active, activate new one
			if (mActiveMeshCollider != null) {
				mActiveMeshCollider.enabled = false;
			}
			
			mActiveMeshCollider = mMeshCollidersToSwitch[mActiveColliderIndex];
			mActiveMeshCollider.enabled = true;
		}
		
		return wasSuitableColliderFound;
	}
}

#endif // UNITY_4_3_AND_LATER
