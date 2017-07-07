using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using UnityEditorInternal;

namespace PW.Node
{
	public class PWNodeBiomeTerrain : PWNode {

		[PWInput]
		[PWMultiple(typeof(object))]
		public PWValues			inputs = new PWValues();

		[PWOutput("Terrain modifiers")]
		public BiomeTerrain		outputBiomeTerrain = new BiomeTerrain();

		ReorderableList			modifierList;
		int						listRequiredInputCout = 0;
		[SerializeField]
		int						lastRequiredInputCount;
	
		public override void OnNodeCreate()
		{
			externalName = "Biome Terrain";

			modifierList = new ReorderableList(outputBiomeTerrain.terrainModifiers, typeof(BiomeTerrainModifer), true, true, true, true);

			float	lh = EditorGUIUtility.singleLineHeight;

			modifierList.elementHeightCallback += (index) => { 
				var		elem = outputBiomeTerrain.terrainModifiers[index];

				switch (elem.type)
				{
					case BiomeTerrainModifierType.Curve:
						return 80;
					case BiomeTerrainModifierType.Max:
						return 30;
					default:
						return lh;
				}
			};

			modifierList.drawElementCallback = (rect, index, isActive, isFocus) => {
				var elem = outputBiomeTerrain.terrainModifiers[index];
				Rect typeRect = rect;
				rect.height = lh;
				elem.type = (BiomeTerrainModifierType)EditorGUI.EnumPopup(typeRect, elem.type);
				rect.y += lh + 5;

				switch (elem.type)
				{
					case BiomeTerrainModifierType.Curve:
						Rect curveRect = rect;
						curveRect.height = 56;
						elem.curve = EditorGUI.CurveField(curveRect, (AnimationCurve)elem.curve);
						break ;
					case BiomeTerrainModifierType.Max:
						UpdateMultiProp("inputs", lastRequiredInputCount);
						//TODO: stabilize rect.y
						if (Event.current.type == EventType.Repaint)
							elem.y = rect.y;
						UpdatePropVisibility("inputs", PWVisibility.Visible, listRequiredInputCout);
						UpdatePropPosition("inputs", elem.y - 10, listRequiredInputCout);
						listRequiredInputCout++;
						break ;
					default:
						break ;
				}
			};

			modifierList.drawHeaderCallback = (rect) => {
				EditorGUI.LabelField(rect, "Biome terrain modifiers");
			};

			modifierList.onReorderCallback += (l) => {
				notifyDataChanged = true;
			};

			modifierList.onAddCallback += (l) => {
				outputBiomeTerrain.terrainModifiers.Add(new BiomeTerrainModifer());
				notifyDataChanged = true;
			};

			modifierList.onRemoveCallback += (l) => {
				if (outputBiomeTerrain.terrainModifiers.Count > 1)
				{
					outputBiomeTerrain.terrainModifiers.RemoveAt(l.index);
					notifyDataChanged = true;
				}
			};
		}

		public override void OnNodeGUI()
		{
			GUILayout.Space(EditorGUIUtility.singleLineHeight);

			for (int i = 0; i < modifierList.count; i++)
				UpdatePropVisibility("inputs", PWVisibility.Invisible, i);
			listRequiredInputCout = 0;
			modifierList.DoLayoutList();
			lastRequiredInputCount = listRequiredInputCout;
		}

		public override void OnNodeProcess()
		{
		}
		
	}
}