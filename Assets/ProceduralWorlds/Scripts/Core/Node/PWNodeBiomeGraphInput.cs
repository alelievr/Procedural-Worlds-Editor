using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Biomator;

namespace PW.Core
{
	public class PWNodeBiomeGraphInput : PWNode
	{
		[PWOutput("Biome data")]
		public BiomeData			outputBiomeData;
		
		[PWOutput]
		public PWArray< object >	outputValues = new PWArray< object >();

		[SerializeField]
		PWMainGraph					previewGraph;

		public override void OnNodeCreation()
		{
			name = "Biome input";

			#if UNITY_EDITOR
			
			var graphs = Resources.FindObjectsOfTypeAll< PWMainGraph >();;
			if (graphs.Length > 0)
				previewGraph = graphs[0];

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
				BiomeUtils.DrawBiomeInfos(outputBiomeData);
		}

		public override void OnNodeProcess()
		{
			if (outputBiomeData != null)
				return ;
			
			if (previewGraph == null)
				return ;
			
		}

	}
}