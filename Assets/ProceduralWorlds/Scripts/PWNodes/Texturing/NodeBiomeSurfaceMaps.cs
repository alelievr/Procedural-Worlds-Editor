using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ProceduralWorlds.Core;
using ProceduralWorlds.Biomator;

namespace ProceduralWorlds.Node
{
	public class NodeBiomeSurfaceMaps : BaseNode
	{
		
		[Input("Albedo"), Offset(58), NotRequired]
		public Texture2D		albedo;
		[Input("Normal"), NotRequired]
		public Texture2D		normal;
		[Input("Height"), NotRequired]
		public Texture2D		height;
		[Input("Emissive"), NotRequired]
		public Texture2D		emissive;
		[Input("Specular"), NotRequired]
		public Texture2D		specular;
		[Input("Opacity"), NotRequired]
		public Texture2D		opacity;
		[Input("Smoothness"), NotRequired]
		public Texture2D		smoothness;
		[Input("Ambiant occulision"), NotRequired]
		public Texture2D		ambiantOcculison;
		[Input("Detail mask"), NotRequired]
		public Texture2D		detailMask;
		[Input("Second albedo"), NotRequired]
		public Texture2D		secondAlbedo;
		[Input("Second normal"), NotRequired]
		public Texture2D		secondNormal;
		[Input("Metallic"), NotRequired]
		public Texture2D		metallic;
		[Input("Roughness"), NotRequired]
		public Texture2D		roughness;
		[Input("Displacement"), NotRequired]
		public Texture2D		displacement;
		
		[SerializeField]
		public BiomeSurfaceMapsObject	surfaceMapsObject;
	
		[Output]
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


		public void UpdateInputVisibilities()
		{
			foreach (var inputName in inputNames)
				SetAnchorVisibility(inputName, Visibility.Gone);
			
			//if we have a surface map object, then dont show any input
			if (surfaceMapsObject != null)
				return ;

			switch (maps.type)
			{
				case SurfaceMapsType.Basic:
					foreach (var inputName in inputNames)
						if (basicInputFields.Contains(inputName))
							SetAnchorVisibility(inputName, Visibility.Visible);
					break ;
				case SurfaceMapsType.Normal:
					foreach (var inputName in inputNames)
						if (normalInputFields.Contains(inputName))
							SetAnchorVisibility(inputName, Visibility.Visible);
					break ;
				case SurfaceMapsType.Complex:
					foreach (var inputName in inputNames)
						SetAnchorVisibility(inputName, Visibility.Visible);
					break ;
				default:
					break ;
			}
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