using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;
using System.Reflection;

namespace ProceduralWorlds.Editor
{
	public class UndoRedoHelper
	{
		Object target;

		public List< ReflectionUtils.GenericField > undoableFields = new List< ReflectionUtils.GenericField >();

		List< object > beforeUndoFields;
		List< object > afterUndoFields;
		List< int > beforeUndoHashes;
		List< int > afterUndoHashes;

		public UndoRedoHelper(Object targetToUndo)
		{
			target = targetToUndo;
		}

		public void LoadUndoableFields()
		{
			System.Reflection.FieldInfo[] fInfos = target.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

			undoableFields.Clear();
			
			foreach (var fInfo in fInfos)
			{
				var attrs = fInfo.GetCustomAttributes(false);

				bool hasSerializeField = false;
				bool hasNonSerialized = false;

				foreach (var attr in attrs)
				{
					if (attr is InputAttribute || attr is OutputAttribute)
						goto skipThisField;
					
					if (attr is System.NonSerializedAttribute)
						hasNonSerialized = true;
					
					if (attr is SerializeField)
						hasSerializeField = true;
				}

				if (fInfo.IsPrivate && !hasSerializeField)
					goto skipThisField;
				
				if (hasNonSerialized)
					goto skipThisField;
				
				if (fInfo.IsNotSerialized)
					goto skipThisField;
				
				undoableFields.Add(ReflectionUtils.CreateGenericField(target.GetType(), fInfo.Name));

				skipThisField:
				continue ;
			}
		}

		public void LoadFields(params string[] fieldNames)
		{
			foreach (var fieldName in fieldNames)
				undoableFields.Add(ReflectionUtils.CreateGenericField(target.GetType(), fieldName));
		}
	
		#if UNITY_EDITOR

		public void Beign()
		{
			TakeSnapshot(ref beforeUndoFields, ref beforeUndoHashes);
		}

		public void End()
		{
			TakeSnapshot(ref afterUndoFields, ref afterUndoHashes);

			if (SnapshotDiffers())
			{
				RestoreSnapshot(beforeUndoFields);

				UnityEditor.Undo.RecordObject(target, "Property updated in " + target.name);
				
				RestoreSnapshot(afterUndoFields);
			}
		}

		void TakeSnapshot(ref List< object > buffer, ref List< int > hashes)
		{
			if (buffer == null)
				buffer = new List< object >(new object[undoableFields.Count]);
			if (hashes == null)
				hashes = new List< int >(new int[undoableFields.Count]);
			
			for (int i = 0; i < undoableFields.Count; i++)
			{
				buffer[i] = undoableFields[i].GetValue(target);

				hashes[i] = (buffer[i] != null) ? buffer[i].GetHashCode() : 0;
			}
		}

		void RestoreSnapshot(List< object > buffer)
		{
			for (int i = 0; i < undoableFields.Count; i++)
				undoableFields[i].SetValue(target, buffer[i]);
		}
		
		bool SnapshotDiffers()
		{
			if (beforeUndoFields.Count != afterUndoFields.Count)
				return true;
			
			for (int i = 0; i < beforeUndoFields.Count; i++)
			{
				if (beforeUndoHashes[i] != afterUndoHashes[i])
					return true;
				
				var p1 = beforeUndoFields[i];
				var p2 = afterUndoFields[i];

				if (p1 == null || p2 == null)
					continue ;

				if (!p1.Equals(p2))
					return true;
			}

			return false;
		}

		#endif
	}
}