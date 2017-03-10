using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PWValues {

	List< object > values = new List< object >();

	public int	Count
	{
		get { return values.Count; }
	}

	public List< T > GetValues<T>(params T[] olol)
	{
		return values.Where(o => o.GetType().IsAssignableFrom(typeof(T))).Select(o => (T)o).ToList();
	}

	public void Add(object val)
	{
		values.Add(val);
	}
}
