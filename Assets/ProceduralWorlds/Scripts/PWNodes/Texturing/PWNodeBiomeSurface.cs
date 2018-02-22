using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;
using PW.Core;
using PW.Biomator;

namespace PW.Node
{
	public class PWNodeBiomeSurface : PWNode
	{
	
		[PWInput, PWNotRequired]
		public PWArray< BiomeSurfaceSwitch >	inputSurfaces = new PWArray< BiomeSurfaceSwitch >();

		[PWOutput]
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