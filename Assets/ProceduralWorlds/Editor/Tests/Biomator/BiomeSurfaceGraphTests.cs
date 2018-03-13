using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using ProceduralWorlds.Biomator;

namespace ProceduralWorlds.Tests.Biomator
{
	public class BiomeSurfaceGraphTests
	{

		class BSCData
		{
			public string name;

			public bool heightEnabled = false;
			public bool slopeEnabled = false;

			public float minHeight;
			public float maxHeight;

			public float minSlope;
			public float maxSlope;
		}

		static List< BiomeSurfaceSwitch > BSCDataToSurfaces(BSCData[] switches)
		{
			List< BiomeSurfaceSwitch > switchList = new List< BiomeSurfaceSwitch >();

			foreach (var s in switches)
			{
				BiomeSurfaceSwitch bs = new BiomeSurfaceSwitch();

				bs.heightEnabled = s.heightEnabled;
				bs.slopeEnabled = s.slopeEnabled;
				bs.minHeight = s.minHeight;
				bs.maxHeight = s.maxHeight;
				bs.minSlope = s.minSlope;
				bs.maxSlope = s.maxSlope;
				bs.surface = new BiomeSurface(){name = s.name};

				switchList.Add(bs);
			}

			return switchList;
		}

		//	Valid biome switch graph:
		//		                 +-3-----------+
		//		                 |height: 00-20|
		//		+-1-----------+  |slope: nope  |
		//		|height: 00-40|  +-------------+
		//		|slope: nope  |
		//		+-------------+  +-4-----------+
		//		                 |height: 20-40|
		//		                 |slope: 00-30 |
		//		+-2-----------+  +-------------+
		//		|height: 40-60|
		//		|slope: nope  |  +-5-----------+
		//		+-------------+  |height: 20-40|
		//		                 |slope: 30-60 |
		//		                 +-------------+

		public static List< BiomeSurfaceSwitch > GenerateValidBiomeSurfaces()
		{
			var switches = new[] {
				new BSCData{name = "1", heightEnabled = true, minHeight = 0f, maxHeight = 40f},
				new BSCData{name = "2", heightEnabled = true, minHeight = 40f, maxHeight = 60f},
				new BSCData{name = "3", heightEnabled = true, minHeight = 0f, maxHeight = 20f},
				new BSCData{name = "4", heightEnabled = true, minHeight = 20f, maxHeight = 40f, slopeEnabled = true, minSlope = 0f, maxSlope = 30f},
				new BSCData{name = "5", heightEnabled = true, minHeight = 20f, maxHeight = 40f, slopeEnabled = true, minSlope = 30f, maxSlope = 60f},
			};

			return BSCDataToSurfaces(switches);
		}

		//	Invalid biome switch graph (hole in height condition between 30 and 40)
		//			+-1-----------+
		//			|height: 00-30|
		//			|slope: nope  |
		//			+-------------+  +-3-----------+
		//			                 |height: 00-20|
		//			                 |slope: nope  |
		//			+-2-----------+  +-------------+
		//			|height: 40-60|
		//			|slope: nope  |
		//			+-------------+

		public static List< BiomeSurfaceSwitch > GenerateInvalidHoleBiomeSurfaces()
		{
			var switches = new[] {
				new BSCData{name = "1", heightEnabled = true, minHeight = 0f, maxHeight = 30f},
				new BSCData{name = "2", heightEnabled = true, minHeight = 40f, maxHeight = 60f},
				new BSCData{name = "3", heightEnabled = true, minHeight = 0f, maxHeight = 20f},
			};

			return BSCDataToSurfaces(switches);
		}

		[Test]
		public static void BiomeSurfaceGraphValidGraph()
		{
			var graph = new BiomeSurfaceGraph();
			graph.BuildGraph(GenerateValidBiomeSurfaces());
			
			var surface1 = graph.GetSurface(1, 1);
			var surface21 = graph.GetSurface(21, 1);
			var surface41 = graph.GetSurface(41, 90);
			var surface22 = graph.GetSurface(22, 50);

			Assert.That(surface1.name == "3");
			Assert.That(surface21.name == "4");
			Assert.That(surface22.name == "5");
			Assert.That(surface41.name == "2");
		}
		

		[Test]
		public static void BiomeSurfaceGraphValidGraphOutOfBounds()
		{
			var graph = new BiomeSurfaceGraph();
			graph.BuildGraph(GenerateValidBiomeSurfaces());
			
			var surface10 = graph.GetSurface(-10);
			var surface99 = graph.GetSurface(99);

			Assert.That(surface10 == null);
			Assert.That(surface99 == null);
		}

		[Test]
		public static void BiomeSurfaceGraphValidGraphBounds()
		{
			var graph = new BiomeSurfaceGraph();
			graph.BuildGraph(GenerateValidBiomeSurfaces());

			var surface0 = graph.GetSurface(0);
			var surface40 = graph.GetSurface(40);
			var surface20 = graph.GetSurface(20, 30);

			Assert.That(surface0.name == "3");
			Assert.That(surface40.name == "4" || surface40.name == "5");
			Assert.That(surface20.name == "4" || surface20.name == "5");
		}

		[Test]
		public static void BiomeSurfaceGraphInvalidHoleGraph()
		{
			var graph = new BiomeSurfaceGraph();
			bool valid = graph.BuildGraph(GenerateInvalidHoleBiomeSurfaces());

			Assert.That(valid == false);

			var surface10 = graph.GetSurface(10);
			var surface35 = graph.GetSurface(35);
			var surface41 = graph.GetSurface(41);

			Assert.That(surface10 == null);
			Assert.That(surface35 == null);
			Assert.That(surface41 == null);
		}
	
	}
}