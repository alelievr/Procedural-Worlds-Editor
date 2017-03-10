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

		static Color	defaultAnchorBackgroundColor = new Color(.75f, .75f, .75f, 1);
		static GUIStyle	boxAnchorStyle = null;
		
		static Texture2D	disabledTexture = null;
		static Texture2D	highlightTexture = null;

		static int		staticWindowId = 0;

		int		windowId; //internal unique name
		Vector2	position;
		int		computeOrder; //to define an order for computing result
		Vector2	scrollPos;
		int		viewHeight;
		Vector2	graphDecal = Vector2.zero;

		List< int > links = new List< int >();

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

		void ForeachPWFields(Action< string, PWAnchorData > callback)
		{
			foreach (var PWAnchorData in propertyDatas)
				callback(PWAnchorData.Key, PWAnchorData.Value);
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
				NodeAnchorType	anchorType = NodeAnchorType.None;
				string			name = field.Name;
				Vector2			offset = Vector2.zero;

				System.Object[] attrs = field.GetCustomAttributes(true);
				foreach (var attr in attrs)
				{
					PWInput		inputAttr = attr as PWInput;
					PWOutput	outputAttr = attr as PWOutput;
					PWColor		colorAttr = attr as PWColor;
					PWOffset	offsetAttr = attr as PWOffset;

					if (inputAttr != null)
					{
						anchorType = NodeAnchorType.Input;
						if (inputAttr.name != null)
							name = inputAttr.name;
					}
					if (outputAttr != null)
					{
						anchorType = NodeAnchorType.Output;
						if (outputAttr.name != null)
							name = outputAttr.name;
					}
					if (colorAttr != null)
						backgroundColor = colorAttr.color;
					if (offsetAttr != null)
						offset = offsetAttr.offset;
					//get attributes values:
				}
				if (anchorType == NodeAnchorType.None) //field does not have a PW attribute
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

		public PWAnchorData RenderAnchors()
		{
			PWAnchorData ret = null;
			
			int		anchorWidth = 38;
			int		anchorHeight = 16;

			Rect	inputAnchorRect = new Rect(windowRect.xMin - anchorWidth + 2, windowRect.y + 20, anchorWidth, anchorHeight);
			Rect	outputAnchorRect = new Rect(windowRect.xMax - 2, windowRect.y + 20, anchorWidth, anchorHeight);
			ForeachPWFields((fieldName, data) => {
				Rect anchorRect = (data.anchorType == NodeAnchorType.Input) ? inputAnchorRect : outputAnchorRect;
				anchorRect.position += graphDecal;

				//draw anchor:
				if (data.visible)
				{
					Color savedBackground = GUI.backgroundColor;
					GUI.backgroundColor = data.color;
					GUI.Box(anchorRect, (data.name.Length > 4) ? data.name.Substring(0, 4) : data.name, boxAnchorStyle);
					GUI.backgroundColor = savedBackground;
					data.anchorRect = anchorRect;
					if (anchorRect.Contains(Event.current.mousePosition))
						ret = data;
					if (!data.enabled)
						GUI.DrawTexture(anchorRect, disabledTexture);
					if (data.anchorType == NodeAnchorType.Input)
						inputAnchorRect.position += data.offset + Vector2.up * 18;
					else if (data.anchorType == NodeAnchorType.Output)
						outputAnchorRect.position += data.offset + Vector2.up * 18;
				}
			});
			return ret;
		}

		public List< Link > GetLinks()
		{
			var links = new List< Link >();
	
			return links;
		}

		public void	AttachLink(PWAnchorData from, PWAnchorData to)
		{
			Debug.Log("Attach");
			//TODO: attack link code
		}

		public void	HighlightAllAnchors(NodeAnchorType anchorType, Type inputType)
		{
			ForeachPWFields((fieldName, data) => {
				if (data.anchorType == anchorType && data.type.IsAssignableFrom(inputType))
				{
					//TODO: another color for locked anchors.
					GUI.DrawTexture(data.anchorRect, highlightTexture);
				}
			});
		}

		public void HighlighAnchor(PWAnchorData anchor)
		{
			GUI.DrawTexture(anchor.anchorRect, highlightTexture);
		}

		/* Utils function to manipulate PWnode variables */

		public void UpdatePropEnabled(string propertyName, bool enabled)
		{
			if (propertyDatas.ContainsKey(propertyName))
				propertyDatas[propertyName].enabled = true;
		}

		public void UpdatePropName(string propertyName, string newName)
		{
			if (propertyDatas.ContainsKey(propertyName))
				propertyDatas[propertyName].name = newName;
		}

		public void UpdatePropBackgroundColor(string propertyName, Color newColor)
		{
			if (propertyDatas.ContainsKey(propertyName))
				propertyDatas[propertyName].color = newColor;
		}

		public void UpdatePropVisibility(string propertyName, bool visible)
		{
			if (propertyDatas.ContainsKey(propertyName))
				propertyDatas[propertyName].visible = visible;
		}

		public bool IsPropLocked(string propertyName)
		{
			if (propertyDatas.ContainsKey(propertyName))
				return propertyDatas[propertyName].locked;
			return false;
		}
    }

	[System.SerializableAttribute]
	public class Link
	{
		//distant link:
		public string	windowName;
		public int		distantAnchorID;

		//connected local property:
		public int		localAnchorID;

		public Link(string dWin, int dAttr, int lAttr)
		{
			windowName = dWin;
			distantAnchorID = dAttr;
			localAnchorID = lAttr;
		}
	}
	
	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class PWInput : System.Attribute
	{
		public string	name = null;
		
		public PWInput()
		{
		}
		
		public PWInput(string fieldName)
		{
			name = fieldName;
		}
	}
	
	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class PWOutput : System.Attribute
	{
		public string	name = null;

		public PWOutput()
		{
		}
		
		public PWOutput(string fieldName)
		{
			name = fieldName;
		}
	}
	
	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class PWOffset : System.Attribute
	{
		public Vector2	offset;

		public PWOffset(int x, int y)
		{
			offset.x = x;
			offset.y = y;
		}
		
		public PWOffset(int y)
		{
			offset.x = 0;
			offset.y = y;
		}
	}
	

	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class PWColor : System.Attribute
	{
		public Color		color;

		public PWColor(float r, float g, float b)
		{
			color.r = r;
			color.g = g;
			color.b = b;
			color.a = .7f;
		}

		public PWColor(float r, float g, float b, float a)
		{
			color.r = r;
			color.g = g;
			color.b = b;
			color.a = a;
		}
	}
}
