using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using ProceduralWorlds.Node;
using System.Linq;

namespace ProceduralWorlds.Core
{

    public static class NodeTypeProvider
    {
		static readonly int				WorldGraph =	1 << 0;
		static readonly int				BiomeGraph =	1 << 1;

        public class NodeTypeInfo
        {
			public string				name;
            public Type					type;

			public NodeTypeInfo(string name, Type type)
			{
				this.name = name;
				this.type = type;
			}
        }

		public class NodeTypeInfoList
		{
			public int						allowedGraphMask;
			public string					title;
            public ColorSchemeName		colorSchemeName = ColorSchemeName.Default;
			public List< NodeTypeInfo >	typeInfos = new List< NodeTypeInfo >();

			public NodeTypeInfoList(int allowedGraphMask, string title, ColorSchemeName colorSchemeName, params object[] nodeTypeInfos)
			{
				this.allowedGraphMask = allowedGraphMask;
				this.title = title;
				this.colorSchemeName = colorSchemeName;

				if (nodeTypeInfos != null)
				{
					for (int i = 0; i < nodeTypeInfos.Length - 1; i += 2)
						typeInfos.Add(new NodeTypeInfo(nodeTypeInfos[i] as string, nodeTypeInfos[i + 1] as Type));
				}
			}
		}

        static List< Type > allNodeTypes = new List< Type >
		{
			//Primitives:
            typeof(NodeSlider), typeof(NodeTexture2D), typeof(NodeMaterial),
			typeof(NodeConstant), typeof(NodeMesh), typeof(NodeGameObject),
			typeof(NodeColor),

			//Operations:
            typeof(NodeAdd), typeof(NodeCurve),

			//Debug:
            typeof(NodeDebugInfo),

			//Noises and masks:
            typeof(NodeCircleNoiseMask), typeof(NodePerlinNoise2D),

			//Graph specific:
			typeof(NodeGraphInput), typeof(NodeGraphOutput), typeof(NodeBiomeGraphInput),
			typeof(NodeBiomeGraphOutput),

			//Biomes:
            typeof(NodeBiomeData), typeof(NodeBiomeBinder), typeof(NodeWaterLevel),
        	typeof(NodeBiomeBlender), typeof(NodeBiomeSwitch), typeof(NodeBiomeTemperature),
            typeof(NodeBiomeWetness), typeof(NodeBiomeSurface), typeof(NodeBiome),
			typeof(NodeBiomeDataDecomposer), typeof(NodeBiomeMerger),
			
			//Texturing:
			typeof(NodeBiomeSurfaceMaps), typeof(NodeBiomeSurfaceSwitch),
			typeof(NodeBiomeSurfaceModifiers), typeof(NodeBiomeSurfaceColor),
			typeof(NodeBiomeSurfaceMaterial), typeof(NodeTerrainDetail),
		};

		static List< NodeTypeInfoList > nodeInfoList = new List< NodeTypeInfoList >
		{
			new NodeTypeInfoList(0, "Graph", ColorSchemeName.Default,
				null, typeof(NodeGraphInput),
				null, typeof(NodeGraphOutput),
				null, typeof(NodeBiomeGraphInput),
				null, typeof(NodeBiomeGraphOutput)),
			new NodeTypeInfoList(WorldGraph | BiomeGraph, "Primitive types", ColorSchemeName.Alizarin,
				"Slider", typeof(NodeSlider),
				"Constant", typeof(NodeConstant),
				"Color", typeof(NodeColor),
				"GameObject", typeof(NodeGameObject),
				"Material", typeof(NodeMaterial),
				"Texture2D", typeof(NodeTexture2D),
				"Mesh", typeof(NodeMesh)
			),
			new NodeTypeInfoList(WorldGraph | BiomeGraph, "Operations", ColorSchemeName.Amethyst,
				"Add", typeof(NodeAdd),
				"Curve", typeof(NodeCurve)
			),
			new NodeTypeInfoList(WorldGraph | BiomeGraph, "Noises and Masks", ColorSchemeName.Emerald,
				"Perlin noise 2D", typeof(NodePerlinNoise2D),
				"Circle Noise Mask", typeof(NodeCircleNoiseMask)
			),
			new NodeTypeInfoList(WorldGraph, "Biomes", ColorSchemeName.Carrot,
				"Water Level", typeof(NodeWaterLevel),
				"To Biome data", typeof(NodeBiomeData),
				"Temperature map", typeof(NodeBiomeTemperature),
				"Wetness map", typeof(NodeBiomeWetness),
				"Biome Switch", typeof(NodeBiomeSwitch),
				"Biome Graph", typeof(NodeBiome),
				"Biome Blender", typeof(NodeBiomeBlender)
			),
			new NodeTypeInfoList(WorldGraph, "Terrain", ColorSchemeName.Pumpkin,
				"Biome merger", typeof(NodeBiomeMerger)
			),
			new NodeTypeInfoList(BiomeGraph, "Biomes", ColorSchemeName.Turquoise,
				"BiomeData decomposer", typeof(NodeBiomeDataDecomposer),
				"Biome surface", typeof(NodeBiomeSurface),
				"Biome binder", typeof(NodeBiomeBinder)
			),
			new NodeTypeInfoList(BiomeGraph, "Landforms & Texturing", ColorSchemeName.SunFlower,
				"Surface maps", typeof(NodeBiomeSurfaceMaps),
				"Surface color", typeof(NodeBiomeSurfaceColor),
				"Surface material", typeof(NodeBiomeSurfaceMaterial),
				"Terrain detail", typeof(NodeTerrainDetail),
				"Surface switch", typeof(NodeBiomeSurfaceSwitch),
				"Surface modifiers", typeof(NodeBiomeSurfaceModifiers)
			),
			new NodeTypeInfoList(WorldGraph | BiomeGraph, "Debug", ColorSchemeName.PeterRiver,
				"DebugLog", typeof(NodeDebugInfo)
			)
		};

		static List< NodeTypeInfoList > worldGraphInfoList;
		static List< NodeTypeInfoList > biomeGraphInfoList;

        static NodeTypeProvider()
		{
			worldGraphInfoList = nodeInfoList.Where(til => (til.allowedGraphMask & WorldGraph) != 0).ToList();
			biomeGraphInfoList = nodeInfoList.Where(til => (til.allowedGraphMask & BiomeGraph) != 0).ToList();

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

        public static List< NodeTypeInfoList > GetAllowedNodesForGraph(BaseGraphType graphType)
        {
			switch (graphType)
			{
				case BaseGraphType.World:
					return worldGraphInfoList;
				case BaseGraphType.Biome:
					return biomeGraphInfoList;
				default:
					Debug.LogError("Could not find allowed nodes for the graph " + graphType);
					return null;
			}
        }

		public static IEnumerable< Type > GetExlusiveNodeTypesForGraph(BaseGraphType graphType)
		{
			foreach (var nodeInfo in nodeInfoList)
				if (nodeInfo.allowedGraphMask == (int)graphType)
					foreach (var ni in nodeInfo.typeInfos)
						yield return ni.type;
		}

		static T GetNodeInfo< T >(Type t, Func< NodeTypeInfo, NodeTypeInfoList, T > fun)
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

		public static ColorSchemeName GetNodeColor(Type t)
		{
			return GetNodeInfo(t, (ni, nil) => (nil == null) ? ColorSchemeName.Default : nil.colorSchemeName);
		}
	}
}