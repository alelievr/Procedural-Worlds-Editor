using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PW
{
	[System.SerializableAttribute]
	public class PWValues {
		
		[SerializeField]
		public int		valuesCount;
	
		List< object > values;
		List< string > names;

		public PWValues()
		{
			if (values == null)
				values = new List< object >();
			if (names == null)
				names = new List< string >();
			while (values.Count < valuesCount)
				Add(null);
		}
	
		public int	Count
		{
			get { return values.Count; }
		}
	
		public List< T > GetValues<T>()
		{
			if (typeof(T) == typeof(object))
				return values.Cast< T >().ToList();
			return values.Where(o => o != null && o.GetType().IsAssignableFrom(typeof(T))).Select(o => (T)o).ToList();
		}

		public List< string > GetNames<T>()
		{
			List< string >	ret = new List< string >();

			for (int i = 0; i < values.Count; i++)
			{
				if (values[i] == null)
					continue ;
				if (typeof(T) == typeof(object) || values[i].GetType().IsAssignableFrom(typeof(T)))
					ret.Add(names[i]);
			}
			return (ret);
		}

		public object At(int index)
		{
			if (index < 0 || index >= values.Count)
				return null;
			return values[index];
		}
		
		public string NameAt(int index)
		{
			if (index < 0 || index >= names.Count)
				return null;
			return names[index];
		}
	
		public void Add(object val)
		{
			valuesCount++;
			values.Add(val);
			names.Add(null);
		}

		public bool AssignAt(int index, object val, string name, bool force = false)
		{
			if (index >= values.Count)
			{
				if (force)
				{
					while (index >= values.Count)
						values.Add(null);
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

			valuesCount--;
			values.RemoveAt(index);
			names.RemoveAt(index);
			return true;
		}
	}
}