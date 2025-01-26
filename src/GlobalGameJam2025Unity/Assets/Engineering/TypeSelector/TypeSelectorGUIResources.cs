#if UNITY_EDITOR
using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace HuskyUnity.Engineering.TypeSelector
{
    internal class TypeSelectorGUIResources : ScriptableObject
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] private static TypeSelectorGUIResources instance;

        public static TypeSelectorGUIResources Instance
        {
            get
            {
                if (instance == null)
                {
                    string[] guids = AssetDatabase.FindAssets($"t:{nameof(TypeSelectorGUIResources)}");

                    if (guids == null || guids.Length == 0)
                    {
                        instance = CreateInstance<TypeSelectorGUIResources>();

                        var resourcesMonoScript = MonoScript.FromScriptableObject(instance);
                        string resourcesMonoScriptPath = AssetDatabase.GetAssetPath(resourcesMonoScript);
                        string resourcePath = AssetDatabase.GenerateUniqueAssetPath(resourcesMonoScriptPath.Replace(".cs", ".asset", StringComparison.OrdinalIgnoreCase));

                        AssetDatabase.CreateAsset(instance, resourcePath);

                        UnityEngine.Debug.LogWarning($"No \"{typeof(TypeSelectorGUIResources).Name}\" found in the AssetDatabase.\nCreating a new one at '{resourcePath}'.", instance);
                        return instance;
                    }

                    string firstGuid = guids[0];
                    string firstPath = AssetDatabase.GUIDToAssetPath(firstGuid);

                    if (guids.Length > 1)
                    {
                        UnityEngine.Debug.LogError($"Multiple \"{typeof(TypeSelectorGUIResources).Name} found in the AssetDatabase, using \"{firstPath}\" as default resources.", instance);
                    }

                    instance = AssetDatabase.LoadAssetAtPath<TypeSelectorGUIResources>(firstPath);
                }

                return instance;
            }
        }

        [SerializeField]
        private Texture2D infoIcon = null;

        public Texture2D InfoIcon => infoIcon;
    }
}
#endif
