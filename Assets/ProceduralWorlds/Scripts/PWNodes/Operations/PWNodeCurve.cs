using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Node
{
	public class PWNodeCurve : PWNode
	{

		//maybe a function to change the visibility when user is dragging a link of Biome type ?
		//or a button to witch the node type

		[PWInput("Terrain input")]
		public Sampler		inputTerrain;

		[PWOutput("Terrain output")]
		public Sampler		outputTerrain;

		public AnimationCurve		curve;
		[SerializeField]
		SerializableAnimationCurve	sCurve = new SerializableAnimationCurve();

		string notifyKey = "curveModify";

		public override void OnNodeCreation()
		{
			name = "Curve";
			curve = (AnimationCurve)sCurve;
		}

		public override void OnNodeEnable()
		{
			delayedChanges.BindCallback(notifyKey, (unused) => {
					NotifyReload();
					CurveTerrain();
					sCurve.SetAnimationCurve(curve);
				});
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(EditorGUIUtility.singleLineHeight * 1.2f);
			EditorGUI.BeginChangeCheck();
			Rect pos = EditorGUILayout.GetControlRect(false, 100);
			curve = EditorGUI.CurveField(pos, curve);
			if (EditorGUI.EndChangeCheck())
				delayedChanges.UpdateValue(notifyKey);

			PWGUI.SamplerPreview(outputTerrain);
		}

		void					CurveTerrain()
		{
			if (inputTerrain == null)
				return ;
			
			Sampler samp = inputTerrain.Clone(outputTerrain);

			if (samp.type == SamplerType.Sampler2D)
			{
				(samp as Sampler2D).Foreach((x, y, val) => {
					return curve.Evaluate(val);
				});
			}
			else if (samp.type == SamplerType.Sampler3D)
			{
				(samp as Sampler3D).Foreach((x, y, z, val) => {
					return curve.Evaluate(val);
				});
			}

			outputTerrain = samp;
		}

		public override void	OnNodeProcess()
		{
			if (inputTerrain == null)
			{
				Debug.LogError("[PWNodeCurve] null inputTerrain received in input !");
				return ;
			}
			
			CurveTerrain();
		}
		
	}
}