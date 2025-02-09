#if UNITY_EDITOR
using PowerUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
public static class SerializedPropertyEx
{
    public static bool IsElementExists(this SerializedProperty arrayProp, Predicate<SerializedProperty> predicate)
    {
        return GetElementIndex(arrayProp, predicate) != -1;
    }

    public static int GetElementIndex(this SerializedProperty arrayProp,Predicate<SerializedProperty> predicate)
    {
        if (!arrayProp.isArray || predicate == null)
            return -1;

        for (int i = 0; i < arrayProp.arraySize; i++)
        {
            if (predicate(arrayProp.GetArrayElementAtIndex(i)))
                return i;
        }
        return -1;
    }

    public static SerializedProperty AppendElement(this SerializedProperty arrayProp,Action<SerializedProperty> onFillContent=null)
    {
        if (!arrayProp.isArray)
            return default;

        var id = arrayProp.arraySize;
        arrayProp.InsertArrayElementAtIndex(id);
        var prop = arrayProp.GetArrayElementAtIndex(id);

        if (onFillContent != null)
        {
            onFillContent(prop);
        }
        return prop;
    }

    /// <summary>
    /// get array 's items
    /// </summary>
    /// <param name="arrayProp"></param>
    /// <returns></returns>
    public static List<SerializedProperty> GetElements(this SerializedProperty arrayProp)
    {
        if (!arrayProp.isArray)
            return new List<SerializedProperty>();

        var elements = new List<SerializedProperty>();
        for (int i = 0; i < arrayProp.arraySize; i++)
        {
            elements.Add(arrayProp.GetArrayElementAtIndex(i));
        }
        return elements;
    }

    /// <summary>
    /// set value for (p.float,p.integer,p.boolean
    /// </summary>
    /// <param name="p"></param>
    /// <param name="value"></param>
    public static void Set(this SerializedProperty p, float value)
    {
        switch (p.propertyType)
        {
            case SerializedPropertyType.Float: p.floatValue = value; break;
            case SerializedPropertyType.Integer: p.intValue = (int)value; break;
            case SerializedPropertyType.Boolean: p.boolValue = value>0; break;
        }
    }

}
#endif