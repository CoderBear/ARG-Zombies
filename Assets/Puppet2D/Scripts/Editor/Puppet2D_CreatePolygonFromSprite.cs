using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
//using Poly2Tri;
using Poly2Tri.Triangulation;
using Poly2Tri.Triangulation.Delaunay;
using Poly2Tri.Triangulation.Delaunay.Sweep;
using Poly2Tri.Triangulation.Polygon;
using System.Linq;


public class Puppet2D_CreatePolygonFromSprite : Editor {

	private GameObject MeshedSprite;
	private MeshFilter mf;
	private MeshRenderer mr;
	private Mesh mesh;
	public Sprite mysprite;

	private Vector3[] finalVertices = {};
	private int[] finalTriangles = {};
	private Vector2[] finalUvs = {};
	private Vector3[] finalNormals = {};

	List<Vector3> results = new List<Vector3>();
	List<int> resultsTriIndexes = new List<int>();
	List<int> resultsTriIndexesReversed = new List<int>();
	List<Vector2> uvs = new List<Vector2>();
	List<Vector3> normals = new List<Vector3>();


	//public bool ReverseNormals;

	public GameObject Run (Transform transform,bool ReverseNormals, int triangleIndex)
	{

		PolygonCollider2D polygonCollider = transform.GetComponent<PolygonCollider2D>();

		//for(int path =0;path<polygonCollider.pathCount;path++)
		//{
		int path =0;
		bool overwrite = false;
		MeshedSprite = new GameObject();
		Undo.RegisterCreatedObjectUndo (MeshedSprite, "Created Mesh");
		mf = MeshedSprite.AddComponent<MeshFilter>();
		mr = MeshedSprite.AddComponent<MeshRenderer>();
		mesh = new Mesh();

		if(AssetDatabase.LoadAssetAtPath("Assets/Puppet2D/Models/"+transform.name+"_MESH.asset",typeof(Mesh)))
		{
			if(EditorUtility.DisplayDialog("Overwrite Asset?","Do you want to overwrite the current Mesh & Material?","Yes, Overwrite","No, Create New Mesh & Material"))
			{
				//mf.mesh = AssetDatabase.LoadAssetAtPath("Assets/Puppet2D/Models/"+transform.name+"_MESH.asset",typeof(Mesh))as Mesh;
				string meshPath = ("Assets/Puppet2D/Models/"+transform.name+"_MESH.asset");
				AssetDatabase.CreateAsset(mesh,meshPath);
				overwrite = true;
			}
			else
			{
				string meshPath = AssetDatabase.GenerateUniqueAssetPath("Assets/Puppet2D/Models/"+transform.name+"_MESH.asset");
				AssetDatabase.CreateAsset(mesh,meshPath);
			}
		}
		else
		{
			string meshPath = AssetDatabase.GenerateUniqueAssetPath("Assets/Puppet2D/Models/"+transform.name+"_MESH.asset");
			AssetDatabase.CreateAsset(mesh,meshPath);
		}

		Vector2[] vertsToCopy = polygonCollider.GetPath(path);
        //vertsToCopy = randomizeArray(vertsToCopy.ToList()).ToArray();
//		if(triangleIndex == 0)
//		{
//			CreateMesh(vertsToCopy, transform);   
//
//		}
//		else 
        CreateSubdividedMesh(vertsToCopy, transform, triangleIndex); 
        //CreateVoronoiMesh(vertsToCopy, transform, triangleIndex);  
		//CreateSimpleMesh(vertsToCopy, transform);   


		mesh.vertices = finalVertices;      
		mesh.uv = finalUvs;
		mesh.normals = finalNormals;        
		mesh.triangles = finalTriangles;
		mesh.RecalculateBounds();

		mf.mesh = mesh;

		results.Clear();
		resultsTriIndexes.Clear();
		resultsTriIndexesReversed.Clear();
		uvs.Clear();
		normals.Clear();


		if(overwrite)
		{
			mr.material = AssetDatabase.LoadAssetAtPath("Assets/Puppet2D/Models/Materials/"+transform.name+"_MAT.mat",typeof(Material)) as Material;
		}
		else
		{

			Material newMat = new Material(Shader.Find("Unlit/Transparent"));
			string materialPath = AssetDatabase.GenerateUniqueAssetPath("Assets/Puppet2D/Models/Materials/"+transform.name+"_MAT.mat");
			AssetDatabase.CreateAsset(newMat, materialPath);
			mr.material = newMat;
		}




		return MeshedSprite;

	}

	public void CreateMesh(Vector2[] vertsToCopy, Transform transform)
	{
		List<Vector3> resultsLocal = new List<Vector3>();
		List<int> resultsTriIndexesLocal = new List<int>();
		List<int> resultsTriIndexesReversedLocal = new List<int>();
		List<Vector2> uvsLocal = new List<Vector2>();
		List<Vector3> normalsLocal = new List<Vector3>();

		Sprite spr = transform.GetComponent<SpriteRenderer>().sprite;
		Rect rec = spr.rect;
		Vector3 bound = transform.GetComponent<Renderer>().bounds.max- transform.GetComponent<Renderer>().bounds.min ;


		TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(spr)) as TextureImporter;


		int i =  0;

		Polygon poly =  new Polygon();

		for (i=0; i <vertsToCopy.Length;i++) 
		{
			poly.Points.Add(new TriangulationPoint(vertsToCopy[i].x, vertsToCopy[i].y));                       

		}

		DTSweepContext tcx = new DTSweepContext();
		tcx.PrepareTriangulation(poly);
		DTSweep.Triangulate(tcx);


		int indexNumber = 0;
		bool multiSprites = false;
		foreach (DelaunayTriangle triangle in poly.Triangles)
		{
			Vector3 v = new Vector3();
			foreach (TriangulationPoint p in triangle.Points)
			{
				v = new Vector3((float)p.X, (float)p.Y,0);
				if(!resultsLocal.Contains(v))
				{
					resultsLocal.Add(v);
					resultsTriIndexesLocal.Add(indexNumber);
					Vector2 newUv = new Vector2((v.x /bound.x) + 0.5f,  (v.y /bound.y)  + 0.5f);

					newUv.x *= rec.width/ spr.texture.width;
					newUv.y *= rec.height/ spr.texture.height;
					//Debug.Log(spr.textureRectOffset);         
					newUv.x += (rec.x)/ spr.texture.width;
					newUv.y += (rec.y) / spr.texture.height;

					//Debug.Log(Application.unityVersion);

					SpriteMetaData[] smdArray = textureImporter.spritesheet;
					Vector2 pivot = new Vector2(.0f,.0f);;

					for (int j = 0; j < smdArray.Length; j++)
					{
						if (smdArray[j].name == spr.name)
						{
							switch(smdArray[j].alignment)
							{
								case(0):
								smdArray[j].pivot = Vector2.zero;
								break;
								case(1):
								smdArray[j].pivot = new Vector2(0f,1f) -new Vector2(.5f,.5f);
								break;
								case(2):
								smdArray[j].pivot = new Vector2(0.5f,1f) -new Vector2(.5f,.5f);
								break;
								case(3):
								smdArray[j].pivot = new Vector2(1f,1f) -new Vector2(.5f,.5f);
								break;
								case(4):
								smdArray[j].pivot = new Vector2(0f,.5f) -new Vector2(.5f,.5f);
								break;
								case(5):
								smdArray[j].pivot = new Vector2(1f,.5f) -new Vector2(.5f,.5f);
								break;
								case(6):
								smdArray[j].pivot = new Vector2(0f,0f) -new Vector2(.5f,.5f);
								break;
								case(7):
								smdArray[j].pivot = new Vector2(0.5f,0f) -new Vector2(.5f,.5f);
								break;
								case(8):
								smdArray[j].pivot = new Vector2(1f,0f) -new Vector2(.5f,.5f);
								break;
								case(9):
								smdArray[j].pivot -= new Vector2(.5f,.5f);
								break;					
							}
							pivot = smdArray[j].pivot ;
						}
					}
					if(textureImporter.spriteImportMode == SpriteImportMode.Single)
						pivot = textureImporter.spritePivot-new Vector2(.5f,.5f);
					newUv.x += ((pivot.x)*rec.width)/ spr.texture.width;
					newUv.y += ((pivot.y)*rec.height)/ spr.texture.height;
					/*
					if(Application.unityVersion != "4.3.0f4")
					{

						Debug.Log(spr.textureRectOffset.x);
						newUv.x += (spr.textureRectOffset.x)/ spr.texture.width;
						newUv.y += (spr.textureRectOffset.y)/ spr.texture.height;
					}
					*/
					uvsLocal.Add(newUv);
					normalsLocal.Add(new Vector3(0,0,-1));


					indexNumber++;

				}
				else
				{
					resultsTriIndexesLocal.Add(resultsLocal.LastIndexOf(v));
				}
			}

		}
		if(!multiSprites)
			Debug.Log("Tip: Sprite Pivot should be set to Center, with no custom pivot before conversion");

		for (int j = resultsTriIndexesLocal.Count-1; j >=0; j--) 
		{
			resultsTriIndexesReversedLocal.Add(resultsTriIndexesLocal[j]);
		}

		results.AddRange(resultsLocal);
		resultsTriIndexes.AddRange(resultsTriIndexesLocal);
		resultsTriIndexesReversed.AddRange(resultsTriIndexesReversedLocal);
		uvs.AddRange(uvsLocal);
		normals.AddRange(normalsLocal);

		resultsLocal.Clear();
		resultsTriIndexesLocal.Clear();
		resultsTriIndexesReversedLocal.Clear();
		uvsLocal.Clear();
		normalsLocal.Clear();

		finalVertices = results.ToArray();

		finalNormals = normals.ToArray();
		finalUvs= uvs.ToArray();

		finalTriangles = resultsTriIndexesReversed.ToArray();

		//results.Clear();
		//resultsTriIndexesReversed.Clear();

	}
	public void CreateSimpleMesh(Vector2[] vertsToCopy, Transform transform)
	{
		Sprite spr = transform.GetComponent<SpriteRenderer>().sprite;
		Rect rec = spr.rect;
		Vector3 bound = transform.GetComponent<Renderer>().bounds.max- transform.GetComponent<Renderer>().bounds.min ;



		//transform vertices
		Vector3[] vertices = new Vector3[vertsToCopy.Length];
		Vector2[] uvs = new Vector2[vertsToCopy.Length];
		Vector3[] normals = new Vector3[vertsToCopy.Length];


		int idx = 0;
		for (int i = 0; i < vertices.Length; i++) 
		{
			vertices [idx] = new Vector3 (vertsToCopy [idx].x, vertsToCopy [idx].y, 0);
			normals [idx] = new Vector3 (0, 0, -1);
			Vector2 newUv = new Vector2 ((vertsToCopy [idx].x / bound.x) + 0.5f, (vertsToCopy [idx].y / bound.y) + 0.5f);
			newUv.x *= rec.width / spr.texture.width;
			newUv.y *= rec.height / spr.texture.height;
			//Debug.Log(spr.textureRectOffset);			
			newUv.x += (rec.x) / spr.texture.width;
			newUv.y += (rec.y) / spr.texture.height;
			uvs [idx] = newUv;
			idx++;
		}

		//transform triangles

		Puppet2D_Triangulator2 tri = new Puppet2D_Triangulator2(vertsToCopy);
		int[] triangles = tri.Triangulate();
		finalVertices = vertices;
		finalTriangles = triangles;
		finalUvs = uvs;
		finalNormals = normals;
	}
	public void CreateSubdividedMesh(Vector2[] vertsToCopy, Transform transform, int smoothLevel)
	{
		Sprite spr = transform.GetComponent<SpriteRenderer>().sprite;
		Rect rec = spr.rect;
		Vector3 bound = transform.GetComponent<Renderer>().bounds.max- transform.GetComponent<Renderer>().bounds.min ;
		TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(spr)) as TextureImporter;

		//Create triangle.NET geometry
		TriangleNet.Geometry.InputGeometry geometry = new TriangleNet.Geometry.InputGeometry(vertsToCopy.Length);

		//Add vertices
		foreach (Vector2 p in vertsToCopy)
		{
			geometry.AddPoint(p.x,p.y);
		}
		//Add segments
		for (int i=0;i<vertsToCopy.Length-1;i++) {
			geometry.AddSegment(i,i+1);
		}
		geometry.AddSegment(vertsToCopy.Length-1,0);

		//Triangulate, refine and smooth
		TriangleNet.Mesh triangleNetMesh = new TriangleNet.Mesh();
		if(smoothLevel>0)
			triangleNetMesh.behavior.MinAngle = 10;

		triangleNetMesh.Triangulate(geometry);
		if (smoothLevel > 0) 
		{
			if (smoothLevel > 1)
				triangleNetMesh.Refine (true);
			TriangleNet.Tools.Statistic statistic = new TriangleNet.Tools.Statistic ();
			statistic.Update (triangleNetMesh, 1);
			// Refine by setting a custom maximum area constraint.
			if (smoothLevel > 2)
				triangleNetMesh.Refine (statistic.LargestArea / 8);

			try {
				triangleNetMesh.Smooth ();
			} catch {
				//Debug.LogWarning("unable to smooth");
			}
			triangleNetMesh.Renumber ();
		}

		//transform vertices
		Vector3[] vertices = new Vector3[triangleNetMesh.Vertices.Count];
		Vector2[] uvs = new Vector2[triangleNetMesh.Vertices.Count];
		Vector3[] normals = new Vector3[triangleNetMesh.Vertices.Count];

		int idx = 0;
		foreach(TriangleNet.Data.Vertex v in triangleNetMesh.Vertices) 
		{

			vertices[idx] = new Vector3(	(float)v.X,	(float)v.Y,	0	);
			normals[idx]=new Vector3(0,0,-1);


			Vector2 newUv = new Vector2(((float)v.X /bound.x) + 0.5f,  ((float)v.Y /bound.y)  + 0.5f);			

			newUv.x *= rec.width/ spr.texture.width;
			newUv.y *= rec.height/ spr.texture.height;
			//Debug.Log(spr.textureRectOffset);			
			newUv.x += (rec.x)/ spr.texture.width;
			newUv.y += (rec.y) / spr.texture.height;

			SpriteMetaData[] smdArray = textureImporter.spritesheet;
			Vector2 pivot = new Vector2(.0f,.0f);;

			for (int i = 0; i < smdArray.Length; i++)
			{
				if (smdArray[i].name == spr.name)
				{
					switch(smdArray[i].alignment)
					{
						case(0):
						smdArray[i].pivot = Vector2.zero;
						break;
						case(1):
						smdArray[i].pivot = new Vector2(0f,1f) -new Vector2(.5f,.5f);
						break;
						case(2):
						smdArray[i].pivot = new Vector2(0.5f,1f) -new Vector2(.5f,.5f);
						break;
						case(3):
						smdArray[i].pivot = new Vector2(1f,1f) -new Vector2(.5f,.5f);
						break;
						case(4):
						smdArray[i].pivot = new Vector2(0f,.5f) -new Vector2(.5f,.5f);
						break;
						case(5):
						smdArray[i].pivot = new Vector2(1f,.5f) -new Vector2(.5f,.5f);
						break;
						case(6):
						smdArray[i].pivot = new Vector2(0f,0f) -new Vector2(.5f,.5f);
						break;
						case(7):
						smdArray[i].pivot = new Vector2(0.5f,0f) -new Vector2(.5f,.5f);
						break;
						case(8):
						smdArray[i].pivot = new Vector2(1f,0f) -new Vector2(.5f,.5f);
						break;
						case(9):
						smdArray[i].pivot -= new Vector2(.5f,.5f);
						break;					
					}
					pivot = smdArray[i].pivot ;
				}
			}
			if(textureImporter.spriteImportMode == SpriteImportMode.Single)
				pivot = textureImporter.spritePivot-new Vector2(.5f,.5f);
			newUv.x += ((pivot.x)*rec.width)/ spr.texture.width;
			newUv.y += ((pivot.y)*rec.height)/ spr.texture.height;


			uvs[idx] = newUv;
			idx++;
		}

		//transform triangles
		int[] triangles = new int[triangleNetMesh.Triangles.Count*3];
		idx = 0;
		foreach (TriangleNet.Data.Triangle t in triangleNetMesh.Triangles) 
		{
			triangles[idx++] = t.P1;
			triangles[idx++] = t.P0;
			triangles[idx++] = t.P2;
		}

		finalVertices = vertices;
		finalTriangles = triangles;
		finalUvs = uvs;
		finalNormals = normals;
    }
    public GameObject MakeFromVerts (bool ReverseNormals ,Vector3[] vertsToCopy, List<int> pathSplitIds, GameObject FFDGameObject)
    {
        bool overwrite = false;


        MeshedSprite =new GameObject();

        Undo.RegisterCreatedObjectUndo (MeshedSprite, "Created Mesh");
        mf = MeshedSprite.AddComponent<MeshFilter>();
        mr = MeshedSprite.AddComponent<MeshRenderer>();
        mesh = new Mesh();

        if(AssetDatabase.LoadAssetAtPath("Assets/Puppet2D/Models/"+FFDGameObject.transform.name+"_MESH.asset",typeof(Mesh)))
        {
            if(EditorUtility.DisplayDialog("Overwrite Asset?","Do you want to overwrite the current Mesh & Material?","Yes, Overwrite","No, Create New Mesh & Material"))
            {
                //mf.mesh = AssetDatabase.LoadAssetAtPath("Assets/Puppet2D/Models/"+transform.name+"_MESH.asset",typeof(Mesh))as Mesh;
                string meshPath = ("Assets/Puppet2D/Models/"+FFDGameObject.transform.name+"_MESH.asset");
                AssetDatabase.CreateAsset(mesh,meshPath);
                overwrite = true;
            }
            else
            {
                string meshPath = AssetDatabase.GenerateUniqueAssetPath("Assets/Puppet2D/Models/"+FFDGameObject.transform.name+"_MESH.asset");
                AssetDatabase.CreateAsset(mesh,meshPath);
            }
        }
        else
        {
            string meshPath = AssetDatabase.GenerateUniqueAssetPath("Assets/Puppet2D/Models/"+FFDGameObject.transform.name+"_MESH.asset");
            AssetDatabase.CreateAsset(mesh,meshPath);
        }

        mesh = CreateMeshFromVerts(vertsToCopy, mesh, pathSplitIds, FFDGameObject.transform); 

        mf.mesh = mesh;

        results.Clear();
        resultsTriIndexes.Clear();
        resultsTriIndexesReversed.Clear();
        uvs.Clear();
        normals.Clear();


        if(overwrite)
        {
            mr.material = AssetDatabase.LoadAssetAtPath("Assets/Puppet2D/Models/Materials/"+FFDGameObject.transform.name+"_MAT.mat",typeof(Material)) as Material;
        }
        else
        {

            Material newMat = new Material(Shader.Find("Unlit/Transparent"));
            string materialPath = AssetDatabase.GenerateUniqueAssetPath("Assets/Puppet2D/Models/Materials/"+FFDGameObject.transform.name+"_MAT.mat");
            AssetDatabase.CreateAsset(newMat, materialPath);
            mr.material = newMat;
        }




        return MeshedSprite;
    }
    public Mesh CreateMeshFromVerts(Vector3[] vertsToCopy, Mesh mesh, List<int> pathSplitIds, Transform SpriteGO = null)
    {      
        Sprite spr = new Sprite();
        Rect rec = new Rect();
        Vector3 bound = Vector3.zero;
        TextureImporter textureImporter = new TextureImporter();

        if(SpriteGO !=null && SpriteGO.GetComponent<SpriteRenderer>() && SpriteGO.GetComponent<SpriteRenderer>().sprite)
		{

            spr = SpriteGO.GetComponent<SpriteRenderer>().sprite;
			rec = spr.rect;
			bound = SpriteGO.GetComponent<Renderer>().bounds.max- SpriteGO.GetComponent<Renderer>().bounds.min ;
            textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(spr)) as TextureImporter;


		}

		//Create triangle.NET geometry
        TriangleNet.Geometry.InputGeometry geometry = new TriangleNet.Geometry.InputGeometry(vertsToCopy.Length);

        //Add vertices
        foreach (Vector3 p in vertsToCopy)
        {
            geometry.AddPoint(p.x,p.y);
        }
        //Add segments
		int prevEnd = 0;
        for (int i=0;i<vertsToCopy.Length-1;i++) 
		{
			if (!pathSplitIds.Contains (i+1)) 
			{
				geometry.AddSegment (i, i + 1);
				//Debug.Log ("joining " + i + " to " + (i + 1));
			}
			else
			{
				geometry.AddSegment (i, prevEnd);
				prevEnd = i + 1;
			}

        }
        if (pathSplitIds.Count <= 1)
        {
            //Debug.Log ("joining " + (vertsToCopy.Length - 1) + " to 0");
            geometry.AddSegment(vertsToCopy.Length - 1, 0);
        }


        //Triangulate, refine and smooth
        TriangleNet.Mesh triangleNetMesh = new TriangleNet.Mesh();
       
        triangleNetMesh.Triangulate(geometry);

        //transform vertices
        Vector3[] vertices = new Vector3[triangleNetMesh.Vertices.Count];
        Vector2[] uvs = new Vector2[triangleNetMesh.Vertices.Count];
        Vector3[] normals = new Vector3[triangleNetMesh.Vertices.Count];

        int idx = 0;
        foreach(TriangleNet.Data.Vertex v in triangleNetMesh.Vertices) 
        {

            vertices[idx] = new Vector3(    (float)v.X, (float)v.Y, 0   );
            normals[idx]=new Vector3(0,0,-1);

            if (SpriteGO != null && SpriteGO.GetComponent<SpriteRenderer>() ) 
			{
				Vector2 newUv = new Vector2 (((float)v.X / bound.x) + 0.5f , ((float)v.Y / bound.y) + 0.5f);          

				newUv.x *= rec.width / spr.texture.width;
				newUv.y *= rec.height / spr.texture.height;
				//Debug.Log(spr.textureRectOffset);         
				newUv.x += (rec.x) / spr.texture.width;
				newUv.y += (rec.y) / spr.texture.height;

				SpriteMetaData[] smdArray = textureImporter.spritesheet;
				Vector2 pivot = new Vector2 (.0f, .0f);
				;

				for (int i = 0; i < smdArray.Length; i++) {
					if (smdArray [i].name == spr.name) {
						switch (smdArray [i].alignment) {
						case(0):
							smdArray [i].pivot = Vector2.zero;
							break;
						case(1):
							smdArray [i].pivot = new Vector2 (0f, 1f) - new Vector2 (.5f, .5f);
							break;
						case(2):
							smdArray [i].pivot = new Vector2 (0.5f, 1f) - new Vector2 (.5f, .5f);
							break;
						case(3):
							smdArray [i].pivot = new Vector2 (1f, 1f) - new Vector2 (.5f, .5f);
							break;
						case(4):
							smdArray [i].pivot = new Vector2 (0f, .5f) - new Vector2 (.5f, .5f);
							break;
						case(5):
							smdArray [i].pivot = new Vector2 (1f, .5f) - new Vector2 (.5f, .5f);
							break;
						case(6):
							smdArray [i].pivot = new Vector2 (0f, 0f) - new Vector2 (.5f, .5f);
							break;
						case(7):
							smdArray [i].pivot = new Vector2 (0.5f, 0f) - new Vector2 (.5f, .5f);
							break;
						case(8):
							smdArray [i].pivot = new Vector2 (1f, 0f) - new Vector2 (.5f, .5f);
							break;
						case(9):
							smdArray [i].pivot -= new Vector2 (.5f, .5f);
							break;                  
						}
						pivot = smdArray [i].pivot;
					}
				}
				if (textureImporter.spriteImportMode == SpriteImportMode.Single)
					pivot = textureImporter.spritePivot - new Vector2 (.5f, .5f);
				newUv.x += ((pivot.x) * rec.width) / spr.texture.width;
				newUv.y += ((pivot.y) * rec.height) / spr.texture.height;


				uvs [idx] = newUv;
			}


            idx++;
        }

        //transform triangles
        int[] triangles = new int[triangleNetMesh.Triangles.Count*3];
        idx = 0;
        foreach (TriangleNet.Data.Triangle t in triangleNetMesh.Triangles) 
        {
            triangles[idx++] = t.P1;
            triangles[idx++] = t.P0;
            triangles[idx++] = t.P2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = normals;

        return mesh;
    }
    List<Vector2> randomizeArray(List<Vector2> arr )
    {
        int counter = arr.Count;
        List <Vector2> reArr = new List <Vector2>();

        while (counter-- >= 1)
        {
            int rndM = Random.Range(0, arr.Count-1);
            reArr.Add(arr[rndM]);
            arr.RemoveAt(rndM);
        }

        return reArr;
    }
}
