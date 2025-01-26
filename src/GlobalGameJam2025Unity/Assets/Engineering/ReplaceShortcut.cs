#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

using Object = UnityEngine.Object;

namespace Husky.Engineering.Shortcuts
{
	public class ReplaceShortcut : EditorWindow
	{
		private const string ObjectSelectorClosed = "ObjectSelectorClosed";
		private const string ObjectSelectorSelectionDone = "ObjectSelectorSelectionDone";

		private static readonly Regex numerationRemover = new("([._ ]?\\d+|\\s*\\(\\d+\\))$");

		private int currentPickerWindow;
		private bool isOpen = false;
		private bool isDoubleClick = false;

		[MenuItem("Edit/Replace GameObject #r", priority = 1000)]
		public static void Open()
		{
			Event.current.Use();
			var window = CreateInstance<ReplaceShortcut>();

			window.ShowAsDropDown(default, default);
			window.titleContent = new GUIContent("Replace");
			window.minSize = new Vector2(0.0f, 0.0f);
			window.position = new Rect(0.0f, 0.0f, 0.0f, 0.0f);

			window.isDoubleClick = false;
		}

		internal void OnGUI()
		{
			if (!isOpen)
			{
				isOpen = true;

				currentPickerWindow = GUIUtility.GetControlID(FocusType.Passive) + 100;

				GetWindow<SceneView>().ShowNotification(new GUIContent("Shift + R"));
				Focus();

				Object searchSelection = null;
				string searchFilter = "";

				// Create a search based on our current selection
				if (Selection.activeGameObject != null)
				{
					var selectionPrefab = PrefabUtility.GetCorrespondingObjectFromSource(Selection.activeGameObject);
					if (selectionPrefab != null)
					{
						searchSelection = selectionPrefab;

						searchFilter = selectionPrefab.name;

						// Remove suffixes like "_01", ".01", and " (01)" that deliminate duplicates.
						searchFilter = numerationRemover.Replace(searchFilter, "");
					}
				}

				// Test to see whether our filter will be effective.
				if (!string.IsNullOrEmpty(searchFilter))
				{
					string[] foundAssets = AssetDatabase.FindAssets(searchFilter);
					if (foundAssets.Length <= 1)
					{
						searchFilter = "";
					}
				}

				isDoubleClick = false;
				EditorGUIUtility.ShowObjectPicker<GameObject>(searchSelection, false, searchFilter, currentPickerWindow);
			}

			if (Event.current.commandName == ObjectSelectorClosed)
			{
				GetWindow<SceneView>().RemoveNotification();
			}

			if (Event.current.commandName == ObjectSelectorSelectionDone
				&& EditorGUIUtility.GetObjectPickerControlID() == currentPickerWindow)
			{
				isDoubleClick = true;
			}

			if (isDoubleClick
				&& Event.current.commandName == ObjectSelectorClosed
				&& EditorGUIUtility.GetObjectPickerControlID() == currentPickerWindow)
			{
				var replaceWith = EditorGUIUtility.GetObjectPickerObject();
				currentPickerWindow = -1;

				var newSelection = new List<Object>();
				foreach (var selectedObject in Selection.gameObjects)
				{
					if (selectedObject == null)
					{
						// A selected object is null.
						// This is likely because we already destroyed it due to the parent object being replaced with this tool.
						continue;
					}
					else if (PrefabUtility.IsAnyPrefabInstanceRoot(selectedObject) || !PrefabUtility.IsPartOfAnyPrefab(selectedObject))
					{
						if (replaceWith != null)
						{
							var instantiatedPrefab = PrefabUtility.InstantiatePrefab(replaceWith, selectedObject.scene) as GameObject;
							instantiatedPrefab.transform.SetParent(selectedObject.transform.parent);

							instantiatedPrefab.transform.SetLocalPositionAndRotation(selectedObject.transform.localPosition, selectedObject.transform.localRotation);
							instantiatedPrefab.transform.localScale = selectedObject.transform.localScale;
							instantiatedPrefab.transform.SetSiblingIndex(selectedObject.transform.GetSiblingIndex());

							newSelection.Add(instantiatedPrefab);

							Undo.DestroyObjectImmediate(selectedObject);
							Undo.RegisterCreatedObjectUndo(instantiatedPrefab, Selection.gameObjects.Length == 1 ? "Replaced Object" : "Replaced Objects");
						}
						else
						{
							Undo.DestroyObjectImmediate(selectedObject);
						}
					}
					else
					{
						Debug.Log($"Can't replace '{selectedObject.name}' as it is a part of a prefab.");
					}
				}
				Selection.objects = newSelection.ToArray();

				EditorApplication.RepaintHierarchyWindow();
			}
		}
	}
}
#endif
