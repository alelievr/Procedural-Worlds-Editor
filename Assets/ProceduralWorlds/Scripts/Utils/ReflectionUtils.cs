using UnityEngine;
using System.Reflection;
using ProceduralWorlds;
using System;
using System.Reflection.Emit;
using System.Linq.Expressions;

using Object = UnityEngine.Object;

namespace ProceduralWorlds.Core
{
	public static class ReflectionUtils
	{
		public delegate object ChildFieldGetter< in T >(T node);
		public delegate object ChildFieldSetter< in T, U >(T node, U value);

		static readonly bool	fastReflection = false;

		public interface GenericField
		{
			object GetValue(Object node);
			void SetValue(Object node, object value);
			void SetField(FieldInfo field);
			void SetSetterDelegate(Delegate d);
			void SetGetterDelegate(Delegate d);
		}

		public class Field< T, U > : GenericField where T : Object
		{
			ChildFieldGetter< T > getter;
			ChildFieldSetter< T, U > setter;
			FieldInfo		field;

			public void SetField(FieldInfo field)
			{
				this.field = field;
			}

			public void SetSetterDelegate(Delegate d)
			{
				setter = (ChildFieldSetter< T, U >)d;
			}
			
			public void SetGetterDelegate(Delegate d)
			{
					getter = (ChildFieldGetter< T >)d;
			}

			public object GetValue(Object node)
			{
				if (fastReflection)
					return getter(node as T);
				else
					return field.GetValue(node);
			}

			public void SetValue(Object node, object value)
			{
				if (fastReflection)
					setter(node as T, (U)value);
				else
					field.SetValue(node, value);
			}
		}

		public static GenericField CreateGenericField(Type childType, string fieldName)
		{
			FieldInfo fi = childType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance);

			//Create a specific type from Field which will cast the generic type to a specific one to call the generated delegate
			var callerType = typeof(Field<,>).MakeGenericType(new Type[] { childType, fi.FieldType });

			//Instantiate this type and bind the delegate
			var genericField = Activator.CreateInstance(callerType) as GenericField;

			genericField.SetField(fi);
			// genericField.SetGetterDelegate(CreateGenericGetterDelegate(childType, fi));
			// genericField.SetSetterDelegate(CreateGenericSetterDelegate(childType, fi));

			return genericField;
		}

		public static Delegate CreateGenericGetterDelegate(Type childType, FieldInfo field)
		{
			//Create the delegate type that takes our node type in parameter
			var delegateType = typeof(ChildFieldGetter<>).MakeGenericType(new Type[] { childType });

			//Get the child field from base class
			FieldInfo fi = childType.GetField(field.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance);

			#if NET_4_6
				ParameterExpression targetExp = Expression.Parameter(childType, "target");

				// Expression.Property can be used here as well
				MemberExpression fieldExp = Expression.Field(targetExp, fi);

				var l = Expression.Lambda(delegateType, fieldExp, targetExp).Compile();
				Debug.Log("here: " + l);
				return l;
			#else
	
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
			#endif
		}

		public static Delegate CreateGenericSetterDelegate(Type childType, FieldInfo field)
		{
			//Create the delegate type that takes our node type in parameter
			var delegateType = typeof(ChildFieldSetter<,>).MakeGenericType(new Type[] { childType, field.FieldType });

			//Get the child field from base class
			FieldInfo fi = childType.GetField(field.Name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance);

			#if NET_4_6
				ParameterExpression targetExp = Expression.Parameter(childType, "target");
				ParameterExpression valueExp = Expression.Parameter(fi.FieldType, "value");

				// Expression.Property can be used here as well
				MemberExpression fieldExp = Expression.Field(targetExp, fi);
				BinaryExpression assignExp = Expression.Assign(fieldExp, valueExp);

				return Expression.Lambda(delegateType, assignExp, targetExp, valueExp).Compile();
			#else
				//Create a new method which return the field fi
				DynamicMethod dm = new DynamicMethod("Set" + fi.Name, typeof(object), new Type[] { childType, typeof(object) }, true);
				ILGenerator il = dm.GetILGenerator();
				// Load the instance of the object (argument 0) and the replacing value onto the stack
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldarg_1);
				if (fi.FieldType.IsValueType)
					il.Emit(OpCodes.Unbox_Any, fi.FieldType);
				// Set the value of the field to the value on top of the stack
				il.Emit(OpCodes.Stfld, fi);
				il.Emit(OpCodes.Ret);
	
				return dm.CreateDelegate(delegateType);
			#endif
		}
	}
}