using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Node
{
	public class PWNodeBiomeBinder : PWNode {

		[PWInput("biome datas")]
		public BiomeData			inputBiome;
		
		public PWTerrainOutputMode	outputMode;
		
		//inputs for 2D topdown map
		[PWInput("surface texture")]
		public Texture2D			terrainSurface;

		//inputs for 3D heightmap terrain
		//TODO

		[PWOutput("biome")]
		public Biome				outputBiome;

		static Dictionary< PWTerrainOutputMode, List< string > > propertiesPerOutputType = new Dictionary< PWTerrainOutputMode, List< string > >()
		{
			{ PWTerrainOutputMode.TopDown2D, new List< string >() {"terrainSurface"} },
			{ PWTerrainOutputMode.Planar3D, new List< string >() {""} },
		};

		void UpdateOutputType()
		{
			foreach (var kp in propertiesPerOutputType)
				if (outputMode == kp.Key)
					foreach (var propName in kp.Value)
						UpdatePropVisibility(propName, PWVisibility.Visible);
				else
					foreach (var propName in kp.Value)
						UpdatePropVisibility(propName, PWVisibility.Gone);
		}

		public override void OnNodeCreate()
		{
			externalName = "Biome binder";
		}

		public override void OnNodeGUI()
		{
			PWTerrainOutputMode	oldTerrainMode = outputMode;
				outputMode = GetTerrainOutputMode();
			if (outputMode != oldTerrainMode)
				UpdateOutputType();

			if (outputBiome != null)
				EditorGUILayout.LabelField(outputBiome.name +  ":" + outputBiome.id);
		}

		public override bool OnNodeAnchorLink(string prop, int index)
		{
			if (prop == "inputBiome" && outputBiome != null)
				outputBiome.biomeDataReference = inputBiome;
			return true;
		}

		public override void OnNodeProcess()
		{
			if (outputBiome == null)
			{
				outputBiome = new Biome();
				outputBiome.biomeDataReference = inputBiome;
			}
			outputBiome.mode = outputMode;
			outputBiome.surfaceTexture = terrainSurface;
		}
	}
}
