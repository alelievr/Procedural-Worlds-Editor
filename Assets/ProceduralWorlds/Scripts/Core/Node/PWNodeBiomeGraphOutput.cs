using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Biomator;

namespace PW.Core
{
	public class PWNodeBiomeGraphOutput : PWNode
	{

		[PWInput("Biome")]
		public Biome				inputBiome;

		[PWInput]
		public PWArray< object >	inputValues = new PWArray< object >();

		public override void OnNodeCreation()
		{
			name = "Biome output";
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(14);
			
			EditorGUILayout.LabelField("Biome: " + inputBiome);
			
			PWGUI.PWArrayField(inputValues);
		}
	}
}