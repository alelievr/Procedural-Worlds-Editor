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
		public Rect		rect;

		static Color	defaultAnchorBackgroundColor = Color.white * .75f;

		string	_name; //internal unique name
		Vector2	position;
		int		computeOrder; //to define an order for computing result
		Vector2	scrollPos;
		int		viewHeight;

		List< int > links = new List< int >();

		public PWNode()
		{
			_name = System.Guid.NewGuid().ToString();
			position = Vector2.one * 100;
			computeOrder = 0;
			rect = new Rect(400, 400, 200, 300);
			viewHeight = 0;
		}

		public void OnEnable()
		{
			name = "basic node";
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
			// set the header of the window as draggable:
			GUI.DragWindow(new Rect(0, 0, rect.width, 20));

			GUILayout.BeginVertical();
			{
				//set the singleLineHeight to 24
				OnNodeGUI();
			}
			GUILayout.EndVertical();

			int	viewH = (int)GUILayoutUtility.GetLastRect().height;
			if (viewH > 2)
				viewHeight = viewH;

			rect.height = viewHeight + 30; //add the window header and footer size
		}
	
		public virtual void	OnNodeGUI()
		{
			EditorGUILayout.LabelField("empty node");
		}

		public bool RenderAnchors(Rect screenWindowRect)
		{
			bool mouseAboveAnchor = false;
			
			//get input variables
			System.Reflection.FieldInfo[] fInfos = GetType().GetFields();

			int		anchorWidth = 40;
			int		anchorHeight = 20;

			Rect	inputAnchorRect = new Rect(screenWindowRect.xMin - anchorWidth + 2, screenWindowRect.y + 20, anchorWidth, anchorHeight);
			Rect	outputAnchorRect = new Rect(screenWindowRect.xMax - 2, screenWindowRect.y + 20, anchorWidth, anchorHeight);
			foreach (var field in fInfos)
			{
				System.Object[] attrs = field.GetCustomAttributes(true);

				foreach (var o in attrs)
				{
					bool	drawAnchor = false;
					Color	backgroundColor = defaultAnchorBackgroundColor;
					Rect	anchorRect = new Rect();
					string	fieldName = "NULL";

					PWInput		input = o as PWInput;
					PWOutput	output = o as PWOutput;
					PWColor		color = o as PWColor;

					if (input != null)
					{
						drawAnchor = true;
						fieldName = (input.name != null) ? input.name : field.Name;
						anchorRect = inputAnchorRect;
					}
					if (output != null)
					{
						if (drawAnchor == true) //value is set in input and output -> error
							continue ;
						anchorRect = outputAnchorRect;
						fieldName = (output.name != null) ? output.name : field.Name;
						drawAnchor = true;
					}
					if (color != null)
						backgroundColor = color.color;
					if (drawAnchor)
					{
						Color savedBackground = GUI.backgroundColor;
						GUI.backgroundColor = backgroundColor;
						GUI.Box(anchorRect, (fieldName.Length > 4) ? fieldName.Substring(0, 4) : fieldName);
						GUI.backgroundColor = savedBackground;
						if (anchorRect.Contains(Event.current.mousePosition))
							mouseAboveAnchor = true;
					}
					inputAnchorRect.y += 24;
					outputAnchorRect.y += 24;
				}
			}
			return mouseAboveAnchor;
		}

		public List< Link > GetLinks()
		{
			var links = new List< Link >();
	
			return links;
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
		}
	}
}