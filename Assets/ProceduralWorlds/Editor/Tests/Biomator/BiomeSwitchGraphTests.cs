using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Linq;
using ProceduralWorlds.Core;
using ProceduralWorlds.Biomator;
using ProceduralWorlds.Node;
using ProceduralWorlds.Biomator.SwitchGraph;

namespace ProceduralWorlds.Tests.Biomator
{
	public class BiomeSwitchGraphTests
	{
		//                                                +----+
		//                                              +-> b1 +--+
		// +--------+      +--------+      +---------+  | +----+  | +----------+
		// | perlin +------> wlevel +------> bswitch +--+         +-> bblender |
		// +--------+      +--------+      +---------+  | +----+  | +----------+
		//                                              +-> b2 +--+
		//                                                +----+

		[Test]
		public static void SimpleSwitchGraphBuild()
		{
			WorldGraph		graph = TestUtils.GenerateTestWorldGraphBiomeSwitch();
			BiomeData		bd = new BiomeData();

			var wlevel = graph.FindNodeByName("wlevel");
			var bswitch = graph.FindNodeByName< NodeBiomeSwitch >("bswitch");
			var b1 = graph.FindNodeByName< NodeBiome >("b1");
			var b2 = graph.FindNodeByName< NodeBiome >("b2");

			bd.biomeSwitchGraphStartPoint = wlevel;

			PartialBiome	p1 = new PartialBiome();
			PartialBiome	p2 = new PartialBiome();

			b1.outputBiome = p1;
			b2.outputBiome = p2;

			//setup the switch values
			var sd = bswitch.switchList.switchDatas;
			sd[0].min = 0;
			sd[0].max = 5;
			sd[0].name = "1";
			sd[0].samplerName = BiomeSamplerName.terrainHeight;
			sd[1].min = 5;
			sd[1].max = 10;
			sd[1].name = "2";
			sd[1].samplerName = BiomeSamplerName.terrainHeight;

			Sampler2D		terrainHeight = new Sampler2D(32, 1);
			terrainHeight.min = 0;
			terrainHeight.max = 10;

			bd.UpdateSamplerValue(BiomeSamplerName.terrainHeight, terrainHeight);

			BiomeSwitchGraph	switchGraph = new BiomeSwitchGraph();

			switchGraph.BuildGraph(bd);

			Assert.That(switchGraph.isBuilt == true);

			var values = new BiomeSwitchValues();

			values[0] = 4.0f;
			Assert.That(switchGraph.FindBiome(values).id == p1.id);
			values[0] = 0f;
			Assert.That(switchGraph.FindBiome(values).id == p1.id);
			values[0] = 5f;
			Assert.That(switchGraph.FindBiome(values).id == p1.id);

			values[0] = 6.0f;
			Assert.That(switchGraph.FindBiome(values).id == p2.id);
			values[0] = 10f;
			Assert.That(switchGraph.FindBiome(values).id == p2.id);
		}

	}
}