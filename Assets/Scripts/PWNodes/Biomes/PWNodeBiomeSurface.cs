using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using PW.Core;

namespace PW.Node
{
	public class PWNodeBiomeSurface : PWNode {
	
		[PWOutput]
		public SurfaceMaps		maps;
		
		[PWInput("Albedo"), PWOffset(20), PWNotRequired]
		public Texture2D		albedo;
		[PWInput("Diffuse"), PWNotRequired]
		public Texture2D		diffuse;
		[PWInput("Normal"), PWNotRequired]
		public Texture2D		normal;
		[PWInput("Height"), PWNotRequired]
		public Texture2D		height;
		[PWInput("Emissive"), PWNotRequired]
		public Texture2D		emissive;
		[PWInput("Specular"), PWNotRequired]
		public Texture2D		specular;
		[PWInput("Opacity"), PWNotRequired]
		public Texture2D		opacity;
		[PWInput("Smoothness"), PWNotRequired]
		public Texture2D		smoothness;
		[PWInput("Ambiant occulision"), PWNotRequired]
		public Texture2D		ambiantOcculison;
		[PWInput("Detail mask"), PWNotRequired]
		public Texture2D		detailMask;
		[PWInput("Second albedo"), PWNotRequired]
		public Texture2D		secondAlbedo;
		[PWInput("Second normal"), PWNotRequired]
		public Texture2D		secondNormal;
		[PWInput("Metallic"), PWNotRequired]
		public Texture2D		metallic;
		[PWInput("Roughness"), PWNotRequired]
		public Texture2D		roughness;
		[PWInput("Displacement"), PWNotRequired]
		public Texture2D		displacement;

		[SerializeField]
		SurfaceMapType			type;

		static string[]			inputNames = {
			"albedo", "diffuse", "normal", "height", "emissive",
			"specular", "opacity", "smoothness", "ambiantOcculison",
			"detailMask", "secondAlbedo", "secondNormal", "metallic",
			"roughness", "displacement"
		};

		static string[]			basicInputFields = {
			"albedo", "normal"
		};

		static string[]			normalInputFields = {
			"albedo", "normal", "diffuse", "opacity", "smoothness", "metallic", "roughness"
		};

		//complex have all maps

		public override void OnNodeCreate()
		{
			externalName = "Biome surface";

			UpdateInputVisibilities();
		}

		void UpdateInputVisibilities()
		{
			foreach (var inputName in inputNames)
				UpdatePropVisibility(inputName, PWVisibility.Gone);

			switch (type)
			{
				case SurfaceMapType.Basic:
					foreach (var inputName in inputNames)
						if (basicInputFields.Contains(inputName))
							UpdatePropVisibility(inputName, PWVisibility.Visible);
					break ;
				case SurfaceMapType.Normal:
					foreach (var inputName in inputNames)
						if (normalInputFields.Contains(inputName))
							UpdatePropVisibility(inputName, PWVisibility.Visible);
					break ;
				case SurfaceMapType.Complex:
					foreach (var inputName in inputNames)
						UpdatePropVisibility(inputName, PWVisibility.Visible);
					break ;
			}
		}

		public override void OnNodeGUI()
		{
			EditorGUIUtility.labelWidth = 110;
			EditorGUI.BeginChangeCheck();
			type = (SurfaceMapType)EditorGUILayout.EnumPopup("Surface complexity", type);
			if (EditorGUI.EndChangeCheck())
				UpdateInputVisibilities();
		}

		public override void OnNodeProcess()
		{
			maps = new SurfaceMaps();

			maps.albedo = albedo;
			maps.normal = normal;
			maps.diffuse = diffuse;
			maps.height = height;
			maps.emissive = emissive;
			maps.specular = specular;
			maps.opacity = opacity;
			maps.smoothness = smoothness;
			maps.ambiantOcculison = ambiantOcculison;
			maps.detailMask = detailMask;
			maps.secondAlbedo = secondAlbedo;
			maps.secondNormal = secondNormal;
			maps.metallic = metallic;
			maps.roughness = roughness;
			maps.displacement = displacement;
		}
	}
}