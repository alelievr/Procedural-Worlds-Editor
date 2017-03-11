using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PW
{
	[System.SerializableAttribute]
	public class PWNode : ScriptableObject
	{
		public Rect		windowRect;
		public int		windowId;

		static Color	defaultAnchorBackgroundColor = new Color(.75f, .75f, .75f, 1);
		static GUIStyle	boxAnchorStyle = null;
		
		static Texture2D	disabledTexture = null;
		static Texture2D	highlightTexture = null;

		static int		staticWindowId = 0;

		Vector2	position;
		int		computeOrder; //to define an order for computing result
		Vector2	scrollPos;
		int		viewHeight;
		Vector2	graphDecal = Vector2.zero;

		[SerializeField]
		List< PWLink > links = new List< PWLink >();

		[System.NonSerializedAttribute]
		Dictionary< string, PWAnchorData >	propertyDatas = new Dictionary< string, PWAnchorData >();

		public PWNode()
		{
			windowId = staticWindowId++;
			position = Vector2.one * 100;
			computeOrder = 0;
			windowRect = new Rect(400, 400, 200, 300);
			viewHeight = 0;
		}

		public void UpdateGraphDecal(Vector2 graphDecal)
		{
			this.graphDecal = graphDecal;
		}

		void ForeachPWAnchors(Action< string, PWAnchorData, int > callback)
		{
			foreach (var PWAnchorData in propertyDatas)
			{
				PWAnchorData data = PWAnchorData.Value;
				if (data.multiple)
				{
					int anchorCount = Mathf.Max(data.minMultipleValues, data.multipleValueInstance.Count);
					for (int i = 0; i < anchorCount; i++)
						callback(PWAnchorData.Key, data, i);
				}
				else
					callback(PWAnchorData.Key, data, -1);
			}
		}

		void LoadFieldAttributes()
		{
			//get input variables
			System.Reflection.FieldInfo[] fInfos = GetType().GetFields();

			propertyDatas.Clear();
			foreach (var field in fInfos)
			{
				propertyDatas[field.Name] = new PWAnchorData(field.Name);
				
				PWAnchorData	data = propertyDatas[field.Name];
				Color			backgroundColor = defaultAnchorBackgroundColor;
				PWAnchorType	anchorType = PWAnchorType.None;
				string			name = field.Name;
				Vector2			offset = Vector2.zero;

				System.Object[] attrs = field.GetCustomAttributes(true);
				foreach (var attr in attrs)
				{
					PWInput		inputAttr = attr as PWInput;
					PWOutput	outputAttr = attr as PWOutput;
					PWColor		colorAttr = attr as PWColor;
					PWOffset	offsetAttr = attr as PWOffset;
					PWMultiple	multipleAttr = attr as PWMultiple;

					if (inputAttr != null)
					{
						anchorType = PWAnchorType.Input;
						if (inputAttr.name != null)
							name = inputAttr.name;
					}
					if (outputAttr != null)
					{
						anchorType = PWAnchorType.Output;
						if (outputAttr.name != null)
							name = outputAttr.name;
					}
					if (colorAttr != null)
						backgroundColor = colorAttr.color;
					if (offsetAttr != null)
						offset = offsetAttr.offset;
					if (multipleAttr != null)
					{
						//check if field is PWValues type otherwise do not implement multi-anchor
						data.multipleValueInstance = field.GetValue(this) as PWValues;
						if (data.multipleValueInstance != null)
						{
							data.multiple = true;
							data.allowedTypes = multipleAttr.allowedTypes;
							data.minMultipleValues = multipleAttr.minValues;
							data.maxMultipleValues = multipleAttr.maxValues;
						}
					}
					//get attributes values:
				}
				if (anchorType == PWAnchorType.None) //field does not have a PW attribute
					propertyDatas.Remove(field.Name);
				else
				{
					data.color = backgroundColor;
					data.name = name;
					data.anchorType = anchorType;
					data.offset = offset;
					data.windowId = windowId;
					data.type = field.GetType();
				}
			}
		}

		public void OnEnable()
		{
			name = "basic node";
			disabledTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
			disabledTexture.SetPixel(0, 0, new Color(.4f, .4f, .4f, .5f));
			disabledTexture.Apply();
			highlightTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
			highlightTexture.SetPixel(0, 0, new Color(0, .5f, 0, .4f));
			highlightTexture.Apply();
			OnNodeCreate();
			LoadFieldAttributes();
		}

		public virtual void OnNodeCreate()
		{
		}

		public void OnGUI()
		{
			Debug.Log("You are on the wrong window");
		}

		public void OnWindowGUI(int id)
		{
			if (boxAnchorStyle == null)
			{
				boxAnchorStyle =  new GUIStyle(GUI.skin.box);
				boxAnchorStyle.padding = new RectOffset(0, 0, 1, 1);
			}

			// set the header of the window as draggable:
			GUI.DragWindow(new Rect(0, 0, windowRect.width, 20));

			GUILayout.BeginVertical();
			{
				RectOffset savedmargin = GUI.skin.label.margin;
				GUI.skin.label.margin = new RectOffset(2, 2, 5, 7);
				OnNodeGUI();
				GUI.skin.label.margin = savedmargin;
			}
			GUILayout.EndVertical();

			int	viewH = (int)GUILayoutUtility.GetLastRect().height;
			if (viewH > 2)
				viewHeight = viewH;

			windowRect.height = viewHeight + 30; //add the window header and footer size
		}
	
		public virtual void	OnNodeGUI()
		{
			EditorGUILayout.LabelField("empty node");
		}

		void RenderAnchor(PWAnchorData data, ref Rect inputAnchorRect, ref Rect outputAnchorRect, ref PWAnchorData ret, int index = -1)
		{
			Rect anchorRect = (data.anchorType == PWAnchorType.Input) ? inputAnchorRect : outputAnchorRect;
			anchorRect.position += graphDecal;

			string anchorName = (data.name.Length > 4) ? data.name.Substring(0, 4) : data.name;
			//if index != -1 then we are rendering a multi-anchor, the name will be incremented
			if (index != -1)
			{
				//TODO: better
				anchorName += index;
				data.anchorRects[index] = anchorRect;
			}
			else
				data.anchorRect = anchorRect;
			Color savedBackground = GUI.backgroundColor;
			GUI.backgroundColor = data.color;
			GUI.Box(anchorRect, anchorName, boxAnchorStyle);
			GUI.backgroundColor = savedBackground;
			if (anchorRect.Contains(Event.current.mousePosition))
			{
				ret = data.Clone();
				ret.anchorRect = anchorRect;
			}
			if (!data.enabled)
				GUI.DrawTexture(anchorRect, disabledTexture);
		}

		public PWAnchorData RenderAnchors()
		{
			PWAnchorData ret = null;
			
			int		anchorWidth = 38;
			int		anchorHeight = 16;

			Rect	inputAnchorRect = new Rect(windowRect.xMin - anchorWidth + 2, windowRect.y + 20, anchorWidth, anchorHeight);
			Rect	outputAnchorRect = new Rect(windowRect.xMax - 2, windowRect.y + 20, anchorWidth, anchorHeight);
			ForeachPWAnchors((fieldName, data, i) => {
				//draw anchor:
				if (data.visibility != PWVisibility.Gone)
				{
					if (data.visibility == PWVisibility.Visible)
						RenderAnchor(data, ref inputAnchorRect, ref outputAnchorRect, ref ret, i);
					if (data.anchorType == PWAnchorType.Input)
						inputAnchorRect.position += data.offset + Vector2.up * 18;
					else if (data.anchorType == PWAnchorType.Output)
						outputAnchorRect.position += data.offset + Vector2.up * 18;
				}
			});
			return ret;
		}

		public List< PWLink > GetLinks()
		{
			return links;
		}

		public void		AttachLink(PWAnchorData from, PWAnchorData to)
		{
			links.Add(new PWLink(to.windowId, to.id, from.windowId, from.id, from.color));
		}

		public void		RemoveLink(int anchorId)
		{
			links.RemoveAll(l => l.localAnchorId == anchorId);
		}

		public Rect?	GetAnchorRect(int id)
		{
			var matches = propertyDatas.Where(o => o.Value.id == id);

			if (matches.Count() == 0)
				return null;
			return matches.First().Value.anchorRect;
		}

		public void		HighlightAllAnchors(PWAnchorType anchorType, Type inputType)
		{
			ForeachPWAnchors((fieldName, data, i) => {
				if (data.anchorType == anchorType && data.type.IsAssignableFrom(inputType))
				{
					Rect	anchorRect = (data.multiple) ? data.anchorRects[i] : data.anchorRect;

					//TODO: another color for locked anchors.
					if (data.visibility == PWVisibility.Visible)
						GUI.DrawTexture(anchorRect, highlightTexture);
				}
				//TODO: mark as invisible all unmatched anchors.
			});
		}

		public void		HighlighAnchor(PWAnchorData anchor)
		{
			GUI.DrawTexture(anchor.anchorRect, highlightTexture);
		}

		/* Utils function to manipulate PWnode variables */

		public void		UpdatePropEnabled(string propertyName, bool enabled)
		{
			if (propertyDatas.ContainsKey(propertyName))
				propertyDatas[propertyName].enabled = true;
		}

		public void		UpdatePropName(string propertyName, string newName)
		{
			if (propertyDatas.ContainsKey(propertyName))
				propertyDatas[propertyName].name = newName;
		}

		public void		UpdatePropBackgroundColor(string propertyName, Color newColor)
		{
			if (propertyDatas.ContainsKey(propertyName))
				propertyDatas[propertyName].color = newColor;
		}

		public void		UpdatePropVisibility(string propertyName, PWVisibility visibility)
		{
			if (propertyDatas.ContainsKey(propertyName))
				propertyDatas[propertyName].visibility = visibility;
		}

		public bool		IsPropLocked(string propertyName)
		{
			if (propertyDatas.ContainsKey(propertyName))
				return propertyDatas[propertyName].locked;
			return false;
		}
    }
}