using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;
using UnityEditor;
using System.Diagnostics;
using UnityEngine.Profiling;
using System.Reflection;
using System;

using Debug = UnityEngine.Debug;

namespace ProceduralWorlds.Editor
{
	public static class PerformanceTestsRunner
	{
		[MenuItem("Window/Procedural Worlds/Run performance tests", priority = 10)]
		public static void Run()
		{
			string[] performanceGraphGUIDs = AssetDatabase.FindAssets("performance* t:WorldGraph");

			foreach (var performanceGraphGUID in performanceGraphGUIDs)
			{
				string path = AssetDatabase.GUIDToAssetPath(performanceGraphGUID);
				WorldGraph graph = AssetDatabase.LoadAssetAtPath(path, typeof(WorldGraph)) as WorldGraph;

				Stopwatch sw = new Stopwatch();

				SetProfileDeepScripts(true);
				Profiler.logFile = "profiler.data";
				Profiler.enableBinaryLog = true;
				Profiler.enabled = true;

				sw.Start();
				graph.ProcessOnce();
				sw.Stop();
				Debug.Log("WorldGraph ProcessOnce time: " + sw.ElapsedMilliseconds + "ms");

				sw.Reset();
				sw.Start();

				graph.Process();

				sw.Stop();
				Debug.Log("WorldGraph Process time: " + sw.ElapsedMilliseconds + "ms");
				
				Profiler.enabled = false;
				
				Debug.Log("Memory allocated: " + Profiler.GetTotalAllocatedMemoryLong());
				Debug.Log("Memory reserved: " + Profiler.GetTotalReservedMemoryLong());
				Debug.Log("Memory unused: " + Profiler.GetTotalUnusedReservedMemoryLong());

				SetProfileDeepScripts(false);
			}
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
