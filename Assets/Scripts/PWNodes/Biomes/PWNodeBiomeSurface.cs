using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using PW.Core;

namespace PW.Node
{
	public class PWNodeBiomeSurface : PWNode {
	
		[PWInput]
		[PWMultiple(1, typeof(BiomeSurfaceMaps))]

		[PWOutput]
		public BiomeSurfaces	surfaces;

		//complex have all maps

		public override void OnNodeCreate()
		{
			externalName = "Biome surface";
		}

		public override void OnNodeGUI()
		{
		}

		public override void OnNodeProcess()
		{
			surfaces = new BiomeSurfaces();
		}
	}
}