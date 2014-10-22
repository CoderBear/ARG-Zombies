#define TK2D_1_8

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PsdLayerTo2DToolKit : EditorWindow
{
	private static List<PsdLayerExtractor> extractors = new List<PsdLayerExtractor>();
	private static bool overwrite = false;
	private static Vector2 scrollPosition;
	
	private static bool makeSpriteCollection = true;
	private static bool makeSpriteObject = true;
	private static float layerGap = 10f;
	private static int maxTextureSize = 4096;
	
	void OnGUI()
	{
		if (GUILayout.Button("Run", GUILayout.MaxWidth(200)))
		{
			foreach (var extractor in extractors)
			{
				var filePathes = extractor.saveLayersToPNGs(overwrite);
				var textures = new Texture2D[filePathes.Count];
				var textureIndex = 0;
				
				var somethingWrong = false;
				AssetDatabase.Refresh();
				foreach (var filePath in filePathes)
				{
					var tex = AssetDatabase.LoadMainAssetAtPath(filePath) as Texture2D;
					textures[textureIndex++] = tex;
					if (somethingWrong = tex == null)
						break;
				}
				if (somethingWrong)
				{
					Debug.Log("Can not found texture assets. Please try again.");
					continue;
				}
				
				var path = System.IO.Path.GetDirectoryName(extractor.filePath);
				var name = extractor.fileName.Substring(0, extractor.fileName.Length - 4);
				var collectionName = name + "_SpriteCollection";
				if (makeSpriteCollection)
					makeTK2DSpriteCollection(path, collectionName, textures);
				
				if (makeSpriteObject)
					makeTK2DSpriteObjects(name, collectionName, extractor);
			}
		}
		
		GUILayout.BeginHorizontal();
		{
			makeSpriteCollection = GUILayout.Toggle(
				makeSpriteCollection, "Make Sprite Collection", GUILayout.MaxWidth(150));
			
			makeSpriteObject = GUILayout.Toggle(
				makeSpriteObject, "Make Sprite Object", GUILayout.MaxWidth(150));
		}
		GUILayout.EndHorizontal();
			
		GUILayout.BeginHorizontal();
		// max texture size
		{
			int[] allowedAtlasSizes = { 128, 256, 512, 1024, 2048, 4096 };
			string[] allowedAtlasSizesString = new string[allowedAtlasSizes.Length];
			for (int i = 0; i < allowedAtlasSizes.Length; ++i)
				allowedAtlasSizesString[i] = allowedAtlasSizes[i].ToString();
			
			maxTextureSize = EditorGUILayout.IntPopup(
				"Max texture size", maxTextureSize, allowedAtlasSizesString, allowedAtlasSizes, GUILayout.MaxWidth(200));
		}
		// gap between layers
		{
#if (UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_4)
			EditorGUILayout.LabelField("Gap between", "layers", GUILayout.MaxWidth(150));
#else
			EditorGUILayout.LabelField("Gap between layers", GUILayout.MaxWidth(150));
#endif
			layerGap = EditorGUILayout.FloatField(layerGap, GUILayout.MaxWidth(50));
		}
		GUILayout.EndHorizontal();
		
		// selection
		
		GUILayout.BeginHorizontal();
		{
			overwrite = GUILayout.Toggle(overwrite, "Overwrite PNG files", GUILayout.MaxWidth(130));
			
			if (GUILayout.Button("Select All", GUILayout.MaxWidth(100)))
			{
				foreach (var extractor in extractors)
					extractor.canLoadData = true;
			}
			if (GUILayout.Button("Select None", GUILayout.MaxWidth(100)))
			{
				foreach (var extractor in extractors)
					extractor.canLoadData = false;
			}
		}
		GUILayout.EndHorizontal();
		GUILayout.Space(30);
		
		// layers
		
		scrollPosition = GUILayout.BeginScrollView(scrollPosition);
		GUILayout.BeginHorizontal();
		{
			foreach (var extractor in extractors)
				extractor.OnGUI();
		}
		GUILayout.EndHorizontal();
		GUILayout.EndScrollView();
	}
	
    public static void makeTK2DSpriteObjects(string groupName, string collectionName, PsdLayerExtractor extractor)
	{
		var screenSize = OrthoCameraSetter.screenSize;
		var parent = new GameObject(groupName);
		parent.transform.position = new Vector3(0, screenSize.y, 0);
		
		var z = 0f;
		var layers = extractor.layers;
		foreach (var layer in layers)
		{
			if (!layer.canLoadLayer)
				continue;
			
			var go = makeTK2DSpriteObject(collectionName, layer.name, layer.psdLayer.opacity);
			if (go == null)
			{
				GameObject.DestroyImmediate(parent);
				return;
			}
			
			// position
			
			var x = (float)layer.psdLayer.area.left;
			var y = (float)layer.psdLayer.area.top;
			var w = (float)layer.psdLayer.area.width;
			
			go.transform.parent = parent.transform;
			{
				var pos = new Vector3(x, -y, 0);
				pos.z = z;
				z -= layerGap;
				go.transform.localPosition = pos;
			}
			
			// size
			
			var mesh = go.GetComponent<MeshFilter>().sharedMesh;
			var bounds = mesh.bounds;
			var scale = w / bounds.size.x;
			go.transform.localScale = new Vector3(scale, scale, scale);
		}
		saveCurrentScene();
	}
	
    public static GameObject makeTK2DSpriteObject(string collectionName, string objectName, float opacity)
    {
		tk2dSpriteCollectionData sprColl = null;
		tk2dSpriteCollectionIndex[] spriteCollections = tk2dEditorUtility.GetOrCreateIndex().GetSpriteCollectionIndex();
		foreach (var v in spriteCollections)
		{
			GameObject scgo = AssetDatabase.LoadAssetAtPath(
				AssetDatabase.GUIDToAssetPath(v.spriteCollectionDataGUID), typeof(GameObject)) as GameObject;
			
			var sc = scgo.GetComponent<tk2dSpriteCollectionData>();
			if (sc != null && sc.spriteDefinitions != null && sc.spriteDefinitions.Length > 0 && 
				sc.spriteCollectionName == collectionName)
			{
				sprColl = sc;
				break;
			}
		}

		if (sprColl == null)
		{
			Debug.Log(string.Format("Can not find {0}.", collectionName));
			return null;
		}

		GameObject go = tk2dEditorUtility.CreateGameObjectInScene(objectName);
		tk2dSprite sprite = go.AddComponent<tk2dSprite>();
		sprite.Collection = sprColl;
		sprite.renderer.material = sprColl.spriteDefinitions[0].material;
		sprite.Build();
		
		sprite.spriteId = sprite.GetSpriteIdByName(objectName);
		sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, opacity);
		return go;
    }
	
	private static string getPrefabPath(string collectionName)
	{
		var obj = Selection.activeObject;
		var assetPath = AssetDatabase.GetAssetPath(obj);
		if (assetPath.Length > 0)
		{
			var i = assetPath.LastIndexOfAny("/".ToCharArray());
			if (i >= 0)
			{
				assetPath = assetPath.Remove(i + 1);
				return System.IO.Path.Combine(assetPath, collectionName + ".prefab");
			}
		}
		
		return null;
	}
	
	public static void saveCurrentScene()
	{
#if (UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_4)
		if (System.IO.File.Exists(EditorApplication.currentScene))
			EditorApplication.SaveScene(EditorApplication.currentScene);
#else
		EditorApplication.SaveScene();
#endif
	}
	
	public static void makeTK2DSpriteCollection(string prePath, string name, Texture2D[] textures)
	{
		tk2dSpriteCollectionIndex[] spriteCollections = tk2dEditorUtility.GetOrCreateIndex().GetSpriteCollectionIndex();
		foreach (var v in spriteCollections)
		{
			if (v.name != name)
				continue;
				
			var prefabPath = getPrefabPath(name);
			var go = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
			if (go == null)
				continue;
			
			tk2dSpriteCollection sc = go.GetComponent<tk2dSpriteCollection>();
			sc.defaults.anchor = tk2dSpriteCollectionDefinition.Anchor.UpperLeft;
			sc.textureParams = new tk2dSpriteCollectionDefinition[0]; //**??
#if TK2D_1_8
			var spriteCollectionProxy = 
				new tk2dEditor.SpriteCollectionEditor.SpriteCollectionProxy(sc);
			foreach (var tex in textures) {
				string tname = spriteCollectionProxy.FindUniqueTextureName(tex.name);
				int slot = spriteCollectionProxy.FindOrCreateEmptySpriteSlot();
				spriteCollectionProxy.textureParams[slot].name = tname;
				spriteCollectionProxy.textureParams[slot].colliderType = tk2dSpriteCollectionDefinition.ColliderType.ForceNone;
				spriteCollectionProxy.textureParams[slot].texture = (Texture2D)tex;
			}
			sc.maxTextureSize = maxTextureSize;
			
			saveCurrentScene();
			{
				spriteCollectionProxy.CopyToTarget();
				tk2dSpriteCollectionBuilder.ResetCurrentBuild();
				tk2dSpriteCollectionBuilder.Rebuild(sc);
				spriteCollectionProxy.CopyFromSource();
				tk2dEditorUtility.UnloadUnusedAssets();
			}
			saveCurrentScene();
#else
			sc.textureRefs = textures;
			sc.maxTextureSize = maxTextureSize;
			
			saveCurrentScene();
			{
				tk2dSpriteCollectionBuilder.ResetCurrentBuild();
				tk2dSpriteCollectionBuilder.Rebuild(sc);
				tk2dEditorUtility.UnloadUnusedAssets();
			}
			saveCurrentScene();
#endif
			return;
		}
		
		string path = prePath + "/" + name + ".prefab";
        if (!string.IsNullOrEmpty(name))
        {
            var go = new GameObject();
            go.AddComponent<tk2dSpriteCollection>();

#if (UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_4)
            go.active = false;
			
			var p = EditorUtility.CreateEmptyPrefab(path);
            var prefab = EditorUtility.ReplacePrefab(go, p, ReplacePrefabOptions.ConnectToPrefab);
#elif (UNITY_3_5)
            go.active = false;
			
			var p = PrefabUtility.CreateEmptyPrefab(path);
            var prefab = EditorUtility.ReplacePrefab(go, p, ReplacePrefabOptions.ConnectToPrefab);
#else
			go.SetActive(false);
			
			var p = PrefabUtility.CreateEmptyPrefab(path);
            var prefab = PrefabUtility.ReplacePrefab(go, p, ReplacePrefabOptions.ConnectToPrefab);
#endif
			saveCurrentScene();
			var sc = prefab.GetComponent<tk2dSpriteCollection>();
			{
				sc.defaults.anchor = tk2dSpriteCollectionDefinition.Anchor.UpperLeft;
#if TK2D_1_8
				var spriteCollectionProxy = 
					new tk2dEditor.SpriteCollectionEditor.SpriteCollectionProxy(sc);
				foreach (var tex in textures) {
					string tname = spriteCollectionProxy.FindUniqueTextureName(tex.name);
					int slot = spriteCollectionProxy.FindOrCreateEmptySpriteSlot();
					spriteCollectionProxy.textureParams[slot].name = tname;
					spriteCollectionProxy.textureParams[slot].anchor = tk2dSpriteCollectionDefinition.Anchor.UpperLeft;
					spriteCollectionProxy.textureParams[slot].colliderType = tk2dSpriteCollectionDefinition.ColliderType.ForceNone;
					spriteCollectionProxy.textureParams[slot].texture = (Texture2D)tex;
				}
				sc.maxTextureSize = maxTextureSize;
				spriteCollectionProxy.CopyToTarget();
				tk2dSpriteCollectionBuilder.ResetCurrentBuild();
				tk2dSpriteCollectionBuilder.Rebuild(sc);
				spriteCollectionProxy.CopyFromSource();
#else
				sc.textureRefs = textures;
				sc.maxTextureSize = maxTextureSize;
				tk2dSpriteCollectionBuilder.Rebuild(sc);
#endif
			}
			
            GameObject.DestroyImmediate(go);
			tk2dEditorUtility.UnloadUnusedAssets();
			saveCurrentScene();
        }
	}
	
	[MenuItem ("Assets/Save PSD Layers to 2D Tool Kit(TK2D)", true, 20001)]
	private static bool saveLayersEnabled()
	{
	    for (var i=0; i<Selection.objects.Length; ++i)
	    {
	        var obj = Selection.objects[i];
	        var filePath = AssetDatabase.GetAssetPath(obj);
			if (filePath.EndsWith(".psd", System.StringComparison.CurrentCultureIgnoreCase))
				return true;
	    }
		
		return false;
	}
	
	[MenuItem ("Assets/Save PSD Layers to 2D Tool Kit(TK2D)", false, 20001)]
	private static void saveLayers()
	{
		extractors.Clear();
		
	    for (var i=0; i<Selection.objects.Length; ++i)
	    {
	        var obj = Selection.objects[i];
	        var filePath = AssetDatabase.GetAssetPath(obj);
			if (!filePath.EndsWith(".psd", System.StringComparison.CurrentCultureIgnoreCase))
				continue;
			
			extractors.Add(new PsdLayerExtractor(filePath));
	    }
		
        var window = EditorWindow.GetWindow<PsdLayerTo2DToolKit>(
			true, "Save PSD Layers to 2D Tool Kit(TK2D)");
		window.minSize = new Vector2(400, 300);
		window.Show();
	}
};