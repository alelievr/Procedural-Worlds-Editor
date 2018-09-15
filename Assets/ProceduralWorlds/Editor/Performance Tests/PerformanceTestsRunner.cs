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

[Serializable]
public struct NodeProcessTime : IEquatable<NodeProcessTime>
{
	public string name;
	public float time;

	public NodeProcessTime(string nodeName, float time)
	{
		this.name = nodeName;
		this.time = time;
	}

	public bool Equals(NodeProcessTime other)
	{
		return time == other.time && name == other.name;
	}
}

[Serializable]
public struct PerformanceResult : IEquatable< PerformanceResult >
{
	public string	name;

	public double	processOnceTime;
	public double	processTime;

	public NodeProcessTime[] nodeProcessTime;

	public long		totalAllocatedMemory;
	public long		totalReservedMemory;
	public long		totalUnusedReservedMemory;

	public bool Equals(PerformanceResult other)
	{
		return name == other.name;
	}

	public override string ToString()
	{
		StringBuilder sb = new StringBuilder();

		sb.Append("name: " + name);
		sb.Append(", processOnceTime: " + processOnceTime);
		sb.Append(", processTime: " + processTime);
		sb.Append(", totalAllocatedMemory: " + totalAllocatedMemory);
		sb.Append(", rotalReservedMemory: " + totalReservedMemory);
		sb.Append(", totalUnusedReservedMemory: " + totalUnusedReservedMemory);

		return sb.ToString();
	}
}

[Serializable]
public struct PerformanceResultMulti : IEquatable< PerformanceResultMulti >
{
	public PerformanceResult[] results;

	public PerformanceResultMulti(int count)
	{
		results = new PerformanceResult[count];
	}

	public bool Equals(PerformanceResultMulti other)
	{
		return results == other.results;
	}
}

namespace ProceduralWorlds.Editor
{
	public static class PerformanceTestsRunner
	{
		public static readonly string	logFilePath = Path.GetFullPath("performance.log");
		public static readonly string	profilerDataFile = Path.GetFullPath("performance_profiler.data");
		public static readonly string	tmpProfilerLogFile = Path.GetFullPath("performance.tmp");
		public static readonly int		testIterationCount = 6;

		static StreamWriter logFile;

		[MenuItem("Window/Procedural Worlds/Run performance tests", priority = 10)]
		public static void Run()
		{
			string[] performanceGraphGUIDs = AssetDatabase.FindAssets("performance* t:WorldGraph");

			//empty log file:
			File.WriteAllText(logFilePath, string.Empty);

			logFile = new StreamWriter(File.OpenWrite(logFilePath));

			var resultsList = new List< PerformanceResultMulti >();

			ProfilerDriver.ClearAllFrames();
			ProfilerDriver.deepProfiling = true;
			Profiler.logFile = tmpProfilerLogFile;
			Profiler.enabled = true;

			foreach (var performanceGraphGUID in performanceGraphGUIDs)
			{
				string path = AssetDatabase.GUIDToAssetPath(performanceGraphGUID);
				WorldGraph graph = AssetDatabase.LoadAssetAtPath(path, typeof(WorldGraph)) as WorldGraph;

				var results = new PerformanceResultMulti(testIterationCount);

				for (int i = 0; i < testIterationCount; i++)
					results.results[i] = RunTestForGraph(graph);
				
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

			string reportPerfs = Environment.GetEnvironmentVariable("PW_REPORT_PERFORMANCES");

			if (reportPerfs == "ON")
			{
				ReportPerformaces(resultsList);
			}

			logFile.Write(text);
			logFile.Flush();
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

			result.nodeProcessTime = new NodeProcessTime[graph.allNodes.Count()];
			for (int i = 0; i < result.nodeProcessTime.Length; i++)
			{
				var node = graph.allNodes.ElementAt(i);
				result.nodeProcessTime[i] = new NodeProcessTime(node.name, node.processTime);
			}
			
			result.totalAllocatedMemory = Profiler.GetTotalAllocatedMemoryLong();
			result.totalReservedMemory = Profiler.GetTotalReservedMemoryLong();
			result.totalUnusedReservedMemory = Profiler.GetTotalUnusedReservedMemoryLong();

			return result;
		}

		static string SerializeResults(List< PerformanceResultMulti > resultsList)
		{
			StringBuilder sb = new StringBuilder();

			foreach (var results in resultsList)
			{
				sb.AppendLine("---");
				foreach (var result in results.results)
					sb.AppendLine(result.ToString());
			}

			return sb.ToString();
		}

		static void ReportPerformaces(List< PerformanceResultMulti > resultsList)
		{
			string ip = Environment.GetEnvironmentVariable("PW_REPORT_IP");

			if (String.IsNullOrEmpty(ip))
			{
				logFile.WriteLine("PW_REPORT_IP environement variable is empty !");
				return ;
			}

			IPAddress ipa;
			IPAddress.TryParse(ip, out ipa);

			if (ipa == null)
			{
				logFile.WriteLine("Can't parse IP: " + ip);
				return ;
			}
			
			try {
				IPEndPoint ipe = new IPEndPoint(ipa, 4204);
				Socket s = new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				s.Connect(ipe);
	
				if (!s.Connected)
				{
					logFile.WriteLine("Can't connect the socket !");
					return ;
				}
	
				Stream stream = new NetworkStream(s);
				var bin = new BinaryFormatter();

				bin.Serialize(stream, resultsList.ToArray());
			} catch (Exception e) {
				logFile.WriteLine("An error occured: " + e);
			}
		}
	}
}