#if UNITY_EDITOR
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace HuskyUnity.Engineering.TypeSelector
{
    public static class TypeSelecting
    {
        public class BaseType
        {
            public Type type;
            public SubType[] subTypes;

            public SubType GetSubTypeFromString(string typeIdentifier)
            {
                return subTypes
                    .FirstOrDefault(t => t.typeIdentifier == typeIdentifier)
                    ?? GetSubTypeFromType(null);
            }

            public SubType GetSubTypeFromType(Type subTypeType)
            {
                return subTypes.FirstOrDefault(t => t.type == subTypeType);
            }
        }

        public class SubType
        {
            public Type type;
            public string typeIdentifier;
            public string typeDisplayName;
            public string typeTooltip;
            public string typeDeclarationFilePath;
            public Texture2D typeIcon;

            public static SubType CreateFromType(Type type)
            {
                string typeDisplayName = "";
                string typeTooltip = "";
                string typeDeclarationFilePath = "";
                Texture2D typeIcon = null;

                var attributes = type.GetCustomAttributes();
                foreach (var attribute in attributes)
                {
                    if (attribute is IconAttribute iconAttribute)
                    {
                        string typeIconPath = iconAttribute.path;
                        if (!string.IsNullOrEmpty(typeIconPath))
                        {
                            typeIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(typeIconPath);
                        }
                    }
                    else if (attribute is DisplayNameAttribute displayNameAttribute)
                    {
                        typeDisplayName = displayNameAttribute.DisplayName;
                    }
                    else if (attribute is TooltipAttribute tooltipAttribute)
                    {
                        typeTooltip = tooltipAttribute.tooltip;
                    }
                    else if (attribute is UseScriptIconAttribute useScriptIconAttribute)
                    {
                        string assetPath = "Assets/" + useScriptIconAttribute.CallerFilePath
                            .Remove(0, Application.dataPath.Length + 1);

                        var loadedAsset = AssetDatabase.LoadMainAssetAtPath(assetPath);
                        typeIcon = AssetPreview.GetMiniThumbnail(loadedAsset);
                        typeDeclarationFilePath = assetPath;
                    }
                }

                if (typeIcon == null)
                {
                    typeIcon = AssetPreview.GetMiniTypeThumbnail(typeof(MonoScript));
                }

                if (string.IsNullOrEmpty(typeDisplayName))
                {
                    typeDisplayName = ObjectNames.NicifyVariableName(type.Name);
                }

                return new SubType()
                {
                    type = type,
                    typeDisplayName = typeDisplayName,
                    typeTooltip = typeTooltip,
                    typeDeclarationFilePath = typeDeclarationFilePath,
                    typeIdentifier = $"{type.Assembly.GetName().Name} {type.FullName}",
                    typeIcon = typeIcon,
                };
            }
        }

        private static readonly Dictionary<string, BaseType> typeCache = new(StringComparer.OrdinalIgnoreCase);

        public static BaseType GetBaseTypeInfoFromString(string typeIdentifier)
        {
            if (!typeCache.TryGetValue(typeIdentifier, out var baseTypeInfo))
            {
                var subTypes = new List<SubType>();

                var baseType = GetRealTypeFromTypeName(typeIdentifier);
                if (baseType != null)
                {
                    var derivedTypes = TypeCache.GetTypesDerivedFrom(baseType);
                    if (derivedTypes.Count > 0)
                    {
                        foreach (var derivedType in derivedTypes)
                        {
                            subTypes.Add(SubType.CreateFromType(derivedType));
                        }
                    }
                }

                if (baseType == null || baseType.GetCustomAttribute<NotNullAttribute>() != null)
                {
                    new SubType()
                    {
                        type = null,
                        typeDisplayName = "None",
                        typeIcon = null,
                        typeIdentifier = ""
                    };
                }

                baseTypeInfo = new BaseType()
                {
                    type = baseType,
                    subTypes = subTypes.ToArray()
                };
                typeCache.Add(typeIdentifier, baseTypeInfo);
            }

            return baseTypeInfo;
        }

        private static Type GetRealTypeFromTypeName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            string[] typeSplitString = typeName.Split(char.Parse(" "));
            string typeClassName = typeSplitString[1];
            string typeAssemblyName = typeSplitString[0];

            var realType = Type.GetType($"{typeClassName}, {typeAssemblyName}");
            return realType;
        }
    }
}
#endif
