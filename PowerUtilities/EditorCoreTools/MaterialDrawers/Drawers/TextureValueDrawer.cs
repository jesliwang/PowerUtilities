#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// copy texture from propName
/// </summary>
public class TextureValueDrawer : MaterialPropertyDrawer
{
    string propName;
    public TextureValueDrawer(string propName)
    {
        this.propName = propName;
    }
    public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
    {
        return 0;
    }

    public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
    {
        if (prop == null || prop.type != MaterialProperty.PropType.Texture)
            return;

        var mat = (Material) editor.target;
        prop.textureValue = mat.GetTexture(propName);
    }
}
#endif