using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PW.Core
{
	[System.SerializableAttribute]
	public class PWValue {
		
		[SerializeField]
		object	value;

		/// Use it to retreive the value of a non-nullable type
		public T? Get< T >() where T : struct
		{
			if (value.GetType().IsAssignableFrom(typeof(T)))
				return (T)value;
			return null;
		}

		/// Use it to retreive the value of a nullable type
		public T GetNullable< T >() where T : class
		{
			if (value.GetType().IsAssignableFrom(typeof(T)))
				return (T)value;
			return null;
		}
	}
}
