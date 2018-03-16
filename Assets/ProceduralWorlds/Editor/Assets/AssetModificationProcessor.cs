using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using ProceduralWorlds.Core;
using System;

namespace ProceduralWorlds.Editor
{
	[InitializeOnLoad]
	public class ProceduralWorldsAssetProcessor : UnityEditor.AssetModificationProcessor
	{

		public static AssetMoveResult OnWillMoveAsset(string fromPath, string toPath)
		{
			//check if we are moving a procedural worlds directory
			if (Directory.Exists(fromPath))
			{
				string folderName = Path.GetFileName(fromPath);
				string assetPath = fromPath + "/" + folderName + ".asset";
			
				if (!File.Exists(assetPath))
					return AssetMoveResult.DidNotMove;

				WorldGraph worldGraph = AssetDatabase.LoadMainAssetAtPath(assetPath) as WorldGraph;

				if (worldGraph == null)
					return AssetMoveResult.DidNotMove;
				
				string toFolderName = Path.GetFileName(toPath);
				string newAssetPath = toPath + "/" + toFolderName + ".asset";

				if (String.IsNullOrEmpty(GraphFactory.GetWorldGraphCreateLocation(toPath)))
				{
					Debug.LogError("Can't move the Procedural world directory to " + newAssetPath + ": Not a Resources directory");
					return AssetMoveResult.FailedMove;
				}
				
				Debug.Log("Moving main asset from " + assetPath + " to " + newAssetPath);
				AssetDatabase.MoveAsset(assetPath, newAssetPath);
			}

			return AssetMoveResult.DidNotMove;
		}

	}
}