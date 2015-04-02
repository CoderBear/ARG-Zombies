using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;

public class Puppet2D_FFD : Editor 
{
    public static GameObject FFDControlsGrp;
    public static Puppet2D_FFDStoreData ffdStoreData;
    public static void FFDCreationMode(Vector3 mousePos)
    {   
        string newCtrlName = "FFD_CTRL";
        string newCtrlGRPName = "FFD_CTRL_GRP";

        if(Puppet2D_Editor.FFDGameObject)
        {
            newCtrlName = Puppet2D_Editor.FFDGameObject.name +"_Ctrl";
            newCtrlGRPName = Puppet2D_Editor.FFDGameObject.name + "_Ctrl_GRP";
        }

        GameObject newCtrl = new GameObject(Puppet2D_BoneCreation.GetUniqueBoneName(newCtrlName));
        GameObject newCtrlGRP = new GameObject(Puppet2D_BoneCreation.GetUniqueBoneName(newCtrlGRPName));
        newCtrl.transform.parent = newCtrlGRP.transform;

        Undo.RegisterCreatedObjectUndo (newCtrl, "Created newCtrl");
        Undo.RegisterCreatedObjectUndo (newCtrlGRP, "Created newCtrlGRP");

        Undo.RecordObject (ffdStoreData, "Adding FFD Control");
        ffdStoreData.FFDCtrls.Add(newCtrl.transform);


        Puppet2D_FFDLineDisplay ffdline = newCtrl.AddComponent<Puppet2D_FFDLineDisplay>();
        ffdline.vertNumber = ffdStoreData.FFDCtrls.Count - 1;
        if(ffdStoreData.FFDCtrls.Count>1)
        {
            if (ffdStoreData.FFDPathNumber.Count > 0)
            {
                if (ffdStoreData.FFDCtrls.Count - 1 > ffdStoreData.FFDPathNumber[ffdStoreData.FFDPathNumber.Count - 1])
                    ffdline.target = ffdStoreData.FFDCtrls[ffdStoreData.FFDCtrls.Count - 2];
            }
            else
                ffdline.target = ffdStoreData.FFDCtrls[ffdStoreData.FFDCtrls.Count - 2];
        }



        newCtrlGRP.transform.position = new Vector3(mousePos.x, mousePos.y, 0);

        SpriteRenderer spriteRenderer = newCtrl.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingLayerName = Puppet2D_Editor._controlSortingLayer;
        string path = ("Assets/Puppet2D/Textures/GUI/ffdBone.psd");

        Sprite sprite =AssetDatabase.LoadAssetAtPath(path, typeof(Sprite)) as Sprite;
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingLayerName = Puppet2D_Editor._controlSortingLayer;


    }

    public static void FFDSetFirstPath()
    {
        FFDControlsGrp = new GameObject(Puppet2D_BoneCreation.GetUniqueBoneName("FFD_Ctrls_GRP"));
        Undo.RegisterCreatedObjectUndo(FFDControlsGrp, "undo create FFD");
        ffdStoreData = FFDControlsGrp.AddComponent <Puppet2D_FFDStoreData>();
        if ((Puppet2D_Editor.FFDGameObject != null) && Puppet2D_Editor.FFDGameObject.GetComponent<PolygonCollider2D>())
        {
            Vector2[] firstPath = Puppet2D_Editor.FFDGameObject.GetComponent<PolygonCollider2D>().GetPath(0);
            foreach (Vector2 pos in firstPath)
            {
                FFDCreationMode(pos);
            }
            CloseFFDPath();
        }

    }

    public static void CloseFFDPath ()
    {
        if (ffdStoreData !=null &&ffdStoreData.FFDCtrls.Count > 2)
        {
            if (ffdStoreData.FFDCtrls[ffdStoreData.FFDCtrls.Count - 1] && ffdStoreData.FFDCtrls[ffdStoreData.FFDCtrls.Count - 1].GetComponent<Puppet2D_FFDLineDisplay>().target2 == null)
            {
                if (ffdStoreData.FFDPathNumber.Count > 0)
                    ffdStoreData.FFDCtrls[ffdStoreData.FFDCtrls.Count - 1].GetComponent<Puppet2D_FFDLineDisplay>().target2 = ffdStoreData.FFDCtrls[ffdStoreData.FFDPathNumber[ffdStoreData.FFDPathNumber.Count - 1]];
                else
                    ffdStoreData.FFDCtrls[ffdStoreData.FFDCtrls.Count - 1].GetComponent<Puppet2D_FFDLineDisplay>().target2 = ffdStoreData.FFDCtrls[0];
                Undo.RecordObject (ffdStoreData, "Adding FFD Control");
                ffdStoreData.FFDPathNumber.Add(ffdStoreData.FFDCtrls.Count);
            }
        }

    }
    public static void FFDFinishCreation ()
    {
        if (ffdStoreData == null)
            return;
        Puppet2D_Editor.FFDCreation = false;
        CloseFFDPath();

        Texture spriteTexture = null;

        //GameObject FFDControlsGrp = new GameObject(Puppet2D_BoneCreation.GetUniqueBoneName("FFD_Ctrls_GRP"));

        if (Puppet2D_Editor.FFDGameObject)
        {
            spriteTexture = Puppet2D_Editor.FFDGameObject.GetComponent<SpriteRenderer>().sprite.texture;


            //            FFDControlsGrp.name = Puppet2D_Editor.GetUniqueBoneName(Puppet2D_Editor.FFDGameObject.GetComponent<SpriteRenderer>().sprite.texture.name);

            foreach (Transform FFDCtrl in ffdStoreData.FFDCtrls)
                FFDCtrl.transform.position = Puppet2D_Editor.FFDGameObject.transform.InverseTransformPoint(FFDCtrl.transform.position);


            FFDControlsGrp.transform.position = Puppet2D_Editor.FFDGameObject.transform.position;
            FFDControlsGrp.transform.rotation = Puppet2D_Editor.FFDGameObject.transform.rotation;
            FFDControlsGrp.transform.localScale = Puppet2D_Editor.FFDGameObject.transform.localScale;


            //            FFDControlsGrp.transform.position = Vector3.zero;
            //            FFDControlsGrp.transform.rotation = Quaternion.identity;
            //            FFDControlsGrp.transform.localScale = Vector3.one;


            Puppet2D_Editor.FFDGameObject.transform.position = Vector3.zero;
            Puppet2D_Editor.FFDGameObject.transform.rotation = Quaternion.identity;
            Puppet2D_Editor.FFDGameObject.transform.localScale = Vector3.one;

        }

        if ( ffdStoreData.FFDCtrls.Count < 3)
        {
            Undo.DestroyObjectImmediate(ffdStoreData);
            return;
        }

        Puppet2D_CreatePolygonFromSprite polyFromSprite = ScriptableObject.CreateInstance("Puppet2D_CreatePolygonFromSprite") as Puppet2D_CreatePolygonFromSprite;

        List<Vector3> verts = new List<Vector3>();

        for (int i=0; i<ffdStoreData.FFDCtrls.Count(); i++)
        {
            if (ffdStoreData.FFDCtrls[i])
                verts.Add(new Vector3(ffdStoreData.FFDCtrls[i].position.x, ffdStoreData.FFDCtrls[i].position.y, 0));
            else
            {
                Debug.LogWarning("A FFD control point has been removed, no mesh created");
                Undo.DestroyObjectImmediate(ffdStoreData);
                return;
            }

        }

        GameObject newMesh;

        if(ffdStoreData.FFDPathNumber.Count>0 && verts.Count>2)
        {
            if (Puppet2D_Editor.FFDGameObject == null)
                Puppet2D_Editor.FFDGameObject = new GameObject();


            Puppet2D_Editor._numberBonesToSkinToIndex = 0;

            string sortingLayer = "";
            int sortingOrder =0 ;
			if (Puppet2D_Editor.FFDGameObject.GetComponent<Renderer>()) 
            {

				sortingLayer = Puppet2D_Editor.FFDGameObject.GetComponent<Renderer>().sortingLayerName;
				sortingOrder = Puppet2D_Editor.FFDGameObject.GetComponent<Renderer>().sortingOrder;

            }
            newMesh = polyFromSprite.MakeFromVerts(true,verts.ToArray(),ffdStoreData.FFDPathNumber, Puppet2D_Editor.FFDGameObject );
			if (Puppet2D_Editor.FFDGameObject.GetComponent<Renderer>()) 
            {
				newMesh.GetComponent<Renderer>().sortingLayerName = sortingLayer;
				newMesh.GetComponent<Renderer>().sortingOrder = sortingOrder;
            }
            Puppet2D_Editor._numberBonesToSkinToIndex = 1;

        }
        else
        {
            Undo.DestroyObjectImmediate(ffdStoreData);
            return;
        }
        DestroyImmediate(polyFromSprite); 

        if (Puppet2D_Editor.FFDGameObject)
        {
			newMesh.GetComponent<Renderer>().sharedMaterial.mainTexture = spriteTexture;

            newMesh.name = Puppet2D_Editor.FFDGameObject.name;

            ffdStoreData.FFDCtrls.Add(newMesh.transform);


            Undo.DestroyObjectImmediate(Puppet2D_Editor.FFDGameObject);
        }

        GameObject globalCtrl = Puppet2D_CreateControls.CreateGlobalControl();
        FFDControlsGrp.transform.parent = globalCtrl.transform;

        List<Object> newObjs = new List<Object>();
        foreach(Transform tr in ffdStoreData.FFDCtrls)
            newObjs.Add(tr.gameObject);
        Selection.objects = newObjs.ToArray();

        Puppet2D_Editor._numberBonesToSkinToIndex = 1;
        Puppet2D_Skinning.BindSmoothSkin();


        for (int i = 0; i < ffdStoreData.FFDCtrls.Count-1; i++)
        {
            //Debug.Log(ffdStoreData.FFDCtrls[i]);
            ffdStoreData.FFDCtrls[i].GetComponent<Puppet2D_FFDLineDisplay>().outputSkinnedMesh = newMesh.GetComponent<SkinnedMeshRenderer>();
            ffdStoreData.FFDCtrls[i].parent.transform.parent = FFDControlsGrp.transform; 
            ffdStoreData.FFDCtrls[i].transform.localPosition = Vector3.zero;

        }


        Undo.DestroyObjectImmediate(ffdStoreData);
		if (globalCtrl.GetComponent<Puppet2D_GlobalControl>().AutoRefresh)
			globalCtrl.GetComponent<Puppet2D_GlobalControl>().Init();

    }
    public static int GetIndexOfVector3(List<GameObject> checkList, Vector3 match)
    {
        float dist = 100000000f;
        int closestIndex = 0;
        for (int i = 0; i < checkList.Count; i++)
        {
            Vector3 check = checkList[i].transform.position;
            float distCheck = Vector3.Distance(check, match);
            if ( distCheck < dist)
            {
                dist = distCheck;
                closestIndex = i;

            }

        }
        return closestIndex;
    }
}
