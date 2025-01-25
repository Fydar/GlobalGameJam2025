#if UNITY_EDITOR
using System;
using System.Collections;
using System.Reflection;
using UnityEditor;

namespace HuskyUnity.Engineering.TypeSelector
{
    public static class AdvancedGUI
    {
        private const string arrayPrefix = "Array.data[";

        public static object[] GetPropertyValues(SerializedProperty property)
        {
            object[] baseObjects = new object[property.serializedObject.targetObjects.Length];
            property.serializedObject.targetObjects.CopyTo(baseObjects, 0);

            var remainingPropertyPath = property.propertyPath.AsSpan();
            do
            {
                if (remainingPropertyPath.StartsWith(arrayPrefix))
                {
                    int endArrayIndex = remainingPropertyPath.IndexOf(']');
                    var arrayIndexSpan = remainingPropertyPath[arrayPrefix.Length..endArrayIndex];
                    remainingPropertyPath = remainingPropertyPath[(endArrayIndex + 1)..];

                    if (remainingPropertyPath.Length != 0)
                    {
                        remainingPropertyPath = remainingPropertyPath[1..];
                    }
                    for (int i = 0; i < baseObjects.Length; i++)
                    {
                        baseObjects[i] = GetArrayValue(baseObjects[i], int.Parse(arrayIndexSpan), out _);
                    }
                }
                else
                {
                    int nextSeparatorIndex = remainingPropertyPath.IndexOf('.');
                    var currentElement = remainingPropertyPath;
                    if (nextSeparatorIndex != -1)
                    {
                        currentElement = remainingPropertyPath[..nextSeparatorIndex];
                        remainingPropertyPath = remainingPropertyPath[(nextSeparatorIndex + 1)..];
                    }
                    else
                    {
                        remainingPropertyPath = ReadOnlySpan<char>.Empty;
                    }

                    for (int i = 0; i < baseObjects.Length; i++)
                    {
                        baseObjects[i] = GetMemberValue(baseObjects[i], currentElement);
                    }
                }
            } while (remainingPropertyPath.Length != 0);

            return baseObjects;
        }

        public static void SetPropertyValues(SerializedProperty property, Func<object> valueFactory)
        {
            object[] baseObjects = new object[property.serializedObject.targetObjects.Length];
            property.serializedObject.targetObjects.CopyTo(baseObjects, 0);

            var remainingPropertyPath = property.propertyPath.AsSpan();
            do
            {
                if (remainingPropertyPath.StartsWith(arrayPrefix))
                {
                    int endArrayIndex = remainingPropertyPath.IndexOf(']');
                    var arrayIndexSpan = remainingPropertyPath[arrayPrefix.Length..endArrayIndex];
                    remainingPropertyPath = remainingPropertyPath[(endArrayIndex + 1)..];

                    if (remainingPropertyPath.Length == 0)
                    {
                        for (int i = 0; i < baseObjects.Length; i++)
                        {
                            SetArrayValue(baseObjects[i], int.Parse(arrayIndexSpan), valueFactory.Invoke());
                        }
                        return;
                    }
                    else
                    {
                        remainingPropertyPath = remainingPropertyPath[1..];
                        for (int i = 0; i < baseObjects.Length; i++)
                        {
                            baseObjects[i] = GetArrayValue(baseObjects[i], int.Parse(arrayIndexSpan), out _);
                        }
                    }
                }
                else
                {
                    int nextSeparatorIndex = remainingPropertyPath.IndexOf('.');
                    var currentElement = remainingPropertyPath;
                    if (nextSeparatorIndex != -1)
                    {
                        currentElement = remainingPropertyPath[..nextSeparatorIndex];
                        remainingPropertyPath = remainingPropertyPath[(nextSeparatorIndex + 1)..];
                    }
                    else
                    {
                        for (int i = 0; i < baseObjects.Length; i++)
                        {
                            SetMemberValue(baseObjects[i], currentElement, valueFactory.Invoke());
                        }
                        return;
                    }

                    for (int i = 0; i < baseObjects.Length; i++)
                    {
                        baseObjects[i] = GetMemberValue(baseObjects[i], currentElement);
                    }
                }
            } while (remainingPropertyPath.Length != 0);
        }

        private static object GetMemberValue(object source, ReadOnlySpan<char> name)
        {
            var fieldInfo = GetField(source, name);
            return fieldInfo?.GetValue(source);
        }

        private static void SetMemberValue(object source, ReadOnlySpan<char> name, object value)
        {
            var fieldInfo = GetField(source, name);
            fieldInfo?.SetValue(source, value);
        }

        private static object GetArrayValue(object source, int index, out bool isOutOfRange)
        {
            if (source is not IList list || index >= list.Count)
            {
                isOutOfRange = true;
                return null;
            }
            isOutOfRange = false;
            return list[index];
        }

        private static void SetArrayValue(object source, int index, object value)
        {
            if (source is not IList list)
            {
                return;
            }
            list[index] = value;
        }

        private static FieldInfo GetField(object source, ReadOnlySpan<char> name)
        {
            if (source == null)
            {
                return null;
            }

            var typeInfo = source.GetType().GetTypeInfo();
            var fieldInfo = typeInfo.GetField(name.ToString(),
                BindingFlags.NonPublic
                | BindingFlags.Public
                | BindingFlags.Instance
                | BindingFlags.FlattenHierarchy);
            return fieldInfo;
        }
    }
}
#endif
