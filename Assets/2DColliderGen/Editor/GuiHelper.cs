using UnityEngine;
using UnityEditor;

public class GuiHelper {

	//-------------------------------------------------------------------------
	public static float FloatSliderGuiElement(UnityEngine.GUIContent label, float inputValue, float min, float max, ref bool hasChanged) {
		
		float newValue = EditorGUILayout.Slider(label, inputValue, min, max);
		if (newValue != inputValue) {
			hasChanged = true;
		}
		else {
			hasChanged = false;
		}
		return newValue;
	}
	
	//-------------------------------------------------------------------------
	public static int IntSliderGuiElement(UnityEngine.GUIContent label, int inputValue, int min, int max, ref bool hasChanged) {
		
		int newValue = EditorGUILayout.IntSlider(label, inputValue, min, max);
		if (newValue != inputValue) {
			hasChanged = true;
		}
		else {
			hasChanged = false;
		}
		return newValue;
	}
	
	//-------------------------------------------------------------------------
	public static bool ToggleGuiElement(UnityEngine.GUIContent label, bool inputValue, ref bool hasChanged) {
		
		bool newValue = EditorGUILayout.Toggle(label, inputValue);
		if (newValue != inputValue) {
			hasChanged = true;
		}
		else {
			hasChanged = false;
		}
		return newValue;
	}
	
	//-------------------------------------------------------------------------
	public static Vector2 Vector2FieldGuiElement(string label, Vector2 inputValue, ref bool hasChanged) {
		
		Vector2 newValue = EditorGUILayout.Vector2Field(label, inputValue);
		if (newValue != inputValue) {
			hasChanged = true;
		}
		else {
			hasChanged = false;
		}
		return newValue;
	}
	
	//-------------------------------------------------------------------------
	public static UnityEngine.Object ObjectFieldGuiElement(UnityEngine.GUIContent label, UnityEngine.Object inputValue, System.Type type, bool allowSceneObjects, ref bool hasChanged) {
		
		UnityEngine.Object newValue = EditorGUILayout.ObjectField(label, inputValue, type, allowSceneObjects);
		if (newValue != inputValue) {
			hasChanged = true;
		}
		else {
			hasChanged = false;
		}
		return newValue;
	}
}
