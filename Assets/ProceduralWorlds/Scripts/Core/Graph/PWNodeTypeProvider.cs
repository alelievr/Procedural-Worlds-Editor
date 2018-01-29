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
            typeof(PWNodeSlider), typeof(PWNodeTexture2D), typeof(PWNodeMaterial),
			typeof(PWNodeConstant), typeof(PWNodeMesh), typeof(PWNodeGameObject),
			typeof(PWNodeColor),

			//Operations:
            typeof(PWNodeAdd), typeof(PWNodeCurve),

			//Debug:
            typeof(PWNodeDebugInfo),

			//Noises and masks:
            typeof(PWNodeCircleNoiseMask), typeof(PWNodePerlinNoise2D),

			//Graph specific:
			typeof(PWNodeGraphInput), typeof(PWNodeGraphOutput), typeof(PWNodeBiomeGraphInput),
			typeof(PWNodeBiomeGraphOutput),

			//Biomes:
            typeof(PWNodeBiomeData), typeof(PWNodeBiomeBinder), typeof(PWNodeWaterLevel),
        	typeof(PWNodeBiomeBlender), typeof(PWNodeBiomeSwitch), typeof(PWNodeBiomeTemperature),
            typeof(PWNodeWetness), typeof(PWNodeBiomeSurface), typeof(PWNodeBiomeTerrain),
			typeof(PWNodeBiome), typeof(PWNodeBiomeDataDecomposer), typeof(PWNodeBiomeMerger),
			
			//Texturing:
			typeof(PWNodeBiomeSurfaceMaps), typeof(PWNodeBiomeSurfaceSwitch),
			typeof(PWNodeBiomeSurfaceModifiers), typeof(PWNodeBiomeSurfaceColor),
			typeof(PWNodeBiomeSurfaceMaterial), typeof(PWNodeTerrainDetail),
		};

		static List< PWNodeTypeInfoList > nodeInfoList = new List< PWNodeTypeInfoList >
		{
			new PWNodeTypeInfoList(0, "Graph", PWColorSchemeName.Default,
				null, typeof(PWNodeGraphInput),
				null, typeof(PWNodeGraphOutput),
				null, typeof(PWNodeBiomeGraphInput),
				null, typeof(PWNodeBiomeGraphOutput)),
			new PWNodeTypeInfoList(PWMainGraph | PWBiomeGraph, "Primitive types", PWColorSchemeName.Alizarin,
				"Slider", typeof(PWNodeSlider),
				"Constant", typeof(PWNodeConstant),
				"Color", typeof(PWNodeColor),
				"GameObject", typeof(PWNodeGameObject),
				"Material", typeof(PWNodeMaterial),
				"Texture2D", typeof(PWNodeTexture2D),
				"Mesh", typeof(PWNodeMesh)
			),
			new PWNodeTypeInfoList(PWMainGraph | PWBiomeGraph, "Operations", PWColorSchemeName.Amethyst,
				"Add", typeof(PWNodeAdd),
				"Curve", typeof(PWNodeCurve)
			),
			new PWNodeTypeInfoList(PWMainGraph | PWBiomeGraph, "Noises and Masks", PWColorSchemeName.Emerald,
				"Perlin noise 2D", typeof(PWNodePerlinNoise2D),
				"Circle Noise Mask", typeof(PWNodeCircleNoiseMask)
			),
			new PWNodeTypeInfoList(PWMainGraph, "Biomes", PWColorSchemeName.Carrot,
				"Water Level", typeof(PWNodeWaterLevel),
				"To Biome data", typeof(PWNodeBiomeData),
				"Remperature map", typeof(PWNodeBiomeTemperature),
				"Wetness map", typeof(PWNodeWetness),
				"Biome Switch", typeof(PWNodeBiomeSwitch),
				"Biome Graph", typeof(PWNodeBiome),
				"Biome blender", typeof(PWNodeBiomeBlender)
			),
			new PWNodeTypeInfoList(PWMainGraph, "Terrain", PWColorSchemeName.Pumpkin,
				"Biome merger", typeof(PWNodeBiomeMerger)
			),
			new PWNodeTypeInfoList(PWBiomeGraph, "Biomes", PWColorSchemeName.Turquoise,
				"BiomeData decomposer", typeof(PWNodeBiomeDataDecomposer),
				"Biome surface", typeof(PWNodeBiomeSurface),
				"Biome terrain", typeof(PWNodeBiomeTerrain),
				"Biome Binder", typeof(PWNodeBiomeBinder)
			),
			new PWNodeTypeInfoList(PWBiomeGraph, "Landforms & Texturing", PWColorSchemeName.SunFlower,
				"Surface maps", typeof(PWNodeBiomeSurfaceMaps),
				"Surface color", typeof(PWNodeBiomeSurfaceColor),
				"Surface material", typeof(PWNodeBiomeSurfaceMaterial),
				"Terrain detail", typeof(PWNodeTerrainDetail),
				"Surface switch", typeof(PWNodeBiomeSurfaceSwitch),
				"Surface modifiers", typeof(PWNodeBiomeSurfaceModifiers)
			),
			new PWNodeTypeInfoList(PWMainGraph | PWBiomeGraph, "Debug", PWColorSchemeName.PeterRiver,
				"DebugLog", typeof(PWNodeDebugInfo)
			)
		};

		static List< PWNodeTypeInfoList > mainGraphInfoList;
		static List< PWNodeTypeInfoList > biomeGraphInfoList;

        static PWNodeTypeProvider()
		{
			mainGraphInfoList = nodeInfoList.Where(til => (til.allowedGraphMask & PWMainGraph) != 0).ToList();
			biomeGraphInfoList = nodeInfoList.Where(til => (til.allowedGraphMask & PWBiomeGraph) != 0).ToList();

			//check if all nodes in the NodeInfoList are also inside the allnodeTypes list:
			foreach (var info in nodeInfoList)
				foreach (var nodeInfo in info.typeInfos)
					if (!allNodeTypes.Contains(nodeInfo.type))
						Debug.LogError("[NodeTypeProvider]: The node type " + nodeInfo.type + " is not present in the allNodeTypes list !");
        }

        public static  IEnumerable< Type >  GetAllNodeTypes()
        {
            return allNodeTypes;
        }

        public static List< PWNodeTypeInfoList > GetAllowedNodesForGraph(PWGraphType graphType)
        {
			switch (graphType)
			{
				case PWGraphType.Main:
					return mainGraphInfoList;
				case PWGraphType.Biome:
					return biomeGraphInfoList;
				//TODO: other type of graph
				default:
					Debug.LogError("Could not find allowed nodes for the graph " + graphType);
					return null;
			}
        }

		public static IEnumerable< Type > GetExlusiveNodeTypesForGraph(PWGraphType graphType)
		{
			foreach (var nodeInfo in nodeInfoList)
				if (nodeInfo.allowedGraphMask == (int)graphType)
					foreach (var ni in nodeInfo.typeInfos)
						yield return ni.type;
		}

		static T GetNodeInfo< T >(Type t, Func< PWNodeTypeInfo, PWNodeTypeInfoList, T > fun)
		{
			foreach (var nil in nodeInfoList)
				foreach (var ni in nil.typeInfos)
					if (ni.type == t)
						return fun(ni, nil);
			return fun(null, null);
		}

		public static string GetNodeName(Type t)
		{
			return GetNodeInfo(t, (ni, nil) => (ni == null) ? t.Name : ni.name);
		}

		public static PWColorSchemeName GetNodeColor(Type t)
		{
			return GetNodeInfo(t, (ni, nil) => (nil == null) ? PWColorSchemeName.Default : nil.colorSchemeName);
		}
	}
}