#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HuskyUnity.Engineering.TypeSelector
{
    public class TypeSelectorField : VisualElement
    {
        public static readonly string ussClassName = "holodrone-typeselector-foldoutfield";

        public static readonly string labelUssClassName = ussClassName + "__label";
        public static readonly string dropdownUssClassName = ussClassName + "__dropdown";
        public static readonly string valueIconUssClassName = ussClassName + "__valueicon";
        public static readonly string textUssClassName = ussClassName + "__text";
        public static readonly string spacerUssClassName = ussClassName + "__spacer";
        public static readonly string arrowUssClassName = ussClassName + "__arrow";

        private static CustomStyleProperty<float> s_LabelWidthRatioProperty = new("--unity-property-field-label-width-ratio");
        private static CustomStyleProperty<float> s_LabelExtraPaddingProperty = new("--unity-property-field-label-extra-padding");
        private static CustomStyleProperty<float> s_LabelBaseMinWidthProperty = new("--unity-property-field-label-base-min-width");
        private static CustomStyleProperty<float> s_LabelExtraContextWidthProperty = new("--unity-base-field-extra-context-width");


        private readonly Label labelElement;
        private readonly Foldout foldoutElement;
        private readonly VisualElement popupButton;
        private readonly Image valueIconElement;
        private readonly Label valueLabelElement;
        private readonly VisualElement arrowElement;

        private readonly SerializedProperty serializedProperty;
        private readonly string label;

        private float m_LabelWidthRatio;
        private float m_LabelExtraPadding;
        private float m_LabelBaseMinWidth;
        private float m_LabelExtraContextWidth;

        private readonly TypeSelecting.BaseType baseTypeInfo;

        internal TypeSelectorField(
            SerializedProperty serializedProperty,
            string label)
        {
            this.serializedProperty = serializedProperty;
            this.label = label;

            styleSheets.AddComponentStylesheet();

            baseTypeInfo = TypeSelecting.GetBaseTypeInfoFromString(serializedProperty.managedReferenceFieldTypename);

            AddToClassList(ussClassName);

            foldoutElement = new Foldout
            {
                text = label,
            };
            foldoutElement.AddToClassList(labelUssClassName);
            Add(foldoutElement);

            labelElement = new Label
            {
                text = label,
                style =
                {
                    display = DisplayStyle.None,
                }
            };
            labelElement.AddToClassList(labelUssClassName);
            Add(labelElement);

            // BaseField<T0>.alignedFieldUssClassName

            popupButton = new VisualElement
            {
                focusable = true,
            };
            popupButton.AddToClassList(dropdownUssClassName);
            Add(popupButton);

            valueIconElement = new Image
            {

            };
            valueIconElement.AddToClassList(valueIconUssClassName);
            popupButton.Add(valueIconElement);

            valueLabelElement = new Label
            {
                text = label
            };
            valueLabelElement.AddToClassList(textUssClassName);
            popupButton.Add(valueLabelElement);

            var spacerElement = new VisualElement();
            spacerElement.AddToClassList(spacerUssClassName);
            popupButton.Add(spacerElement);

            arrowElement = new VisualElement
            {
                pickingMode = PickingMode.Ignore
            };
            arrowElement.AddToClassList(arrowUssClassName);
            arrowElement.AddToClassList("unity-base-popup-field__arrow");
            popupButton.Add(arrowElement);

            popupButton.RegisterCallback<PointerDownEvent>(OnPointerDownEvent);
            popupButton.RegisterCallback<PointerMoveEvent>(OnPointerMoveEvent);
            popupButton.RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
            popupButton.RegisterCallback(delegate (MouseDownEvent evt)
            {
                if (evt.button == 0)
                {
                    evt.StopPropagation();
                }
                else if (evt.button == 2)
                {
                    var subTypeInfo = baseTypeInfo.GetSubTypeFromString(serializedProperty.managedReferenceFullTypename);

                    if (subTypeInfo != null
                        && !string.IsNullOrEmpty(subTypeInfo.typeDeclarationFilePath))
                    {
                        var sourceObject = AssetDatabase.LoadMainAssetAtPath(subTypeInfo.typeDeclarationFilePath);
                        EditorGUIUtility.PingObject(sourceObject);
                    }
                }
            });

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);


            void OnPropertyChangedCallback(SerializedProperty actionProperty)
            {
                // propertyField.Unbind();
                // propertyField.BindProperty(serializedProperty);
                // propertyField.TrackPropertyValue(serializedProperty, OnPropertyChangedCallback);

                UpdateLabels();
            }
            this.TrackPropertyValue(serializedProperty, OnPropertyChangedCallback);
            // propertyField.BindProperty(serializedProperty);

            UpdateLabels();

            void UpdateLabels()
            {
                object[] labelCurrentValues = AdvancedGUI.GetPropertyValues(serializedProperty);

                bool labelIsValuesNull = IsValuesNull(labelCurrentValues);
                bool isDifferentTypes = IsValuesOfDifferentTypes(labelCurrentValues);

                // if (labelIsValuesNull || isDifferentTypes)
                // {
                // 	typePopup.pickingMode = PickingMode.Position;
                // 	typePopup.labelElement.pickingMode = PickingMode.Position;
                // 	typePopup.labelElement.style.visibility = Visibility.Visible;
                // }
                // else
                // {
                // 	typePopup.pickingMode = PickingMode.Ignore;
                // 	typePopup.labelElement.pickingMode = PickingMode.Ignore;
                // 	typePopup.labelElement.style.visibility = Visibility.Hidden;
                // }

                if (isDifferentTypes)
                {
                    AddToClassList("mixedvalues");

                    valueLabelElement.text = "―";
                    valueIconElement.style.display = DisplayStyle.None;
                    valueIconElement.image = null;
                }
                else
                {
                    RemoveFromClassList("mixedvalues");

                    var subTypeInfo = baseTypeInfo.GetSubTypeFromString(serializedProperty.managedReferenceFullTypename);
                    if (subTypeInfo != null)
                    {
                        valueLabelElement.text = subTypeInfo.typeDisplayName;
                        if (subTypeInfo.typeIcon != null)
                        {
                            valueIconElement.style.display = DisplayStyle.Flex;
                            valueIconElement.image = subTypeInfo.typeIcon;
                        }
                        else
                        {
                            valueIconElement.style.display = DisplayStyle.None;
                            valueIconElement.image = null;
                        }
                    }
                    else
                    {
                        valueLabelElement.text = "None";
                        valueIconElement.style.display = DisplayStyle.None;
                        valueIconElement.image = null;
                    }
                }
            }
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            if (evt.customStyle.TryGetValue(s_LabelWidthRatioProperty, out float labelWidthRatio))
            {
                m_LabelWidthRatio = labelWidthRatio;
            }

            if (evt.customStyle.TryGetValue(s_LabelExtraPaddingProperty, out float labelExtraPadding))
            {
                m_LabelExtraPadding = labelExtraPadding;
            }

            if (evt.customStyle.TryGetValue(s_LabelBaseMinWidthProperty, out float labelBaseMinWidth))
            {
                m_LabelBaseMinWidth = labelBaseMinWidth;
            }

            if (evt.customStyle.TryGetValue(s_LabelExtraContextWidthProperty, out float labelExtraContextWidth))
            {
                m_LabelExtraContextWidth = labelExtraContextWidth;
            }

            AlignLabel();
        }

        private void OnAttachToPanel(AttachToPanelEvent e)
        {
            m_LabelWidthRatio = 0.45f;
            m_LabelExtraPadding = 37f;
            m_LabelBaseMinWidth = 123f;
            m_LabelExtraContextWidth = 1f;
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
            RegisterCallback<GeometryChangedEvent>(OnInspectorFieldGeometryChanged);
        }

        private void OnInspectorFieldGeometryChanged(GeometryChangedEvent e)
        {
            AlignLabel();
        }

        private void AlignLabel()
        {
            // if (ClassListContains(alignedFieldUssClassName) && m_CachedInspectorElement != null)
            {
                float labelExtraPadding = m_LabelExtraPadding;
                float num = base.worldBound.x - panel.visualTree.worldBound.x - panel.visualTree.resolvedStyle.paddingLeft;
                labelExtraPadding += num;
                labelExtraPadding += base.resolvedStyle.paddingLeft;
                float a = m_LabelBaseMinWidth - num - base.resolvedStyle.paddingLeft;
                var visualElement = panel.visualTree;
                foldoutElement.style.minWidth = Mathf.Max(a, 0f);
                float num2 = ((visualElement.resolvedStyle.width + m_LabelExtraContextWidth) * m_LabelWidthRatio) - labelExtraPadding;
                if (Mathf.Abs(foldoutElement.resolvedStyle.width - num2) > 1E-30f)
                {
                    foldoutElement.style.width = Mathf.Max(0f, num2);
                }
            }
        }


        internal TypeSelectorField(
            SerializedProperty serializedProperty)
            : this(serializedProperty, serializedProperty.displayName)
        {
        }

        private void OnPointerDownEvent(PointerDownEvent evt)
        {
            ProcessPointerDown(evt);
        }

        private void OnPointerMoveEvent(PointerMoveEvent evt)
        {
            if (evt.button == 0 && ((uint)evt.pressedButtons & (true ? 1u : 0u)) != 0)
            {
                ProcessPointerDown(evt);
            }
        }


        private void ProcessPointerDown<T>(PointerEventBase<T> evt) where T : PointerEventBase<T>, new()
        {
            if (evt.button == 0)
            {
                // base.schedule.Execute(ShowMenu);
                ShowMenu();
                evt.StopPropagation();
            }
        }

        private void OnNavigationSubmit(NavigationSubmitEvent evt)
        {
            ShowMenu();
            evt.StopPropagation();
        }

        internal void ShowMenu()
        {
            var screenPoint = GUIUtility.GUIToScreenPoint(new Vector2(popupButton.worldBound.x, popupButton.worldBound.y));
            var screenSpacePopupButtonRect = new Rect(
                screenPoint.x,
                screenPoint.y,
                popupButton.worldBound.width,
                popupButton.worldBound.height);

            object[] currentValues = AdvancedGUI.GetPropertyValues(serializedProperty);
            TypeSelectorListPopupWindow.OpenPopupWindow(screenSpacePopupButtonRect, baseTypeInfo, currentValues, selectedType =>
            {
                Undo.RegisterCompleteObjectUndo(serializedProperty.serializedObject.targetObjects, "Change type");

                serializedProperty.serializedObject.ApplyModifiedProperties();

                // Set to null and THEN set the values to invoke the changed events properly.
                bool isValuesNull = IsValuesNull(currentValues);
                if (!isValuesNull)
                {
                    AdvancedGUI.SetPropertyValues(serializedProperty, () =>
                    {
                        return null;
                    });
                }

                if (selectedType != null)
                {
                    AdvancedGUI.SetPropertyValues(serializedProperty, () =>
                    {
                        return Activator.CreateInstance(selectedType);
                    });
                }

                // Invoke OnValidate methods.
                InvokeOnValidate(serializedProperty.serializedObject.targetObjects);

                foreach (var target in serializedProperty.serializedObject.targetObjects)
                {
                    EditorUtility.SetDirty(target);
                }
                serializedProperty.serializedObject.Update();

                // UpdateLabels();
            });
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
    }

    /*
	public static class TypeSelectorUI
	{
		public static VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			return new TypeSelectorField(property, property.displayName);

			var container = new TypeSelectorField(property, property.displayName)
			{
				style = {
					minHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing
				}
			};

			var propertyField = new PropertyField(property)
			{
				label = property.displayName,
				style =
				{
					minHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing
				}
			};
			container.Add(propertyField);

			var typePopup = new PopupField<string>(property.displayName)
			{
				style =
				{
					position = Position.Absolute,
					top = -1,
					left = 0,
					right = 0,
				}
			};
			container.Add(typePopup);
			typePopup.AddToClassList("unity-base-field__aligned");

			typePopup.labelElement.tooltip = property.tooltip;

			var typePopupButton = typePopup.ElementAt(1);
			typePopupButton.pickingMode = PickingMode.Position;

			var typePopupButtonIcon = new Image()
			{
				style =
				{
					width = 16,
					height = 16,
					marginRight = 4,
					display = DisplayStyle.None,
				}
			};
			typePopupButton.Insert(0, typePopupButtonIcon);


			return container;
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

			EditorGUI.LabelField(labelRect, label);

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
						EditorGUI.PropertyField(rowRect, enumerator, label, true);
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
			TypeSelecting.TypeSelectorBaseType baseTypeInfo,
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
	}
	*/
}
#endif
