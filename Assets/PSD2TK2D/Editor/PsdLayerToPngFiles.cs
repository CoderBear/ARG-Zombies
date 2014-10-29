using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PsdLayerToPngFiles : EditorWindow
{
	private static List<PsdLayerExtractor> extractors = new List<PsdLayerExtractor>();
	private static bool overwrite = false;
	private static Vector2 scrollPosition;
	
	void OnGUI()
	{
		if (GUILayout.Button("Run", GUILayout.MaxWidth(200)))
		{
			foreach (var extractor in extractors)
				extractor.saveLayersToPNGs(overwrite);
			AssetDatabase.Refresh();
		}
		
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
	
	[MenuItem ("Assets/Save PSD Layers to PNG files", true, 20000)]
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
	
	[MenuItem ("Assets/Save PSD Layers to PNG files", false, 20000)]
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
		
		var window = EditorWindow.GetWindow<PsdLayerToPngFiles>(
			true, "Save PSD Layers to PNG files");
		window.minSize = new Vector2(400, 300);
		window.Show();
	}
};