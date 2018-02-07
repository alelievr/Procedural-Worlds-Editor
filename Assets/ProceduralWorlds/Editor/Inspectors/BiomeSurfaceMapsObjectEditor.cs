using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using PW.Biomator;
using UnityEditor.AnimatedValues;
using System.IO;

[CustomEditor(typeof(BiomeSurfaceMapsObject))]
public class BiomeSurfaceMapsObjectEditor : Editor
{
	[SerializeField]
	AnimBool	showNormalMaps;
	[SerializeField]
	AnimBool	showComplexMaps;

	BiomeSurfaceMaps	maps;

	public void OnEnable()
	{
		showNormalMaps = new AnimBool();
		showNormalMaps.valueChanged.AddListener(Repaint);
		showComplexMaps = new AnimBool();
		showComplexMaps.valueChanged.AddListener(Repaint);

		maps = (target as BiomeSurfaceMapsObject).maps;
	}

	public override void OnInspectorGUI()
	{
		maps.name = EditorGUILayout.TextField("Name", maps.name);

		maps.type = (SurfaceMapsType)EditorGUILayout.EnumPopup("Surface complexity", maps.type);

		showNormalMaps.target = maps.type != SurfaceMapsType.Basic;
		showComplexMaps.target = maps.type == SurfaceMapsType.Complex;

		using (new DefaultGUISkin())
		EditorGUILayout.BeginVertical(new GUIStyle("box"));
		{
			EditorGUILayout.LabelField("Surface Maps", EditorStyles.boldLabel);

			EditorGUI.BeginChangeCheck();
			maps.albedo = EditorGUILayout.ObjectField("Albedo", maps.albedo, typeof(Texture2D), false) as Texture2D;
			if (EditorGUI.EndChangeCheck())
				TryCompleteOtherMaps();
			maps.normal = EditorGUILayout.ObjectField("Normal", maps.normal, typeof(Texture2D), false) as Texture2D;
	
			if (EditorGUILayout.BeginFadeGroup(showNormalMaps.faded))
			{
				maps.opacity = EditorGUILayout.ObjectField("Opacity", maps.opacity, typeof(Texture2D), false) as Texture2D;
				maps.smoothness = EditorGUILayout.ObjectField("Smoothness", maps.smoothness, typeof(Texture2D), false) as Texture2D;
				maps.metallic = EditorGUILayout.ObjectField("Metallic", maps.metallic, typeof(Texture2D), false) as Texture2D;
				maps.roughness = EditorGUILayout.ObjectField("Roughness", maps.roughness, typeof(Texture2D), false) as Texture2D;
			}
			EditorGUILayout.EndFadeGroup();
			if (EditorGUILayout.BeginFadeGroup(showComplexMaps.faded))
			{
				maps.height = EditorGUILayout.ObjectField("Height", maps.height, typeof(Texture2D), false) as Texture2D;
				maps.emissive = EditorGUILayout.ObjectField("Emissive", maps.emissive, typeof(Texture2D), false) as Texture2D;
				maps.ambiantOcculison = EditorGUILayout.ObjectField("Ambiant Occlusion", maps.ambiantOcculison, typeof(Texture2D), false) as Texture2D;
				maps.secondAlbedo = EditorGUILayout.ObjectField("Second Albedo", maps.secondAlbedo, typeof(Texture2D), false) as Texture2D;
				maps.secondNormal = EditorGUILayout.ObjectField("Second Normal", maps.secondNormal, typeof(Texture2D), false) as Texture2D;
				maps.detailMask = EditorGUILayout.ObjectField("DetailMask", maps.detailMask, typeof(Texture2D), false) as Texture2D;
				maps.displacement = EditorGUILayout.ObjectField("Displacement", maps.displacement, typeof(Texture2D), false) as Texture2D;
			}
			EditorGUILayout.EndFadeGroup();
		}
		EditorGUILayout.EndVertical();
	}

	List< string > albedoLikeNames = new List< string >()
	{
		"albedo", "diffuse", "_BC", "_D", "_CO"
	};

	Texture2D Texture2DExists(string albedoName, params string[] replacements)
	{
		foreach (var replacement in replacements)
		{
			var newName = albedoName;
			foreach (var al in albedoLikeNames)
				if (newName.Contains(al))
				{
					newName = newName.Replace(al, replacement);
					break ;
				}

			var tex = Resources.Load< Texture2D >(newName);

			if (tex != null)
				return tex;
		}

		return null;
	}

	void TryCompleteOtherMaps()
	{
		if (maps.albedo == null)
			return ;

		var name = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(maps.albedo));

		Texture2D tex;

		if ((tex = Texture2DExists(name, "nm", "normal", "_N")) != null)
			maps.normal = tex;
		if ((tex = Texture2DExists(name, "met", "metalic", "_MT")) != null)
			maps.metallic = tex;
	}
}
