using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;

public class Puppet2D_Skinning : Editor 
{
    [MenuItem ("GameObject/Puppet2D/Skin/ConvertSpriteToMesh")]
    public static void ConvertSpriteToMesh(int triIndex)
    {
        GameObject[] selection = Selection.gameObjects;
        foreach(GameObject spriteGO in selection)
        {
            if(spriteGO.GetComponent<SpriteRenderer>())
            {
                string spriteName = spriteGO.GetComponent<SpriteRenderer>().sprite.name;
                if(spriteName.Contains("Bone"))
                {
                    Debug.LogWarning("You can't convert Bones to Mesh");
                    return;
                }
                if((spriteName=="orientControl")||(spriteName=="parentControl")||(spriteName=="VertexHandleControl")||(spriteName=="IKControl"))
                {
                    Debug.LogWarning("You can't convert Controls to Mesh");
                    return;
                }
                PolygonCollider2D polyCol;
                GameObject MeshedSprite;
                Quaternion rot = spriteGO.transform.rotation;
                spriteGO.transform.eulerAngles = Vector3.zero;
                int layer = spriteGO.layer;
				string sortingLayer = spriteGO.GetComponent<Renderer>().sortingLayerName;
				int sortingOrder = spriteGO.GetComponent<Renderer>().sortingOrder;


                if(spriteGO.GetComponent<PolygonCollider2D>()==null)
                {
                    polyCol = Undo.AddComponent<PolygonCollider2D> (spriteGO);
                    Puppet2D_CreatePolygonFromSprite polyFromSprite = ScriptableObject.CreateInstance("Puppet2D_CreatePolygonFromSprite") as Puppet2D_CreatePolygonFromSprite;
                    MeshedSprite = polyFromSprite.Run(spriteGO.transform, true,triIndex);

                    MeshedSprite.name = (spriteGO.name+"_GEO");
                    DestroyImmediate(polyFromSprite);
                    Undo.DestroyObjectImmediate(polyCol);



                }
                else
                {
                    polyCol = spriteGO.GetComponent<PolygonCollider2D>();

                    Puppet2D_CreatePolygonFromSprite polyFromSprite = ScriptableObject.CreateInstance("Puppet2D_CreatePolygonFromSprite") as Puppet2D_CreatePolygonFromSprite;
                    MeshedSprite = polyFromSprite.Run(spriteGO.transform, true,triIndex);

                    MeshedSprite.name = (spriteGO.name+"_GEO");

                    DestroyImmediate(polyFromSprite); 
                    Undo.DestroyObjectImmediate(polyCol);

                }
                MeshedSprite.layer = layer;
				MeshedSprite.GetComponent<Renderer>().sortingLayerName = sortingLayer;
				MeshedSprite.GetComponent<Renderer>().sortingOrder = sortingOrder;
                MeshedSprite.AddComponent<Puppet2D_SortingLayer>();


                MeshedSprite.transform.position = spriteGO.transform.position;
                MeshedSprite.transform.rotation = rot;

                Sprite spriteInfo = spriteGO.GetComponent<SpriteRenderer>().sprite;

                TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(spriteInfo)) as TextureImporter;

				MeshedSprite.GetComponent<Renderer>().sharedMaterial.shader = Shader.Find("Unlit/Transparent");

				MeshedSprite.GetComponent<Renderer>().sharedMaterial.SetTexture("_MainTex", spriteInfo.texture);

                textureImporter.textureType = TextureImporterType.Sprite;

                DestroyImmediate(spriteGO);

                Selection.activeGameObject = MeshedSprite;

            }
            else
            {
                Debug.LogWarning("Object is not a sprite");
                return;
            }
        }
    }


    [MenuItem ("GameObject/Puppet2D/Skin/Parent Mesh To Bones")]
    public static void BindRigidSkin()
    {
        GameObject[] selection = Selection.gameObjects;
        List<GameObject> selectedBones = new List<GameObject>();
        List<GameObject> selectedMeshes= new List<GameObject>();


        foreach (GameObject Obj in selection)
        {
            if(Obj.GetComponent<SpriteRenderer>())
            {
                if (Obj.GetComponent<SpriteRenderer>().sprite && Obj.GetComponent<SpriteRenderer>().sprite.name.Contains("ffd"))
                    selectedMeshes.Add(Obj.transform.parent.gameObject);
                else
                {
                    if (Obj.GetComponent<SpriteRenderer>().sprite.name.Contains("Bone"))
                    {
                        if (Obj.transform.childCount > 0)
                        {
                            foreach (Transform child in Obj.transform)
                            {
                                if (child.GetComponent<Puppet2D_HiddenBone>())
                                {
                                    selectedBones.Add(child.gameObject);
                                }
                            }
                        }
                        else
                            selectedBones.Add(Obj);

                    }
                    else
                    {
                        selectedMeshes.Add(Obj);
                    }
                }
            }
            else
            {
                selectedMeshes.Add(Obj);
            }
        }

        if((selectedBones.Count == 0)||(selectedMeshes.Count==0))
        {
            Debug.LogWarning("You need to select at least one bone and one other object");
            return;
        }
        foreach (GameObject mesh in selectedMeshes)
        {
            float testdist = 1000000;
            GameObject closestBone =  null;
            foreach (GameObject bone in selectedBones)
            {
				float dist = Vector2.Distance(new Vector2(bone.GetComponent<Renderer>().bounds.center.x,bone.GetComponent<Renderer>().bounds.center.y), new Vector2(mesh.transform.position.x,mesh.transform.position.y));
                if (dist < testdist)
                {
                    testdist = dist;
                    //Debug.Log("closest bone to " + mesh.name + " is " + bone.name + " distance " + dist);
                    if(bone.GetComponent<Puppet2D_HiddenBone>())
                        closestBone = bone.transform.parent.gameObject;
                    else
                        closestBone = bone;

                }

            }
            Undo.SetTransformParent (mesh.transform, closestBone.transform, "parent bone");

        }

    }
    static bool ContainsPoint (Vector3[] polyPoints, Vector3 p) 
    { 
        bool inside = false; 
        float a1 = Vector3.Angle(polyPoints[0] - p, polyPoints[1] - p);
        float a2 = Vector3.Angle(polyPoints[1] - p, polyPoints[2] - p);
        float a3 = Vector3.Angle(polyPoints[2] - p, polyPoints[0] - p);

        if (Mathf.Abs ((a1 + a2 + a3) - 360) < 0.1f) {
            inside = true;
            //Debug.Log((a1 + a2 + a3));
        }
        //        for (int index = 0; index < polyPoints.Length; j = index++) 
        //        { 
        //            if ( ((polyPoints[index].y <= p.y && p.y < polyPoints[j].y) || (polyPoints[j].y <= p.y && p.y < polyPoints[index].y)) && 
        //                (p.x < (polyPoints[j].x - polyPoints[index].x) * (p.y - polyPoints[index].y) / (polyPoints[j].y - polyPoints[index].y) + polyPoints[index].x)) 
        //                inside = !inside; 
        //        } 
        return inside; 
    }
    static Vector3 Barycentric(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
    {
        Vector3 v0 = b - a; 
        Vector3 v1 = c - a;
        Vector3 v2 = p - a;
        float d00 = Vector3.Dot(v0, v0);
        float d01 = Vector3.Dot(v0, v1);
        float d11 = Vector3.Dot(v1, v1);
        float d20 = Vector3.Dot(v2, v0);
        float d21 = Vector3.Dot(v2, v1);
        float denom = d00 * d11 - d01 * d01;


        float v = (d11 * d20 - d01 * d21) / denom;
        float w = (d00 * d21 - d01 * d20) / denom;
        float u = 1.0f - v - w;
        return new Vector3(v, w, u);
    }
    [MenuItem ("GameObject/Puppet2D/Skin/Bind Smooth Skin")]
    public static GameObject BindSmoothSkin()
    {
        GameObject[] selection = Selection.gameObjects;
        List<Transform> selectedBones = new List<Transform>();
        List<GameObject> selectedMeshes= new List<GameObject>();
        List<GameObject> ffdControls= new List<GameObject>();

        foreach (GameObject Obj in selection)
        {
            if (Obj.GetComponent<SpriteRenderer>()== null)
            {
                if ((Obj.GetComponent<MeshRenderer>())||(Obj.GetComponent<SkinnedMeshRenderer>()))
                {
                    selectedMeshes.Add(Obj);
                }
                else
                {
                    Debug.LogWarning("Please select a mesh with a MeshRenderer, and some bones");
                    //return null;
                }

            }
            else if (Obj.GetComponent<SpriteRenderer>().sprite.name.Contains("Bone"))
            {
                if (Obj.GetComponent<SpriteRenderer>().sprite.name.Contains("ffdBone"))
                    ffdControls.Add(Obj);

                selectedBones.Add(Obj.transform);
                if (Obj.GetComponent<SpriteRenderer>().sprite.name.Contains("BoneScaled"))
                    Obj.GetComponent<SpriteRenderer>().sprite = Puppet2D_Editor.boneOriginal;


            }
            else
            {
                Debug.LogWarning("Please select a mesh with a MeshRenderer, not a sprite");
                //return null;
            }
        }
        if (selectedBones.Count == 0)
        {
            if (selectedMeshes.Count > 0)
            {
                if(EditorUtility.DisplayDialog("Detatch Skin?","Do you want to detatch the Skin From the bones?", "Detach", "Do Not Detach")) 
                {
                    foreach (GameObject mesh in selectedMeshes)
                    {
                        SkinnedMeshRenderer smr = mesh.GetComponent<SkinnedMeshRenderer>();
                        if (smr)
                        {
                            Material mat = smr.sharedMaterial;
                            Undo.DestroyObjectImmediate(smr);
                            MeshRenderer mr = mesh.AddComponent<MeshRenderer>();
                            mr.sharedMaterial = mat;
                        }
                    }
                    return null;
                }



            }
            return null;
        }
        for (int i = selectedMeshes.Count - 1; i >= 0; i--)
        {
            // check to make sure its not a FFD mesh
            GameObject mesh = selectedMeshes[i];
            Puppet2D_FFDLineDisplay[] allFFDPointsInScene = Transform.FindObjectsOfType<Puppet2D_FFDLineDisplay>();
            bool isFFDMesh = false;
            foreach (Puppet2D_FFDLineDisplay ffdPoint in allFFDPointsInScene)
            {
                if (ffdPoint.outputSkinnedMesh && ffdPoint.outputSkinnedMesh.gameObject == mesh)
                {
                    ffdControls.Add(ffdPoint.gameObject);
                    selectedBones.Add(ffdPoint.transform);
                    isFFDMesh = true;
                }
            }
            if (isFFDMesh)
                selectedMeshes.Remove(mesh);
        }
        if ((ffdControls.Count > 0)&&selectedMeshes.Count==0 && ffdControls[0].GetComponent<Puppet2D_FFDLineDisplay>().outputSkinnedMesh)
        {
            GameObject preSkinnedMesh = new GameObject();
            MeshFilter mf = preSkinnedMesh.AddComponent<MeshFilter>();
            preSkinnedMesh.AddComponent<MeshRenderer>();
            Mesh mesh = new Mesh();
            ffdControls[0].GetComponent<Puppet2D_FFDLineDisplay>().outputSkinnedMesh.BakeMesh(mesh);
            mf.sharedMesh = mesh ;

            List<Object> newObjs = new List<Object>();
            foreach (Transform tr in selectedBones)
            {
                if(tr.GetComponent<SpriteRenderer>() && tr.GetComponent<SpriteRenderer>().sprite && !tr.GetComponent<SpriteRenderer>().sprite.name.Contains("ffd")&&!tr.GetComponent<Puppet2D_HiddenBone>())
                    newObjs.Add(tr.gameObject);
            }
            newObjs.Add(preSkinnedMesh);
            Selection.objects = newObjs.ToArray(); 
            GameObject newGO = BindSmoothSkin();
            foreach (GameObject go in ffdControls)
            {
                go.GetComponent<Puppet2D_FFDLineDisplay>().skinnedMesh = newGO.GetComponent<SkinnedMeshRenderer>();
                go.GetComponent<Puppet2D_FFDLineDisplay>().Init();
            }
            Undo.DestroyObjectImmediate(newGO);

            return preSkinnedMesh;   
        }
        foreach (GameObject mesh in selectedMeshes)
        {

            Material mat = null;
            string sortingLayer = "";
            int sortingOrder = 0;
            if(mesh.GetComponent<MeshRenderer>()!=null )
            {
                mat = mesh.GetComponent<MeshRenderer>().sharedMaterial;

				sortingLayer = mesh.GetComponent<Renderer>().sortingLayerName;
				sortingOrder = mesh.GetComponent<Renderer>().sortingOrder;

                Undo.DestroyObjectImmediate(mesh.GetComponent<MeshRenderer>());
            }

            SkinnedMeshRenderer renderer = mesh.GetComponent<SkinnedMeshRenderer>();
            if(renderer == null)
                renderer = Undo.AddComponent<SkinnedMeshRenderer>(mesh);



            Puppet2D_SortingLayer puppet2D_SortingLayer = mesh.GetComponent<Puppet2D_SortingLayer>();
            if(puppet2D_SortingLayer != null)
                Undo.DestroyObjectImmediate(puppet2D_SortingLayer);


            Mesh sharedMesh = mesh.transform.GetComponent<MeshFilter>().sharedMesh;
            Vector3[] verts = sharedMesh.vertices;

            Matrix4x4[] bindPoses = new Matrix4x4[selectedBones.Count];


            List<Transform> closestBones =  new List<Transform>();
            closestBones.Clear();
            BoneWeight[] weights = new BoneWeight[verts.Length];
            int index = 0;
            int index2 = 0;
            int index3 = 0;

            for (int j = 0; j < weights.Length; j++)
            {
                float testdist = 1000000;
                float testdist2 = 1000000;
                for (int i = 0; i < selectedBones.Count; i++)
                {

                    Vector3 worldPt = mesh.transform.TransformPoint(verts[j]);

					float dist = Vector2.Distance(new Vector2(selectedBones[i].GetComponent<Renderer>().bounds.center.x,selectedBones[i].GetComponent<Renderer>().bounds.center.y), new Vector2(worldPt.x,worldPt.y));

                    if (dist < testdist)
                    {
                        testdist = dist;
                        index = selectedBones.IndexOf(selectedBones[i]);

                    }


                    Transform bone = selectedBones[i];
                    bindPoses[i] = bone.worldToLocalMatrix * mesh.transform.localToWorldMatrix;
                }
                for (int i = 0; i < selectedBones.Count; i++)
                {
                    if(!(index==(selectedBones.IndexOf(selectedBones[i]))))
                    {
                        Vector3 worldPt = mesh.transform.TransformPoint(verts[j]);
						float dist = Vector2.Distance(new Vector2(selectedBones[i].GetComponent<Renderer>().bounds.center.x,selectedBones[i].GetComponent<Renderer>().bounds.center.y), new Vector2(worldPt.x,worldPt.y));

                        if (dist < testdist2)
                        {
                            testdist2 = dist;
                            index2 = selectedBones.IndexOf(selectedBones[i]);                           


                        }
                    }

                }

                float combinedDistance = testdist+testdist2;
                float weight1 = (testdist/combinedDistance);
                float weight2 =  (testdist2/combinedDistance);
                weight1 = Mathf.Lerp(1, 0, weight1);
                weight2 = Mathf.Lerp(1, 0, weight2);

                weight1= Mathf.Clamp01((weight1+0.5f)*(weight1+0.5f)*(weight1+0.5f) - 0.5f);
                weight2= Mathf.Clamp01((weight2+0.5f)*(weight2+0.5f)*(weight2+0.5f) - 0.5f);

                if (Puppet2D_Editor._numberBonesToSkinToIndex == 1)
                {
                    weights [j].boneIndex0 = index;
                    weights [j].weight0 = weight1;
                    weights [j].boneIndex1 = index2;
                    weights [j].weight1 = weight2;
                } 
                else if(Puppet2D_Editor._numberBonesToSkinToIndex == 2)
                {

                    Vector3 worldPt = mesh.transform.TransformPoint(verts[j]);
                    //Vector3 worldPt = verts[j];

                    renderer.quality = SkinQuality.Bone4;

                    if (ffdControls.Count == 0)
                    {
                        Debug.LogWarning("You must select some FFD controls to bind to");
                        return null;
                    }
                    if (ffdControls[0].GetComponent<Puppet2D_FFDLineDisplay>().outputSkinnedMesh == null || ffdControls[0].GetComponent<Puppet2D_FFDLineDisplay>().outputSkinnedMesh.sharedMesh == null)
                    {
                        Debug.LogWarning("You need the original FFD output mesh to copy skin weights. Make sure the outputSkinnedMesh is assigned to the ffdControl");
                        return null;
                    }
                    if (!ffdControls[0].transform.parent || !ffdControls[0].transform.parent.parent)
                    {
                        Debug.LogWarning("Your FFD Controls need a parent Group for offset");
                        return null;
                    }

                    int[] tris = ffdControls[0].GetComponent<Puppet2D_FFDLineDisplay>().outputSkinnedMesh.sharedMesh.triangles;
                    Vector3[] ffdMeshVerts = ffdControls[0].GetComponent<Puppet2D_FFDLineDisplay>().outputSkinnedMesh.sharedMesh.vertices;

                    bool insideTriangle = false;
                    for (int t =0; t<tris.Length-2; t+=3)
                    {
                        Vector3[] polygon = new Vector3[3];
                        polygon[0] = ffdControls[0].transform.parent.parent.TransformPoint(ffdMeshVerts[tris[t]]);
                        polygon[1] = ffdControls[0].transform.parent.parent.TransformPoint(ffdMeshVerts[tris[t+1]]);
                        polygon[2] = ffdControls[0].transform.parent.parent.TransformPoint(ffdMeshVerts[tris[t+2]]);

                        //Debug.Log(worldPt+" "+polygon[0]+" "+polygon[1]+" "+polygon[2]);
                        if (ContainsPoint(polygon, worldPt))
                        {                           
                            index = Puppet2D_FFD.GetIndexOfVector3(ffdControls,polygon[0] );
                            index2 = Puppet2D_FFD.GetIndexOfVector3(ffdControls,polygon[1] );
                            index3 = Puppet2D_FFD.GetIndexOfVector3(ffdControls,polygon[2] );
                            insideTriangle = true;
                        }


                    }
                    if(insideTriangle)
                    {
                        Vector3 weightBary = Barycentric(ffdControls[index].transform.position, ffdControls[index2].transform.position, ffdControls[index3].transform.position, worldPt);
                        //Debug.Log(ffdControls[index] + " " + ffdControls[index2] + " " + ffdControls[index3]);
                        //if (index != -1 && weights[j].weight0 > 0)
                        {
                            weights[j].boneIndex0 = index;
                            weights[j].weight0 = weightBary.z;
                        }
                        //if (index2 != -1 && weights[j].weight1 > 0)
                        {
                            weights[j].boneIndex1 = index2;
                            weights[j].weight1 = weightBary.x;
                        }
                        //if (index3 != -1 && weights[j].weight2 > 0)
                        {
                            weights[j].boneIndex2 = index3;
                            weights[j].weight2 = weightBary.y;
                        }



                    }
                    else
                    {
                        weights [j].boneIndex0 = 0;
                        weights [j].weight0 = 1;

                    }

                }
                else
                {

					weights [j].boneIndex0 = index;
					weights [j].weight0 = 1;
					
                }

            }

            sharedMesh.boneWeights = weights;

            sharedMesh.bindposes = bindPoses;

            renderer.bones = selectedBones.ToArray();

            renderer.sharedMesh = sharedMesh;
            if(mat)
                renderer.sharedMaterial = mat;

            renderer.sortingLayerName = sortingLayer;
            renderer.sortingOrder = sortingOrder;
            mesh.AddComponent<Puppet2D_SortingLayer>();



        }
        foreach (Transform bone in selectedBones) 
        {
            if (bone.GetComponent<SpriteRenderer> ().sprite.name=="Bone")
                bone.GetComponent<SpriteRenderer> ().sprite = Puppet2D_Editor.boneSprite;
        }
        if (selectedMeshes.Count > 0)
            return selectedMeshes[0];
        else
            return null;
    }

    [MenuItem ("GameObject/Puppet2D/Skin/Edit Skin Weights")]
    public static bool EditWeights()
    {
        GameObject[] selection = Selection.gameObjects;

        foreach(GameObject sel in selection)
        {
            if ((sel.GetComponent<Puppet2D_Bakedmesh>() != null))
            {
                Debug.LogWarning("Already in edit mode");
                return false;
            }
            if ((sel.GetComponent<SkinnedMeshRenderer>()))
            {
                SkinnedMeshRenderer renderer = sel.GetComponent<SkinnedMeshRenderer>();
                Undo.RecordObject(sel, "add mesh to meshes being editted");
                Undo.AddComponent<Puppet2D_Bakedmesh>(sel);
                Mesh mesh = sel.GetComponent<MeshFilter>().sharedMesh;


                Vector3[] verts = mesh.vertices;
                BoneWeight[] boneWeights = mesh.boneWeights;

                for (int i = 0; i < verts.Length; i++)
                {
                    Vector3 vert = verts[i];
                    Vector3 vertPos = sel.transform.TransformPoint(vert);
                    GameObject handle = new GameObject("vertex" + i);
                    Undo.RegisterCreatedObjectUndo (handle, "vertex created");
                    handle.transform.position = vertPos;
                    Undo.SetTransformParent(handle.transform, sel.transform, "parent handle");

                    SpriteRenderer spriteRenderer = Undo.AddComponent<SpriteRenderer>(handle);
                    string path = ("Assets/Puppet2D/Textures/GUI/VertexHandle.psd");
                    Sprite sprite = AssetDatabase.LoadAssetAtPath(path, typeof(Sprite)) as Sprite;
                    spriteRenderer.sprite = sprite;
                    spriteRenderer.sortingLayerName = Puppet2D_Editor._controlSortingLayer;
                    Puppet2D_EditSkinWeights editSkinWeights = Undo.AddComponent<Puppet2D_EditSkinWeights>(handle);

                    editSkinWeights.verts = mesh.vertices;

                    editSkinWeights.Weight0 = boneWeights[i].weight0;
                    editSkinWeights.Weight1 = boneWeights[i].weight1;
                    editSkinWeights.Weight2 = boneWeights[i].weight2;
                    editSkinWeights.Weight3 = boneWeights[i].weight3;

                    if (boneWeights[i].weight0 > 0)
                    {
                        editSkinWeights.Bone0 = renderer.bones[boneWeights[i].boneIndex0].gameObject;
                        editSkinWeights.boneIndex0 = boneWeights[i].boneIndex0;
                    }
                    else
                        editSkinWeights.Bone0 = null;

                    if (boneWeights[i].weight1 > 0)
                    {
                        editSkinWeights.Bone1 = renderer.bones[boneWeights[i].boneIndex1].gameObject;
                        editSkinWeights.boneIndex1 = boneWeights[i].boneIndex1;
                    }
                    else
                    {
                        editSkinWeights.Bone1 = null;
                        editSkinWeights.boneIndex1 = renderer.bones.Length;
                    }

                    if (boneWeights[i].weight2 > 0)
                    {
                        editSkinWeights.Bone2 = renderer.bones[boneWeights[i].boneIndex2].gameObject;
                        editSkinWeights.boneIndex2 = boneWeights[i].boneIndex2;
                    }
                    else
                    {
                        editSkinWeights.Bone2 = null;
                        editSkinWeights.boneIndex2 = renderer.bones.Length;
                    }

                    if (boneWeights[i].weight3 > 0)
                    {
                        editSkinWeights.Bone3 = renderer.bones[boneWeights[i].boneIndex3].gameObject;
                        editSkinWeights.boneIndex3 = boneWeights[i].boneIndex3;
                    }
                    else
                    {
                        editSkinWeights.Bone3 = null;
                        editSkinWeights.boneIndex3 = renderer.bones.Length;
                    }

                    editSkinWeights.mesh = mesh;
                    editSkinWeights.meshRenderer = renderer;
                    editSkinWeights.vertNumber = i;
                }

            }
            else
            {
                Debug.LogWarning("Selection does not have a meshRenderer");
                return false;
            }


        }
        return true;
    }

    [MenuItem ("GameObject/Puppet2D/Skin/Finish Editting Skin Weights")]
    public static Object[] FinishEditingWeights()
    {
        SpriteRenderer[] sprs = FindObjectsOfType<SpriteRenderer>();
        Puppet2D_Bakedmesh[] skinnedMeshesBeingEditted = FindObjectsOfType<Puppet2D_Bakedmesh>();
        List<Object> returnObjects = new List<Object>();
        foreach(SpriteRenderer spr in sprs)
        {
            if(spr.sprite)      
                if(spr.sprite.name.Contains("Bone"))            
                    spr.gameObject.GetComponent<SpriteRenderer>().color = Color.white;                  

        }
        foreach(Puppet2D_Bakedmesh bakedMesh in skinnedMeshesBeingEditted)
        {
            GameObject sel = bakedMesh.gameObject;
            returnObjects.Add(sel);

            DestroyImmediate(bakedMesh);

            int numberChildren = sel.transform.childCount;
            List<GameObject> vertsToDestroy = new List<GameObject>();
            for(int i = 0;i< numberChildren;i++)
            {
                vertsToDestroy.Add(sel.transform.GetChild(i).gameObject);


            }
            foreach(GameObject vert in vertsToDestroy)
                DestroyImmediate(vert);
        }
        return returnObjects.ToArray();
    }

    static Mesh SmoothSkinWeights(Mesh sharedMesh)
    {
        Debug.Log("smoothing weights");
        int[] triangles = sharedMesh.GetTriangles(0);
        BoneWeight[] boneWeights = sharedMesh.boneWeights;

        for(int i =0;i<triangles.Length;i+=3)
        {
            BoneWeight v1 = boneWeights[triangles[i]];
            BoneWeight v2 = boneWeights[triangles[i+1]];
            BoneWeight v3 = boneWeights[triangles[i+2]];

            List<int> v1Bones = new List<int>(new int[] {v1.boneIndex0,v1.boneIndex1,v1.boneIndex2,v1.boneIndex3 });
            List<int> v2Bones = new List<int>(new int[]  {v2.boneIndex0,v2.boneIndex1,v2.boneIndex2,v2.boneIndex3 });
            List<int> v3Bones = new List<int>(new int[]  {v3.boneIndex0,v3.boneIndex1,v3.boneIndex2,v3.boneIndex3 });

            List<float> v1Weights = new List<float>(new float[] {v1.weight0,v1.weight1,v1.weight2,v1.weight3 });
            List<float> v2Weights = new List<float>(new float[]  {v2.weight0,v2.weight1,v2.weight2,v2.weight3 });
            List<float> v3Weights = new List<float>(new float[]  {v3.weight0,v3.weight1,v3.weight2,v3.weight3 });


            for (int j = 0; j < 2; j++)
            {
                for (int k = 0; k < 2; k++)
                {
                    if (v1Bones[j] == v2Bones[k])
                    {
                        for (int l = 0; l < 2; l++)
                        {
                            if (v1Bones[j] == v3Bones[l])
                            {

                                v1Weights[j] =(v1Weights[j]+v2Weights[k]+v3Weights[l])/3;
                                v2Weights[k] = (v1Weights[j]+v2Weights[k]+v3Weights[l])/3;
                                v3Weights[l] = (v1Weights[j]+v2Weights[k]+v3Weights[l])/3;


                            }
                        }
                    }
                }

            }
            boneWeights[triangles[i]].weight0 = v1Weights[0];
            boneWeights[triangles[i]].weight1 = v1Weights[1];


            boneWeights[triangles[i+1]].weight0 = v2Weights[0];
            boneWeights[triangles[i+1]].weight1 = v2Weights[1];


            boneWeights[triangles[i+2]].weight0 = v3Weights[0];
            boneWeights[triangles[i+2]].weight1 = v3Weights[1];


        }
        sharedMesh.boneWeights = boneWeights;
        return sharedMesh;
    }
	
	public static void DrawHandle(Vector3 mousepos) {
		
		Handles.DrawWireDisc(mousepos+Vector3.forward*10,Vector3.back,Puppet2D_Editor.EditSkinWeightRadius);
		
		Handles.color = Puppet2D_Editor.paintControlColor;
		Handles.DrawSolidDisc(mousepos+Vector3.forward*11,Vector3.back,Puppet2D_Editor.EditSkinWeightRadius*Puppet2D_Editor.paintWeightsStrength);	
		SceneView.RepaintAll();
 
	}
	public static void PaintWeights(Vector3 mousepos, float weightStrength) {
		
		
		Vector3[] vertices =  Puppet2D_Editor.currentSelectionMesh.vertices;
		Color[] colrs  =  Puppet2D_Editor.currentSelectionMesh.colors;	
		BoneWeight[] boneWeights =  Puppet2D_Editor.currentSelectionMesh.boneWeights;
		
		Vector3 pos = Puppet2D_Editor.currentSelection.transform.InverseTransformPoint(mousepos);
		Undo.RecordObject( Puppet2D_Editor.currentSelectionMesh, "Weight paint");
		pos = new Vector3(pos.x,pos.y,0);
		
		SkinnedMeshRenderer smr = Puppet2D_Editor.currentSelection.GetComponent<SkinnedMeshRenderer>();
		int boneIndex = smr.bones.ToList().IndexOf( Puppet2D_Editor.paintWeightsBone.transform);
		
        if (boneIndex < 0)
        {
            Debug.LogWarning(Puppet2D_Editor.paintWeightsBone.name + " is not connected to skin");
            return;
        }
		for (int i=0;i<vertices.Length;i++)
		{
			if (boneWeights [i].boneIndex0<0)
				boneWeights [i].boneIndex0 = 0;
			if (boneWeights [i].boneIndex1<0)
				boneWeights [i].boneIndex1 = 0;

			float sqrMagnitude = (vertices[i] - pos).magnitude;
			if (sqrMagnitude > Puppet2D_Editor.EditSkinWeightRadius)
				continue;
			if(weightStrength>0)
				colrs[i] = Color.Lerp( colrs[i],Color.white, Puppet2D_Editor.paintWeightsStrength*Puppet2D_Editor.paintWeightsStrength);
			else
				colrs[i] = Color.Lerp( colrs[i],Color.black,Puppet2D_Editor.paintWeightsStrength* Puppet2D_Editor.paintWeightsStrength);
			
			if (boneWeights[i].boneIndex0 == boneIndex)
			{
				//if (colrs[i].r != 0 || boneWeights[i].weight1 + boneWeights[i].weight2 + boneWeights[i].weight3 > 0)
				{
					boneWeights[i].weight0 = colrs[i].r;
					boneWeights[i].weight1 = 1-colrs[i].r;
				}
			}
			else if (boneWeights[i].boneIndex1 == boneIndex)
			{
				//if (colrs[i].r != 0 || boneWeights[i].weight0 + boneWeights[i].weight2 + boneWeights[i].weight3 > 0)
				{
					boneWeights[i].weight1 = colrs[i].r;
					boneWeights[i].weight0 = 1-colrs[i].r;
				}
				
			}
			/*else if (boneWeights[i].boneIndex2 == boneIndex)
                {
                    if (colrs[i].r != 0 || boneWeights[i].weight0 + boneWeights[i].weight1 + boneWeights[i].weight3 > 0)
                        boneWeights[i].weight2 = colrs[i].r;
                }
                else if (boneWeights[i].boneIndex3 == boneIndex)
                {
                    if (colrs[i].r != 0 || boneWeights[i].weight0 + boneWeights[i].weight1 + boneWeights[i].weight2 > 0)
                        boneWeights[i].weight3 = colrs[i].r;
                }*/
			else if (colrs[i].r != 0 || boneWeights[i].weight1 + boneWeights[i].weight2 + boneWeights[i].weight3 > 0)
			{
				if(boneWeights[i].weight0<boneWeights[i].weight1)
				{
					boneWeights[i].weight0 = colrs[i].r ;
					boneWeights[i].boneIndex0 = boneIndex;
					boneWeights[i].weight1 = 1-colrs[i].r ;
				}
				else
				{
					boneWeights[i].weight1 = colrs[i].r ;
					boneWeights[i].boneIndex1 = boneIndex;
					boneWeights[i].weight0 = 1-colrs[i].r ;
				}
			}

			
			
		}
		
		 Puppet2D_Editor.currentSelectionMesh.colors = colrs;
		 Puppet2D_Editor.currentSelectionMesh.boneWeights = boneWeights;
	}
	public static void PaintSmoothWeights(Vector3 mousepos) {
		
		
		Vector3[] vertices =  Puppet2D_Editor.currentSelectionMesh.vertices;
		Color[] colrs  =  Puppet2D_Editor.currentSelectionMesh.colors;	
		BoneWeight[] boneWeights =  Puppet2D_Editor.currentSelectionMesh.boneWeights;
        int[] tris =  Puppet2D_Editor.currentSelectionMesh.triangles;
		
		Vector3 pos = Puppet2D_Editor.currentSelection.transform.InverseTransformPoint(mousepos);
		Undo.RecordObject( Puppet2D_Editor.currentSelectionMesh, "Weight paint");
		pos = new Vector3(pos.x,pos.y,0);
		
		SkinnedMeshRenderer smr = Puppet2D_Editor.currentSelection.GetComponent<SkinnedMeshRenderer>();
		int boneIndex = smr.bones.ToList().IndexOf( Puppet2D_Editor.paintWeightsBone.transform);

        if (boneIndex < 0)
        {
            Debug.LogWarning(Puppet2D_Editor.paintWeightsBone.name + " is not connected to skin");
            return;
        }

//		List<int> vertsToSmoothA = new List<int>();
//        List<int> vertsToSmoothB = new List<int>();
//        List<int> vertsToSmoothC = new List<int>();

		for (int i=0;i<tris.Length;i++)
		{			

			if (boneWeights [tris[i]].boneIndex0<0)
				boneWeights [tris[i]].boneIndex0 = 0;
			if (boneWeights [tris[i]].boneIndex1<0)
				boneWeights [tris[i]].boneIndex1 = 0;

			int indexB = 0;
			int indexC = 0;

			if (i % 3 == 2)
			{
				indexB = tris[i-1];
				indexC = tris[i-2];
				
			}
			else if((i) % 3 == 1)
			{
				indexB = tris[i-1];
				indexC = tris[i+1];
			}
			else if((i) % 3 == 0)
			{

				indexB = tris[i+1];
				indexC = tris[i+2];

			}
            float sqrMagnitude = (vertices[tris[i]] - pos).magnitude;
			if (sqrMagnitude < Puppet2D_Editor.EditSkinWeightRadius)
			{
                
            	//Debug.Log("h");
				colrs[tris[i]] = Color.black;
				int blend =1;
				if (boneWeights [tris[i]].boneIndex0 == boneIndex)
				{
                	colrs[tris[i]] += new Color(boneWeights [tris[i]].weight0, boneWeights [tris[i]].weight0, boneWeights [tris[i]].weight0);
				}
				else if (boneWeights [tris[i]].boneIndex1 == boneIndex)
				{
                	colrs[tris[i]] += new Color(boneWeights [tris[i]].weight1, boneWeights [tris[i]].weight1, boneWeights [tris[i]].weight1);
				}
				else
				{
					if(boneWeights[tris[i]].weight0<boneWeights[tris[i]].weight1)
					{
						boneWeights[tris[i]].boneIndex0 = boneIndex;
					}
					else
					{
						boneWeights[tris[i]].boneIndex1 = boneIndex;
					}
				}


				if (boneWeights [indexB].boneIndex0 == boneIndex)
				{
					colrs[tris[i]] += new Color(boneWeights [indexB].weight0, boneWeights [indexB].weight0, boneWeights [indexB].weight0);
					blend++;
				}
				else if (boneWeights [indexB].boneIndex1 == boneIndex)
				{ 
					colrs[tris[i]] += new Color(boneWeights [indexB].weight1, boneWeights [indexB].weight1, boneWeights [indexB].weight1);
					blend++;

				}
				if (boneWeights [indexC].boneIndex0 == boneIndex)
				{
					blend++;
					colrs[tris[i]] += new Color(boneWeights [indexC].weight0, boneWeights [indexC].weight0, boneWeights [indexC].weight0);
				}
				else if (boneWeights [indexC].boneIndex1 == boneIndex)
				{ 
					blend++;
					colrs[tris[i]] += new Color(boneWeights [indexC].weight1, boneWeights [indexC].weight1, boneWeights [indexC].weight1);
				}

            	colrs[tris[i]] /= blend;
				if (boneWeights [tris[i]].boneIndex0 == boneIndex)
				{
					boneWeights [tris[i]].weight0 = Mathf.Lerp(boneWeights [tris[i]].weight0, colrs[tris[i]].r, Puppet2D_Editor.paintWeightsStrength*Puppet2D_Editor.paintWeightsStrength);
					boneWeights [tris[i]].weight1 = 1-boneWeights [tris[i]].weight0 ;
					colrs[tris[i]] = new Color (boneWeights [tris[i]].weight0,boneWeights [tris[i]].weight0,boneWeights [tris[i]].weight0) ;				
				}
				else if (boneWeights [tris[i]].boneIndex1 == boneIndex)
				{
					boneWeights [tris[i]].weight1 = Mathf.Lerp(boneWeights [tris[i]].weight1, colrs[tris[i]].r, Puppet2D_Editor.paintWeightsStrength*Puppet2D_Editor.paintWeightsStrength);
					boneWeights [tris[i]].weight0 = 1-boneWeights [tris[i]].weight1 ;
					colrs[tris[i]] = new Color (boneWeights [tris[i]].weight1,boneWeights [tris[i]].weight1,boneWeights [tris[i]].weight1) ;
				}

       	 	}


		}
        Puppet2D_Editor.currentSelectionMesh.colors = colrs;
        Puppet2D_Editor.currentSelectionMesh.boneWeights = boneWeights;

		
	}

	public static void ChangePaintRadius(Vector3 pos)
	{
		Puppet2D_Editor.EditSkinWeightRadius = (pos -  Puppet2D_Editor.ChangeRadiusStartPosition).x +  Puppet2D_Editor.ChangeRadiusStartValue;
		
	}
	public static void ChangePaintStrength(Vector3 pos)
	{
		
		Puppet2D_Editor.paintWeightsStrength = (pos -  Puppet2D_Editor.ChangeRadiusStartPosition).x*0.1f +  Puppet2D_Editor.ChangeRadiusStartValue;
		Puppet2D_Editor.paintWeightsStrength = Mathf.Clamp01(Puppet2D_Editor.paintWeightsStrength);
	}
	public static float GetNeighbourWeight(Vector3[] vertices ,BoneWeight[] boneWeights, List<int> indexes, int index, int boneIndex)
	{
		float distance = 1000000f;
		int closestIndex = indexes[0];
		for (int i = 0; i < indexes.Count; i++) 
		{
			float checkDistance = (vertices[indexes[i]] - vertices[index]).magnitude;
			if(checkDistance < distance)
			{
				closestIndex = indexes[i];
				distance =checkDistance;
			}
			
		}
		if(boneWeights[closestIndex].boneIndex0 == boneIndex)
			return boneWeights[closestIndex].weight0;
		if(boneWeights[closestIndex].boneIndex1 == boneIndex)
			return boneWeights[closestIndex].weight1;
		if(boneWeights[closestIndex].boneIndex2 == boneIndex)
			return boneWeights[closestIndex].weight2;
		if(boneWeights[closestIndex].boneIndex3 == boneIndex)
			return boneWeights[closestIndex].weight3;
		return 0;
		
	}
}
