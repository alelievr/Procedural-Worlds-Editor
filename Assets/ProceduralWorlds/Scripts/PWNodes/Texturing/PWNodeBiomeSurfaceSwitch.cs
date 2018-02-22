using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PW.Core;
using PW.Biomator;

namespace PW.Node
{
	public class PWNodeBiomeSurfaceSwitch : PWNode
	{

		[PWInput]
		public BiomeSurfaceMaps		inputMaps;

		[PWInput]
		public BiomeSurfaceColor	inputColor;

		[PWInput]
		public BiomeSurfaceMaterial	inputMaterial;

		[PWInput, PWNotRequired]
		public BiomeDetails			inputDetails;

		[PWOutput]
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
					SetAnchorVisibility("inputMaps", PWVisibility.Visible);
					SetAnchorVisibility("inputColor", PWVisibility.Gone);
					SetAnchorVisibility("inputMaterial", PWVisibility.Gone);
					break ;
				case BiomeSurfaceType.Color:
					SetAnchorVisibility("inputMaps", PWVisibility.Gone);
					SetAnchorVisibility("inputColor", PWVisibility.Visible);
					SetAnchorVisibility("inputMaterial", PWVisibility.Gone);
					break ;
				case BiomeSurfaceType.Material:
					SetAnchorVisibility("inputMaps", PWVisibility.Gone);
					SetAnchorVisibility("inputColor", PWVisibility.Gone);
					SetAnchorVisibility("inputMaterial", PWVisibility.Visible);
					break ;
			}
		}
	}
}