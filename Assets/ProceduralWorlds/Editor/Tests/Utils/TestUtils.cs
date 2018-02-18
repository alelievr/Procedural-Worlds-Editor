using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using PW.Node;
using PW.Core;
using PW.Biomator;

namespace PW.Tests
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
	
		public static PWMainGraph	GenerateTestMainGraph()
		{
			return PWGraphBuilder.NewGraph< PWMainGraph >()
				.NewNode(typeof(PWNodeSlider), "slider")
				.NewNode(typeof(PWNodeConstant), "constant")
				.NewNode(typeof(PWNodeAdd), "add1")
				.NewNode(typeof(PWNodeAdd), "add2")
				.NewNode(typeof(PWNodeAdd), "add3")
				.NewNode(typeof(PWNodeAdd), "add4")
				.NewNode(typeof(PWNodeDebugInfo), "debug1")
				.NewNode(typeof(PWNodeDebugInfo), "debug2")
				.Link("slider", "add1")
				.Link("slider", "add2")
				.Link("constant", "add2")
				.Link("constant", "add3")
				.Link("add1", "add4")
				.Link("add4", "debug1")
				.Link("add2", "debug2")
				.Execute()
				.GetGraph() as PWMainGraph;
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
		// c*: PWNodeBiomeSurfaceColor, s*: PWNodeBiomeSurfaceSwitch, surf: PWNodeBiomeSurface
	
		public static PWBiomeGraph	GenerateTestBiomeGraph()
		{
			return PWGraphBuilder.NewGraph< PWBiomeGraph >()
				.NewNode< PWNodeBiomeSurfaceColor >("c1")
				.NewNode< PWNodeBiomeSurfaceColor >("c2")
				.NewNode< PWNodeBiomeSurfaceColor >("c3")
				.NewNode< PWNodeBiomeSurfaceSwitch >("s1")
				.NewNode< PWNodeBiomeSurfaceSwitch >("s2")
				.NewNode< PWNodeBiomeSurfaceSwitch >("s3")
				.NewNode< PWNodeBiomeSurface >("surf")
				.Link("s1", "surf")
				.Link("s2" ,"surf")
				.Link("s3" ,"surf")
				.Link("c1", "s1")
				.Link("c2" ,"s2")
				.Link("c3" ,"s3")
				.Custom(g => {
					(g as PWBiomeGraph).surfaceType = BiomeSurfaceType.Color;
				})
				.Execute()
				.GetGraph() as PWBiomeGraph;
		}

		// +--------------+     +-------------+   +-----------+
		// | perlin noise +-----> noise remap +---> view noise|
		// +--------------+     +-------------+   +-----------+

		public static PWMainGraph	GenerateTestMainGraphWhitespaces()
		{
			return PWGraphBuilder.NewGraph< PWMainGraph >()
				.NewNode< PWNodePerlinNoise2D >("perlin noise")
				.NewNode< PWNodeCurve >("noise remap")
				.NewNode< PWNodeDebugInfo >("view noise")
				.Link("perlin noise", "noise remap")
				.Link("noise remap", "view noise")
				.Execute()
				.GetGraph() as PWMainGraph;
		}

		//                                                +----+
		//                                              +-> b1 +--+
		// +--------+      +--------+      +---------+  | +----+  | +----------+
		// | perlin +------> wlevel +------> bswitch +--+         +-> bblender |
		// +--------+      +--------+      +---------+  | +----+  | +----------+
		//                                              +-> b2 +--+
		//                                                +----+

		public static PWMainGraph	GenerateTestMainGraphBiomeSwitch()
		{
			return PWGraphBuilder.NewGraph< PWMainGraph >()
				.NewNode< PWNodePerlinNoise2D >("perlin")
				.NewNode< PWNodeWaterLevel >("wlevel")
				.NewNode< PWNodeBiomeSwitch >("bswitch")
				.NewNode< PWNodeBiome >("b1")
				.NewNode< PWNodeBiome >("b2")
				.NewNode< PWNodeBiomeBlender >("bblender")
				.Link("perlin", "wlevel")
				.Link("wlevel", "bswitch")
				.Link("bswitch", "outputBiomes", "b1", "inputBiomeData")
				.Link("bswitch", "outputBiomes", "b2", "inputBiomeData")
				.Link("b1", "bblender")
				.Link("b2", "bblender")
				.Execute()
				.GetGraph() as PWMainGraph;
		}
	}
}