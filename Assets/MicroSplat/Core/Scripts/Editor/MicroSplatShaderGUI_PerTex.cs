//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth, slipster216@gmail.com
//////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using JBooth.MicroSplat;
using System.Linq;


public partial class MicroSplatShaderGUI : ShaderGUI 
{
   
   // get, load, or create the property texture for this material..
   public static MicroSplatPropData FindOrCreatePropTex(Material targetMat)
   {
      MicroSplatPropData propData = null;
      // look for it next to the material?
      var path = AssetDatabase.GetAssetPath(targetMat);
      path = path.Replace("\\", "/");
      if (!string.IsNullOrEmpty(path))
      {
         path = path.Substring(0, path.IndexOf("."));
         path += "_propdata.asset";
         propData = AssetDatabase.LoadAssetAtPath<MicroSplatPropData>(path);
         if (propData == null)
         {
            propData = MicroSplatPropData.CreateInstance<MicroSplatPropData>();
            AssetDatabase.CreateAsset(propData, path);
            AssetDatabase.SaveAssets();
         }
      }

      return propData;
   }
}


