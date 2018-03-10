// Place this script in the Editor folder in your project.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[InitializeOnLoad]
class CategoryTool 
{
	// SaveDataKeyName is the project name along with " CategoryToolDataKey". This make sure
	// there are individual keys for each project that uses this tool.
	static string SaveDataKeyName = Application.productName + " CategoryToolDataKey"; 
	
	static string CategoryLayer = "Category";
	static bool CategoryLayerFound = false;
	static bool ShowAciveInactiveOption = false;
	static bool ShowDeleteOption = false;
	static Color CategorySelectedBackgroundColor;
	static Color CategoryNotSelectedBackgroundColor;
	static Color PrevCategorySelectedBackgroundColor;
	static Color PrevCategoryNotSelectedBackgroundColor;
	static Color CategorySelectedTextColor;
	static Color CategoryNotSelectedTextColor;
	static Color CategoryInactiveTextColor;


	static Texture2D categoryNotSelectedBackgroundTex2D;
	static Texture2D categorySelectedBackgroundTex2D;
	static Vector2 categoryBackgroundTextureSize;

	static CategoryTool()
	{
		// Initialize the texture size at the beginning. The actual values get set later. 
		categoryBackgroundTextureSize = Vector2.one;

		if(EditorPrefs.HasKey(SaveDataKeyName)) LoadData();
		else 
		{
			ShowAciveInactiveOption = false; ShowDeleteOption = false;
			CategorySelectedBackgroundColor = new Color(135f/255f, 127f/255f, 233f/255f, 1f);
			CategoryNotSelectedBackgroundColor = new Color(234f/255f, 129f/255f, 129f/255f, 1f);
			CategorySelectedTextColor = Color.black; CategoryNotSelectedTextColor = Color.black; CategoryInactiveTextColor = Color.grey;
		}

		// Hook into the hierarchyWindowItemOnGUI event so that we can write GUI code for each item in the Hierarchy window.
		EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemOnGUI;

		// Check if Category layer exists, and if it doesn't display a warning message in the Console window.
		CategoryLayerFound = LayerMask.NameToLayer(CategoryLayer) != -1;
		if(!CategoryLayerFound) Debug.LogWarning("'" + CategoryLayer + "' layer not found. Create a '" + CategoryLayer + 
			"' layer in the project, otherwise the Category Tool won't work properly.");		
	}

	static void HierarchyItemOnGUI(int instanceID, Rect hierarchyItemRect)
	{
		CategoryLayerFound = LayerMask.NameToLayer(CategoryLayer) != -1;
		if(!CategoryLayerFound) return; // If 'Category' layer is not found, do not proceed.

		Rect categoryRect = new Rect(0, hierarchyItemRect.y, hierarchyItemRect.width+hierarchyItemRect.x, hierarchyItemRect.height);
		Rect activeInactiveRect = new Rect(0f, hierarchyItemRect.y, 20f, hierarchyItemRect.height);
		
		// The logic behind this calculation is:
		// A gameobject 'SomeGameObject' is displayed in the Hirarchy window, with it's X position offset by 30.
		// A child gameobject 'SomeChildGameObject', which is a child of 'SomeGameObject', will be displayed at the offset 30+14. 
		// A child gameobject 'SomeChildChildGameObject', which is a child of 'SomeChildGameObject', will be displayer at
		// the offset 30+14+14.
		// hierarchyItemRect.x shows the left side position of the rect.
		// hierarchyItemRect.width shows the right side position of the rect.
		// So the calculation goes X = (hierarchyItemRect.width - (30f - hierarchyItemRect.x))
		// The result is that delRect is always displayed on the right side of the Hierarchy window.
		Rect delRect = new Rect(
			(hierarchyItemRect.width - (30f - hierarchyItemRect.x)), hierarchyItemRect.y, 20f, hierarchyItemRect.height);
		
		// EditorUtility.InstanceIDToObject(instanceID) will return null if the object is not a GameObject, for example, if
		// the instanceID points to the Scene name in the Hierarchy.  
		if(EditorUtility.InstanceIDToObject(instanceID) != null)
		{
			categoryBackgroundTextureSize = new Vector2(hierarchyItemRect.width, hierarchyItemRect.height);

			if(categorySelectedBackgroundTex2D == null) UpdateCategorySelectedBackgroundTex2D();
			if(categoryNotSelectedBackgroundTex2D == null) UpdateCategoryNotSelectedBackgroundTex2D();

			GameObject hierarchyItemGameObject = (GameObject)EditorUtility.InstanceIDToObject(instanceID);

			GUIStyle catergorySelectedTextGUIStyle = new GUIStyle()
			{
				fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
				normal = new GUIStyleState() { 
					textColor = hierarchyItemGameObject.activeSelf ? CategorySelectedTextColor : CategoryInactiveTextColor
				}
			};

			GUIStyle categorySelectedBackgroundGUIStyle = new GUIStyle(GUI.skin.button)
			{ normal = new GUIStyleState() { textColor = CategorySelectedTextColor, background = categorySelectedBackgroundTex2D } };

			GUIStyle categoryNotSelectedTextGUIStyle = new GUIStyle()
			{
				fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter,
				normal = new GUIStyleState() { 
					textColor = hierarchyItemGameObject.activeSelf ? CategoryNotSelectedTextColor : CategoryInactiveTextColor 
				}			
			};

			GUIStyle categoryNotSelectedBackgroundGUIStyle = new GUIStyle(GUI.skin.button)
			{ normal = new GUIStyleState() { textColor = CategoryNotSelectedTextColor, background = categoryNotSelectedBackgroundTex2D } };


			if(hierarchyItemGameObject.layer == LayerMask.NameToLayer("Category")) 
			{
				GUIStyle categoryBackgroundGUIStyleToUse = new GUIStyle();

				// Code to support multi-category selection in Hierarchy.
				if(Selection.gameObjects.Length > 0)
				{
					GameObject[] selectedGameObjects = Selection.gameObjects;

					for(int i = 0; i < selectedGameObjects.Length; i++)
					{
						if(selectedGameObjects[i] == hierarchyItemGameObject)
						{ categoryBackgroundGUIStyleToUse = categorySelectedBackgroundGUIStyle; break; }
						else { categoryBackgroundGUIStyleToUse = categoryNotSelectedBackgroundGUIStyle; }
					}
				}
				// Code for when no gameobject are selected in the Hierarchy.
				else if(Selection.gameObjects.Length == 0) { categoryBackgroundGUIStyleToUse = categoryNotSelectedBackgroundGUIStyle; }

				GUI.Box(categoryRect, "", categoryBackgroundGUIStyleToUse);
				GUI.Label(categoryRect,	hierarchyItemGameObject.name, 
					Selection.activeGameObject == hierarchyItemGameObject ? catergorySelectedTextGUIStyle : categoryNotSelectedTextGUIStyle);
			}
			if(ShowDeleteOption)
				// If [X] button pressed, call Undo.DestroyObjectImmediate() so that the destroy action is record and undo can
				// be called later if required.
				if(GUI.Button(delRect, "X")){ Undo.DestroyObjectImmediate(hierarchyItemGameObject); return; }
			
			if(ShowAciveInactiveOption)
			{
				bool gameObjectActiveState = hierarchyItemGameObject.activeSelf;
				gameObjectActiveState = GUI.Toggle(activeInactiveRect, gameObjectActiveState, "");
				
				// If gameobject active state is different from gameObjectActiveState variable, record object undo, then
				// change gameobject active state.
				if(gameObjectActiveState != hierarchyItemGameObject.activeSelf) 
				{ Undo.RecordObject(hierarchyItemGameObject, "Go Active State"); hierarchyItemGameObject.SetActive(gameObjectActiveState); }
			}
		}
	}

	// GUI code for the Category Tool section in the Preferences window.
	[PreferenceItem("Category Tool")]
	static void PreferenceGUI()
	{
		GUIStyle OptionLabelGUIStyle = new GUIStyle();
		OptionLabelGUIStyle.fontStyle = FontStyle.Bold;
		OptionLabelGUIStyle.fontSize = 12;

		EditorGUILayout.LabelField("Category Options", OptionLabelGUIStyle);
		CategorySelectedBackgroundColor = EditorGUILayout.ColorField("Selected Background Color", CategorySelectedBackgroundColor);
		CategorySelectedBackgroundColor.a = 1f;
		CategoryNotSelectedBackgroundColor = EditorGUILayout.ColorField("Not Selected Background Color", 
			CategoryNotSelectedBackgroundColor);
		CategoryNotSelectedBackgroundColor.a = 1f;
		CategorySelectedTextColor = EditorGUILayout.ColorField("Selected Text Color", CategorySelectedTextColor);
		CategoryNotSelectedTextColor = EditorGUILayout.ColorField("Not Selected Text Color", CategoryNotSelectedTextColor);
		CategoryInactiveTextColor = EditorGUILayout.ColorField("Inactive Text Color", CategoryInactiveTextColor);
 
		EditorGUILayout.Space(); EditorGUILayout.Space(); 

		EditorGUILayout.LabelField("Other Options", OptionLabelGUIStyle);
		ShowAciveInactiveOption = EditorGUILayout.Toggle("Show Enable/Disable Option", ShowAciveInactiveOption);
		ShowDeleteOption = EditorGUILayout.Toggle("Show Delete Option", ShowDeleteOption);

		/// This is a Perfomance Optimisation.The Textures will only be reinitialized and updated if the color has changed.
		if(CategorySelectedBackgroundColor != PrevCategorySelectedBackgroundColor) UpdateCategorySelectedBackgroundTex2D();
		if(CategoryNotSelectedBackgroundColor != PrevCategoryNotSelectedBackgroundColor) UpdateCategoryNotSelectedBackgroundTex2D();

		PrevCategorySelectedBackgroundColor = CategorySelectedBackgroundColor;
		PrevCategoryNotSelectedBackgroundColor = CategoryNotSelectedBackgroundColor;

		EditorGUILayout.HelpBox("Enable/Disable and Delete options don't work on multiple objects. " + 
			"These operations only work on individual objects.", MessageType.Info);

		EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); EditorGUILayout.Space(); 

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.Separator(); EditorGUILayout.Separator(); 
		if(GUILayout.Button("Reset Category Tool"))
		{ if(EditorPrefs.HasKey(SaveDataKeyName)) EditorPrefs.DeleteKey(SaveDataKeyName); ResetValues(); }
		EditorGUILayout.Separator(); EditorGUILayout.Separator();
		EditorGUILayout.EndHorizontal();

		// Saving data at every PreferenceGUI call is not the best method of saving data but it's one of the best methods I could find.
		SaveData();

		// Repaint Hierarchy window so changes reflect.
		EditorApplication.RepaintHierarchyWindow();
	}

	static void ResetValues()
	{
		ShowAciveInactiveOption = false; ShowDeleteOption = false;
		CategorySelectedBackgroundColor = new Color(135f/255f, 127f/255f, 233f/255f, 1f);
		CategoryNotSelectedBackgroundColor = new Color(234f/255f, 129f/255f, 129f/255f, 1f);
		CategorySelectedTextColor = Color.black; CategoryNotSelectedTextColor = Color.black; CategoryInactiveTextColor = Color.grey;
	}

	static void UpdateCategorySelectedBackgroundTex2D()
	{
		categorySelectedBackgroundTex2D = new Texture2D((int)categoryBackgroundTextureSize.x,(int)categoryBackgroundTextureSize.y);
		for(int i = 0; i < categorySelectedBackgroundTex2D.width; i++)
		{
			for(int j = 0; j < categorySelectedBackgroundTex2D.height; j++)
			{
				categorySelectedBackgroundTex2D.SetPixel(i,j,CategorySelectedBackgroundColor);
			}
		}
		categorySelectedBackgroundTex2D.Apply();
	}

	static void UpdateCategoryNotSelectedBackgroundTex2D()
	{
		categoryNotSelectedBackgroundTex2D = new Texture2D((int)categoryBackgroundTextureSize.x,(int)categoryBackgroundTextureSize.y);
		for(int i = 0; i < categoryNotSelectedBackgroundTex2D.width; i++)
		{
			for(int j = 0; j < categoryNotSelectedBackgroundTex2D.height; j++)
			{
				categoryNotSelectedBackgroundTex2D.SetPixel(i,j,CategoryNotSelectedBackgroundColor);
			}
		}
		categoryNotSelectedBackgroundTex2D.Apply();
	}

	[MenuItem("GameObject/Create Other/Category")]
	static void CreateCategory()
	{
		// Make sure Category layer exists. If it doesn't exist, give the user a warning and do not proceed.
		if(!CategoryLayerFound) { 
			Debug.LogError("'" + CategoryLayer + "' layer not found. Aborting action. Make sure the project containts a layer " + 
			"named '" + CategoryLayer + "'.");
			return; 
		}
		
		// Create gameobject and set it's tag to "EditorOnly" and layer to "Category".
		GameObject categoryGameObject = new GameObject("New Category");
		categoryGameObject.layer = LayerMask.NameToLayer(CategoryLayer);
		categoryGameObject.tag = "EditorOnly";

		// Register the created object undo so that the action can be reversed if required. 
		Undo.RegisterCreatedObjectUndo(categoryGameObject, "Category GameObject");

		UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
	}

	static void SaveData()
	{
		CategoryToolData categoryToolData = new CategoryToolData() {
			ShowAciveInactiveOptionData = ShowAciveInactiveOption, ShowDeleteOptionData = ShowDeleteOption,
			CategorySelectedBackgroundColorData = CategorySelectedBackgroundColor,
			CategoryNotSelectedBackgroundColorData = CategoryNotSelectedBackgroundColor,
			CategorySelectedTextColorData = CategorySelectedTextColor, CategoryNotSelectedTextColorData = CategoryNotSelectedTextColor,
			CategoryInactiveTextColorData = CategoryInactiveTextColor
		};

		string keyData = JsonUtility.ToJson(categoryToolData);

		EditorPrefs.SetString(SaveDataKeyName, keyData);
	}

	static void LoadData()
	{
		if(EditorPrefs.HasKey(SaveDataKeyName))
		{
			string keyData = EditorPrefs.GetString(SaveDataKeyName);

			CategoryToolData categoryToolData = (CategoryToolData) JsonUtility.FromJson(keyData, typeof(CategoryToolData));

			ShowAciveInactiveOption = categoryToolData.ShowAciveInactiveOptionData;
			ShowDeleteOption = categoryToolData.ShowDeleteOptionData;
			CategorySelectedBackgroundColor = categoryToolData.CategorySelectedBackgroundColorData;
			CategoryNotSelectedBackgroundColor = categoryToolData.CategoryNotSelectedBackgroundColorData;
			CategorySelectedTextColor = categoryToolData.CategorySelectedTextColorData;
			CategoryNotSelectedTextColor = categoryToolData.CategoryNotSelectedTextColorData;
			CategoryInactiveTextColor = categoryToolData.CategoryInactiveTextColorData;
		}
	}
}

public class CategoryToolData
{
	public bool ShowAciveInactiveOptionData = false; public bool ShowDeleteOptionData = false;
	public Color CategorySelectedBackgroundColorData;	public Color CategoryNotSelectedBackgroundColorData;
	public Color CategorySelectedTextColorData; public Color CategoryNotSelectedTextColorData; public Color CategoryInactiveTextColorData;
}
