using UnityEngine;
using System.Reflection;
using ProceduralWorlds;
using System;
using System.Reflection.Emit;

using System.Linq.Expressions;
namespace ProceduralWorlds.Core
{
	public static class ReflectionUtils
	{
		public delegate object ChildFieldGetter< T >(T node);
		public delegate object ChildFieldSetter< T >(T node, object value);

		public interface GenericField
		{
			object GetValue(BaseNode node);
			void SetValue(BaseNode node, object value);
			void SetSetterDelegate(Delegate d);
			void SetGetterDelegate(Delegate d);
		}

		public class Field< T > : GenericField where T : BaseNode
		{
			ChildFieldGetter< T > getter;
			ChildFieldSetter< T > setter;

			public void SetSetterDelegate(Delegate d)
			{
				setter = (ChildFieldSetter< T >)d;
			}
			
			public void SetGetterDelegate(Delegate d)
			{
				getter = (ChildFieldGetter< T >)d;
			}

			public object GetValue(BaseNode node)
			{
				return getter(node as T);
			}

			public void SetValue(BaseNode node, object value)
			{
				setter(node as T, value);
			}
		}

		public static GenericField CreateGenericField(Type childType, string fieldName)
		{
			//Create a specific type from Field which will cast the generic type to a specific one to call the generated delegate
			var callerType = typeof(Field<>).MakeGenericType(new Type[] { childType });

			//Instantiate this type and bind the delegate
			var genericField = Activator.CreateInstance(callerType) as GenericField;

			genericField.SetGetterDelegate(CreateGenericGetterDelegate(childType, fieldName));
			// genericField.SetSetterDelegate(CreateGenericSetterDelegate(childType, fieldName));

			return genericField;
		}

		public static Delegate CreateGenericGetterDelegate(Type childType, string fieldName)
		{
			//Create the delegate type that takes our node type in parameter
			var delegateType = typeof(ChildFieldGetter<>).MakeGenericType(new Type[] { childType });

			//Get the child field from base class
			FieldInfo fi = childType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance);

			//Create a new method which return the field fi
			DynamicMethod dm = new DynamicMethod("Get" + fi.Name, typeof(object), new Type[] { childType }, childType);
			ILGenerator il = dm.GetILGenerator();
			// Load the instance of the object (argument 0) onto the stack
			il.Emit(OpCodes.Ldarg_0);
			// Load the value of the object's field (fi) onto the stack
			il.Emit(OpCodes.Ldfld, fi);
			// return the value on the top of the stack
			il.Emit(OpCodes.Ret);

			return dm.CreateDelegate(delegateType);
		}

		public static Delegate CreateGenericSetterDelegate(Type childType, string fieldName)
		{
			//Create the delegate type that takes our node type in parameter
			var delegateType = typeof(ChildFieldSetter<>).MakeGenericType(new Type[] { childType });

			//Get the child field from base class
			FieldInfo fi = childType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance);

			//Create a new method which return the field fi
			DynamicMethod dm = new DynamicMethod("Set" + fi.Name, typeof(object), new Type[] { childType, typeof(object) }, true);
			ILGenerator il = dm.GetILGenerator();
			// Load the instance of the object (argument 0) and the replacing value onto the stack
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			// Set the value of the field to the value on top of the stack
			il.Emit(OpCodes.Stfld, fi);

			return dm.CreateDelegate(delegateType);
		}
	}
}