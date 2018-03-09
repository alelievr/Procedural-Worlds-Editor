using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using PW.Biomator;
using System.Linq;
using PW.Core;

namespace PW.Editor
{
	public class BiomeSwitchListDrawer : PWDrawer
	{
		const int				previewTextureWidth = 200;
		const int				previewTextureHeight = 40;

		ReorderableList			reorderableSwitchDataList;

		BiomeSwitchList			bsl;
		List< BiomeSwitchData >	switchDatas;
		
		float					localCoveragePercent;
		Texture2D				biomeRepartitionPreview;

		public override void OnEnable()
		{
			bsl = target as BiomeSwitchList;
			switchDatas = bsl.switchDatas;

			reorderableSwitchDataList = new ReorderableList(bsl.switchDatas, typeof(BiomeSwitchData), true, true, true, true);

			reorderableSwitchDataList.elementHeight = EditorGUIUtility.singleLineHeight * 2 + 4; //padding
			
            reorderableSwitchDataList.drawElementCallback = DrawElementCallback;

			reorderableSwitchDataList.drawHeaderCallback = (rect) => {
				EditorGUI.LabelField(rect, "switches");
			};

			reorderableSwitchDataList.onReorderCallback += (ReorderableList l) => {
				if (bsl.OnBiomeDataReordered != null)
					bsl.OnBiomeDataReordered();
			};

			reorderableSwitchDataList.onAddCallback += (ReorderableList l) => {
				BiomeSwitchData	bsd = new BiomeSwitchData(bsl.sampler, bsl.samplerName);
				BiomeSwitchData lastSwitch = switchDatas.Last();
				bsd.min = lastSwitch.min + (lastSwitch.max - lastSwitch.min) / 2;
				switchDatas.Add(bsd);

				if (bsl.OnBiomeDataAdded != null)
					bsl.OnBiomeDataAdded(bsd);
			};

			reorderableSwitchDataList.onRemoveCallback += (ReorderableList l) => {
				if (switchDatas.Count > 1)
				{
					switchDatas.RemoveAt(l.index);

					if (bsl.OnBiomeDataRemoved != null)
						bsl.OnBiomeDataRemoved();
				}
			};

			//add at least one biome switch:
			if (switchDatas.Count == 0)
				switchDatas.Add(new BiomeSwitchData(bsl.sampler, bsl.samplerName));
		}

		public void OnGUI(BiomeData biomeData)
		{
			base.OnGUI(new Rect());

			if (biomeRepartitionPreview == null)
			{
				biomeRepartitionPreview = new Texture2D(previewTextureWidth, 1);
				UpdateBiomeRepartitionPreview(biomeData);
			}

			using (DefaultGUISkin.Get())
			{
				reorderableSwitchDataList.DoLayoutList();
			}

			EditorGUILayout.LabelField("repartition map: (" + localCoveragePercent.ToString("F1") + "%)");
			Rect previewRect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(0));
			
			previewRect.height = previewTextureHeight;
			GUILayout.Space(previewTextureHeight);

			PWGUI.TexturePreview(previewRect, biomeRepartitionPreview, false);
			PWGUI.SetScaleModeForField(PWGUIFieldType.Sampler2DPreview, -1, ScaleMode.StretchToFill);
		}
		
		void DrawElementCallback(Rect rect, int index, bool isActive, bool selected)
		{
			BiomeSwitchData elem = switchDatas[index];

			rect.y += 2;
			int		floatFieldSize = 70;
			int		colorFieldSize = 20;
			int		nameFieldSize = (int)rect.width - colorFieldSize - 2;
			float	lineHeight = EditorGUIUtility.singleLineHeight;
			Rect	nameRect = new Rect(rect.x, rect.y, nameFieldSize, EditorGUIUtility.singleLineHeight);
			Rect	colorFieldRect = new Rect(rect.x + nameFieldSize + 4, rect.y - 2, colorFieldSize, colorFieldSize);
			Rect	minRect = new Rect(rect.x, rect.y + lineHeight + 2, floatFieldSize, EditorGUIUtility.singleLineHeight);
			Rect	maxRect = new Rect(rect.x + floatFieldSize, rect.y + lineHeight + 2, floatFieldSize, EditorGUIUtility.singleLineHeight);

			EditorGUIUtility.labelWidth = 25;
			EditorGUI.BeginChangeCheck();
			{
				float oldMin = elem.min;
				float oldMax = elem.max;

				bool first = index == 0;
				bool last = index == reorderableSwitchDataList.count - 1;
				
				PWGUI.ColorPicker(colorFieldRect, ref elem.color, false, true);
				elem.name = EditorGUI.TextField(nameRect, elem.name);
				EditorGUI.BeginDisabledGroup(first);
				elem.min = EditorGUI.FloatField(minRect, "min", elem.min);
				EditorGUI.EndDisabledGroup();
				EditorGUI.BeginDisabledGroup(last);
				elem.max = EditorGUI.FloatField(maxRect, "max", elem.max);
				EditorGUI.EndDisabledGroup();

				if (last)
					elem.max = bsl.relativeMax;
				if (first)
					elem.min = bsl.relativeMin;

				elem.min = Mathf.Max(elem.min, bsl.relativeMin);
				elem.max = Mathf.Min(elem.max, bsl.relativeMax);

				//affect up/down cell value
				if (elem.min != oldMin && index > 0)
					switchDatas[index - 1].max = elem.min;
				if (elem.max != oldMax && index + 1 < switchDatas.Count)
					switchDatas[index + 1].min = elem.max;
			}
			if (EditorGUI.EndChangeCheck() && bsl.OnBiomeDataModified != null)
				bsl.OnBiomeDataModified(elem);
			EditorGUIUtility.labelWidth = 0;

			switchDatas[index] = elem;
		}

		public void UpdateBiomeRepartitionPreview(BiomeData biomeData)
		{
			if (bsl.sampler == null || biomeRepartitionPreview == null)
				return ;
			
			float min = bsl.sampler.min;
			float max = bsl.sampler.max;
			float range = max - min;

			//clear the current texture:
			for (int x = 0; x < previewTextureWidth; x++)
				biomeRepartitionPreview.SetPixel(x, 0, Color.white);

			localCoveragePercent = 0;
			int		i = 0;

			foreach (var switchData in switchDatas)
			{
				float switchMin = Mathf.Max(switchData.min, min);
				float switchMax = Mathf.Min(switchData.max, max);
				float rMin = ((switchMin - min) / range) * previewTextureWidth;
				float rMax = ((switchMax - min) / range) * previewTextureWidth;
				localCoveragePercent += (rMax - rMin) / previewTextureWidth * 100;

				//Clamp values to image size:
				rMin = Mathf.Clamp(rMin, 0, biomeRepartitionPreview.width);
				rMax = Mathf.Clamp(rMax, 0, biomeRepartitionPreview.width);

				for (int x = (int)rMin; x < (int)rMax; x++)
					biomeRepartitionPreview.SetPixel(x, 0, switchData.color);
				i++;
			}

			//add water if there is and if switch mode is height:
			if (!biomeData.isWaterless && bsl.samplerName == BiomeSamplerName.terrainHeight)
			{
				float rMax = (biomeData.waterLevel / range) * previewTextureWidth;

				rMax = Mathf.Clamp(rMax, 0, biomeRepartitionPreview.width);

				for (int x = 0; x < rMax; x++)
					biomeRepartitionPreview.SetPixel(x, 0, Color.blue);
			}

			biomeRepartitionPreview.Apply();
		}
	}
}