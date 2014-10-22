#if !(UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2)
#define UNITY_4_3_AND_LATER
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]

//-------------------------------------------------------------------------
/// <summary>
/// A class to provide compatibility of AlphaMeshColliders with SmoothMoves
/// sprites. There are some properties that we like to have synced between
/// the SmoothMoves and AlphaMeshCollider components.
/// </summary>
public class AlphaMeshColliderSmoothMovesRestore : MonoBehaviour {
	
	//-------------------------------------------------------------------------
	[System.Serializable]
	public class RestoreData {
		public bool mIsTrigger = false;
		public bool mConvex = false;
		public PhysicMaterial mSharedMaterial = null;
#if UNITY_4_3_AND_LATER
		public PhysicsMaterial2D mSharedMaterial2D = null;
#endif
		public bool mSmoothSphereCollisions = false;
		public Mesh mColliderMesh = null;
#if UNITY_4_3_AND_LATER
		public int mPolygonColliderPathCount = 0;
		public Vector2[] mPolygonColliderPoints = null;
#endif
	}
	//-------------------------------------------------------------------------
	
	public RestoreData[] mDataToRestore = null;
	public string[] mNodePaths = null;
	public bool[] mIsSmoothMovesScaleAnimAppliedAtNode = null;
	public bool mHasRestoredData = false;
	
	//-------------------------------------------------------------------------
	void Start () {
		mHasRestoredData = false;
	}
	
	//-------------------------------------------------------------------------
	void Update () {
#if UNITY_EDITOR
		if (Application.isEditor && !Application.isPlaying) {
			StoreColliderData();
		}
		else {
#endif
			if (!mHasRestoredData) {
				RestoreColliderData();
				mHasRestoredData = true;
			}
#if UNITY_EDITOR
		}
#endif
	}

#if UNITY_EDITOR
    //-------------------------------------------------------------------------
	protected void StoreColliderData() {
		List<AlphaMeshCollider> collidersList = new List<AlphaMeshCollider>();
		List<RestoreData> dataList = new List<RestoreData>();
		List<string> pathsList = new List<string>();
		List<bool> isScaleAnimNodeList = new List<bool>();
		
		AddChildColliderDataRecursively(this.transform, "", ref collidersList, ref dataList, ref pathsList, ref isScaleAnimNodeList);
		
		mDataToRestore = dataList.ToArray();
		mNodePaths = pathsList.ToArray();
		mIsSmoothMovesScaleAnimAppliedAtNode = isScaleAnimNodeList.ToArray();
	}
	
    //-------------------------------------------------------------------------
	protected void AddChildColliderDataRecursively(Transform node, string nodePath, ref List<AlphaMeshCollider> collidersList, ref List<RestoreData> dataList, ref List<string> pathsList, ref List<bool> isScaleAnimNodeList) {
		
		foreach (Transform child in node) {
			string childNodePath = (nodePath.Length == 0) ? child.name : nodePath + "/" + child.name;
			
			AlphaMeshCollider alphaMeshColliderComponent = child.GetComponent<AlphaMeshCollider>();
			if (alphaMeshColliderComponent != null) {

				string colliderNodePath = childNodePath;
				bool isScaleAnimNode = alphaMeshColliderComponent.ApplySmoothMovesScaleAnim;
				if (isScaleAnimNode) {
					colliderNodePath += "/" + alphaMeshColliderComponent.TargetNodeNameToAttachMeshCollider;
				}
				
				MeshCollider meshCollider = alphaMeshColliderComponent.TargetNodeToAttachMeshCollider.gameObject.GetComponent<MeshCollider>();
				if (meshCollider != null) {
				
					collidersList.Add(alphaMeshColliderComponent);
					RestoreData data = new RestoreData();
					
					data.mColliderMesh = meshCollider.sharedMesh;
					data.mIsTrigger = meshCollider.isTrigger;
					data.mConvex = meshCollider.convex;
					data.mSharedMaterial = meshCollider.sharedMaterial;
					data.mSmoothSphereCollisions = meshCollider.smoothSphereCollisions;
					
					dataList.Add(data);
					pathsList.Add(colliderNodePath);
					isScaleAnimNodeList.Add(isScaleAnimNode);
				}
#if UNITY_4_3_AND_LATER
				PolygonCollider2D polygonCollider = alphaMeshColliderComponent.TargetNodeToAttachMeshCollider.gameObject.GetComponent<PolygonCollider2D>();
				if (polygonCollider != null) {

					collidersList.Add(alphaMeshColliderComponent);
					RestoreData data = new RestoreData();

					//int numPaths = polygonCollider.pathCount;
					//data.mPolygonColliderPaths = new List<Vector2[]>(numPaths);
					//for (int pathIndex = 0; pathIndex < numPaths; ++pathIndex) {
					//	data.mPolygonColliderPaths[pathIndex] = polygonCollider.GetPath(pathIndex);
					//}
					data.mPolygonColliderPathCount = polygonCollider.pathCount;
					data.mPolygonColliderPoints = polygonCollider.points;

					data.mIsTrigger = polygonCollider.isTrigger;
					// data.mConvex = meshCollider.convex; // Note: polygon colliders have no convex flag.
					data.mSharedMaterial2D = polygonCollider.sharedMaterial;
					//data.mSmoothSphereCollisions = meshCollider.smoothSphereCollisions; // Note: polygon colliders have no smoothSphereCollisions flag.
					
					dataList.Add(data);
					pathsList.Add(colliderNodePath);
					isScaleAnimNodeList.Add(isScaleAnimNode);
				}
#endif
			}
			
			AddChildColliderDataRecursively(child, childNodePath, ref collidersList, ref dataList, ref pathsList, ref isScaleAnimNodeList);
		}
	}
#endif // UNITY_EDITOR

    //-------------------------------------------------------------------------
	protected void RestoreColliderData() {
		for (int index = 0; index < mDataToRestore.Length; ++index) {
			Transform restoreNode = this.transform.Find(mNodePaths[index]);

			RestoreData data = mDataToRestore[index];
			bool hasMeshCollider = (data.mColliderMesh != null);
			if (hasMeshCollider) {

				MeshCollider collider = restoreNode.GetComponent<MeshCollider>();
				if (collider == null) {
					collider = restoreNode.gameObject.AddComponent<MeshCollider>();
				}
				collider.sharedMesh = null;

				collider.sharedMesh = data.mColliderMesh;
				collider.isTrigger = data.mIsTrigger;
				collider.convex = data.mConvex;
				collider.sharedMaterial = data.mSharedMaterial;
				collider.smoothSphereCollisions = data.mSmoothSphereCollisions;
			}
#if UNITY_4_3_AND_LATER
			else { // has a polygon collider
				PolygonCollider2D collider = restoreNode.GetComponent<PolygonCollider2D>();
				if (collider == null) {
					collider = restoreNode.gameObject.AddComponent<PolygonCollider2D>();
				}

				collider.pathCount = data.mPolygonColliderPathCount;
				collider.points = data.mPolygonColliderPoints;
				collider.isTrigger = data.mIsTrigger;
				//collider.convex = data.mConvex; // Note: polygon colliders have no convex flag.
				collider.sharedMaterial = data.mSharedMaterial2D;
				//collider.smoothSphereCollisions = data.mSmoothSphereCollisions; // Note: polygon colliders have no smoothSphereCollisions flag.
			}
#endif
			
			Transform referenceColliderNode = restoreNode;
			bool parentHoldsReferenceCollider = mIsSmoothMovesScaleAnimAppliedAtNode[index];
			if (parentHoldsReferenceCollider) {
				referenceColliderNode = restoreNode.transform.parent;
			}
						
			bool hasSmoothMovesCollider = referenceColliderNode.GetComponent<BoxCollider>();
			if (!hasSmoothMovesCollider)
				hasSmoothMovesCollider = referenceColliderNode.GetComponent<SphereCollider>();
			
			if (hasSmoothMovesCollider) {
				// copy the SmoothMoves collider's enabled state at runtime.
				AlphaMeshColliderCopyColliderEnabled copyStateComponent = restoreNode.GetComponent<AlphaMeshColliderCopyColliderEnabled>();
				if (copyStateComponent == null) {
					copyStateComponent = restoreNode.gameObject.AddComponent<AlphaMeshColliderCopyColliderEnabled>();
				}
			}
		}
	}
}
