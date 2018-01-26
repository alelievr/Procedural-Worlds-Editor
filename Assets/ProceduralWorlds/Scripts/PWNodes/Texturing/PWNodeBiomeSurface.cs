using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;
using PW.Core;

namespace PW.Node
{
	public class PWNodeBiomeSurface : PWNode
	{
	
		[PWInput, PWNotRequired]
		public PWArray< BiomeSurfaceSwitch >	inputSurfaces = new PWArray< BiomeSurfaceSwitch >();

		[PWOutput]
		public BiomeSurfaces	surfaces = new BiomeSurfaces();

		public override void OnNodeCreation()
		{
			name = "Biome surface";
		}

		public override void OnNodeEnable()
		{
		}

		public override void OnNodeGUI()
		{
			EditorGUIUtility.labelWidth = 80;
			
			EditorGUILayout.LabelField("Texturing switches: " + GetInputNodes().Count());
		}

		public override void OnNodeProcessOnce()
		{
			foreach (var biomeSwitch in inputSurfaces.GetValues())
			{
				Debug.Log("Biome switch !");
				surfaces.surfaceSwitches.Add(biomeSwitch);
			}
		}

		public override void OnNodeProcess()
		{
			Debug.Log("output biome surface: " + surfaces);
		}

		//nothing to process, output already computed by processOnce
	}
}