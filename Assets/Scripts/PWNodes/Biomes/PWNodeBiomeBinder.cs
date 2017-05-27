using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PW
{
	public class PWNodeBiomeBinder : PWNode {

		[PWInput("biome datas")]
		public BiomeData		inputBiome;
		
		public PWTerrainOutputMode		outputType;

		//inputs for 2D topdown map
		[PWInput("surface texture")]
		public Texture2D		terrainSurface;

		//inputs for 3D heightmap terrain
		//TODO

		[PWOutput("biome")]
		public Biome			outputBiome;

		[SerializeField]
		bool					initialized = false;

		static Dictionary< PWTerrainOutputMode, List< string > > propertiesPerOutputType = new Dictionary< PWTerrainOutputMode, List< string > >()
		{
			{ PWTerrainOutputMode.TopDown2D, new List< string >() {"terrainSurface"} },
			{ PWTerrainOutputMode.Planar3D, new List< string >() {""} },
		};

		void UpdateOutputType()
		{
			foreach (var kp in propertiesPerOutputType)
				if (outputType == kp.Key)
					foreach (var propName in kp.Value)
						UpdatePropVisibility(propName, PWVisibility.Visible);
				else
					foreach (var propName in kp.Value)
						UpdatePropVisibility(propName, PWVisibility.Gone);
		}

		public override void OnNodeCreate()
		{
			if (!initialized)
			{
				outputType = GetOutputType();
				UpdateOutputType();
			}

			externalName = "Biome binder";
			initialized = true;
		}

		public override void OnNodeGUI()
		{

		}

		public override void OnNodeAnchorLink(string prop, int index)
		{
			if (prop == "inputBiome" && outputBiome != null)
				outputBiome.biomeDataReference = inputBiome;
		}

		public override void OnNodeProcess()
		{
			if (outputBiome == null)
			{
				outputBiome = new Biome();
				outputBiome.biomeDataReference = inputBiome;
			}
			outputBiome.mode = outputType;
			outputBiome.surfaceTexture = terrainSurface;
		}

	}
}
