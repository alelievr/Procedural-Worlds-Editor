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
		public SerializableColor	color;

		public BiomeSwitchData(Sampler samp)
		{
			UpdateSampler(samp);
			name = "swampland";
			min = 70;
			max = 90;
			color = (SerializableColor)new Color(0.196f, 0.804f, 0.196f, 1);
		}

		public void UpdateSampler(Sampler samp)
		{
			if (samp != null)
			{
				absoluteMax = samp.max;
				absoluteMin = samp.min;
			}
		}

		public BiomeSwitchData() : this(null) {}
	}

	[System.Serializable]
	public class BiomeSwitchList
	{
		public List< BiomeSwitchData >		switchDatas = new List< BiomeSwitchData >();

		public Action< BiomeSwitchData >	OnBiomeDataAdded;
		public Action						OnBiomeDataRemoved;
		public Action						OnBiomeDataReordered;
		public Action< BiomeSwitchData >	OnBiomeDataModified;

		public Sampler						currentSampler;
		public BiomeSwitchMode				currentSwitchMode;
		public BiomeData					currentBiomeData;
		
		Texture2D			biomeRepartitionPreview;
		float				localCoveragePercent;
		const int			previewTextureWidth = 200;
		const int			previewTextureHeight = 40;
		
		bool				updatePreview;

		PWGUIManager		PWGUI;
		
		ReorderableList		reprderableSwitchDataList;

		public int			Count { get { return switchDatas.Count; } }

		public void OnEnable()
		{
			PWGUI = new PWGUIManager();

			reprderableSwitchDataList = new ReorderableList(switchDatas, typeof(BiomeSwitchData), true, true, true, true);

			reprderableSwitchDataList.elementHeight = EditorGUIUtility.singleLineHeight * 2 + 4; //padding
			
            reprderableSwitchDataList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
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
					
					PWGUI.ColorPicker(colorFieldRect, ref elem.color, false, true);
					elem.name = EditorGUI.TextField(nameRect, elem.name);
					elem.min = EditorGUI.FloatField(minRect, "min", elem.min);
					elem.max = EditorGUI.FloatField(maxRect, "max", elem.max);

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

				switchDatas[index] = elem;
            };

			reprderableSwitchDataList.drawHeaderCallback = (rect) => {
				EditorGUI.LabelField(rect, "switches");
			};

			reprderableSwitchDataList.onReorderCallback += (ReorderableList l) => {
				OnBiomeDataReordered();
			};

			reprderableSwitchDataList.onAddCallback += (ReorderableList l) => {
				BiomeSwitchData	bsd = new BiomeSwitchData(currentSampler);
				switchDatas.Add(bsd);
				OnBiomeDataAdded(bsd);
				updatePreview = true;
			};

			reprderableSwitchDataList.onRemoveCallback += (ReorderableList l) => {
				if (switchDatas.Count > 1)
				{
					switchDatas.RemoveAt(l.index);
					OnBiomeDataRemoved();
					updatePreview = true;
				}
			};

			//add at least one biome switch:
			if (switchDatas.Count == 0)
				switchDatas.Add(new BiomeSwitchData(currentSampler));
		}

		public void OnGUI()
		{
			PWGUI.StartFrame(new Rect(0, 0, 0, 0));

			if (biomeRepartitionPreview == null)
				biomeRepartitionPreview = new Texture2D(previewTextureWidth, 1);

			using (new DefaultGUISkin())
			{
				reprderableSwitchDataList.DoLayoutList();
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

				for (int x = (int)rMin; x < (int)rMax; x++)
					biomeRepartitionPreview.SetPixel(x, 0, switchData.color);
				i++;
			}
			
			//add water if there is and if switch mode is height:
			if (!currentBiomeData.isWaterless && currentSwitchMode == BiomeSwitchMode.Height)
			{
				float rMax = (currentBiomeData.waterLevel / range) * previewTextureWidth;
				for (int x = 0; x < rMax; x++)
					biomeRepartitionPreview.SetPixel(x, 0, Color.blue);
			}

			biomeRepartitionPreview.Apply();

			updatePreview = false;
		}

		public BiomeSwitchData this[int index]
		{
			get { return switchDatas[index]; }
		}
	}
}