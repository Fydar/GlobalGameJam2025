using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// A way for an Animator to inform the gameplay elements of the visual state via a float
/// </summary>
public abstract class AnimatorFloatLabel : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField] private string labelName;
    [SerializeField] private float value;

    [SerializeField] private Animator informAnimator;
    [SerializeField] private string propertyName;

    public Action<float> OnValueChanged;

    private float previousValue;

    /// <summary>
    /// Gets the name of this label.
    /// </summary>
    public string LabelName
    {
        get
        {
            return labelName;
        }
    }

    /// <summary>
    /// Gets the current state of this label.
    /// </summary>
    public float Value
    {
        get
        {
            return value;
        }
    }

    private void Start()
    {
        if (informAnimator != null)
        {
            if (Application.isPlaying)
            {
                informAnimator.SetFloat(propertyName, value);
            }
        }
    }

    private void OnDidApplyAnimationProperties()
    {
        RecheckValue();
    }

    private void RecheckValue()
    {
        if (value != previousValue)
        {
            previousValue = value;
            OnValueChanged?.Invoke(value);

            if (informAnimator != null)
            {
                if (Application.isPlaying)
                {
                    informAnimator.SetFloat(propertyName, value);
                }
            }
        }
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
    }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        RecheckValue();
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AnimatorFloatLabel), true)]
    public class AnimatorFloatLabelEditor : Editor
    {
        private SerializedProperty scriptProperty;
        private SerializedProperty valueProperty;
        private SerializedProperty informAnimatorProperty;
        private SerializedProperty propertyNameProperty;

        private void OnEnable()
        {
            scriptProperty = serializedObject.FindProperty("m_Script");
            valueProperty = serializedObject.FindProperty("value");
            informAnimatorProperty = serializedObject.FindProperty("informAnimator");
            propertyNameProperty = serializedObject.FindProperty("propertyName");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(scriptProperty);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(valueProperty);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(informAnimatorProperty);

            if (informAnimatorProperty.objectReferenceValue != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(propertyNameProperty);
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
