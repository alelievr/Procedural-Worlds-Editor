using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;

namespace PW.Node
{
	public class PWNodeCurve : PWNode {

		//maybe a function to change the visibility when user is dragging a link of Biome type ?
		//or a button to witch the node type

		[PWInput("Terrain input")]
		[PWNotRequired, PWCopy]
		public Sampler		inputTerrain;

		[PWOutput("Terrain output")]
		public Sampler		outputTerrain;

		AnimationCurve		curve;
		[SerializeField]
		SerializableAnimationCurve	sCurve = new SerializableAnimationCurve();

		public override void OnNodeCreation()
		{
			name = "Curve";
			curve = (AnimationCurve)sCurve;
		}

		public override void OnNodeGUI()
		{
			if (inputTerrain == null)
			{
				EditorGUILayout.LabelField("Please connect the input terrain");
				return ;
			}

			GUILayout.Space(EditorGUIUtility.singleLineHeight * 1.2f);
			EditorGUI.BeginChangeCheck();
			Rect pos = EditorGUILayout.GetControlRect(false, 100);
			curve = EditorGUI.CurveField(pos, curve);
			if (EditorGUI.EndChangeCheck())
			{
				NotifyReload();
				CurveTerrain(inputTerrain);
				sCurve.SetAnimationCurve(curve);
			}

			PWGUI.SamplerPreview(outputTerrain);
		}

		void					CurveTerrain(Sampler samp)
		{
			if (inputTerrain.type == SamplerType.Sampler2D)
			{
				(samp as Sampler2D).Foreach((x, y, val) => {
					return curve.Evaluate(val);
				});
			}
			else
			{
				//TODO
			}
		}

		public override void	OnNodeProcess()
		{
			if (inputTerrain == null)
			{
				Debug.LogError("[PWNodeCurve] null inputTerrain received in input !");
				return ;
			}

			outputTerrain = inputTerrain;
			
			CurveTerrain(outputTerrain);
		}
		
	}
}