using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;

namespace ProceduralWorlds.Editor
{
	public static class AssetUtils
	{
		static string _proceduralWorldsEditorPath = "/ProceduralWorlds/Editor/";
		public static string proceduralWorldsEditorPath
		{
			get
			{
				if (_proceduralWorldsEditorPath == null || !File.Exists(Path.GetFullPath(_proceduralWorldsEditorPath)))
					TryFindEditorPath();
				return _proceduralWorldsEditorPath;
			}
		}

		static void TryFindEditorPath()
		{
			var dirs = Directory.GetDirectories(Application.dataPath, "ProceduralWorlds/Editor", SearchOption.AllDirectories);

			foreach (var dir in dirs)
			{
				Debug.Log("Procedural Worlds Editor dir: " + dir);
			}

			if (dirs.Length == 0)
				_proceduralWorldsEditorPath = null;

			_proceduralWorldsEditorPath = dirs.First().Substring(Application.dataPath.Length + 1);
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