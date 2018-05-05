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
		public delegate U ChildFieldGetter< in T, U >(T node);
		public delegate void ChildFieldSetter< in T, in U >(T node, U value);

		#if NET_4_6
			static readonly bool	fastReflection = true;
		#else
			static readonly bool	fastReflection = false;
		#endif

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
			ChildFieldGetter< T, U > getter;
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
				getter = (ChildFieldGetter< T, U >)d;
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
			FieldInfo fi = childType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			//Create a specific type from Field which will cast the generic type to a specific one to call the generated delegate
			var callerType = typeof(Field<,>).MakeGenericType(new[] { childType, fi.FieldType });

			//Instantiate this type and bind the delegate
			var genericField = Activator.CreateInstance(callerType) as GenericField;

			genericField.SetField(fi);
			if (fastReflection)
			{
				genericField.SetGetterDelegate(CreateGenericGetterDelegate(childType, fi));
				genericField.SetSetterDelegate(CreateGenericSetterDelegate(childType, fi));
			}

			return genericField;
		}

		public static Delegate CreateGenericGetterDelegate(Type childType, FieldInfo field)
		{
			//Create the delegate type that takes our node type in parameter
			var delegateType = typeof(ChildFieldGetter<,>).MakeGenericType(new[] { childType, field.FieldType });

			#if NET_4_6
				ParameterExpression targetExp = Expression.Parameter(childType, "target");

				// Expression.Property can be used here as well
				MemberExpression fieldExp = Expression.Field(targetExp, field);
				Expression convert = Expression.Convert(fieldExp, field.FieldType);

				var l = Expression.Lambda(delegateType, convert, targetExp).Compile();
				return l;
			#else
	
				//Create a new method which return the field field
				DynamicMethod dm = new DynamicMethod("Get" + field.Name, typeof(object), new[] { childType }, childType);
				ILGenerator il = dm.GetILGenerator();
				// Load the instance of the object (argument 0) onto the stack
				il.Emit(OpCodes.Ldarg_0);
				// Load the value of the object's field (field) onto the stack
				il.Emit(OpCodes.Ldfld, field);
				// return the value on the top of the stack
				il.Emit(OpCodes.Ret);
	
				return dm.CreateDelegate(delegateType);
			#endif
		}

		public static Delegate CreateGenericSetterDelegate(Type childType, FieldInfo field)
		{
			//Create the delegate type that takes our node type in parameter
			var delegateType = typeof(ChildFieldSetter<,>).MakeGenericType(new[] { childType, field.FieldType });

			#if NET_4_6
				ParameterExpression targetExp = Expression.Parameter(childType, "target");
				ParameterExpression valueExp = Expression.Parameter(field.FieldType, "value");

				MemberExpression fieldExp = Expression.Field(targetExp, field);
				BinaryExpression assignExp = Expression.Assign(fieldExp, valueExp);

				return Expression.Lambda(delegateType, assignExp, targetExp, valueExp).Compile();
			#else
				//Create a new method which return the field field
				DynamicMethod dm = new DynamicMethod("Set" + field.Name, typeof(object), new[] { childType, typeof(object) }, true);
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