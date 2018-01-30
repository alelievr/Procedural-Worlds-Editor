using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using PW.Core;
using PW.Biomator;

namespace PW.Node
{
	public class PWNodeBiomeSurfaceMaps : PWNode
	{
		
		[PWInput("Albedo"), PWOffset(58), PWNotRequired]
		public Texture2D		albedo;
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
		BiomeSurfaceMapsObject	surfaceMapsObject;
	
		[PWOutput]
		public BiomeSurfaceMaps	maps = new BiomeSurfaceMaps();

		static string[]			inputNames = {
			"albedo", "normal", "height", "emissive",
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

		public override void OnNodeCreation()
		{
			name = "Surface maps";
			UpdateInputVisibilities();
		}


		void UpdateInputVisibilities()
		{
			foreach (var inputName in inputNames)
				SetAnchorVisibility(inputName, PWVisibility.Gone);
			
			//if we have a surface map object, then dont show any input
			if (surfaceMapsObject != null)
				return ;

			switch (maps.type)
			{
				case SurfaceMapsType.Basic:
					foreach (var inputName in inputNames)
						if (basicInputFields.Contains(inputName))
							SetAnchorVisibility(inputName, PWVisibility.Visible);
					break ;
				case SurfaceMapsType.Normal:
					foreach (var inputName in inputNames)
						if (normalInputFields.Contains(inputName))
							SetAnchorVisibility(inputName, PWVisibility.Visible);
					break ;
				case SurfaceMapsType.Complex:
					foreach (var inputName in inputNames)
						SetAnchorVisibility(inputName, PWVisibility.Visible);
					break ;
			}
		}

		public override void OnNodeGUI()
		{
			EditorGUIUtility.labelWidth = 110;
			EditorGUI.BeginChangeCheck();
			surfaceMapsObject = EditorGUILayout.ObjectField("Surface maps", surfaceMapsObject, typeof(BiomeSurfaceMapsObject), false) as BiomeSurfaceMapsObject;
			
			if (surfaceMapsObject != null)
			{
				maps = surfaceMapsObject.maps;	
				EditorGUILayout.LabelField("maps: " + maps.name);
				EditorGUILayout.LabelField("Type: " + maps.type);
			}
			else
			{
				maps.type = (SurfaceMapsType)EditorGUILayout.EnumPopup("Surface complexity", maps.type);
			
				maps.name = EditorGUILayout.TextField("Name", maps.name);
	
				albedo = EditorGUILayout.ObjectField(albedo, typeof(Texture2D), false) as Texture2D;
				normal = EditorGUILayout.ObjectField(normal, typeof(Texture2D), false) as Texture2D;
				if (maps.type == SurfaceMapsType.Normal || maps.type == SurfaceMapsType.Complex)
				{
					opacity = EditorGUILayout.ObjectField(opacity, typeof(Texture2D), false) as Texture2D;
					smoothness = EditorGUILayout.ObjectField(smoothness, typeof(Texture2D), false) as Texture2D;
					metallic = EditorGUILayout.ObjectField(metallic, typeof(Texture2D), false) as Texture2D;
					roughness = EditorGUILayout.ObjectField(roughness, typeof(Texture2D), false) as Texture2D;
				}
				if (maps.type == SurfaceMapsType.Complex)
				{
					height = EditorGUILayout.ObjectField(height, typeof(Texture2D), false) as Texture2D;
					emissive = EditorGUILayout.ObjectField(emissive, typeof(Texture2D), false) as Texture2D;
					ambiantOcculison = EditorGUILayout.ObjectField(ambiantOcculison, typeof(Texture2D), false) as Texture2D;
					secondAlbedo = EditorGUILayout.ObjectField(secondAlbedo, typeof(Texture2D), false) as Texture2D;
					secondNormal = EditorGUILayout.ObjectField(secondNormal, typeof(Texture2D), false) as Texture2D;
					detailMask = EditorGUILayout.ObjectField(detailMask, typeof(Texture2D), false) as Texture2D;
					displacement = EditorGUILayout.ObjectField(displacement, typeof(Texture2D), false) as Texture2D;
				}
			}
			
			if (EditorGUI.EndChangeCheck())
				UpdateInputVisibilities();
		}

		//no process needed, everything already assigned in ProcessOnce

		public override void OnNodeProcessOnce()
		{
			if (surfaceMapsObject != null)
			{
				maps = surfaceMapsObject.maps;
				return ;
			}
			
			maps.albedo = albedo;
			maps.normal = normal;
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