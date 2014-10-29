using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class PsdLayerExtractor
{
	public bool canLoadData;
	public PsdParser.PSD psd;
	
	public class Layer
	{
		public bool canLoadLayer = true;
		public PsdParser.PSDLayer psdLayer;
		private string groupName;
		
		public string name
		{
			get { return this.groupName + this.psdLayer.name; }
		}
		
		public Layer(string groupName, PsdParser.PSDLayer psdLayer)
		{
			this.groupName = groupName;
			this.psdLayer = psdLayer;
		}
		
		public void loadData(BinaryReader br, int bpp)
        {
			var channelCount = this.psdLayer.channels.Length;
            for (var k=0; k<channelCount; ++k)
            {
                var channel = this.psdLayer.channels[k];
				if (this.psdLayer.hasData && this.canLoadLayer)
					channel.loadData(br, bpp);
            }
        }
	};
	public List<Layer> layers = new List<Layer>();
	
	public PsdLayerExtractor(string filePath)
	{
		this.canLoadData = true;
		this.psd = new PsdParser.PSD();
		this.psd.loadHeader(filePath);
		
		var psdLayers = this.psd.layerInfo.layers;
		this.loadPsdLayer("", psdLayers.Length - 1, psdLayers);
		this.layers.Reverse();
	}
	
	public string filePath
	{
		get { return this.psd.filePath; }
	}
	
	public string fileName
	{
		get { return this.psd.fileName; }
	}
	
	private int loadPsdLayer(string groupName, int i, PsdParser.PSDLayer[] psdLayers)
	{
		while (i >= 0)
		{
			var psdLayer = psdLayers[i--];
			if (psdLayer.groupStarted)
			{
				var temp = groupName;
				temp += string.IsNullOrEmpty(groupName) ? "" : "_";
				temp += psdLayer.name + "_";
				i = this.loadPsdLayer(temp, i, psdLayers);
			}
			else if (psdLayer.groupEnded)
			{
				break;
			}
			else if (psdLayer.hasData)
			{
				this.layers.Add(new Layer(groupName, psdLayer));
			}
		}
		return i;
	}
	
	private void loadPsdLayerData()
	{
		using (FileStream stream = new FileStream(this.psd.filePath, 
			FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            using (BinaryReader br = new BinaryReader(stream))
            {
                try
                {
		            foreach (var layer in this.layers)
						layer.loadData(br, this.psd.headerInfo.bpp);
                }
                catch (System.Exception e)
                {
                    Debug.Log(e.Message);
                }
			}
		}
	}
	
	public List<string> saveLayersToPNGs(bool overwrite)
	{
		if (!this.canLoadData)
			return null;
		
		this.loadPsdLayerData();
		
		var psdFilePath = this.psd.filePath;
		var extractPath = psdFilePath.Substring(0, psdFilePath.Length - 4) + "_layers";
		if (!System.IO.Directory.Exists(extractPath))
			System.IO.Directory.CreateDirectory(extractPath);
			
		var filePathes = new List<string>();
		foreach (var layer in this.layers)
        {
            if (!layer.canLoadLayer)
				continue;
			
            var data = layer.psdLayer.mergeChannels();
			if (data == null)
				continue;
			
			var fileName = layer.name;
			var filePath = extractPath + "/" + fileName + ".png";
			filePathes.Add(filePath);
			if (!overwrite && File.Exists(filePath))
				continue;
			
			var channelCount = layer.psdLayer.channels.Length;
			var pitch = layer.psdLayer.pitch;
			var w = layer.psdLayer.area.width;
			var h = layer.psdLayer.area.height;
			
			var format = channelCount == 3 ? TextureFormat.RGB24 : TextureFormat.ARGB32;
			var tex = new Texture2D(w, h, format, false);
			var colors = new Color32[data.Length / channelCount];
			var k = 0;
			for (var y=h-1; y>=0; --y)
			{
				for (var x=0; x<pitch; x+=channelCount)
				{
					var n = x + y * pitch;
					var c = new Color32(1,1,1,1);
					if (channelCount == 4)
					{
						c.b = data[n++];
						c.g = data[n++];
						c.r = data[n++];
						c.a = data[n++];
					}
					else
					{
						c.b = data[n++];
						c.g = data[n++];
						c.r = data[n++];
						c.a = 1;
					}
					colors[k++] = c;
				}
			}
			tex.SetPixels32(colors);
			tex.Apply();
			data = tex.EncodeToPNG();
			
			System.IO.File.WriteAllBytes(filePath, data);
			AssetDatabase.ImportAsset(filePath, ImportAssetOptions.Default);
			
			Texture2D.DestroyImmediate(tex);
		}
		
		return filePathes;
	}
	
	public void OnGUI()
	{
		this.canLoadData = EditorGUILayout.BeginToggleGroup(this.fileName, this.canLoadData);
		{
			// selection
			
			GUILayout.BeginHorizontal();
			{
				if (GUILayout.Button("Select All", GUILayout.MaxWidth(100)))
				{
					foreach (var layer in this.layers)
						layer.canLoadLayer = true;
				}
				if (GUILayout.Button("Select None", GUILayout.MaxWidth(100)))
				{
					foreach (var layer in this.layers)
						layer.canLoadLayer = false;
				}
			}
			GUILayout.EndHorizontal();
			
			// layers
			
			for (var i=0; i<this.layers.Count; ++i)
			{
				var layer = this.layers[i];
				layer.canLoadLayer = EditorGUILayout.Toggle(layer.name, layer.canLoadLayer);
			}
		}
		EditorGUILayout.EndToggleGroup();
	}
};