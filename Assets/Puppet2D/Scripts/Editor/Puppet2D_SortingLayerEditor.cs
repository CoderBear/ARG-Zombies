using UnityEngine;
using System.Collections;
using System;
using UnityEditor;

using UnityEditorInternal;

using System.Reflection;

[CustomEditor(typeof(Puppet2D_SortingLayer))]
public class Puppet2D_SortingLayerEditor : Editor {


	string[] sortingLayerNames;//we load here our Layer names to be displayed at the popup GUI
	
    public int popupMenuIndex;
    public int orderInLayer;
		
	void OnEnable()		
	{
		
		sortingLayerNames = GetSortingLayerNames(); //First we load the name of our layers
		var renderer = (target as Puppet2D_SortingLayer).gameObject.GetComponent<Renderer>();
		if (!renderer)
		{
			return;
		}
		//popupMenuIndex = renderer.sortingLayerID;
		//orderInLayer = renderer.sortingOrder;	
        SetSortingLayer(renderer.sortingLayerName, renderer.sortingOrder);
	}
    public void SetSortingLayer(string sortingLayerName,int orderInLayerSet )       
    {   
        for (int i = 0; i < sortingLayerNames.Length; i++) 
        {
            if ( sortingLayerNames [i] == sortingLayerName)
                popupMenuIndex = i;
        }
        orderInLayer = orderInLayerSet; 
    }
	public override void OnInspectorGUI()
		
	{
		var renderer = (target as Puppet2D_SortingLayer).gameObject.GetComponent<Renderer>();
		
		// If there is no renderer, we can't do anything
		if (!renderer)
		{
			return;
		}
		
		// Expose the sorting layer name

		popupMenuIndex = EditorGUILayout.Popup("Sorting Layer", popupMenuIndex, sortingLayerNames);//The popup menu is displayed simple as that


        // if (sortingLayerNames [popupMenuIndex] != renderer.sortingLayerName) {

        /*if (popupMenuIndex != renderer.sortingLayerID) {

            renderer.sortingLayerID = popupMenuIndex;

            EditorUtility.SetDirty(renderer);
        }*/
		if (sortingLayerNames[popupMenuIndex] != renderer.sortingLayerName) {
			Undo.RecordObject(renderer, "Edit Sorting Layer Name");
			renderer.sortingLayerName = sortingLayerNames[popupMenuIndex];
			EditorUtility.SetDirty(renderer);
		}


		int newSortingLayerOrder = orderInLayer;
		newSortingLayerOrder = EditorGUILayout.IntField("Sorting Layer Order", renderer.sortingOrder);
		if (newSortingLayerOrder != renderer.sortingOrder) {
			Undo.RecordObject(renderer, "Edit Sorting Order");
			renderer.sortingOrder = newSortingLayerOrder;
			EditorUtility.SetDirty(renderer);
		}
		//popupMenuIndex = EditorGUILayout.Popup("Sorting Layer", popupMenuIndex, sortingLayerNames);//The popup menu is displayed simple as that
		
			
	}
	
	
	
	// Get the sorting layer names
	
	public string[] GetSortingLayerNames()
		
	{
		
		Type internalEditorUtilityType = typeof(InternalEditorUtility);
		
		PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
		
		return (string[])sortingLayersProperty.GetValue(null, new object[0]);
		
	}
		

}
