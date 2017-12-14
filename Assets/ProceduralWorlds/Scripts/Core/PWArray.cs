using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.Reflection;

namespace PW.Core
{
	[System.SerializableAttribute]
	public class PWArray< T > : IEnumerable< T >
	{

		public static MethodInfo assignAtReflection = typeof(PWArray<>).GetMethod("AssignAt");
		
		[SerializeField]
		public int		valuesCount { get { return values.Count; } }
	
		List< T >		values;
		List< string >	names;

		public PWArray()
		{
			if (values == null)
				values = new List< T >();
			if (names == null)
				names = new List< string >();
			
			while (values.Count < valuesCount)
				Add(default(T));
		}
	
		public int	Count
		{
			get { return values.Count; }
		}
	
		public List< T > GetValues()
		{
			return values;
		}

		public List< string > GetNames()
		{
			return names;
		}

		public T At(int index)
		{
			if (index < 0 || index >= values.Count)
				return default(T);
			return values[index];
		}
		
		public string NameAt(int index)
		{
			if (index < 0 || index >= names.Count)
				return null;
			return names[index];
		}
	
		public void Add(T val)
		{
			values.Add(val);
			names.Add(null);
		}

		public bool AssignAt(int index, T val, string name, bool force = false)
		{
			if (index >= values.Count)
			{
				if (force)
				{
					while (index >= values.Count)
						values.Add(default(T));
					while (index >= names.Count)
						names.Add(null);
				}
				else
					return false;
			}

			values[index] = val;
			names[index] = name;
			return true;
		}

		public bool RemoveAt(int index)
		{
			if (index >= values.Count)
				return false;

			values.RemoveAt(index);
			names.RemoveAt(index);
			return true;
		}

		public void Clear()
		{
			values.Clear();
			names.Clear();
		}

		public  T  this[int index]  
		{  
			get { return values[index]; }  
			set { values.Insert(index, value); }  
		}
	
		public IEnumerator< T > GetEnumerator()
		{
			return values.GetEnumerator();
		}
	
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	
	}
}
