﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    /// <summary>
    /// open RenderTargetIdentifier fields
    /// </summary>
    public static class RenderTargetIdentifierEx
    {
        static FieldInfo 
            m_NameID,
            m_Type; //BuiltinRenderTextureType

        static RenderTargetIdentifierEx()
        {
            var t = typeof(RenderTargetIdentifier);
            m_NameID = t.GetField("m_NameID", BindingFlags.Instance | BindingFlags.NonPublic);
            m_Type = t.GetField("m_Type", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public static bool IsNameIdEquals(this RenderTargetIdentifier a, RenderTargetIdentifier b)
        {
            return GetNameId(a) == GetNameId(b);
        }

        public static int GetNameId(this RenderTargetIdentifier a)
        {
            unsafe
            {
                int idNum = 0;
                ByteTools.ReadBytes((byte*)&a, sizeof(int)+sizeof(BuiltinRenderTextureType), sizeof(int), (byte*)&idNum);
                return idNum;
            }
            // 
            //var id = (int)m_NameID.GetValue(a);
            //return id;
        }
        public static BuiltinRenderTextureType GetBuiltinRenderTextureType(this RenderTargetIdentifier a)
        {
            unsafe
            {
                int idNum = 0;
                ByteTools.ReadBytes((byte*)&a, sizeof(int), sizeof(BuiltinRenderTextureType), (byte*)&idNum);
                return (BuiltinRenderTextureType)idNum;
            }
            //(BuiltinRenderTextureType)m_Type.GetValue(a);
        } 
    }
}
