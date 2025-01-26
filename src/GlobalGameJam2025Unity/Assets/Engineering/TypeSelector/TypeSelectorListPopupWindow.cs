#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace HuskyUnity.Engineering.TypeSelector
{
    internal class TypeSelectorListPopupWindow : EditorWindow
    {
        public TypeSelecting.BaseType baseTypeInfo;
        public object[] currentValues;
        public Action<Type> selectCallback;

        public float TargetHeight => rootVisualElement.resolvedStyle.paddingTop
                    + rootVisualElement.resolvedStyle.paddingBottom
                    + rootVisualElement.resolvedStyle.borderTopWidth
                    + rootVisualElement.resolvedStyle.borderBottomWidth
                    + (Mathf.Clamp(baseTypeInfo.subTypes.Length, 1, 10) * (EditorGUIUtility.singleLineHeight + 2));

        public static void OpenPopupWindow(
            Rect buttonRect,
            TypeSelecting.BaseType baseTypeInfo,
            object[] currentValues,
            Action<Type> selectCallback)
        {
            var window = CreateInstance<TypeSelectorListPopupWindow>();

            window.titleContent = new GUIContent("Popup");
            window.baseTypeInfo = baseTypeInfo;
            window.currentValues = currentValues;
            window.selectCallback = selectCallback;

            window.ShowAsDropDown(buttonRect, new Vector2(buttonRect.width, window.TargetHeight));
        }

        public void CreateGUI()
        {
            if (baseTypeInfo == null)
            {
                return;
            }

            rootVisualElement.styleSheets.AddComponentStylesheet();

            rootVisualElement.AddToClassList("holodrone-typeselector-window");

            rootVisualElement.experimental.animation.Start(0.0f, 1.0f, 150, (element, time) =>
            {
                minSize = new Vector2(position.width, TargetHeight * time);
                maxSize = new Vector2(position.width, TargetHeight * time);
            }).Ease(Easing.InOutSine);

            float lineHeight = EditorGUIUtility.singleLineHeight + 2;

            var listView = new ListView
            {
                fixedItemHeight = lineHeight,
                virtualizationMethod = CollectionVirtualizationMethod.FixedHeight,
                style =
                {
                    minHeight = 280.0f
                },
                makeItem = () =>
                {
                    var rowElement = new VisualElement();
                    rowElement.style.flexDirection = FlexDirection.Row;
                    rowElement.AddToClassList("holodrone-typeselector-window-row");

                    rowElement.RegisterCallback<NavigationSubmitEvent>(navigationSubmitEvent =>
                    {
                        selectCallback.Invoke(baseTypeInfo.subTypes[int.Parse(rowElement.name)].type);
                        Close();
                    });

                    rowElement.RegisterCallback<PointerDownEvent>(pointerDownEvent =>
                    {
                        if (pointerDownEvent.button == 0)
                        {
                            selectCallback.Invoke(baseTypeInfo.subTypes[int.Parse(rowElement.name)].type);
                            Close();
                        }
                        else if (pointerDownEvent.button == 1)
                        {
                            var genericMenu = new GenericMenu();

                            genericMenu.AddItem(new GUIContent("Go to Implementation"), false, () =>
                            {
                                var sourceObject = AssetDatabase.LoadMainAssetAtPath(baseTypeInfo.subTypes[int.Parse(rowElement.name)].typeDeclarationFilePath);
                                EditorGUIUtility.PingObject(sourceObject);

                                Close();
                            });
                            genericMenu.AddItem(new GUIContent("Open Documentation"), false, () =>
                            {
                                Application.OpenURL("https://fydar.dev/play/astralelites");

                                Close();
                            });

                            genericMenu.ShowAsContext();
                        }
                        else if (pointerDownEvent.button == 2)
                        {
                            var sourceObject = AssetDatabase.LoadMainAssetAtPath(baseTypeInfo.subTypes[int.Parse(rowElement.name)].typeDeclarationFilePath);
                            EditorGUIUtility.PingObject(sourceObject);

                            Close();
                        }
                    });

                    var beforeElement = new VisualElement();
                    beforeElement.AddToClassList("holodrone-before");
                    rowElement.Add(beforeElement);

                    var checkElement = new VisualElement();
                    checkElement.AddToClassList("holodrone-typeselector-window-row__check");
                    rowElement.Add(checkElement);

                    var typeIconImage = new VisualElement();
                    typeIconImage.AddToClassList("holodrone-typeselector-window-row__icon");
                    rowElement.Add(typeIconImage);

                    var typeNameLabel = new Label();
                    typeNameLabel.AddToClassList("holodrone-typeselector-window-row__label");
                    rowElement.Add(typeNameLabel);

                    var spacerElement = new VisualElement();
                    spacerElement.AddToClassList("holodrone-typeselector-window-row__spacer");
                    rowElement.Add(spacerElement);

                    var infoIconImage = new VisualElement();
                    infoIconImage.AddToClassList("holodrone-typeselector-window-row__help");
                    rowElement.Add(infoIconImage);

                    infoIconImage.RegisterCallback<PointerDownEvent>(pointerDownEvent =>
                    {
                        if (pointerDownEvent.button == 0)
                        {
                            pointerDownEvent.StopPropagation();
                        }
                    });
                    infoIconImage.RegisterCallback<ClickEvent>(clickEvent =>
                    {
                        if (clickEvent.button == 0)
                        {
                            Application.OpenURL("https://fydar.dev/play/astralelites");
                        }
                    });
                    // infoIconImage.RegisterCallback<PointerOverEvent>(pointerDownEvent =>
                    // {
                    // 	OpenPopupWindow(infoIconImage.worldBound, baseTypeInfo, currentValues, selectCallback);
                    // });

                    return rowElement;
                },
                bindItem = (item, index) =>
                {
                    var typeInfo = baseTypeInfo.subTypes[index];

                    item.name = index.ToString();

                    var typeIconImage = item.ElementAt(2);
                    var typeNameLabel = item.ElementAt(3) as Label;
                    var infoIconImage = item.ElementAt(4);

                    typeIconImage.style.backgroundImage = typeInfo.typeIcon;
                    typeNameLabel.text = typeInfo.typeDisplayName;

                    infoIconImage.visible = !string.IsNullOrEmpty(typeInfo.typeTooltip);
                    infoIconImage.tooltip = typeInfo.typeTooltip + "\n<size=6>\n</size><size=10><u>Click</u> to open the documentation.</size>\n<size=2>\n</size><size=10><color=#a5c2fc>https://fydar.dev/portfolio/outrage</color><u>";
                    SetCursor(infoIconImage, MouseCursor.Link);
                },
                itemsSource = baseTypeInfo.subTypes
            };

            rootVisualElement.Add(listView);

            var label = new Label()
            {
                text = "Learn how to make your own"
            };
            rootVisualElement.Add(label);

            var selectedIndices = new List<int>();
            for (int i = 0; i < baseTypeInfo.subTypes.Length; i++)
            {
                var subTypeInfo = baseTypeInfo.subTypes[i];

                for (int j = 0; j < currentValues.Length; j++)
                {
                    var valueType = currentValues[j]?.GetType();
                    if (valueType == subTypeInfo.type)
                    {
                        selectedIndices.Add(i);
                        break;
                    }
                }
            }
            listView.SetSelection(selectedIndices);
        }

        public static void SetCursor(VisualElement element, MouseCursor cursor)
        {
            object objCursor = new UnityEngine.UIElements.Cursor();
            var fields = typeof(UnityEngine.UIElements.Cursor).GetProperty("defaultCursorId", BindingFlags.NonPublic | BindingFlags.Instance);
            fields.SetValue(objCursor, (int)cursor);
            element.style.cursor = new StyleCursor((UnityEngine.UIElements.Cursor)objCursor);
        }
    }
}
#endif
