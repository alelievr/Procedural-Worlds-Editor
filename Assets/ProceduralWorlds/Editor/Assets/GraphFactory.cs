using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System;
using ProceduralWorlds.Core;
using UnityEditorInternal;
namespace ProceduralWorlds.Editor
{
    public static class GraphFactory
    {
    
        //World graph settings:
        public readonly static string	WorldGraphDefaultName = "New ProceduralWorld";
    
        //Biome graph settings:
        public readonly static string	BiomeGraphDefaultName = "New ProceduralBiome";
		public readonly static string	baseGraphBiomeFolderName = "Biomes";

		public readonly static string	unityResourcesFolderName = "Resources";	

		public readonly static string	presetGraphFolderName = "GraphPresets";
    
        public static T CreateGraph< T >(string directory, string fileName, bool rename = true) where T : BaseGraph
        {
			if (!fileName.EndsWith(".asset"))
				fileName += ".asset";

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
			if (rename)
				ProjectWindowUtil.CreateAsset(mg, path);
			else
				AssetDatabase.CreateAsset(mg, path);
    
            //save and refresh Project view
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
    
            //focus the project window
            EditorUtility.FocusProjectWindow();	
    
            //focus the asset file
            Selection.activeObject = mg;

            return mg;
        }

		public static string GetCurrentPath()
		{
			var path = "";
			var obj = Selection.activeObject;

			if (obj == null)
                return null;
			else
				path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
			
			if (path.Length > 0)
			{
				if (Directory.Exists(path))
					return path;
				else
					return new FileInfo(path).Directory.FullName;
			}
			return null;
		}

    	public static string GetBiomeGraphCreateLocation(string currentPath = null)
		{
			if (String.IsNullOrEmpty(currentPath))
				currentPath = GetCurrentPath();

			if (String.IsNullOrEmpty(currentPath))
				return currentPath;

			if (currentPath.IndexOf(presetGraphFolderName) != -1)
				return currentPath;
			
			int resourcesIndex = currentPath.IndexOf(unityResourcesFolderName, StringComparison.InvariantCulture);
			int biomesIndex = currentPath.IndexOf(baseGraphBiomeFolderName, StringComparison.InvariantCulture);
			
			//if the path don't contains Resources nor Biomes folder
			if (resourcesIndex == -1 || biomesIndex == -1)
				return null;

			//If biomes is before the resources folder
			if (biomesIndex < resourcesIndex)
				return null;
			
			return currentPath;
		}

		public static string GetWorldGraphCreateLocation(string currentPath = null)
		{
			if (String.IsNullOrEmpty(currentPath))
				currentPath = GetCurrentPath();
			
			if (String.IsNullOrEmpty(currentPath))
				return currentPath;
				
			if (!currentPath.Contains(unityResourcesFolderName))
				return null;
			
			return currentPath;
		}

        public static void CreateWorldGraph(string fileName = null)
        {
            if (fileName == null)
                fileName = WorldGraphDefaultName;

			string path = GetWorldGraphCreateLocation();

			if (String.IsNullOrEmpty(path))
			{
				Debug.LogError("Can't create a main graph outside of a Resources folder");
				return ;
			}

			ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
				0,
				ScriptableObject.CreateInstance< DoCreateWorldGraph >(),
				fileName,
				EditorGUIUtility.FindTexture("Folder Icon"),
				null
			);
        }
    
        public static BiomeGraph CreateBiomeGraph(string fileName = null)
        {
            if (fileName == null)
                fileName = BiomeGraphDefaultName;
			
			string path = GetBiomeGraphCreateLocation();
			
			if (String.IsNullOrEmpty(path))
			{
                Debug.LogError("Can't create a biome graph outside of the Biome folder of a procedural worlds directory");
				return null;
			}
            
            return CreateGraph< BiomeGraph >(path, fileName);
        }
    }
}