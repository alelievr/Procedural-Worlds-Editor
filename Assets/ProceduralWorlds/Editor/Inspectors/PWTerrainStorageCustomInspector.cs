using UnityEngine;
using UnityEditor;
using PW;

namespace PW.Editor
{
	[CustomEditor(typeof(PWTerrainStorage))]
	public class PWTerrainStorageCustomInspector : UnityEditor.Editor
	{
	
		PWTerrainStorage 	terrain;
	
		public void OnEnable()
		{
			terrain = (PWTerrainStorage)target;
		}
	
		public override void OnInspectorGUI()
		{
			terrain.storeMode = (PWStorageMode)EditorGUILayout.EnumPopup(terrain.storeMode);
			switch (terrain.storeMode)
			{
				case PWStorageMode.Memory:
					EditorGUILayout.HelpBox(
						"The terrain will not be serialized, " +
						"this mode must be use when editing the PWgraph " +
						"or making a lot of changes in the procedural generation process.",
						MessageType.Info
					);
					break ;
				case PWStorageMode.File:
					terrain.editorMode = EditorGUILayout.Toggle("Editor mode", terrain.editorMode);
					if (!terrain.editorMode)
						terrain.storageFolder = EditorGUILayout.TextField("storage folder", terrain.storageFolder);
					break ;
			}
		}
	}
}