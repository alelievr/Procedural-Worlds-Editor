using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PW
{
	public class PWNodeAdd : PWNode {
	
		[PWMultiple(2, typeof(float), typeof(int), typeof(Vector2), typeof(Vector3), typeof(Vector4))]
		[PWInput("A")]
		public PWValues	values = new PWValues();
	
		[PWOutput("flt")]
		public float	fOutput;
		[PWOutput("vec2")]
		public Vector2	v2Output;
		[PWOutput("vec3")]
		public Vector3	v3Output;
		[PWOutput("vec4")]
		public Vector4	v4Output;
		
		List< int >		ints;
		List< float >	floats;
		List< Vector2 >	vec2s;
		List< Vector3 >	vec3s;
		List< Vector4 >	vec4s;
	
		bool			intify = false;
	
		public override void OnNodeCreate()
		{
			//override window width
			windowRect.width = 150;
		}
	
		void			HideOutputExcept(string propName)
		{
			foreach (var prop in new string[]{"fOutput", "v2Output", "v3Output", "v4Output"})
				UpdatePropVisibility(prop, (prop == propName) ? PWVisibility.Visible : PWVisibility.Gone);
		}
	
		string			GetModeName()
		{
			if (vec4s.Count != 0)
				return ("Vector4");
			if (vec3s.Count != 0)
				return ("Vector3");
			if (vec2s.Count != 0)
				return ("Vector2");
			return ("float");
		}

		void DisplayResult< T >(T result)
		{
			EditorGUIUtility.labelWidth = 100;
			EditorGUILayout.LabelField("result: " + result);
		}
	
		public override void OnNodeGUI()
		{
			ints = values.GetValues< int >();
			floats = values.GetValues< float >();
			vec2s = values.GetValues< Vector2 >();
			vec3s = values.GetValues< Vector3 >();
			vec4s = values.GetValues< Vector4 >();
			
			EditorGUILayout.LabelField("Mode: " + GetModeName());
	
			EditorGUIUtility.labelWidth = 100;
			intify = EditorGUILayout.Toggle("Integer round", intify);

			if (vec4s.Count != 0)
				DisplayResult(v4Output);
			else if (vec3s.Count != 0)
				DisplayResult(v3Output);
			else if (vec2s.Count != 0)
				DisplayResult(v2Output);
			else
				DisplayResult(fOutput);
		}

		public override void OnNodeProcess()
		{
			ints = values.GetValues< int >();
			floats = values.GetValues< float >();
			vec2s = values.GetValues< Vector2 >();
			vec3s = values.GetValues< Vector3 >();
			vec4s = values.GetValues< Vector4 >();
			
			//check nominal type:

			fOutput = 0;
			v2Output = Vector2.zero;
			v3Output = Vector3.zero;
			v4Output = Vector4.zero;
	
			if (vec4s.Count != 0)
			{
				HideOutputExcept("v4Output");
				foreach (var vec4 in vec4s)
					v4Output += vec4;
				foreach (var vec3 in vec3s)
					v4Output += (Vector4)vec3;
				foreach (var vec2 in vec2s)
					v4Output += (Vector4)vec2;
				foreach (var flt in floats)
					v4Output += new Vector4(flt, flt, flt, flt);
				foreach (var integer in ints)
					v4Output += new Vector4(integer, integer, integer, integer);
				if (intify)
					v4Output = PWUtils.Round(v4Output);
			}
			else if (vec3s.Count != 0)
			{
				HideOutputExcept("v3Output");
				foreach (var vec3 in vec3s)
					v3Output += (Vector3)vec3;
				foreach (var vec2 in vec2s)
					v3Output += (Vector3)vec2;
				foreach (var flt in floats)
					v3Output += new Vector3(flt, flt, flt);
				foreach (var integer in ints)
					v3Output += new Vector3(integer, integer, integer);
				if (intify)
					v3Output = PWUtils.Round(v3Output);
			}
			else if (vec2s.Count != 0)
			{
				HideOutputExcept("v2Output");
				foreach (var vec2 in vec2s)
					v2Output += (Vector2)vec2;
				foreach (var flt in floats)
					v2Output += new Vector2(flt, flt);
				foreach (var integer in ints)
					v2Output += new Vector2(integer, integer);
				if (intify)
					v2Output = PWUtils.Round(v2Output);
			}
			else //int and floats
			{
				HideOutputExcept("fOutput");
				foreach (var flt in floats)
					fOutput += flt;
				foreach (var integer in ints)
					fOutput += integer;
				if (intify)
					fOutput = Mathf.Round(fOutput);
			}
		}
	}
}