using UnityEngine;
using UnityEditor;
using HuskyUnity.Engineering.TypeSelector;

[CustomPropertyDrawer(typeof(IInterpolatorVector2))]
public class IInterpolatorVector2PropertyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return TypeSelectorUI.GetStaticSelectorPropertyHeight(property, label);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        TypeSelectorUI.DrawStaticSelector(position, property, label);
    }
}
