using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;
using ProceduralWorlds.Biomator;

namespace ProceduralWorlds.Node
{
	public class NodeBiomeSurfaceSwitch : BaseNode
	{

		[Input]
		public BiomeSurfaceMaps		inputMaps;

		[Input]
		public BiomeSurfaceColor	inputColor;

		[Input]
		public BiomeSurfaceMaterial	inputMaterial;

		[Input, NotRequired]
		public BiomeDetails			inputDetails;

		[Output]
		public BiomeSurfaceSwitch	outputSwitch = new BiomeSurfaceSwitch();

		public override void OnNodeCreation()
		{
			name = "Surface switch";
			outputSwitch.minSlope = 0;
			outputSwitch.maxSlope = 90;
		}

		public override void OnNodeEnable()
		{
			
			UpdateSurfaceType(biomeGraphRef.surfaceType);
		}

		public void UpdateSurfaceType(BiomeSurfaceType surfaceType)
		{
			switch (surfaceType)
			{
				case BiomeSurfaceType.SurfaceMaps:
					SetAnchorVisibility("inputMaps", Visibility.Visible);
					SetAnchorVisibility("inputColor", Visibility.Gone);
					SetAnchorVisibility("inputMaterial", Visibility.Gone);
					break ;
				case BiomeSurfaceType.Color:
					SetAnchorVisibility("inputMaps", Visibility.Gone);
					SetAnchorVisibility("inputColor", Visibility.Visible);
					SetAnchorVisibility("inputMaterial", Visibility.Gone);
					break ;
				case BiomeSurfaceType.Material:
					SetAnchorVisibility("inputMaps", Visibility.Gone);
					SetAnchorVisibility("inputColor", Visibility.Gone);
					SetAnchorVisibility("inputMaterial", Visibility.Visible);
					break ;
				default:
					break ;
			}
		}
	}
}