using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
public class IslandDetectorTestHelper {
	
	//-------------------------------------------------------------------------
	public static bool WriteClassificationImageToPNG(int[,] classificationImage, string path) {
		int width = classificationImage.GetLength(1);
		int height = classificationImage.GetLength(0);
		int numIslands = 0;
		int numSeaRegions = 0;
		CountClassifiedRegions(classificationImage, out numIslands, out numSeaRegions);
		float colorMultFactorIsland = 1.0f / numIslands;
		float colorMultFactorSeaRegion = -1.0f / numSeaRegions;
		
		Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
		Color[] colorData = new Color[height * width];
		for (int y = 0; y < height; ++y) {
			for (int x = 0; x < width; ++x) {
				int destIndex = y * width + x;
				
				colorData[destIndex].r = 0.0f;
				colorData[destIndex].g = 0.0f;
				colorData[destIndex].b = 0.0f;
				colorData[destIndex].a = 0.0f;
				
				int classificationValue = classificationImage[y,x];
				if (classificationValue < 0) { // sea region
					colorData[destIndex].b = classificationValue * colorMultFactorSeaRegion;
				}
				else if (classificationValue > 0) { // island region
					colorData[destIndex].g = classificationValue * colorMultFactorIsland;
				}
				else { // classificationValue == 0 == unassigned!
					colorData[destIndex].r = 1.0f;
				}
			}
		}
		
    	texture.SetPixels(colorData);
    	texture.Apply();

		byte[] bytes = texture.EncodeToPNG();
	    Object.DestroyImmediate(texture);
    
    	System.IO.File.WriteAllBytes(path, bytes);
		return true;
	}
	
	//-------------------------------------------------------------------------
	// This method assumes that classificationImage was filled by DetectIslandsFromBinaryImage()
	// which sets classification values of regions ascending by 1 at islands and descending by 1 at sea-regions.
	private static bool CountClassifiedRegions(int[,] classificationImage, out int numIslands, out int numSeaRegions) {
		
		int minValue = 0;
		int maxValue = 0;
		bool anyUnassignedValueFound = false;
		
		for (int y = 0; y < classificationImage.GetLength(0); ++y) { // classificationImage.height
			for (int x = 0; x < classificationImage.GetLength(1); ++x) { // classificationImage.width
				int classificationValue = classificationImage[y,x];
				if (classificationValue < minValue) {
					minValue = classificationValue;
				}
				if (classificationValue > maxValue) {
					maxValue = classificationValue;
				}
				if (classificationValue == 0) {
					anyUnassignedValueFound = true;
				}
			}
		}
		
		numIslands = maxValue;
		numSeaRegions = -minValue;
		return anyUnassignedValueFound;
	}
}
#endif // UNITY_EDITOR
