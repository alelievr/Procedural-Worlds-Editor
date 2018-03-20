using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;
using UnityEditor;
using System.Diagnostics;
using UnityEngine.Profiling;
using System.Reflection;
using System;
using System.IO;
using System.Linq;
using UnityEditorInternal;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

using Debug = UnityEngine.Debug;

namespace ProceduralWorlds.Editor
{
	public static class PerformanceTestsRunner
	{
		[Serializable]
		public struct PerformanceResult
		{
			public string	name;

			public double	processOnceTime;
			public double	processTime;

			public List< Pair< string, float > > nodeProcessTime;

			public long		totalAllocatedMemory;
			public long		totalReservedMemory;
			public long		totalUnusedReservedMemory;

			public override string ToString()
			{
				var toJsonify = new List< Pair< string, object > >
				{
					new Pair< string, object >("name", name),
					new Pair< string, object >("processOnceTime", processOnceTime),
					new Pair< string, object >("processTime", processTime),
					new Pair< string, object >("totalAllocatedMemory", totalAllocatedMemory),
					new Pair< string, object >("totalReservedMemory", totalReservedMemory),
					new Pair< string, object >("totalUnusedReservedMemory", totalUnusedReservedMemory),
				};
				return Jsonizer.Generate(toJsonify);
			}
		}

		public static readonly string	logFilePath = Path.GetFullPath("performance.log");
		public static readonly string	profilerDataFile = Path.GetFullPath("performance_profiler.data");
		public static readonly string	tmpProfilerLogFile = Path.GetFullPath("performance.tmp");
		public static readonly int		testIterationCount = 6;

		[MenuItem("Window/Procedural Worlds/Run performance tests", priority = 10)]
		public static void Run()
		{
			string[] performanceGraphGUIDs = AssetDatabase.FindAssets("performance* t:WorldGraph");

			var resultsList = new List< List< PerformanceResult > >();

			ProfilerDriver.ClearAllFrames();
			ProfilerDriver.deepProfiling = true;
			Profiler.logFile = tmpProfilerLogFile;
			Profiler.enabled = true;

			foreach (var performanceGraphGUID in performanceGraphGUIDs)
			{
				string path = AssetDatabase.GUIDToAssetPath(performanceGraphGUID);
				WorldGraph graph = AssetDatabase.LoadAssetAtPath(path, typeof(WorldGraph)) as WorldGraph;

				var results = new List< PerformanceResult >();

				for (int i = 0; i < testIterationCount; i++)
					results.Add(RunTestForGraph(graph));
				
				resultsList.Add(results);
			}
			
			Profiler.enabled = false;
			ProfilerDriver.deepProfiling = false;

			//this is totally broken in 2017.3 ...
			#if !UNITY_2017_3
				
				ProfilerDriver.LoadProfile(tmpProfilerLogFile, false);
	
				ProfilerDriver.SaveProfile(profilerDataFile);

			#endif

			string text = SerializeResults(resultsList);

			string reportPerfs = System.Environment.GetEnvironmentVariable("PW_REPORT_PERFORMANCES");

			if (reportPerfs == "ON")
			{
				ReportPerformaces(resultsList);
			}

			File.WriteAllText(logFilePath, text);
		}

		static PerformanceResult RunTestForGraph(WorldGraph graph)
		{
			var result = new PerformanceResult();
			Stopwatch sw = new Stopwatch();

			result.name = graph.name;

			sw.Start();
			graph.ProcessOnce();
			sw.Stop();
			result.processOnceTime = sw.Elapsed.TotalMilliseconds;

			sw.Reset();
			sw.Start();

			graph.Process();

			sw.Stop();
			result.processTime = sw.Elapsed.TotalMilliseconds;

			result.nodeProcessTime = new List< Pair< string, float > >();
			foreach (var node in graph.allNodes)
				result.nodeProcessTime.Add(new Pair< string, float >(node.name, node.processTime));
			
			result.totalAllocatedMemory = Profiler.GetTotalAllocatedMemoryLong();
			result.totalReservedMemory = Profiler.GetTotalReservedMemoryLong();
			result.totalUnusedReservedMemory = Profiler.GetTotalUnusedReservedMemoryLong();

			return result;
		}

		static string SerializeResults(List< List< PerformanceResult > > resultsList)
		{
			StringBuilder sb = new StringBuilder();

			foreach (var results in resultsList)
			{
				sb.AppendLine("---");
				foreach (var result in results)
					sb.AppendLine(result.ToString());
			}

			return sb.ToString();
		}

		static void ReportPerformaces(List< List< PerformanceResult > > resultsList)
		{
			string ip = System.Environment.GetEnvironmentVariable("PW_REPORT_IP");

			IPAddress ipa;
			IPAddress.TryParse(ip, out ipa);

			if (ipa == null)
				return ;
			
			IPEndPoint ipe = new IPEndPoint(ipa, 4204);
			Socket s = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			s.Connect(ipe);

			if (!s.Connected)
				return ;

			Stream stream = new NetworkStream(s);
			var bin = new BinaryFormatter();
			bin.Serialize(stream, resultsList);
		}
	}
}
