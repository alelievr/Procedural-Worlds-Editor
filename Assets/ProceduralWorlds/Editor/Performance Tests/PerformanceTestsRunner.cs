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

using Debug = UnityEngine.Debug;

namespace ProceduralWorlds.Editor
{
	public static class PerformanceTestsRunner
	{
		public static readonly string logFilePath = Path.GetFullPath("performance.log");

		[MenuItem("Window/Procedural Worlds/Run performance tests", priority = 10)]
		public static void Run()
		{
			string[] performanceGraphGUIDs = AssetDatabase.FindAssets("performance* t:WorldGraph");

			FileStream logFile = new FileStream(logFilePath, FileMode.OpenOrCreate);
			StreamWriter log = new StreamWriter(logFile);

			SetProfileDeepScripts(true);
			Profiler.logFile = "profiler.data";
			Profiler.enableBinaryLog = true;
			Profiler.enabled = true;

			foreach (var performanceGraphGUID in performanceGraphGUIDs)
			{
				string path = AssetDatabase.GUIDToAssetPath(performanceGraphGUID);
				WorldGraph graph = AssetDatabase.LoadAssetAtPath(path, typeof(WorldGraph)) as WorldGraph;

				RunTestForGraph(graph, log);
			}

			Profiler.enabled = false;
			SetProfileDeepScripts(false);

			log.Flush();
			log.Close();
			logFile.Close();
		}

		static void RunTestForGraph(WorldGraph graph, StreamWriter log)
		{
			Stopwatch sw = new Stopwatch();

			log.WriteLine("Testing graph " + graph.name + ":");

			sw.Start();
			graph.ProcessOnce();
			sw.Stop();
			log.WriteLine("WorldGraph ProcessOnce time: " + sw.ElapsedMilliseconds + "ms");

			sw.Reset();
			sw.Start();

			graph.Process();

			sw.Stop();
			log.WriteLine("WorldGraph Process time: " + sw.ElapsedMilliseconds + "ms");
			
			log.WriteLine("Memory allocated: " + Profiler.GetTotalAllocatedMemoryLong());
			log.WriteLine("Memory reserved: " + Profiler.GetTotalReservedMemoryLong());
			log.WriteLine("Memory unused: " + Profiler.GetTotalUnusedReservedMemoryLong());
		}

		public static void SetProfileDeepScripts(bool deep)
		{
			var asm = typeof(UnityEditor.EditorWindow).Assembly;
			var profilerWindow = asm.GetType("UnityEditor.ProfilerWindow");

			if (profilerWindow == null)
				return ;

			var setProfileDeepScripts = profilerWindow.GetMethod("SetProfileDeepScripts", BindingFlags.NonPublic | BindingFlags.Static);

			if (setProfileDeepScripts != null)
				setProfileDeepScripts.Invoke(null, new object[] {deep});
		}
	}
}
