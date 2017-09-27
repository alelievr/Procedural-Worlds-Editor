using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PW.Node;
using System.Linq;

namespace PW.Core
{
    public static class PWNodeTypeProvider
    {
		static readonly int				PWMainGraph =	1 << 0;
		static readonly int				PWBiomeGraph =	1 << 1;

        public class PWNodeTypeInfo
        {
			public string				name = null;
            public Type					type = null;

			public PWNodeTypeInfo(string name, Type type)
			{
				this.name = name;
				this.type = type;
			}
        }

		public class PWNodeTypeInfoList
		{
			public int						allowedGraphMask;
			public string					title;
            public PWColorSchemeName		colorSchemeName = PWColorSchemeName.Default;
			public List< PWNodeTypeInfo >	typeInfos = new List< PWNodeTypeInfo >();

			public PWNodeTypeInfoList(int allowedGraphMask, string title, PWColorSchemeName colorSchemeName, params object[] nodeTypeInfos)
			{
				this.allowedGraphMask = allowedGraphMask;
				this.title = title;
				this.colorSchemeName = colorSchemeName;

				if (nodeTypeInfos != null)
				{
					for (int i = 0; i < nodeTypeInfos.Length - 1; i += 2)
						typeInfos.Add(new PWNodeTypeInfo(nodeTypeInfos[i] as string, nodeTypeInfos[i + 1] as Type));
				}
			}
		}

        static List< Type > allNodeTypes = new List< Type >
		{
			//Primitives:
            typeof(PWNodeSlider), typeof(PWNodeTexture2D), typeof(PWNodeMaterial), typeof(PWNodeConstant), typeof(PWNodeMesh), typeof(PWNodeGameObject), typeof(PWNodeColor), typeof(PWNodeSurfaceMaps),

			//Operations:
            typeof(PWNodeAdd), typeof(PWNodeCurve),

			//Debug:
            typeof(PWNodeDebugLog),

			//Noises and masks:
            typeof(PWNodeCircleNoiseMask), typeof(PWNodePerlinNoise2D),

			//Materializers:
            typeof(PWNodeSideView2DTerrain), typeof(PWNodeTopDown2DTerrain),

			//Graph specific:
			typeof(PWNodeGraphInput), typeof(PWNodeGraphOutput),

			//Biomes:
            typeof(PWNodeBiomeData), typeof(PWNodeBiomeBinder), typeof(PWNodeWaterLevel),
        	typeof(PWNodeBiomeBlender), typeof(PWNodeBiomeSwitch), typeof(PWNodeBiomeTemperature),
            typeof(PWNodeBiomeWetness), typeof(PWNodeBiomeSurface), typeof(PWNodeBiomeTerrain),
		};

		static List< PWNodeTypeInfoList > nodeInfoList = new List< PWNodeTypeInfoList >
		{
			new PWNodeTypeInfoList(PWMainGraph | PWBiomeGraph, "Primitive types", PWColorSchemeName.Alizarin,
				"Slider", typeof(PWNodeSlider),
				"Constant", typeof(PWNodeConstant),
				"Color", typeof(PWNodeColor),
				"Surface maps", typeof(PWNodeSurfaceMaps),
				"GameObject", typeof(PWNodeGameObject),
				"Material", typeof(PWNodeMaterial),
				"Texture2D", typeof(PWNodeTexture2D),
				"Mesh", typeof(PWNodeMesh)
			),
			new PWNodeTypeInfoList(PWMainGraph | PWBiomeGraph, "Operations", PWColorSchemeName.Amethyst,
				"Add", typeof(PWNodeAdd),
				"Curve", typeof(PWNodeCurve)
			),
			new PWNodeTypeInfoList(PWMainGraph, "Biomes", PWColorSchemeName.Carrot,
				"Water Level", typeof(PWNodeWaterLevel),
				"To Biome data", typeof(PWNodeBiomeData),
				"Biome switch", typeof(PWNodeBiomeSwitch),
				"Biome Binder", typeof(PWNodeBiomeBinder),
				"Biome blender", typeof(PWNodeBiomeBlender),
				"Biome temperature map", typeof(PWNodeBiomeTemperature),
				"Biome wetness map", typeof(PWNodeBiomeWetness)
			),
			new PWNodeTypeInfoList(PWBiomeGraph, "Biomes", PWColorSchemeName.Clouds,
				"Biome surface", typeof(PWNodeBiomeSurface),
				"Biome terrain", typeof(PWNodeBiomeTerrain)
			),
			new PWNodeTypeInfoList(PWMainGraph | PWBiomeGraph, "Landforms", PWColorSchemeName.Concrete,
				"Terrain detail", typeof(PWNodeTerrainDetail)
			),
			new PWNodeTypeInfoList(PWMainGraph | PWBiomeGraph, "Noises and Masks", PWColorSchemeName.Emerald,
				"Perlin noise 2D", typeof(PWNodePerlinNoise2D),
				"Circle Noise Mask", typeof(PWNodeCircleNoiseMask)
			),
			new PWNodeTypeInfoList(PWMainGraph | PWBiomeGraph, "Debug", PWColorSchemeName.PeterRiver,
				"DebugLog", typeof(PWNodeDebugLog)
			)
		};

		static List< PWNodeTypeInfoList > mainGraphInfoList;
		static List< PWNodeTypeInfoList > biomeGraphInfoList;

        static PWNodeTypeProvider()
		{
			mainGraphInfoList = nodeInfoList.Where(til => (til.allowedGraphMask & PWMainGraph) != 0).ToList();
			biomeGraphInfoList = nodeInfoList.Where(til => (til.allowedGraphMask & PWBiomeGraph) != 0).ToList();
        }

        public static  IEnumerable< Type >  GetAllNodeTypes()
        {
            return allNodeTypes;
        }

        public static List< PWNodeTypeInfoList > GetAllowedNodesForGraph(Type graphType)
        {
            if (graphType == typeof(PWMainGraph))
				return mainGraphInfoList;
			else if (graphType == typeof(PWBiomeGraph))
				return biomeGraphInfoList;
			//TODO: other types of graph
			return null;
        }
	}
}