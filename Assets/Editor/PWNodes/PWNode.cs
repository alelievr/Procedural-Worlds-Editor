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
		static Texture2D disabledTexture = null;
		static GUIStyle	boxAnchorStyle = null;

		static int		staticWindowID = 0;

		int		windowID; //internal unique name
		Vector2	position;
		int		computeOrder; //to define an order for computing result
		Vector2	scrollPos;
		int		viewHeight;

		List< int > links = new List< int >();

		Dictionary< string, PropMetadata >	propertyMetadatas = new Dictionary< string, PropMetadata >();

		public class PWNodeAnchor
		{
			public Rect				anchorRect;
			public NodeAnchorType	type;
			public bool				mouseAbove;
			public int				id;
			public int				windowId;

			public PWNodeAnchor(Rect anchorRect, NodeAnchorType type, bool mouseAbove, int anchorID, int windowId)
			{
				this.anchorRect = anchorRect;
				this.type = type;
				this.mouseAbove = mouseAbove;
				this.id = anchorID;
				this.windowId = windowId;
			}
		}

		public enum NodeAnchorType
		{
			Input,
			Output,
			None,
		}

		private class PropMetadata
		{
			public bool		enabled;
			public Color	color;
			public string	name;
			public bool		visible;
			public bool		locked; //if prop is driven by external window output.

			public PropMetadata()
			{
				enabled = true;
				visible = true;
				name = null;
				color = defaultAnchorBackgroundColor;
			}
		}

		public PWNode()
		{
			windowID = staticWindowID++;
			position = Vector2.one * 100;
			computeOrder = 0;
			windowRect = new Rect(400, 400, 200, 300);
			viewHeight = 0;
		}

		public void OnEnable()
		{
			name = "basic node";
			disabledTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
			disabledTexture.SetPixel(0, 0, new Color(.4f, .4f, .4f, .5f));
			disabledTexture.Apply();
			OnNodeCreate();
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

		public PWNodeAnchor RenderAnchors(Rect screenWindowRect)
		{
			PWNodeAnchor ret = null;
			
			//get input variables
			System.Reflection.FieldInfo[] fInfos = GetType().GetFields();

			int		anchorWidth = 38;
			int		anchorHeight = 16;

			Rect	inputAnchorRect = new Rect(screenWindowRect.xMin - anchorWidth + 2, screenWindowRect.y + 20, anchorWidth, anchorHeight);
			Rect	outputAnchorRect = new Rect(screenWindowRect.xMax - 2, screenWindowRect.y + 20, anchorWidth, anchorHeight);
			foreach (var field in fInfos)
			{
				System.Object[] attrs = field.GetCustomAttributes(true);
				
				if (!propertyMetadatas.ContainsKey(field.Name))
					propertyMetadatas[field.Name] = new PropMetadata();

				PropMetadata	metadata = propertyMetadatas[field.Name];
				bool			drawAnchor = false;
				Color			backgroundColor = defaultAnchorBackgroundColor;
				Rect			anchorRect = new Rect();
				string			fieldName = "NULL";
				bool			visible = metadata.visible;
				bool			enabled = metadata.enabled;
				NodeAnchorType	type = NodeAnchorType.None;

				foreach (var o in attrs)
				{
					PWInput		input = o as PWInput;
					PWOutput	output = o as PWOutput;
					PWColor		color = o as PWColor;

					if (input != null)
					{
						drawAnchor = true;
						fieldName = (input.name != null) ? input.name : field.Name;
						anchorRect = inputAnchorRect;
						type = NodeAnchorType.Input;
					}
					if (output != null)
					{
						if (drawAnchor == true) //value is set in input and output -> error
							continue ;
						anchorRect = outputAnchorRect;
						fieldName = (output.name != null) ? output.name : field.Name;
						drawAnchor = true;
						type = NodeAnchorType.Output;
					}
					if (color != null)
						backgroundColor = color.color;
					if (o as HideInInspector != null)
						visible = false;
				}
				//override property if metadata have been stored on the field:
				if (metadata.color != defaultAnchorBackgroundColor)
					backgroundColor = metadata.color;
				if (metadata.name != null)
					fieldName = metadata.name;

				//draw anchor:
				if (visible)
				{
					Color savedBackground = GUI.backgroundColor;
					GUI.backgroundColor = backgroundColor;
					GUI.Box(anchorRect, (fieldName.Length > 4) ? fieldName.Substring(0, 4) : fieldName, boxAnchorStyle);
					GUI.backgroundColor = savedBackground;
					if (anchorRect.Contains(Event.current.mousePosition))
					{
						//TODO: implement anchor id system
						ret = new PWNodeAnchor(anchorRect, type, true, 0, windowID);
					}
					if (!enabled)
						GUI.DrawTexture(anchorRect, disabledTexture);
					if (type == NodeAnchorType.Input)
						inputAnchorRect.y += 18;
					else if (type == NodeAnchorType.Output)
						outputAnchorRect.y += 18;
				}
			}
			return ret;
		}

		public List< Link > GetLinks()
		{
			var links = new List< Link >();
	
			return links;
		}

		public void	AttackLink(string externalWindow, int externalAnchor, int internalAnchor)
		{
			//TODO: attack link code
		}

		public void	hilightAllInputAnchor(Type inputType)
		{
			//TODO: hilight all input matching enabled input anchors
		}

		public void hilightAllOutputAnchor(Type outputType)
		{
			//TODO: hilight all output matching enabled input anchors
		}

		/*Utils function to manipulate PWnode variables*/

		void ForeachFieldAttribute(string propName, Action<PWOutput, PWInput> action)
		{
			foreach (var field in GetType().GetFields())
				foreach (var attr in field.GetCustomAttributes(true))
				{
					PWOutput	output = attr as PWOutput;
					PWInput		input = attr as PWInput;

					if (output != null && field.Name == propName)
						action(output, null);
					if (input != null && field.Name == propName)
						action(null, input);
				}
		}

		public void UpdateEnabled(string propertyName, bool enabled)
		{
			if (propertyMetadatas.ContainsKey(propertyName))
				propertyMetadatas[propertyName].enabled = true;
		}

		public void UpdateName(string propertyName, string newName)
		{
			if (propertyMetadatas.ContainsKey(propertyName))
				propertyMetadatas[propertyName].name = newName;
		}

		public void UpdateBackgroundColor(string propertyName, Color newColor)
		{
			if (propertyMetadatas.ContainsKey(propertyName))
				propertyMetadatas[propertyName].color = newColor;
		}

		public void UpdateVisible(string propertyName, bool visible)
		{
			if (propertyMetadatas.ContainsKey(propertyName))
				propertyMetadatas[propertyName].visible = visible;
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