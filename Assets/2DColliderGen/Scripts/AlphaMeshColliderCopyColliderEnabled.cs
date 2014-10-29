using UnityEngine;
using System.Collections;

//-------------------------------------------------------------------------
/// <summary>
/// Copies the enabled/disabled state of a box-, sphere- or capsule-collider
/// to a target mesh collider. Needed for Smooth Moves animated sprites where
/// colliders can be enabled and disabled at certain frames.
/// </summary>
public class AlphaMeshColliderCopyColliderEnabled : MonoBehaviour {
	
	public Collider mReferenceCollider = null;
	public MeshCollider mMeshCollider = null;
	
	// Use this for initialization
	void Start () {
		// check this node for mReferenceCollider
		if (mReferenceCollider == null) {
			mReferenceCollider = this.GetComponent<BoxCollider>();
		}
		if (mReferenceCollider == null) {
			mReferenceCollider = this.GetComponent<SphereCollider>();
		}
		if (mReferenceCollider == null) {
			mReferenceCollider = this.GetComponent<CapsuleCollider>(); // unlikely.
		}
		// check the parent node for mReferenceCollider
		if (mReferenceCollider == null) {
			mReferenceCollider = this.transform.parent.GetComponent<BoxCollider>();
		}
		if (mReferenceCollider == null) {
			mReferenceCollider = this.transform.parent.GetComponent<SphereCollider>();
		}
		if (mReferenceCollider == null) {
			mReferenceCollider = this.transform.parent.GetComponent<CapsuleCollider>(); // unlikely.
		}
		if (mMeshCollider == null) {
			mMeshCollider = this.GetComponent<MeshCollider>();
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (mReferenceCollider && mMeshCollider) {
			mMeshCollider.enabled = mReferenceCollider.enabled;
		}
	}
}
