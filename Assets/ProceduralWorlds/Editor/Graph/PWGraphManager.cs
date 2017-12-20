using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System;
using PW.Core;

namespace PW.Editor
{
    //TODO: rename this class
    public static class PWGraphManager
    {
    
        //Main graph settings:
        public static string        PWMainGraphPath = "Assets/ProceduralWorlds/Resources";
        public static string        PWMainGraphDefaultFileName = "New ProceduralWorld.asset";
    
        //Biome graph settings:
        public static string        PWBiomeGraphPath = "Assets/ProceduralWorlds/Resources/Biomes";
        public static string        PWBiomeGraphDefaultFileName = "New ProceduralBiome.asset";
    
        public static void CreateGraph< T >(string directory, string fileName) where T : PWGraph
        {
            //generate the file path
            string path = directory + "/" + fileName;
    
            //Create the directory resource if not exists
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
                
            //uniquify path
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            
            //Create the graph, this will call OnEnable too but since the graph is not initialized this will do nothing.
            T mg = ScriptableObject.CreateInstance< T >();
    
            //Create the asset file and let the user rename it
            ProjectWindowUtil.CreateAsset(mg, path);
    
            //save and refresh Project view
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
    
            //focus the project window
            EditorUtility.FocusProjectWindow();	
    
            //focus the asset file
            Selection.activeObject = mg;
        }
    
        public static void CreateMainGraph(string fileName = null)
        {
            if (fileName == null)
                fileName = PWMainGraphDefaultFileName;
            
            CreateGraph< PWMainGraph >(PWMainGraphPath, fileName);
        }
    
        public static void CreateBiomeGraph(string fileName = null)
        {
            if (fileName == null)
                fileName = PWBiomeGraphDefaultFileName;
            
            CreateGraph< PWBiomeGraph >(PWBiomeGraphPath, fileName);
        }
    }
}