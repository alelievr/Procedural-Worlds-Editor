using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

namespace ProceduralWorlds.Editor
{
	public static class AssetUtils
	{
		static string _proceduralWorldsEditorPath = null;
		public static string proceduralWorldsEditorPath
		{
			get
			{
				string path =  Application.dataPath + _proceduralWorldsEditorPath;
				if (_proceduralWorldsEditorPath == null || !Directory.Exists(path))
					TryFindEditorPath();
				return _proceduralWorldsEditorPath;
			}
		}

		static void TryFindEditorPath()
		{
			string[] dirs = Directory.GetDirectories(Application.dataPath, "ProceduralWorlds", SearchOption.AllDirectories);
			string editorDir = null;

			foreach (var dir in dirs)
			{
				if (Directory.Exists(dir + "/Editor"))
					editorDir = dir + "/Editor";
			}

			if (editorDir == null)
				_proceduralWorldsEditorPath = null;
			else
				_proceduralWorldsEditorPath = editorDir.Substring(Application.dataPath.Length);
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
	}
}