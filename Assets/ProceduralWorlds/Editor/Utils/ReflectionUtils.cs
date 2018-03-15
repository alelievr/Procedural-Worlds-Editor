using UnityEngine;
using System.Reflection;
using ProceduralWorlds;
using System;

namespace ProceduralWorlds.Editor
{
	public static class ReflectionUtils
	{
		public delegate object ChildFieldGetter< T >(T node);

		public interface GenericCaller
		{
			object Call(BaseNode node);
			void SetDelegate(Delegate d);
		}


		public class Caller< T > : GenericCaller where T : BaseNode
		{
			ChildFieldGetter< T > getter;

			public void SetDelegate(Delegate d)
			{
				getter = (ChildFieldGetter< T >)d;
			}

			public object Call(BaseNode node)
			{
				return getter(node as T);
			}
		}
	}
}