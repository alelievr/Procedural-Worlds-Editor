using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Biomator;

namespace PW.Core
{
	public class PWNodeBiomeGraphInput : PWNode
	{
		[PWOutput("Partial Biome data")]
		public PartialBiome			outputBiomeData;
		
		[PWOutput]
		public PWArray< object >	outputValues = new PWArray< object >();

		[SerializeField]
		PWMainGraph					previewGraph;

		public override void OnNodeCreation()
		{
			name = "Biome input";

		}

		public override void OnNodeEnable()
		{
			#if UNITY_EDITOR
			
			if (previewGraph == null)
			{
				var graphs = Resources.FindObjectsOfTypeAll< PWMainGraph >();;
				if (graphs.Length > 0)
					previewGraph = graphs[0];
			}

			#endif
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(14);
			
			PWGUI.PWArrayField(outputValues);

			EditorGUILayout.LabelField("Preview graph");
			previewGraph = EditorGUILayout.ObjectField(previewGraph, typeof(PWMainGraph), false) as PWMainGraph;

			if (previewGraph == null)
				EditorGUILayout.HelpBox("Can't process the graph without a preview graph ", MessageType.Error);
			
			if (outputBiomeData != null)
				BiomeUtils.DrawBiomeInfos(outputBiomeData.biomeDataReference);
		}

		public override void OnNodeProcess()
		{
			if (outputBiomeData != null)
				return ;
			
			if (previewGraph == null)
				return ;
			
			//we process the graph to provide the outputBiomeData
			//it require that biomeGraph to be contained in the previewGraph.
			previewGraph.Process();
		}

	}
}