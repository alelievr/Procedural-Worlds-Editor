using UnityEngine;
using UnityEditor;
using PW;

[CustomEditor(typeof(PWTerrainStorage))]
public class PWTerrainStorageEditor : Editor {
 
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
			case PWStorageMode.MEMORY:
				EditorGUILayout.LabelField("The terrain will not be serialized,");
				EditorGUILayout.LabelField("this mode must be use when editing the PWgraph");
				EditorGUILayout.LabelField("or making a lot of changes in the procedural generation process.");
			 	break ;
			case PWStorageMode.FILE:
				terrain.editorMode = EditorGUILayout.Toggle("Editor mode", terrain.editorMode);
				if (!terrain.editorMode)
					terrain.storageFolder = EditorGUILayout.TextField("storage folder", terrain.storageFolder);
				break ;
		}
	}
}