using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using PW.Core;
using System;

namespace PW.Editor
{
	[InitializeOnLoad]
	public class PWAssetProcessor : UnityEditor.AssetModificationProcessor
	{

		public static AssetMoveResult OnWillMoveAsset(string fromPath, string toPath)
		{
			Debug.Log("moving " + fromPath + " to " + toPath);

			//check if we are moving a procedural worlds directory
			if (Directory.Exists(fromPath))
			{
				string folderName = Path.GetFileName(fromPath);
				string assetPath = fromPath + "/" + folderName + ".asset";
			
				if (!File.Exists(assetPath))
					return AssetMoveResult.DidNotMove;

				PWMainGraph mainGraph = AssetDatabase.LoadMainAssetAtPath(assetPath) as PWMainGraph;

				if (mainGraph == null)
					return AssetMoveResult.DidNotMove;
				
				string toFolderName = Path.GetFileName(toPath);
				string newAssetPath = toPath + "/" + toFolderName + ".asset";

				if (String.IsNullOrEmpty(PWGraphFactory.GetMainGraphCreateLocation(toPath)))
				{
					Debug.LogError("Can't move the Procedural world directory");
					return AssetMoveResult.FailedMove;
				}
				
				Debug.Log("Moving main asset from " + assetPath + " to " + newAssetPath);
				AssetDatabase.MoveAsset(assetPath, newAssetPath);
			}

			return AssetMoveResult.DidNotMove;
		}

	}
}