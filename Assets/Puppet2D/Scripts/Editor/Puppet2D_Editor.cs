using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;
using System.Reflection;
using System.Linq;
using System.Text.RegularExpressions;

public class Puppet2D_Editor : EditorWindow 
{

	public static bool SkinWeightsPaint = false;
	public static Mesh currentSelectionMesh;
	public static GameObject currentSelection;
	public static Color[] previousVertColors;
	public static float EditSkinWeightRadius =5f;
	public static GameObject paintWeightsBone;
	public static Shader previousShader;
    public static Vector3 ChangeRadiusStartPosition;
	public static float ChangeRadiusStartValue =0f;
	public static bool ChangingRadius = false;
	public static float paintWeightsStrength =0.25f;
	public static Color paintControlColor = new Color(.8f,1f,.8f, .5f);


    public static bool BoneCreation = false;
	static bool EditSkinWeights = false;
    public static bool SplineCreation = false;
	public static bool FFDCreation = false;

    GameObject currentBone;
    GameObject previousBone;

    public bool ReverseNormals ;

    public static string _boneSortingLayer,_controlSortingLayer;
	public static int _boneSortingIndex,_controlSortingIndex, _triangulationIndex, _numberBonesToSkinToIndex = 1;

	public static Sprite boneNoJointSprite =new Sprite();
	public static Sprite boneSprite  =new Sprite();
    public static Sprite boneHiddenSprite  =new Sprite();
	public static Sprite boneOriginal  =new Sprite();

    public static GameObject currentActiveBone = null;

    //public static List<Transform> splineCtrls = new List<Transform>();
    public static int numberSplineJoints = 4;

	static public List<Transform> FFDCtrls = new List<Transform>();
	static public List<int> FFDPathNumber = new List<int>();
	private GameObject FFDSprite;
    private Mesh FFDMesh;
    public static GameObject FFDGameObject;
     
	[SerializeField]
	static float BoneSize;
	static float ControlSize;
	static float VertexHandleSize;

	private string pngSequPath = Application.dataPath, checkPath;
	bool recordPngSequence = false;
	private int imageCount =0, resolution = 1;
	private float recordDelta = 0f;
    bool ExportPngAlpha;

	public static List<List<string>> selectedControls = new List<List<string>>();
    public static List<List<string>> selectedControlsData = new List<List<string>>();

    public enum GUIChoice
    {

        BoneCreation,
        RigginSetup,
        Skinning,
        Animation,


    }
    GUIChoice currentGUIChoice;

	[MenuItem ("GameObject/Puppet2D/Window/Puppet2D")]
	[MenuItem ("Window/Puppet2D")]
    static void Init () 
    {
		Puppet2D_Editor window = (Puppet2D_Editor)EditorWindow.GetWindow (typeof (Puppet2D_Editor));
		window.Show();
    }
	void OnEnable() 
	{
		BoneSize = EditorPrefs.GetFloat("Puppet2D_EditorBoneSize", 0.85f);
		ControlSize = EditorPrefs.GetFloat("Puppet2D_EditorControlSize", 0.85f);
		VertexHandleSize = EditorPrefs.GetFloat("Puppet2D_EditorVertexHandleSize", 0.8f);
        BoneCreation = EditorPrefs.GetBool("Puppet2D_BoneCreation", false);

        _boneSortingIndex = EditorPrefs.GetInt("Puppet2D_BoneLayer", 0);
        _controlSortingIndex = EditorPrefs.GetInt("Puppet2D_ControlLayer", 0);

                
        Puppet2D_Selection.GetSelectionString();

	}
	
	void OnGUI () 
	{
        string path = ("Assets/Puppet2D/Textures/GUI/BoneNoJoint.psd");
        string path2 = ("Assets/Puppet2D/Textures/GUI/BoneScaled.psd");
        string path3 = ("Assets/Puppet2D/Textures/GUI/BoneJoint.psd");
		string path4 = ("Assets/Puppet2D/Textures/GUI/Bone.psd");
        boneNoJointSprite =AssetDatabase.LoadAssetAtPath(path, typeof(Sprite)) as Sprite;
        boneSprite =AssetDatabase.LoadAssetAtPath(path2, typeof(Sprite)) as Sprite;
        boneHiddenSprite =AssetDatabase.LoadAssetAtPath(path3, typeof(Sprite)) as Sprite;
		boneOriginal =AssetDatabase.LoadAssetAtPath(path4, typeof(Sprite)) as Sprite;
        Texture aTexture = AssetDatabase.LoadAssetAtPath("Assets/Puppet2D/Textures/GUI/GUI_Bones.png", typeof(Texture))as Texture;
        Texture puppetManTexture = AssetDatabase.LoadAssetAtPath("Assets/Puppet2D/Textures/GUI/GUI_puppetman.png", typeof(Texture))as Texture;
        Texture rigTexture = AssetDatabase.LoadAssetAtPath("Assets/Puppet2D/Textures/GUI/GUI_Rig.png", typeof(Texture))as Texture;
        Texture ControlTexture = AssetDatabase.LoadAssetAtPath("Assets/Puppet2D/Textures/GUI/parentControl.psd", typeof(Texture))as Texture;
        Texture VertexTexture = AssetDatabase.LoadAssetAtPath("Assets/Puppet2D/Textures/GUI/VertexHandle.psd", typeof(Texture))as Texture;


        string[] sortingLayers = GetSortingLayerNames();
        Color bgColor = GUI.backgroundColor;

        if (currentGUIChoice == GUIChoice.BoneCreation)
            GUI.backgroundColor = Color.green;

        if (GUI.Button(new Rect(0, 0, 80, 20),"Skeleton" ))
        {
            currentGUIChoice = GUIChoice.BoneCreation;
        }

        GUI.backgroundColor = bgColor;
        if (currentGUIChoice == GUIChoice.RigginSetup)
            GUI.backgroundColor = Color.green;

        if (GUI.Button(new Rect(80, 0, 80, 20),"Rigging" ))
        {
            currentGUIChoice = GUIChoice.RigginSetup;
        }
        GUI.backgroundColor = bgColor;
        if (currentGUIChoice == GUIChoice.Skinning)
            GUI.backgroundColor = Color.green;

        if (GUI.Button(new Rect(160, 0, 80, 20),"Skinning" ))
        {
            currentGUIChoice = GUIChoice.Skinning;
        }
        GUI.backgroundColor = bgColor;
        if (currentGUIChoice == GUIChoice.Animation)
            GUI.backgroundColor = Color.green;

        if (GUI.Button(new Rect(240, 0, 80, 20),"Animation" ))
        {
            currentGUIChoice = GUIChoice.Animation;
        }
        GUI.backgroundColor = bgColor;

        if (EditSkinWeights || SplineCreation || FFDCreation )
            GUI.backgroundColor = Color.grey;


        GUI.DrawTexture(new Rect(25, 40, 32, 32), boneSprite.texture, ScaleMode.StretchToFill, true, 10.0F);

        EditorGUI.BeginChangeCheck ();
        BoneSize = EditorGUI.Slider(new Rect(80, 40, 150, 20), BoneSize, 0F, 0.9999F);
        if (EditorGUI.EndChangeCheck())
        {
            ChangeBoneSize();
            EditorPrefs.SetFloat("Puppet2D_EditorBoneSize", BoneSize);
        }
        EditorGUI.BeginChangeCheck ();
        _boneSortingIndex = EditorGUI.Popup(new Rect(80, 60, 150, 30), _boneSortingIndex, sortingLayers);
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetInt("Puppet2D_BoneLayer", _boneSortingIndex);
        }
        if (sortingLayers.Length <= _boneSortingIndex)
        {
            _boneSortingIndex = 0;
            EditorPrefs.SetInt("Puppet2D_BoneLayer", _boneSortingIndex);
        }
        _boneSortingLayer = sortingLayers[_boneSortingIndex];


        GUI.DrawTexture(new Rect(25, 100, 32, 32), ControlTexture, ScaleMode.StretchToFill, true, 10.0F);

        EditorGUI.BeginChangeCheck ();
        ControlSize = EditorGUI.Slider(new Rect(80, 100, 150, 20), ControlSize, 0F, .9999F);
        if (EditorGUI.EndChangeCheck())
        {
            ChangeControlSize();
            EditorPrefs.SetFloat("Puppet2D_EditorControlSize", ControlSize);
        }
        EditorGUI.BeginChangeCheck ();
        _controlSortingIndex = EditorGUI.Popup(new Rect(80, 130, 150, 30), _controlSortingIndex, sortingLayers);
        if (EditorGUI.EndChangeCheck())
        {
            EditorPrefs.SetInt("Puppet2D_ControlLayer", _controlSortingIndex);
        }
        if (sortingLayers.Length <= _controlSortingIndex)
        {
            _controlSortingIndex = 0;
            EditorPrefs.SetInt("Puppet2D_ControlLayer", _controlSortingIndex);
        }
        _controlSortingLayer = sortingLayers[_controlSortingIndex];


        GUI.DrawTexture(new Rect(15, 160, 275, 5), aTexture, ScaleMode.StretchToFill, true, 10.0F);

        int offsetControls = 130;

        if (currentGUIChoice == GUIChoice.BoneCreation)
        {
            //GUILayout.Label("Bone Creation", EditorStyles.boldLabel);

            GUILayout.Space(15);
            GUI.DrawTexture(new Rect(0, 60+offsetControls, 64, 128), aTexture, ScaleMode.StretchToFill, true, 10.0F);
            GUILayout.Space(15);
         
         
            if (BoneCreation)      
                GUI.backgroundColor = Color.green;


            if (GUI.Button(new Rect(80, 60+offsetControls, 150, 30), "Create Bone Tool"))
            {                      
                BoneCreation = true;
                currentActiveBone = null;
                EditorPrefs.SetBool("Puppet2D_BoneCreation", BoneCreation);
			
            }
            if (BoneCreation)      
                GUI.backgroundColor = bgColor;

        
            if (GUI.Button(new Rect(80, 90+offsetControls, 150, 30), "Finish Bone"))
            {
                Puppet2D_BoneCreation.BoneFinishCreation();
            }

            if (BoneCreation)
                GUI.backgroundColor = Color.grey;



            if (SplineCreation)
            {
                GUI.backgroundColor = Color.green;
            }
            if (GUI.Button(new Rect(80, 150+offsetControls, 150, 30), "Create Spline Tool"))
            {  
                //Puppet2D_Spline.splineStoreData.FFDCtrls.Clear();
                //SplineCreation = true; 
                Puppet2D_Spline.CreateSplineTool();
            }
            if (SplineCreation)
            {
                GUI.backgroundColor = bgColor;
            }
            numberSplineJoints = EditorGUI.IntSlider(new Rect(80, 190+offsetControls, 150, 20), numberSplineJoints, 1, 10);

            if (GUI.Button(new Rect(80, 220+offsetControls, 150, 30), "Finish Spline"))
            {   
                Puppet2D_Spline.SplineFinishCreation();            

            }
        }
        if (currentGUIChoice == GUIChoice.RigginSetup)
        {
           // GUILayout.Label("Rigging Setup", EditorStyles.boldLabel);

            GUI.DrawTexture(new Rect(0, 60+offsetControls, 64, 128), rigTexture, ScaleMode.StretchToFill, true, 10.0F);
            if (GUI.Button(new Rect(80, 60+offsetControls, 150, 30), "Create IK Control"))
            {
                Puppet2D_CreateControls.IKCreateTool();

            }
            if (GUI.Button(new Rect(80, 90+offsetControls, 150, 30), "Create Parent Control"))
            {
                Puppet2D_CreateControls.CreateParentControl();

            }
            if (GUI.Button(new Rect(80, 120+offsetControls, 150, 30), "Create Orient Control"))
            {        
                Puppet2D_CreateControls.CreateOrientControl();

            }
			/*if (GUI.Button(new Rect(80, 160+offsetControls, 150, 30), "Create Avatar"))
			{
                Puppet2D_CreateControls.CreateAvatar();

			}*/

        }
        if (currentGUIChoice == GUIChoice.Skinning)
        {
            //GUILayout.Label("Skinning", EditorStyles.boldLabel);

            GUI.DrawTexture(new Rect(0, 50+offsetControls, 64, 128), puppetManTexture, ScaleMode.StretchToFill, true, 10.0F);

            GUILayout.Space(55+offsetControls);
            GUIStyle labelNew = EditorStyles.label;
            labelNew.alignment = TextAnchor.LowerLeft;
            labelNew.contentOffset = new Vector2(80, 0);
            GUILayout.Label("Type of Mesh: ", labelNew);
            labelNew.contentOffset = new Vector2(0, 0);
            string[] TriangulationTypes = { "0", "1", "2", "3" };

            _triangulationIndex = EditorGUI.Popup(new Rect(180, 60+offsetControls, 50, 30), _triangulationIndex, TriangulationTypes);


            if (GUI.Button(new Rect(80, 80+offsetControls, 150, 30), "Convert Sprite To Mesh"))
            {
                Puppet2D_Skinning.ConvertSpriteToMesh(_triangulationIndex);
            }
            if (GUI.Button(new Rect(80, 110+offsetControls, 150, 30), "Parent Object To Bones"))
            {
                Puppet2D_Skinning.BindRigidSkin();

            }
            GUILayout.Space(75);
            labelNew.alignment = TextAnchor.LowerLeft;
            labelNew.contentOffset = new Vector2(80, 0);
            GUILayout.Label("Num Skin Bones: ", labelNew);
            labelNew.contentOffset = new Vector2(0, 0);
            string[] NumberBonesToSkinTo = { "1", "2", "4 (FFD)" };

            _numberBonesToSkinToIndex = EditorGUI.Popup(new Rect(180, 150+offsetControls, 50, 30), _numberBonesToSkinToIndex, NumberBonesToSkinTo);

            if (GUI.Button(new Rect(80, 170+offsetControls, 150, 30), "Bind Smooth Skin"))
            {
                Puppet2D_Skinning.BindSmoothSkin();

            }
            if (EditSkinWeights ||SkinWeightsPaint )
            {
                GUI.backgroundColor = Color.green;
            }
            if (SkinWeightsPaint)
            {   
                if (GUI.Button(new Rect(80, 200+offsetControls, 150, 30), "Manually Edit Weights"))
                {
                    // finish paint weights
                    Selection.activeGameObject = currentSelection;
                    if(currentSelection)
                    {
                        if(previousShader )
							currentSelection.GetComponent<Renderer>().sharedMaterial.shader = previousShader;
                        SkinWeightsPaint = false;
                        if(previousVertColors != null && previousVertColors.Length >0)
							currentSelectionMesh.colors = previousVertColors;
                        currentSelectionMesh = null;
                        currentSelection = null;
						previousVertColors=null;
                    }

                    EditSkinWeights = Puppet2D_Skinning.EditWeights();

                }
            }
            if (!SkinWeightsPaint)
            {
                if (GUI.Button(new Rect(80, 200 + offsetControls, 150, 30), "Paint Weights"))
                {   
                    if (EditSkinWeights)
                    {
                        EditSkinWeights = false;
                        Object[] bakedMeshes = Puppet2D_Skinning.FinishEditingWeights(); 

                        Selection.objects = bakedMeshes;
                    }

                    if (Selection.activeGameObject && Selection.activeGameObject.GetComponent<SkinnedMeshRenderer>() && Selection.activeGameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh)
                    {
                        SkinWeightsPaint = true;
                        SkinnedMeshRenderer smr = Selection.activeGameObject.GetComponent<SkinnedMeshRenderer>();
                        currentSelectionMesh = smr.sharedMesh;
                        currentSelection = Selection.activeGameObject;
						previousShader = currentSelection.GetComponent<Renderer>().sharedMaterial.shader;
						currentSelection.GetComponent<Renderer>().sharedMaterial.shader = Shader.Find("Puppet2D/vertColor");

                        if (currentSelectionMesh.colors.Length != currentSelectionMesh.vertices.Length)
                        {
                            currentSelectionMesh.colors = new Color[currentSelectionMesh.vertices.Length];
                            EditorUtility.SetDirty(currentSelection);
                            EditorUtility.SetDirty(currentSelectionMesh);
                            AssetDatabase.SaveAssets();
                            EditorApplication.SaveAssets();
                        }
						else
							previousVertColors = currentSelectionMesh.colors;
                        Selection.activeGameObject = smr.bones[0].gameObject;
                    }
                }
            }



            if (EditSkinWeights || SkinWeightsPaint)
                GUI.backgroundColor = bgColor;

            if (GUI.Button(new Rect(80, 230+offsetControls, 150, 30), "Finish Edit Skin Weights"))
            {   
                if (SkinWeightsPaint)
                {
                    if (currentSelection)
                    {
                        Selection.activeGameObject = currentSelection;

                        if (previousShader)
							currentSelection.GetComponent<Renderer>().sharedMaterial.shader = previousShader;
                        SkinWeightsPaint = false;
                        if(previousVertColors != null && previousVertColors.Length >0)
							currentSelectionMesh.colors = previousVertColors;
						currentSelectionMesh = null;
						currentSelection = null;
						previousVertColors=null;

                        Puppet2D_HiddenBone[] hiddenBones = Transform.FindObjectsOfType<Puppet2D_HiddenBone>();
                        foreach (Puppet2D_HiddenBone hiddenBone in hiddenBones)
                        {
                            hiddenBone.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
                            if (hiddenBone.transform.parent != null)
                                hiddenBone.transform.parent.GetComponent<SpriteRenderer>().color = Color.white;

                        }

					}
                    else
                        SkinWeightsPaint = false;
				}
				else
				{
					EditSkinWeights = false;
                    Puppet2D_Skinning.FinishEditingWeights(); 
                }

            }
            float SkinWeightsPaintOffset = -80;

            if (EditSkinWeights)
            {
                SkinWeightsPaintOffset = -40;
                GUI.DrawTexture(new Rect(25, 260 + offsetControls, 32, 32), VertexTexture, ScaleMode.StretchToFill, true, 10.0F);
                EditorGUI.BeginChangeCheck();
                VertexHandleSize = EditorGUI.Slider(new Rect(80, 270 + offsetControls, 150, 20), VertexHandleSize, 0F, .9999F);
                if (EditorGUI.EndChangeCheck())
                {
                    ChangeVertexHandleSize();
                    EditorPrefs.SetFloat("Puppet2D_EditorVertexHandleSize", VertexHandleSize);
                }
            }
            if (SkinWeightsPaint)
            {
                SkinWeightsPaintOffset = -20;

                GUILayout.Space(offsetControls - 20);
                GUILayout.Label(" Brush Size", EditorStyles.boldLabel);
                EditSkinWeightRadius = EditorGUI.Slider(new Rect(80, 275 + offsetControls, 150, 20), EditSkinWeightRadius, 0F, 100F);
                GUILayout.Label(" Strength", EditorStyles.boldLabel);
                paintWeightsStrength = EditorGUI.Slider(new Rect(80, 295 + offsetControls, 150, 20), paintWeightsStrength, 0F, 1F);
            }

            if (EditSkinWeights ||SkinWeightsPaint )
                GUI.backgroundColor = Color.grey;

            if (FFDCreation)
                GUI.backgroundColor = Color.green;

            if (GUI.Button(new Rect(80, 360+offsetControls+SkinWeightsPaintOffset, 150, 30), "Create FFD Tool"))
            {   
				if (!FFDCreation)
				{
					FFDCreation = true;
					if (Selection.activeGameObject && Selection.activeGameObject.GetComponent<SpriteRenderer>() && Selection.activeGameObject.GetComponent<SpriteRenderer>().sprite && !Selection.activeGameObject.GetComponent<SpriteRenderer>().sprite.name.Contains("bone"))
						FFDGameObject = Selection.activeGameObject;
					else
						Debug.LogWarning("Need to select a sprite to make an FFD mesh, will create a dummy mesh instead");
					Puppet2D_FFD.FFDSetFirstPath();
				}

            }
            if (FFDCreation)
                GUI.backgroundColor = bgColor;
            if (GUI.Button(new Rect(80, 390+offsetControls+SkinWeightsPaintOffset, 150, 30), "Finish FFD"))
            {   
                Puppet2D_FFD.FFDFinishCreation();
            }

        }
        if (currentGUIChoice == GUIChoice.Animation)
        {
            //GUILayout.Label("Animation", EditorStyles.boldLabel);

            if (GUI.Button(new Rect(80, 50+offsetControls, 150, 30), "Bake Animation"))
            {   
                Puppet2D_GlobalControl[] globalCtrlScripts = Transform.FindObjectsOfType<Puppet2D_GlobalControl>();
                for (int i = 0; i < globalCtrlScripts.Length; i++)
                {
                    Puppet2D_BakeAnimation BakeAnim = globalCtrlScripts[i].gameObject.AddComponent<Puppet2D_BakeAnimation>();
                    BakeAnim.Run();
                    DestroyImmediate(BakeAnim);
                    globalCtrlScripts[i].enabled = false;
                }
            }
			if(recordPngSequence && !ExportPngAlpha)
				GUI.backgroundColor = Color.green;
            if (GUI.Button(new Rect(80, 100+offsetControls, 150, 30), "Render Animation"))
			{
				checkPath = EditorUtility.SaveFilePanel("Choose Directory", pngSequPath, "exportedAnim", "");
				if(checkPath != "")
				{
					pngSequPath = checkPath;
					recordPngSequence = true;
					EditorApplication.ExecuteMenuItem("Edit/Play");
				}
            }
            GUI.backgroundColor = bgColor;
			if(ExportPngAlpha )
				GUI.backgroundColor = Color.green;
            if (GUI.Button(new Rect(80, 130+offsetControls, 150, 30), "Render Alpha"))
            {
                checkPath = EditorUtility.SaveFilePanel("Choose Directory", pngSequPath, "exportedAnim", "");
                if(checkPath != "")
                {
                    pngSequPath = checkPath;
                    recordPngSequence = true;
                    ExportPngAlpha = true;
                    EditorApplication.ExecuteMenuItem("Edit/Play");
                }
            }
			if (ExportPngAlpha || recordPngSequence)
				GUI.backgroundColor = bgColor;
            if(GUI.Button(new Rect(80, 200 + offsetControls, 150, 30), "Save Selection"))
            {
                selectedControls.Add(new List<string>());
                selectedControlsData.Add(new List<string>());

                foreach (GameObject go in Selection.gameObjects)
                {
                    selectedControls[selectedControls.Count-1].Add(Puppet2D_Selection.GetGameObjectPath(go));
                    selectedControlsData[selectedControlsData.Count-1].Add(go.transform.localPosition.x + " " + go.transform.localPosition.y + " "+ go.transform.localPosition.z + " "+ go.transform.localRotation.x + " "+ go.transform.localRotation.y + " "+ go.transform.localRotation.z + " "+ go.transform.localRotation.w + " "+ go.transform.localScale.x + " "+ go.transform.localScale.y + " "+ go.transform.localScale.z + " ");

                }
                Puppet2D_Selection.SetSelectionString();
            }
            if (GUI.Button(new Rect(80, 230 + offsetControls, 150, 30), "Clear Selections"))
            {
                selectedControls.Clear();
                selectedControlsData.Clear();
                Puppet2D_Selection.SetSelectionString();
            }


            for(int i=0;i< selectedControls.Count;i++)
            {
                int column = i%3;
                int row = 0;

                row = i / 3;
                Rect newLoadButtonPosition = new Rect(80 + (50 * column), 265 + offsetControls + row * 30, 50, 30);

                if(Event.current.type== EventType.ContextClick)
                {   
                    Vector2 mousePos = Event.current.mousePosition;
                    if ((Event.current.button == 1)&&newLoadButtonPosition.Contains(mousePos ))
                    {
                        GenericMenu menu = new GenericMenu ();

                        menu.AddItem (new GUIContent ("Select Objects"), false, Puppet2D_Selection.SaveSelectionLoad, i);
                        menu.AddItem (new GUIContent ("Remove Selection"), false, Puppet2D_Selection.SaveSelectionRemove, i);
                        menu.AddItem (new GUIContent ("Append Selection"), false, Puppet2D_Selection.SaveSelectionAppend, i);
                        menu.AddItem (new GUIContent ("Store Pose"), false, Puppet2D_Selection.StorePose, i);
                        menu.AddItem (new GUIContent ("Load Pose"), false, Puppet2D_Selection.LoadPose, i);



                        menu.ShowAsContext ();
                        Event.current.Use();

                    }

                }
                GUI.Box(newLoadButtonPosition, "Load");
                /*if (GUI.Button(newLoadButtonPosition, "Load"))
                {
                    Selection.objects = selectedControls[i].ToArray();
                }*/
            }


        }


    }
	void OnFocus() {

		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
		
		SceneView.onSceneGUIDelegate += this.OnSceneGUI;
	}
	
	void OnDestroy() {

		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;

		EditorPrefs.SetFloat("Puppet2D_EditorBoneSize", BoneSize);
		EditorPrefs.SetFloat("Puppet2D_EditorControlSize", ControlSize);
		EditorPrefs.SetFloat("Puppet2D_EditorVertexHandleSize", VertexHandleSize);

		Puppet2D_Selection.SetSelectionString();
	}
	
	void OnSceneGUI(SceneView sceneView) 
	{
		Event e = Event.current;

		switch (e.type)
		{
		case EventType.keyDown:
		{
			if (Event.current.keyCode == (KeyCode.Return))
			{
				if(BoneCreation)
					Puppet2D_BoneCreation.BoneFinishCreation();
				if(SplineCreation)
					Puppet2D_Spline.SplineFinishCreation();
                if (FFDCreation)
                {
                    FFDCreation = false;
                    Puppet2D_FFD.FFDFinishCreation();
                }
                Repaint();
				
			}
			if (Event.current.keyCode == (KeyCode.KeypadPlus) && SkinWeightsPaint)
			{
				EditSkinWeightRadius +=0.2f;

			}
			if (Event.current.keyCode == (KeyCode.KeypadMinus) && SkinWeightsPaint)
			{
				EditSkinWeightRadius -=0.2f;
			}
			if (BoneCreation)
			{
				if (Event.current.keyCode == (KeyCode.Backspace))
				{
                    Puppet2D_BoneCreation.BoneDeleteMode();
				}
			}
			if (SkinWeightsPaint)
			{
	            
				if (Event.current.keyCode == (KeyCode.N))
				{

					Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
					
					if(!ChangingRadius)
					{
						ChangeRadiusStartPosition = worldRay.GetPoint(0);
						ChangeRadiusStartValue = paintWeightsStrength;
					}
					
					Puppet2D_Skinning.ChangePaintStrength(worldRay.GetPoint(0));
					ChangingRadius = true;

				}
				if (Event.current.keyCode == (KeyCode.B))
				{
					
					Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
					
					if(!ChangingRadius)
					{
						ChangeRadiusStartPosition = worldRay.GetPoint(0);
						ChangeRadiusStartValue = EditSkinWeightRadius;
					}
					
					Puppet2D_Skinning.ChangePaintRadius(worldRay.GetPoint(0));
					ChangingRadius = true;
					
					
				}
			}
			break;
		}
        case EventType.mouseMove:
        {
            if (Event.current.button == 0)
            {

                if (BoneCreation)
                {
                    Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

                    if(Event.current.control == true)
                    {
                        Puppet2D_BoneCreation.BoneMoveMode(worldRay.GetPoint(0));
                    }
                    if(Event.current.shift == true)
                    {
                        Puppet2D_BoneCreation.BoneMoveIndividualMode(worldRay.GetPoint(0));
                    }

                }  
                if (FFDCreation || SplineCreation )
                {
                    Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

                    if((Event.current.control == true)||(Event.current.shift == true))
                    {
                        MoveControl(worldRay.GetPoint(0));
                    }
                }  

            }
            break;
        }
		case EventType.MouseDown:
		{
			
			if (Event.current.button == 0)
			{
				
				if (BoneCreation)
				{
					Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                        int controlID = GUIUtility.GetControlID(FocusType.Passive);
                        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
                    GameObject c = HandleUtility.PickGameObject(Event.current.mousePosition, true);
                    if (c)
                    {
                        Selection.activeGameObject = c;
                    }
                    else
                    {
    					if (Event.current.alt)                        		
                            Puppet2D_BoneCreation.BoneAddMode(worldRay.GetPoint(0));
    					else
                            Puppet2D_BoneCreation.BoneCreationMode(worldRay.GetPoint(0));
                    }
                        HandleUtility.AddDefaultControl(controlID);

						
					
				}                      
                else if(SplineCreation)
                {
                    Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

                    Puppet2D_Spline.SplineCreationMode(worldRay.GetPoint(0));
                }
				else if(FFDCreation)
				{
					Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                    
					Puppet2D_FFD.FFDCreationMode(worldRay.GetPoint(0));
				}
				else if(SkinWeightsPaint)
				{					

					GameObject c = HandleUtility.PickGameObject(Event.current.mousePosition, true);
					if (c && c.GetComponent<SpriteRenderer>() && c.GetComponent<SpriteRenderer>().sprite && c.GetComponent<SpriteRenderer>().sprite.name.Contains("Bone"))
					{
						Selection.activeGameObject = c;
					}
				}
                

			}

			else if (Event.current.button == 1)
			{
				if (BoneCreation)
				{                       
					Puppet2D_BoneCreation.BoneFinishCreation();
					Selection.activeObject = null;
					currentActiveBone = null;
					BoneCreation = true;
                   
				} 
				else if(FFDCreation)
				{
					Puppet2D_FFD.CloseFFDPath ();
				}
			}
			break;
			
		}
        case EventType.keyUp:
        {
			if (Event.current.keyCode == (KeyCode.B) || Event.current.keyCode == (KeyCode.N))
            {
                if (SkinWeightsPaint)
                {
					ChangingRadius = false;
                    
                }
            }
            break;
        }
		case EventType.mouseDrag:
		{
            paintControlColor = new Color(.8f,1f,.8f,.5f);


				
			if(SkinWeightsPaint)
			{

				Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
				if(Event.current.control == true)
				{
                    paintControlColor = new Color(1f,.8f,.8f,.5f);
					if (Event.current.button == 0)
						Puppet2D_Skinning.PaintWeights(worldRay.GetPoint(0), -1);
				}
				else if(Event.current.shift == true)
				{                    
                    paintControlColor = new Color(.8f,.8f,1f,.5f);
					if (Event.current.button == 0)
						Puppet2D_Skinning.PaintSmoothWeights(worldRay.GetPoint(0));

				}
				else
				{
					paintControlColor = new Color(.8f,1f,.8f,.5f);
					if (Event.current.button == 0)
						Puppet2D_Skinning.PaintWeights(worldRay.GetPoint(0), 1);
				}

				}


			break;
		}
		}
		if(SkinWeightsPaint)
		{
			Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
			if(ChangingRadius)
				Puppet2D_Skinning.DrawHandle(ChangeRadiusStartPosition);
			else
				Puppet2D_Skinning.DrawHandle(worldRay.GetPoint(0));
            Repaint();
			SceneView.RepaintAll();

			int controlID = GUIUtility.GetControlID(FocusType.Passive);
			HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));



			HandleUtility.AddDefaultControl(controlID);


		}
		// Do your drawing here using Handles.
		
		GameObject[] selection = Selection.gameObjects;
		
		Handles.BeginGUI();
		if(BoneCreation)
		{
			if(selection.Length>0)
			{
				Handles.color = Color.blue;
				Handles.Label(selection[0].transform.position + new Vector3(2,2,0),
				              "Left Click To Draw Bones\nPress Enter To Finish.\nBackspace To Delete A Bone\nHold Shift To Move Individual Bone\nHold Ctrl To Move Bone & Hierachy\nAlt Left Click To Add A Bone In Chain\nRight Click To Deselect");
			}
			else
			{
				Handles.color = Color.blue;
				Handles.Label(SceneView.lastActiveSceneView.camera.transform.position+Vector3.forward*2,
				              "Bone Create Mode.\nLeft Click to Draw Bones.\nOr click on a bone to be a parent");
			}
			
		}
		if(SkinWeightsPaint)
		{
			Handles.color = Color.blue;
			Handles.Label(new Vector3(20,-40,0),
			              "Select Bones to paint their Weights\n" +
			              "Left Click Adds Weights\n" +
			              "Left Click & Ctrl Removes Weights\n" +
			              "Left Click & Shift Smooths Weights\n"+			             
			              "Hold B to Change Brush Size\n" +
			              "Hold N to Change Strength" );

		}
		// Do your drawing here using GUI.
		Handles.EndGUI();   

	}
	


 

	
	

	void ChangeBoneSize ()
	{
		string path = ("Assets/Puppet2D/Textures/GUI/BoneNoJoint.psd");
		Sprite sprite =AssetDatabase.LoadAssetAtPath(path, typeof(Sprite)) as Sprite;
		TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite)) as TextureImporter;
		textureImporter.spritePixelsToUnits = (1-BoneSize)*(1-BoneSize)*1000f;
		AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

	}

	void ChangeControlSize ()
	{
		string path = ("Assets/Puppet2D/Textures/GUI/IKControl.psd");
		Sprite sprite =AssetDatabase.LoadAssetAtPath(path, typeof(Sprite)) as Sprite;
		TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite)) as TextureImporter;
		textureImporter.spritePixelsToUnits = (1-ControlSize)*(1-ControlSize)*1000f;
		AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

		path = ("Assets/Puppet2D/Textures/GUI/orientControl.psd");
		sprite =AssetDatabase.LoadAssetAtPath(path, typeof(Sprite)) as Sprite;
		textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite)) as TextureImporter;
		textureImporter.spritePixelsToUnits = (1-ControlSize)*(1-ControlSize)*1000f;
		AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

		path = ("Assets/Puppet2D/Textures/GUI/parentControl.psd");
		sprite =AssetDatabase.LoadAssetAtPath(path, typeof(Sprite)) as Sprite;
		textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite)) as TextureImporter;
		textureImporter.spritePixelsToUnits = (1-ControlSize)*(1-ControlSize)*1000f;
		AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        path = ("Assets/Puppet2D/Textures/GUI/splineControl.psd");
        sprite =AssetDatabase.LoadAssetAtPath(path, typeof(Sprite)) as Sprite;
        textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite)) as TextureImporter;
        textureImporter.spritePixelsToUnits = (1-ControlSize)*(1-ControlSize)*1000f;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        path = ("Assets/Puppet2D/Textures/GUI/splineMiddleControl.psd");
        sprite =AssetDatabase.LoadAssetAtPath(path, typeof(Sprite)) as Sprite;
        textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite)) as TextureImporter;
        textureImporter.spritePixelsToUnits = (1-ControlSize)*(1-ControlSize)*1000f;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        path = ("Assets/Puppet2D/Textures/GUI/ffdBone.psd");
        sprite =AssetDatabase.LoadAssetAtPath(path, typeof(Sprite)) as Sprite;
        textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite)) as TextureImporter;
        textureImporter.spritePixelsToUnits = (1-ControlSize)*(1-ControlSize)*1000f;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

	}

	void ChangeVertexHandleSize ()
	{
		string path = ("Assets/Puppet2D/Textures/GUI/VertexHandle.psd");
		Sprite sprite =AssetDatabase.LoadAssetAtPath(path, typeof(Sprite)) as Sprite;
		TextureImporter textureImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite)) as TextureImporter;
		textureImporter.spritePixelsToUnits = (1-VertexHandleSize)*(1-VertexHandleSize)*1000f;
		AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
	}
   
        
    static public void AddNewSortingName() 
    {
        object newName= new object();

        var internalEditorUtilityType = typeof(InternalEditorUtility);
        PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
        string[] stuff = (string[])sortingLayersProperty.GetValue(null, new object[0]);

        sortingLayersProperty.SetValue(null, newName,new object[stuff.Length]);
    }

    public string[] GetSortingLayerNames() 
    {
        var internalEditorUtilityType = typeof(InternalEditorUtility);
        PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);

        return (string[])sortingLayersProperty.GetValue(null, new object[0]);
    }



    void MoveControl(Vector3 mousePos)
    {
        GameObject selectedGO = Selection.activeGameObject; 
		if(selectedGO && selectedGO.transform && selectedGO.transform.parent)     
            selectedGO.transform.parent.position = new Vector3(mousePos.x, mousePos.y, 0);


    }
	void OnSelectionChange()
	{
		if( SkinWeightsPaint)
		{
			if (currentSelection == null)
				return;

			GameObject c = Selection.activeGameObject;
			if (c && c.GetComponent<SpriteRenderer>() && c.GetComponent<SpriteRenderer>().sprite && c.GetComponent<SpriteRenderer>().sprite.name.Contains("Bone"))
			{
                Puppet2D_HiddenBone[] hiddenBones = Transform.FindObjectsOfType<Puppet2D_HiddenBone>();
                foreach (Puppet2D_HiddenBone hiddenBone in hiddenBones)
                {
                    if (hiddenBone.transform.parent != null && hiddenBone.transform.parent && hiddenBone.transform.parent == c.transform)
                    {
                        hiddenBone.gameObject.GetComponent<SpriteRenderer>().color = new Color(1, .5f, 0);
                        hiddenBone.transform.parent.GetComponent<SpriteRenderer>().color = new Color(1, .5f, 0);
                    }
                    else if (hiddenBone.transform.parent)
                    {
                        hiddenBone.transform.parent.GetComponent<SpriteRenderer>().color = Color.white;
                        hiddenBone.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
                    }
                    else
                    {
                        hiddenBone.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
                    }
                }
				paintWeightsBone = c;

			}
			
			Vector3[] vertices =  currentSelectionMesh.vertices;
			Color[] colrs  =  currentSelectionMesh.colors;			
			
			SkinnedMeshRenderer smr = currentSelection.GetComponent<SkinnedMeshRenderer>();
			//Debug.Log("pos is " +pos);
			for (int i=0;i<vertices.Length;i++)
			{
				colrs[i]=Color.black;
				if(smr.bones.ToList().IndexOf( paintWeightsBone.transform) >-1&&  currentSelectionMesh.boneWeights[i].boneIndex0==smr.bones.ToList().IndexOf( paintWeightsBone.transform))
					colrs[i] =new Color( currentSelectionMesh.boneWeights[i].weight0, currentSelectionMesh.boneWeights[i].weight0, currentSelectionMesh.boneWeights[i].weight0);
				else if(smr.bones.ToList().IndexOf( paintWeightsBone.transform)>-1 &&  currentSelectionMesh.boneWeights[i].boneIndex1==smr.bones.ToList().IndexOf( paintWeightsBone.transform))
					colrs[i] =new Color( currentSelectionMesh.boneWeights[i].weight1, currentSelectionMesh.boneWeights[i].weight1, currentSelectionMesh.boneWeights[i].weight1);
				//				else if(smr.bones[ Puppet2D_Editor.currentSelectionMesh.boneWeights[i].boneIndex2]== Puppet2D_Editor.paintWeightsBone.transform)
				//					colrs[i] =new Color( Puppet2D_Editor.currentSelectionMesh.boneWeights[i].weight2, Puppet2D_Editor.currentSelectionMesh.boneWeights[i].weight2, Puppet2D_Editor.currentSelectionMesh.boneWeights[i].weight2);
				//				else if(smr.bones[ Puppet2D_Editor.currentSelectionMesh.boneWeights[i].boneIndex3]== Puppet2D_Editor.paintWeightsBone.transform)
				//					colrs[i] =new Color( Puppet2D_Editor.currentSelectionMesh.boneWeights[i].weight3, Puppet2D_Editor.currentSelectionMesh.boneWeights[i].weight3, Puppet2D_Editor.currentSelectionMesh.boneWeights[i].weight3);
				
				
				
			}
			currentSelectionMesh.colors = colrs;
		}
	}
	
	void Start()
	{
		imageCount = 0;
	}
	void Update()
	{
		if(recordPngSequence && Application.isPlaying)
		{
            Time.captureFramerate = 30;

			recordDelta +=Time.deltaTime;
			
			if(recordDelta >= 1/30)
			{ 
				imageCount++;


                if (ExportPngAlpha)
                {
                    Shader newshad = Shader.Find("Puppet2D/BlackAndWhite");
                    Camera.main.SetReplacementShader(newshad, null);
                    Camera.main.backgroundColor = Color.black;

                    Application.CaptureScreenshot(pngSequPath + "_Alpha." + imageCount.ToString("D4") + ".png", resolution); 
                }
                else
                {
                    Application.CaptureScreenshot(pngSequPath + "." + imageCount.ToString("D4") + ".png", resolution); 
                }

 

				recordDelta = 0f;
			}
			Repaint();

		}
		if(!Application.isPlaying && imageCount >0)
		{
			recordPngSequence = false;
			imageCount =0;
            ExportPngAlpha = false;
		}


	}

}
