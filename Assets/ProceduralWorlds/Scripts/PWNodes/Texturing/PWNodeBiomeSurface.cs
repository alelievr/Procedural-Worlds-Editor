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

		GUIContent		surfaceGraphError = new GUIContent("Surface graph not built !", "You have a gap in some parameter so the graph can't be correctly built");

		public override void OnNodeCreation()
		{
			name = "Biome surface";
		}

		public override void OnNodeEnable()
		{
			OnPostProcess += UpdateGraph;
			OnReload += ReloadCallback;
		}

		public override void OnNodeDisable()
		{
			OnPostProcess -= UpdateGraph;
			OnReload -= ReloadCallback;
		}

		public override void OnNodeGUI()
		{
			EditorGUIUtility.labelWidth = 80;

			int switchCount = GetInputNodes().Count();
			
			EditorGUILayout.LabelField("Texturing switches: " + switchCount);

			if (surfaceGraph.isBuilt)
				EditorGUILayout.LabelField("Graph built without error");
			else
				EditorGUILayout.LabelField(surfaceGraphError, PWStyles.errorLabel);
			
			//TODO: print more infos about why the graph can't be built
		}

		void ReloadCallback(PWNode from) { UpdateGraph(); }

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