using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class ExampleEditor
{
	static ExampleEditor()
	{
		EditorApplication.update += OnUpdate;
	}
	
	static void OnUpdate()
	{
		/*if (!EditorApplication.currentScene.EndsWith("Example.unity"))
			return;
		
		var go = GameObject.Find("big");
		if (go != null)
			return;
		EditorApplication.update -= OnUpdate;
		
		var extractor = new PsdLayerExtractor("Assets/PSD2TK2D/Example/big.psd");
		{
			var filePathes = extractor.saveLayersToPNGs(false);
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
				return;
			
			var path = System.IO.Path.GetDirectoryName(extractor.filePath);
			var name = extractor.fileName.Substring(0, extractor.fileName.Length - 4);
			var collectionName = name + "_SpriteCollection";
			PsdLayerTo2DToolKit.makeTK2DSpriteCollection(path, collectionName, textures);
			PsdLayerTo2DToolKit.makeTK2DSpriteObjects(name, collectionName, extractor);
		}*/
    }
};