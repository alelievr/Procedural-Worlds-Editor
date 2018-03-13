using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using ProceduralWorlds.Node;
using ProceduralWorlds.Core;
using ProceduralWorlds.Biomator;

namespace ProceduralWorlds.Tests
{
	public static class TestUtils
	{
	
		//test main graph
		// 	              +-----+
		//            +---> Add1+---+
		// +------+   |   +-----+   |   +-----+   +------+
		// |Slider+---+             +---> Add4+--->Debug1|
		// +------+   |   +-----+       +-----+   +------+
		//            +---> Add2+---+
		// +------+   |   +-----+   |   +------+
		// |Float +---+             +--->Debug2|
		// +------+   |   +-----+       +------+
		//            +---> Add3+    
		//                +-----+
	
		public static WorldGraph	GenerateTestWorldGraph()
		{
			return BaseGraphBuilder.NewGraph< WorldGraph >()
				.NewNode(typeof(NodeSlider), "slider")
				.NewNode(typeof(NodeConstant), "constant")
				.NewNode(typeof(NodeAdd), "add1")
				.NewNode(typeof(NodeAdd), "add2")
				.NewNode(typeof(NodeAdd), "add3")
				.NewNode(typeof(NodeAdd), "add4")
				.NewNode(typeof(NodeDebugInfo), "debug1")
				.NewNode(typeof(NodeDebugInfo), "debug2")
				.Link("slider", "add1")
				.Link("slider", "add2")
				.Link("constant", "add2")
				.Link("constant", "add3")
				.Link("add1", "add4")
				.Link("add4", "debug1")
				.Link("add2", "debug2")
				.Execute()
				.GetGraph() as WorldGraph;
		}

		//Test biome graph
		// +----+      +----+
		// | c1 +------> s1 +----+
		// +----+      +----+    |
		//                       |
		// +----+      +----+    |  +------+
		// | c2 +------> s2 +-------> surf |
		// +----+      +----+    |  +------+
		//                       |
		// +----+      +----+    |
		// | c3 +------> s3 +----+
		// +----+      +----+
		// c*: NodeBiomeSurfaceColor, s*: NodeBiomeSurfaceSwitch, surf: NodeBiomeSurface
	
		public static BiomeGraph	GenerateTestBiomeGraph()
		{
			return BaseGraphBuilder.NewGraph< BiomeGraph >()
				.NewNode< NodeBiomeSurfaceColor >("c1")
				.NewNode< NodeBiomeSurfaceColor >("c2")
				.NewNode< NodeBiomeSurfaceColor >("c3")
				.NewNode< NodeBiomeSurfaceSwitch >("s1")
				.NewNode< NodeBiomeSurfaceSwitch >("s2")
				.NewNode< NodeBiomeSurfaceSwitch >("s3")
				.NewNode< NodeBiomeSurface >("surf")
				.Link("s1", "surf")
				.Link("s2" ,"surf")
				.Link("s3" ,"surf")
				.Link("c1", "s1")
				.Link("c2" ,"s2")
				.Link("c3" ,"s3")
				.Custom(g => {
					(g as BiomeGraph).surfaceType = BiomeSurfaceType.Color;
				})
				.Execute()
				.GetGraph() as BiomeGraph;
		}

		// +--------------+     +-------------+   +-----------+
		// | perlin noise +-----> noise remap +---> view noise|
		// +--------------+     +-------------+   +-----------+

		public static WorldGraph	GenerateTestWorldGraphWhitespaces()
		{
			return BaseGraphBuilder.NewGraph< WorldGraph >()
				.NewNode< NodePerlinNoise2D >("perlin noise")
				.NewNode< NodeCurve >("noise remap")
				.NewNode< NodeDebugInfo >("view noise")
				.Link("perlin noise", "noise remap")
				.Link("noise remap", "view noise")
				.Execute()
				.GetGraph() as WorldGraph;
		}

		//                                                +----+
		//                                              +-> b1 +--+
		// +--------+      +--------+      +---------+  | +----+  | +----------+
		// | perlin +------> wlevel +------> bswitch +--+         +-> bblender |
		// +--------+      +--------+      +---------+  | +----+  | +----------+
		//                                              +-> b2 +--+
		//                                                +----+

		public static WorldGraph	GenerateTestWorldGraphBiomeSwitch()
		{
			return BaseGraphBuilder.NewGraph< WorldGraph >()
				.NewNode< NodePerlinNoise2D >("perlin")
				.NewNode< NodeWaterLevel >("wlevel")
				.NewNode< NodeBiomeSwitch >("bswitch")
				.NewNode< NodeBiome >("b1")
				.NewNode< NodeBiome >("b2")
				.NewNode< NodeBiomeBlender >("bblender")
				.Link("perlin", "wlevel")
				.Link("wlevel", "bswitch")
				.Link("bswitch", "outputBiomes", "b1", "inputBiomeData")
				.Link("bswitch", "outputBiomes", "b2", "inputBiomeData")
				.Link("b1", "bblender")
				.Link("b2", "bblender")
				.Execute()
				.GetGraph() as WorldGraph;
		}
	}
}