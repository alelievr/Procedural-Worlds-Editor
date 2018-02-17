using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PW.Core;
using UnityEditorInternal;
using System;

namespace PW.Biomator
{
	[System.Serializable]
	public class BiomeSwitchData
	{
		public float				min;
		public float				max;
		public string				name;
		public float				absoluteMax; //max value in the map
		public float				absoluteMin; //min value in the map
		public string				samplerName;
		public SerializableColor	color;

		public BiomeSwitchData(Sampler samp, string samplerName)
		{
			UpdateSampler(samp);
			name = "swampland";
			min = 70;
			max = 90;
			this.samplerName = samplerName;
			var rand = new System.Random();
			color = (SerializableColor)new Color((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble());
		}

		public void UpdateSampler(Sampler samp)
		{
			if (samp != null)
			{
				absoluteMax = samp.max;
				absoluteMin = samp.min;
			}
		}

		public BiomeSwitchData(string samplerName) : this(null, samplerName) {}
	}

	[System.Serializable]
	public class BiomeSwitchList : IEnumerable< BiomeSwitchData >
	{
		public List< BiomeSwitchData >		switchDatas = new List< BiomeSwitchData >();

		public Action< BiomeSwitchData >	OnBiomeDataAdded;
		public Action						OnBiomeDataRemoved;
		public Action						OnBiomeDataReordered;
		public Action< BiomeSwitchData >	OnBiomeDataModified;

		public Sampler						currentSampler;
		public string						currentSamplerName;
		public BiomeData					currentBiomeData;
		
		Texture2D			biomeRepartitionPreview;
		float				localCoveragePercent;
		const int			previewTextureWidth = 200;
		const int			previewTextureHeight = 40;
		
		float				relativeMin = float.MinValue;
		float				relativeMax = float.MaxValue;

		bool				updatePreview;

		PWGUIManager		PWGUI;
		
		ReorderableList		reorderableSwitchDataList;

		public int			Count { get { return switchDatas.Count; } }

		public void OnEnable()
		{
			PWGUI = new PWGUIManager();

			reorderableSwitchDataList = new ReorderableList(switchDatas, typeof(BiomeSwitchData), true, true, true, true);

			reorderableSwitchDataList.elementHeight = EditorGUIUtility.singleLineHeight * 2 + 4; //padding
			
            reorderableSwitchDataList.drawElementCallback = DrawElementCallback;

			reorderableSwitchDataList.drawHeaderCallback = (rect) => {
				EditorGUI.LabelField(rect, "switches");
			};

			reorderableSwitchDataList.onReorderCallback += (ReorderableList l) => {
				OnBiomeDataReordered();
			};

			reorderableSwitchDataList.onAddCallback += (ReorderableList l) => {
				BiomeSwitchData	bsd = new BiomeSwitchData(currentSampler, currentSamplerName);
				switchDatas.Add(bsd);
				OnBiomeDataAdded(bsd);
				updatePreview = true;
			};

			reorderableSwitchDataList.onRemoveCallback += (ReorderableList l) => {
				if (switchDatas.Count > 1)
				{
					switchDatas.RemoveAt(l.index);
					OnBiomeDataRemoved();
					updatePreview = true;
				}
			};

			//add at least one biome switch:
			if (switchDatas.Count == 0)
				switchDatas.Add(new BiomeSwitchData(currentSampler, currentSamplerName));
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
				{
					if (elem.max != relativeMax)
						GUI.changed = true;
					elem.max = relativeMax;
				}
				if (first)
				{
					if (elem.min != relativeMin)
						GUI.changed = true;
					elem.min = relativeMin;
				}

				elem.min = Mathf.Max(elem.min, relativeMin);
				elem.max = Mathf.Min(elem.max, relativeMax);

				//affect up/down cell value
				if (elem.min != oldMin && index > 0)
					switchDatas[index - 1].max = elem.min;
				if (elem.max != oldMax && index + 1 < switchDatas.Count)
					switchDatas[index + 1].min = elem.max;
			}
			if (EditorGUI.EndChangeCheck())
			{
				OnBiomeDataModified(elem);
				updatePreview = true;
			}
			EditorGUIUtility.labelWidth = 0;
			
			elem.UpdateSampler(currentSampler);

			switchDatas[index] = elem;
		}

		public void OnGUI()
		{
			PWGUI.StartFrame(new Rect(0, 0, 0, 0));

			if (biomeRepartitionPreview == null)
				biomeRepartitionPreview = new Texture2D(previewTextureWidth, 1);

			using (new DefaultGUISkin())
			{
				reorderableSwitchDataList.DoLayoutList();
			}

			if (updatePreview && currentSampler != null)
				UpdateBiomeRepartitionPreview();

			EditorGUILayout.LabelField("repartition map: (" + localCoveragePercent.ToString("F1") + "%)");
			Rect previewRect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true), GUILayout.Height(0));
			
			previewRect.height = previewTextureHeight;
			GUILayout.Space(previewTextureHeight);

			PWGUI.TexturePreview(previewRect, biomeRepartitionPreview, false);
			PWGUI.SetScaleModeForField(-1, ScaleMode.StretchToFill);
		}

		public void UpdateBiomeRepartitionPreview()
		{
			if (currentSampler == null || biomeRepartitionPreview == null)
				return ;
			
			float min = currentSampler.min;
			float max = currentSampler.max;
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
			if (!currentBiomeData.isWaterless && currentSamplerName == BiomeSamplerName.terrainHeight)
			{
				float rMax = (currentBiomeData.waterLevel / range) * previewTextureWidth;

				rMax = Mathf.Clamp(rMax, 0, biomeRepartitionPreview.width);

				for (int x = 0; x < rMax; x++)
					biomeRepartitionPreview.SetPixel(x, 0, Color.blue);
			}

			biomeRepartitionPreview.Apply();

			updatePreview = false;
		}

		public void UpdateMinMax(float min, float max)
		{
			this.relativeMin = min;
			this.relativeMax = max;
		}

		public void UpdateSwitchMode(string samplerName)
		{
			this.currentSamplerName = samplerName;

			foreach (var switchData in switchDatas)
				switchData.samplerName = samplerName;
		}

		IEnumerator< BiomeSwitchData > IEnumerable< BiomeSwitchData >.GetEnumerator()
		{
			foreach (var sw in switchDatas)
				yield return sw;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			yield return switchDatas;
		}

		public BiomeSwitchData this[int index]
		{
			get { return switchDatas[index]; }
		}
	}
}