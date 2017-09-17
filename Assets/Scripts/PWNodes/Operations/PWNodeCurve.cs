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
		[PWNotRequired]
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
			bool updatePreview = false;
			GUILayout.Space(EditorGUIUtility.singleLineHeight * 1.2f);
			EditorGUI.BeginChangeCheck();
			Rect pos = EditorGUILayout.GetControlRect(false, 100);
			curve = EditorGUI.CurveField(pos, curve);
			if (EditorGUI.EndChangeCheck())
			{
				updatePreview = true;
				notifyDataChanged = true;
				UpdateTerrain();
				sCurve.SetAnimationCurve(curve);
			}

			if (inputTerrain != null)
			{
				if (inputTerrain.type == SamplerType.Sampler2D)
					PWGUI.Sampler2DPreview(outputTerrain as Sampler2D, needUpdate || updatePreview);
				else
				{
				}
			}
		}

		void					CurveTerrain(Sampler input, Sampler output)
		{
			if (input.type == SamplerType.Sampler2D)
			{
				(output as Sampler2D).Foreach((x, y, val) => {
					return curve.Evaluate((input as Sampler2D)[x, y]);
				});
			}
			else
			{
				//TODO
			}
		}

		void					UpdateTerrain()
		{
			if (outputTerrain == null)
				return ;

			if (inputTerrain != null)
				CurveTerrain(inputTerrain, outputTerrain);
		}

		public override void	OnNodeProcess()
		{
			if (inputTerrain != null)
				PWUtils.ResizeSamplerIfNeeded(inputTerrain, ref outputTerrain);
			
			if (!needUpdate)
				return ;
			
			UpdateTerrain();
		}
		
	}
}