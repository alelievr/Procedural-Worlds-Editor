using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

namespace PW.Editor
{
	public class DelayedChanges
	{
	
		//time before a stable value will trigger a callback
		public float	delayedTime = 300; //ms
	
		private class ChangeData
		{
			public object			value = null;
			public double			lastUpdate = 0;
			public bool				called = true;
			public Action< object >	callback = null;
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
					cd.callback(cd.value);
					cd.called = true;
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