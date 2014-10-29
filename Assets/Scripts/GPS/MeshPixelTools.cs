using UnityEngine;
using System.Collections;

public class MeshPixelTools : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	public Vector3 UvTo3D(Vector2 uv){
		
		Mesh mesh = GetComponent<MeshFilter>().mesh;
		int[] tris = mesh.triangles;
		Vector2[] uvs = mesh.uv;
		Vector3[] verts = mesh.vertices;
		
		for (int i = 0; i < tris.Length; i += 3){
			Vector2 u1 = uvs[tris[i]]; // get the triangle UVs
			Vector2 u2 = uvs[tris[i+1]];
			Vector2 u3 = uvs[tris[i+2]];
			
			// calculate triangle area - if zero, skip it
			float a = Area(u1, u2, u3); if (a == 0) continue;
			
			// calculate barycentric coordinates of u1, u2 and u3
			// if anyone is negative, point is outside the triangle: skip it
			float a1 = Area(u2, u3, uv)/a; if (a1 < 0) continue;
			float a2 = Area(u3, u1, uv)/a; if (a2 < 0) continue;
			float a3 = Area(u1, u2, uv)/a; if (a3 < 0) continue;
			
			// point inside the triangle - find mesh position by interpolation...
			Vector3 p3D = a1*verts[tris[i]]+a2*verts[tris[i+1]]+a3*verts[tris[i+2]];
			
			// and return it in world coordinates:
			return transform.TransformPoint(p3D);
		}
		
		// point outside any uv triangle: return Vector3.zero
		return Vector3.zero;
	}
	
	// calculate signed triangle area using a kind of "2D cross product":
	public float Area(Vector2 p1, Vector2 p2, Vector2 p3) {
		
		Vector2 v1 = p1 - p3;
		Vector2 v2 = p2 - p3;
		
		return (v1.x * v2.y - v1.y * v2.x)/2f;
	}
}