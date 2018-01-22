using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;
using PW.Core;

namespace PW.Node
{
	public class PWNodeBiomeSurface : PWNode
	{
	
		[PWInput, PWNotRequired]
		public PWArray< BiomeSurfaceMaps >	inputSurfaces = new PWArray< BiomeSurfaceMaps >();

		[PWOutput]
		public BiomeSurfaces	surfaces = new BiomeSurfaces();

		ReorderableList			layerList;
		List< ReorderableList >	slopeLists = new List< ReorderableList >();
		//complex have all maps

		int						inputIndex;
		[SerializeField]
		BiomeSurfaceMode		mode;

		enum BiomeSurfaceMode
		{
			SingleSurface,
			LayerSurface,
			LayerAndSlopeSurface,
		}

		public override void OnNodeCreation()
		{
			name = "Biome surface";
			layerList = new ReorderableList(surfaces.biomeLayers, typeof(BiomeSurfaceLayer));

			layerList.elementHeight = EditorGUIUtility.singleLineHeight * 2 + 4;

			layerList.drawElementCallback = (rect, index, isActive, isFocused) => {
                rect.y += 2;

				var elem = surfaces.biomeLayers[index];
				EditorGUIUtility.labelWidth = 25;
				int		floatFieldSize = 70;
				int		colorFieldSize = 20;
				int		nameFieldSize = (int)rect.width - colorFieldSize - 2;
				float	lineHeight = EditorGUIUtility.singleLineHeight;
				Rect	nameRect = new Rect(rect.x, rect.y, nameFieldSize, EditorGUIUtility.singleLineHeight);
				Rect	minRect = new Rect(rect.x, rect.y + lineHeight + 2, floatFieldSize, EditorGUIUtility.singleLineHeight);
            	Rect	maxRect = new Rect(rect.x + floatFieldSize, rect.y + lineHeight + 2, floatFieldSize, EditorGUIUtility.singleLineHeight);
				
				elem.name = EditorGUI.TextField(nameRect, elem.name);
				elem.minHeight = EditorGUI.FloatField(minRect, "min", elem.minHeight);
				elem.maxHeight = EditorGUI.FloatField(maxRect, "max", elem.maxHeight);

				if (mode == BiomeSurfaceMode.LayerSurface)
				{
					if (elem.slopeMaps.Count == 0)
						elem.slopeMaps.Add(new BiomeSurfaceSlopeMaps());
					var slopeMap = elem.slopeMaps[0];
					slopeMap.minSlope = 0;
					slopeMap.maxSlope = 180;
					if (Event.current.type == EventType.Repaint)
						slopeMap.y = rect.y;
					SetMultiAnchor("inputSurfaces", surfaces.biomeLayers.Count);
					SetAnchorPosition("inputSurfaces", (int)slopeMap.y, index);
					slopeMap.surfaceMaps = inputSurfaces.At(index) as BiomeSurfaceMaps;
				}
			};

			layerList.drawHeaderCallback = (rect) => {
				EditorGUI.LabelField(rect, "Biome layers");
			};
		}

		void	AddNewSlopeList(int index)
		{
			var layer = surfaces.biomeLayers[index];
			ReorderableList r = new ReorderableList(layer.slopeMaps, typeof(BiomeSurfaceSlopeMaps));

			r.elementHeight = EditorGUIUtility.singleLineHeight * 2 + 4;
			
			r.drawElementCallback = (rect, i, isActive, isFocused) => {
				var elem = layer.slopeMaps[i];
				
				EditorGUIUtility.labelWidth = 25;
				int		floatFieldSize = 70;
				int		colorFieldSize = 20;
				int		nameFieldSize = (int)rect.width - colorFieldSize - 2;
				float	lineHeight = EditorGUIUtility.singleLineHeight;
				Rect	nameRect = new Rect(rect.x, rect.y, nameFieldSize, EditorGUIUtility.singleLineHeight);
				Rect	minRect = new Rect(rect.x, rect.y + lineHeight + 2, floatFieldSize, EditorGUIUtility.singleLineHeight);
            	Rect	maxRect = new Rect(rect.x + floatFieldSize, rect.y + lineHeight + 2, floatFieldSize, EditorGUIUtility.singleLineHeight);
				
				if (elem.surfaceMaps != null && !string.IsNullOrEmpty(elem.surfaceMaps.name))
					EditorGUI.LabelField(nameRect, elem.surfaceMaps.name);
				else
					EditorGUI.LabelField(nameRect, "unnamed");
				elem.minSlope = EditorGUI.FloatField(minRect, "min", elem.minSlope);
				elem.maxSlope = EditorGUI.FloatField(maxRect, "max", elem.maxSlope);
				
				if (Event.current.type == EventType.Repaint)
					elem.y = rect.y;

				SetAnchorPosition("inputSurfaces", (int)elem.y, i);
				SetAnchorVisibility("inputSurfaces", PWVisibility.Visible, i);
				elem.surfaceMaps = inputSurfaces.At(inputIndex) as BiomeSurfaceMaps;
				inputIndex++;
			};

			r.drawHeaderCallback = (rect) => {
				EditorGUI.LabelField(rect, "surfaces per slope");
			};
			slopeLists.Add(r);
		}

		public override void OnNodeGUI()
		{
			EditorGUIUtility.labelWidth = 80;
			mode = (BiomeSurfaceMode)EditorGUILayout.EnumPopup("Surface mode", mode);
			if (mode == BiomeSurfaceMode.SingleSurface)
			{
				if (surfaces.biomeLayers.Count == 0)
					surfaces.biomeLayers.Add(new BiomeSurfaceLayer());
				var layer = surfaces.biomeLayers[0];
				if (layer.slopeMaps.Count == 0)
					layer.slopeMaps.Add(new BiomeSurfaceSlopeMaps());
				var slope = layer.slopeMaps[0];
				SetMultiAnchor("inputSurfaces", 1);
				SetAnchorPosition("inputSurfaces", 25, 0);
				slope.surfaceMaps = inputSurfaces.At(0) as BiomeSurfaceMaps;
				return ;
			}
			//Min and Max here start from the biome height min/max if there is a switch on height
			//else, min and max refer to mapped terrain value in ToBiomeData / WaterLevel node
			layerList.DoLayoutList();

			if (mode == BiomeSurfaceMode.LayerSurface)
				return ;

			//list per slopes:
			int i = 0;
			int	slopeCount = 0;
			foreach (var layer in surfaces.biomeLayers)
				slopeCount += layer.slopeMaps.Count;
			SetMultiAnchor("inputSurfaces", slopeCount);
			foreach (var layer in surfaces.biomeLayers)
			{
				if ((layer.foldout = EditorGUILayout.Foldout(layer.foldout, layer.name)))
				{
					if (i >= slopeLists.Count)
						AddNewSlopeList(i);
					inputIndex = 0;
					slopeLists[i].DoLayoutList();
				}
				// else
					// SetAnchorVisibility("inputSurfaces", PWVisibility.Invisible, i);
				i++;
			}
		}

		//nothing to process, output already set
	}
}