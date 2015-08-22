using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(EnumListAttribute))]
public class EnumListDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return EditorGUI.GetPropertyHeight(property);
	}
	
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EnumListAttribute enumListAttribute = attribute as EnumListAttribute;
		
		//look at the property path to see if it is an array
		string path = property.propertyPath;
		bool isArray = (path.LastIndexOf(".Array") >= 0);
		
		if (isArray)
		{
			//get element index
			int indStart = path.IndexOf("[") + 1;
			int indEnd = path.IndexOf("]");
			string indString = path.Substring(indStart, indEnd - indStart);
			int myIndex = int.Parse(indString);
			
			label.text = enumListAttribute.GetName(myIndex);
		}
		
		Rect rc = position;
		EditorGUI.PropertyField(rc, property, label, true);
	}
}