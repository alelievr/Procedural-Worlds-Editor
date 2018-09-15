using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ProceduralWorlds.Core;
using ProceduralWorlds.Biomator;

namespace ProceduralWorlds.Nodes
{
	public class NodeBiomeSurface : BaseNode
	{
	
		[Input, NotRequired]
		public PWArray< BiomeSurfaceSwitch >	inputSurfaces = new PWArray< BiomeSurfaceSwitch >();

		[Output]
		public BiomeSurfaceGraph	surfaceGraph = new BiomeSurfaceGraph();

		public override void OnNodeCreation()
		{
			name = "Biome surface";
		}

		public override void OnNodeEnable()
		{
			OnPostProcess += UpdateGraph;
		}

		public override void OnNodeDisable()
		{
			OnPostProcess -= UpdateGraph;
		}

		void UpdateGraph()
		{
			surfaceGraph.BuildGraph(inputSurfaces.GetValuesWithoutNull());
		}

		public override void OnNodeProcessOnce()
		{
			UpdateGraph();
		}

		//nothing to process, output already computed by processOnce
	}
}