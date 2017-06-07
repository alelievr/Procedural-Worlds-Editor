using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Node
{
	public class PWNodeBiomeSurface : PWNode {
	
		[PWOutput]
		public SurfaceMaps		maps;
		
		[PWInput("Albedo")]
		public Texture2D		albedo;
		[PWInput("Diffuse")]
		public Texture2D		diffuse;
		[PWInput("Normal")]
		public Texture2D		normal;
		[PWInput("Height")]
		public Texture2D		height;
		[PWInput("Emissive")]
		public Texture2D		emissive;
		[PWInput("Specular")]
		public Texture2D		specular;
		[PWInput("Opacity")]
		public Texture2D		opacity;
		[PWInput("Smoothness")]
		public Texture2D		smoothness;
		[PWInput("Ambiant occulision")]
		public Texture2D		ambiantOcculison;
		[PWInput("Detail mask")]
		public Texture2D		detailMask;
		[PWInput("Second albedo")]
		public Texture2D		secondAlbedo;
		[PWInput("Second normal")]
		public Texture2D		secondNormal;
		[PWInput("Metallic")]
		public Texture2D		metallic;
		[PWInput("Roughness")]
		public Texture2D		roughness;
		[PWInput("Displacement")]
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
		}

		public override void OnNodeGUI()
		{
			EditorGUIUtility.labelWidth = 110;
			type = (SurfaceMapType)EditorGUILayout.EnumPopup("Surface complexity", type);

			if (type != SurfaceMapType.Complex || type != SurfaceMapType.Normal)
			{
				//display basic maps
			}
			if (type != SurfaceMapType.Complex)
			{
				//display normal maps
			}
			if (type == SurfaceMapType.Complex)
			{
				//display complex maps
			}
		}

		public override void OnNodeProcess()
		{
			maps = new SurfaceMaps();
		}
		
	}
}