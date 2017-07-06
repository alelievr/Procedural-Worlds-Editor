using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Node
{
	public class PWNodeCurve : PWNode {

		[PWInput("in")]
		public Sampler		inputTerrain;

		[PWOutput("out")]
		public Sampler		outputTerrain;

		AnimationCurve		curve;
		[SerializeField]
		SerializableAnimationCurve	sCurve = new SerializableAnimationCurve();
		Sampler				samplerPreview;
	
		public override void OnNodeCreate()
		{
			externalName = "Curve";
			curve = (AnimationCurve)sCurve;
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(EditorGUIUtility.singleLineHeight * 1.2f);
			EditorGUI.BeginChangeCheck();
			Rect pos = EditorGUILayout.GetControlRect(false, 100);
			curve = EditorGUI.CurveField(pos, curve);
			if (EditorGUI.EndChangeCheck())
			{
				notifyDataChanged = true;
				sCurve.SetAnimationCurve(curve);
			}

			//TODO: Duplicate the sampler to save it's state and display it
			if (inputTerrain != null)
			{
				if (inputTerrain.type == SamplerType.Sampler2D)
				{
					PWGUI.Sampler2DPreview(inputTerrain as Sampler2D, needUpdate);
				}
				else
				{

				}
			}
		}

		public override void OnNodeProcess()
		{
			outputTerrain = inputTerrain;

			if (!needUpdate)
				return ;
			
			if (inputTerrain.type == SamplerType.Sampler2D)
			{
				(inputTerrain as Sampler2D).Foreach((x, y, val) => {
					val = curve.Evaluate(val);
				});
			}
		}
		
	}
}