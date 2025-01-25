#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HuskyUnity.Engineering.TypeSelector
{
    public static class TypeSelectorUI
    {
        public static VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return new TypeSelectorField(property, property.displayName);
        }

        private static void InvokeOnValidate(UnityEngine.Object[] targets)
        {
            foreach (var target in targets)
            {
                var targetMonoBehaviour = target as MonoBehaviour;
                if (targetMonoBehaviour)
                {
                    var targetMonoBehaviourType = targetMonoBehaviour.GetType();
                    var onValidateMethod = targetMonoBehaviourType.GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

                    onValidateMethod?.Invoke(targetMonoBehaviour, Array.Empty<object>());
                }
            }
        }

        public static float GetFoldoutSelectorPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.serializedObject.targetObjects.Length > 1)
            {
                object[] currentValues = AdvancedGUI.GetPropertyValues(property);
                bool isDifferentTypes = IsValuesOfDifferentTypes(currentValues);

                if (isDifferentTypes)
                {
                    if (!property.isExpanded)
                    {
                        return EditorGUIUtility.singleLineHeight;
                    }
                    return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + 30;
                }
            }
            return EditorGUI.GetPropertyHeight(property);
        }

        public static float GetStaticSelectorPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.serializedObject.targetObjects.Length > 1)
            {
                object[] currentValues = AdvancedGUI.GetPropertyValues(property);
                bool isDifferentTypes = IsValuesOfDifferentTypes(currentValues);

                if (isDifferentTypes)
                {
                    return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing + 30 + 20;
                }
            }

            float totalHeight = EditorGUIUtility.singleLineHeight;

            var enumerator = property.Copy();
            EditorGUI.indentLevel++;
            EditorGUIUtility.labelWidth += 17;
            if (enumerator.NextVisible(true))
            {
                do
                {
                    if (enumerator.depth <= property.depth)
                    {
                        break;
                    }

                    totalHeight += EditorGUI.GetPropertyHeight(enumerator) + EditorGUIUtility.standardVerticalSpacing;
                }
                while (enumerator.NextVisible(false));
            }
            EditorGUIUtility.labelWidth -= 17;
            EditorGUI.indentLevel--;

            if (totalHeight == EditorGUIUtility.singleLineHeight)
            {
                return totalHeight;
            }
            return totalHeight + 20;
        }

        public static void DrawStaticSelector(Rect position, SerializedProperty property, GUIContent label, bool allowNull = true)
        {
            var rowRect = new Rect(
                position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight);

            Rect popupButtonRect;

            if (string.IsNullOrEmpty(label.text))
            {
                popupButtonRect = new Rect(
                    rowRect.x,
                    rowRect.y,
                    rowRect.width,
                    EditorGUIUtility.singleLineHeight);
            }
            else
            {
                popupButtonRect = new Rect(
                    rowRect.x + EditorGUIUtility.labelWidth + EditorGUIUtility.standardVerticalSpacing,
                    rowRect.y,
                    rowRect.width - EditorGUIUtility.labelWidth - EditorGUIUtility.standardVerticalSpacing,
                    EditorGUIUtility.singleLineHeight);
            }

            var baseTypeInfo = TypeSelecting.GetBaseTypeInfoFromString(property.managedReferenceFieldTypename);
            object[] currentValues = AdvancedGUI.GetPropertyValues(property);

            DrawDropdownTypeSelectorButton(popupButtonRect, property, baseTypeInfo, currentValues);

            if (!string.IsNullOrEmpty(label.text))
            {
                var labelRect = new Rect(
                    rowRect.x,
                    rowRect.y,
                    EditorGUIUtility.labelWidth,
                    EditorGUIUtility.singleLineHeight);
                EditorGUI.LabelField(labelRect, label);
            }

            bool isDifferentTypes = IsValuesOfDifferentTypes(currentValues);
            if (isDifferentTypes)
            {
                var helpBoxRowRect = new Rect(
                    rowRect.x,
                    rowRect.y + rowRect.height + EditorGUIUtility.standardVerticalSpacing + 10,
                    rowRect.width,
                    30);

                MultipleTypesHint(helpBoxRowRect);
            }
            else
            {
                // If the GUI is disabled, prevent clicking on the greyed out button from collapsing the property.
                if (!GUI.enabled
                    && (Event.current.rawType == EventType.MouseDown || Event.current.rawType == EventType.MouseUp)
                    && Event.current.button == 0
                    && popupButtonRect.Contains(Event.current.mousePosition))
                {
                    Event.current.Use();
                }

                rowRect.y += 10;

                var enumerator = property.Copy();
                EditorGUI.indentLevel++;
                EditorGUIUtility.labelWidth += 17;
                if (enumerator.NextVisible(true))
                {
                    do
                    {
                        if (enumerator.depth <= property.depth)
                        {
                            break;
                        }

                        rowRect.y += rowRect.height + EditorGUIUtility.standardVerticalSpacing;
                        rowRect.height = EditorGUI.GetPropertyHeight(enumerator);
                        EditorGUI.PropertyField(rowRect, enumerator, true);
                    }
                    while (enumerator.NextVisible(false));
                }
                EditorGUIUtility.labelWidth -= 17;
                EditorGUI.indentLevel--;
            }
        }

        public static void DrawFoldoutSelector(Rect position, SerializedProperty property, GUIContent label, bool allowNull = true)
        {
            var rowRect = new Rect(
                position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight);

            var labelRect = new Rect(
                rowRect.x,
                rowRect.y,
                EditorGUIUtility.labelWidth,
                EditorGUIUtility.singleLineHeight);

            var popupButtonRect = new Rect(
                labelRect.xMax + EditorGUIUtility.standardVerticalSpacing,
                rowRect.y,
                rowRect.width - labelRect.width - EditorGUIUtility.standardVerticalSpacing,
                EditorGUIUtility.singleLineHeight);

            var baseTypeInfo = TypeSelecting.GetBaseTypeInfoFromString(property.managedReferenceFieldTypename);
            object[] currentValues = AdvancedGUI.GetPropertyValues(property);

            DrawDropdownTypeSelectorButton(popupButtonRect, property, baseTypeInfo, currentValues);

            bool isDifferentTypes = IsValuesOfDifferentTypes(currentValues);
            bool isValuesNull = IsValuesNull(currentValues);

            if (isValuesNull)
            {
                EditorGUI.LabelField(position, label);
            }
            else if (isDifferentTypes || !property.isExpanded)
            {
                bool originalValue = EditorGUI.showMixedValue;
                EditorGUI.showMixedValue = false;
                property.isExpanded = EditorGUI.Foldout(labelRect, property.isExpanded, label, true);
                EditorGUI.showMixedValue = originalValue;
            }

            if (property.isExpanded)
            {
                if (isDifferentTypes)
                {
                    var helpBoxRowRect = new Rect(
                        rowRect.x,
                        rowRect.y + rowRect.height + EditorGUIUtility.standardVerticalSpacing,
                        rowRect.width,
                        30);

                    MultipleTypesHint(helpBoxRowRect);
                }
                else
                {
                    // If the GUI is disabled, prevent clicking on the greyed out button from collapsing the property.
                    if (!GUI.enabled
                        && (Event.current.rawType == EventType.MouseDown || Event.current.rawType == EventType.MouseUp)
                        && Event.current.button == 0
                        && popupButtonRect.Contains(Event.current.mousePosition))
                    {
                        Event.current.Use();
                    }

                    EditorGUI.PropertyField(rowRect, property, label, true);
                }
            }
        }

        private static void DrawDropdownTypeSelectorButton(
            Rect popupButtonRect,
            SerializedProperty property,
            TypeSelecting.BaseType baseTypeInfo,
            object[] currentValues)
        {
            bool isDifferentTypes = IsValuesOfDifferentTypes(currentValues);

            GUIContent label;
            if (isDifferentTypes)
            {
                label = new GUIContent("―", "Mixed Types");
            }
            else
            {
                var subTypeInfo = baseTypeInfo.GetSubTypeFromString(property.managedReferenceFullTypename);

                if (subTypeInfo == null)
                {
                    label = new GUIContent(" Missing Type!");
                }
                else
                {
                    label = new GUIContent(" " + subTypeInfo.typeDisplayName, subTypeInfo.typeIcon);
                }
            }

            if (GUI.Button(popupButtonRect, label, EditorStyles.popup))
            {
                if (Event.current.button == 2)
                {
                    var subTypeInfo = baseTypeInfo.GetSubTypeFromString(property.managedReferenceFullTypename);

                    if (subTypeInfo != null
                        && !string.IsNullOrEmpty(subTypeInfo.typeDeclarationFilePath))
                    {
                        var sourceObject = AssetDatabase.LoadMainAssetAtPath(subTypeInfo.typeDeclarationFilePath);
                        EditorGUIUtility.PingObject(sourceObject);
                    }
                }
                else
                {
                    var screenPoint = GUIUtility.GUIToScreenPoint(new Vector2(popupButtonRect.x, popupButtonRect.y));
                    var screenSpacePopupButtonRect = new Rect(
                        screenPoint.x,
                        screenPoint.y,
                        popupButtonRect.width,
                        popupButtonRect.height);

                    TypeSelectorListPopupWindow.OpenPopupWindow(screenSpacePopupButtonRect, baseTypeInfo, currentValues, selectedType =>
                    {
                        Undo.RegisterCompleteObjectUndo(property.serializedObject.targetObjects, "Change type");

                        property.serializedObject.ApplyModifiedProperties();

                        // Set to null and THEN set the values to invoke the changed events properly.
                        bool isValuesNull = IsValuesNull(currentValues);
                        if (!isValuesNull)
                        {
                            AdvancedGUI.SetPropertyValues(property, () =>
                            {
                                return null;
                            });
                        }

                        if (selectedType != null)
                        {
                            AdvancedGUI.SetPropertyValues(property, () =>
                            {
                                return Activator.CreateInstance(selectedType);
                            });
                        }

                        // Invoke OnValidate methods.
                        InvokeOnValidate(property.serializedObject.targetObjects);

                        foreach (var target in property.serializedObject.targetObjects)
                        {
                            EditorUtility.SetDirty(target);
                        }

                        property.serializedObject.Update();
                    });
                }
            }
        }

        private static void MultipleTypesHint(Rect helpBoxRowRect)
        {
            EditorGUI.indentLevel++;
            var helpBoxRect = EditorGUI.IndentedRect(helpBoxRowRect);
            EditorGUI.indentLevel--;

            EditorGUI.HelpBox(helpBoxRect, "Multiple object types", MessageType.Info);
        }

        private static bool IsValuesOfDifferentTypes(object[] values)
        {
            if (values.Length <= 1)
            {
                return false;
            }
            bool isDifferentTypes = false;
            var type = values[0]?.GetType();
            for (int i = 1; i < values.Length; i++)
            {
                if (values[i]?.GetType() != type)
                {
                    isDifferentTypes = true;
                    break;
                }
            }
            return isDifferentTypes;
        }

        private static bool IsValuesNull(object[] values)
        {
            bool anyNotNull = false;
            foreach (object value in values)
            {
                if (value != null)
                {
                    anyNotNull = true;
                }
            }
            return !anyNotNull;
        }
    }
}
#endif
