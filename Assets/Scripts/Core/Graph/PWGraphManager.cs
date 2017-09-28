using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace PW.Core
{
    public static class PWGraphManager {
    
        //Main graph settings:
        public static string        PWMainGraphPath = "Assets/ProceduralWorlds/Resources/";
        public static string        PWMainGraphDefaultFileName = "New ProceduralWorld.asset";
    
        //Biome graph settings:
        public static string        PWBiomeGraphPath = "Assets/ProceduralWorlds/Resources/Biomes";
        public static string        PWBiomeGraphDefaultFileName = "New ProceduralBiome.asset";
    
        public static void CreateMainGraph(string fileName = null)
        {
            if (fileName == null)
                fileName = PWMainGraphDefaultFileName;
            
            string path = PWMainGraphPath + "/" + fileName;
            path = AssetDatabase.GenerateUniqueAssetPath(path);
    
            if (!Directory.Exists(PWMainGraphPath))
                Directory.CreateDirectory(PWMainGraphPath);
            
            PWMainGraph mg = ScriptableObject.CreateInstance< PWMainGraph >();

            mg.Initialize();
            
            ProjectWindowUtil.CreateAsset(mg, path);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = mg;
        }

        public static void CreateBiomeGraph(string fileName = null)
        {

        }
    }
}