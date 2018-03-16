using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

namespace ProceduralWorlds.Core
{
	public class DelayedChanges
	{
	
		//time before a stable value will trigger a callback
		public float	delayedTime = 300; //ms
	
		private class ChangeData
		{
			public object			value;
			public double			lastUpdate;
			public bool				called = true;
			public Action< object >	callback;
		}
	
		[System.NonSerialized]
		private  Dictionary< string, ChangeData > values = new Dictionary< string, ChangeData >();
	
		public void	UpdateValue(string key, object value = null)
		{
			if (!values.ContainsKey(key))
				values[key] = new ChangeData();
			var v = values[key];
			v.value = value;
			v.lastUpdate = EditorApplication.timeSinceStartup;
			v.called = false;
		}
	
		public void	BindCallback(string key, Action< object > callback)
		{
			if (!values.ContainsKey(key))
				values[key] = new ChangeData();
			values[key].callback = callback;
		}
	
		public void Update()
		{
			int i = 0;
			foreach (var valKP in values)
			{
				var cd = valKP.Value;
				if (cd.callback != null && !cd.called && EditorApplication.timeSinceStartup - cd.lastUpdate > delayedTime / 1000)
				{
					cd.called = true;
					cd.callback(cd.value);
				}
				i++;
			}
		}
	
		public void Clear()
		{
			values.Clear();
		}
	}
}