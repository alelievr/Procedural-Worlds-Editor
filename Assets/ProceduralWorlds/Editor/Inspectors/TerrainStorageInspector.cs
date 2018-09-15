using UnityEngine;
using UnityEditor;
using ProceduralWorlds;

namespace ProceduralWorlds.Editor
{
	[CustomEditor(typeof(TerrainStorage))]
	public class TerrainStorageInspector : UnityEditor.Editor
	{
	
		TerrainStorage 	terrain;
	
		public void OnEnable()
		{
			terrain = (TerrainStorage)target;
		}
	
		public override void OnInspectorGUI()
		{
			terrain.storeMode = (StorageMode)EditorGUILayout.EnumPopup(terrain.storeMode);
			switch (terrain.storeMode)
			{
				case StorageMode.Memory:
					EditorGUILayout.HelpBox(
						"The terrain will not be serialized, " +
						"this mode must be use when editing the PWgraph " +
						"or making a lot of changes in the procedural generation process.",
						MessageType.Info
					);
					break ;
				case StorageMode.File:
					terrain.editorMode = EditorGUILayout.Toggle("Editor mode", terrain.editorMode);
					if (!terrain.editorMode)
						terrain.storageFolder = EditorGUILayout.TextField("storage folder", terrain.storageFolder);
					break ;
				default:
					break ;
			}
		}
	}
}