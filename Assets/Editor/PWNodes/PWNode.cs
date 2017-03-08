using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace PW
{
	public class PWNode
	{
		public string	name;

		public Rect		rect;

		string	_name; //internal unique name
		Vector2	position;
		int		computeOrder; //to define an order for computing result
		Vector2	size = new Vector2(100, 150);

		List< int > links = new List< int >();

		public PWNode()
		{
			_name = System.Guid.NewGuid().ToString();
			position = Vector2.one * 100;
			computeOrder = 0;
			name = "basic node";
			rect = new Rect(400, 400, 250, 400);
			OnCreate();
		}

		public virtual void OnCreate()
		{
		}

		public virtual void OnGUI(int id)
		{
			GUI.DragWindow();
			
			//get input variables
			System.Reflection.FieldInfo[] fInfos = GetType().GetFields();

			foreach (var field in fInfos)
			{
				System.Object[] attrs = field.GetCustomAttributes(true);

				foreach (var o in attrs)
				{
					if (o as PWInput != null)
					{
						Debug.Log("input: " + o);
					}
					if (o as PWOutput != null)
					{
						Debug.Log("output: " + o);
					}
				}
			}
		}
    }

	struct Link
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
		public PWInput()
		{

		}
	}
	
	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class PWOutput : System.Attribute
	{
		public PWOutput()
		{
			
		}
	}
}