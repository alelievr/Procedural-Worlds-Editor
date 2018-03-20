using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.Reflection;

namespace ProceduralWorlds.Core
{
	[System.Serializable]
	public class PWArray< T > : IEnumerable< T >, IPWArray
	{
		
		[SerializeField]
		public int		valuesCount { get { return values.Count; } }
	
		readonly List< T >		values;
		readonly List< string >	names;

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
		
		public List< T > GetValuesWithoutNull()
		{
			return values.Where(v => v != null).ToList();
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

		public int	FindName(string name)
		{
			return names.FindIndex((n => n == name));
		}
	
		public void Add(T val)
		{
			values.Add(val);
			names.Add(null);
		}

		public void Add(T val, string name)
		{
			values.Add(val);
			names.Add(name);
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
			if (index < 0 || index >= values.Count)
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

		object IPWArray.this[int index]
		{
			get { return this[index]; }
			set { this[index] = (T)value; }
		}
	
		public IEnumerator< T > GetEnumerator()
		{
			return values.GetEnumerator();
		}
	
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		List<object> IPWArray.GetGenericValues()
		{
			return values.Cast< object >().ToList();
		}

		object IPWArray.At(int index)
		{
			return this.At(index);
		}

		void IPWArray.GenericAdd(object val)
		{
			this.Add((T)val);
		}

		void IPWArray.GenericAdd(object val, string name)
		{
			this.Add((T)val, name);
		}

		bool IPWArray.GenericAssignAt(int index, object val, string name, bool force)
		{
			return this.AssignAt(index, (T)val, name, force);
		}
	}
}
