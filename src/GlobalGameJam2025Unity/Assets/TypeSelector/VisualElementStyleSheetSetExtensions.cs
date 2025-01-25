#if UNITY_EDITOR
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace HuskyUnity.Engineering.TypeSelector
{
    public static class VisualElementStyleSheetSetExtensions
    {
        public static void AddComponentStylesheet(
            this VisualElementStyleSheetSet visualElementStyleSheetSet,
            [CallerFilePath] string callerFilePath = "")
        {
            string assetPath = "Assets/" + callerFilePath.Remove(0, Application.dataPath.Length + 1) + ".uss";
            var componentStylesheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(assetPath);
            if (componentStylesheet != null)
            {
                visualElementStyleSheetSet.Add(componentStylesheet);
            }
        }
    }
}
#endif
