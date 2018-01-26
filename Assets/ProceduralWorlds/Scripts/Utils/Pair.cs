using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PW.Core
{
	[Serializable]
	public class Pair< T, U >
	{
		[SerializeField]
		public T	first;
		[SerializeField]
		public U	second;

		public Pair(T f, U s)
		{
			first = f;
			second = s;
		}
	}

	[Serializable]
	public class Pairs< T, U > : List< Pair< T, U> >
	{
		public void Add(T f, U s)
		{
			var p = new Pair< T, U >(f, s);
			this.Add(p);
		}
	}
}