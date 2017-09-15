using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Header rendering and processing for nodes
namespace PW
{
	public partial class PWNode {
	
		void RenderHeader()
		{
			//TODO: display node color pad
			
			RenderRenamable();
		}

		void RenderRenamable()
		{
			Event e = Event.current;
			
			//rendering node rename field
			if (renamable)
			{
				Vector2	winSize = windowRect.size;
				Rect	renameRect = new Rect(0, 0, winSize.x, 18);
				Rect	renameIconRect = new Rect(winSize.x - 28, 3, 12, 12);
				string	renameNodeField = "renameWindow";

				GUI.color = Color.black * .9f;
				GUI.DrawTexture(renameIconRect, editIcon);
				GUI.color = Color.white;

				if (windowNameEdit)
				{
					GUI.SetNextControlName(renameNodeField);
					name = GUI.TextField(renameRect, name, renameNodeTextFieldStyle);
	
					if (e.type == EventType.MouseDown && !renameRect.Contains(e.mousePosition))
					{
						windowNameEdit = false;
						GUI.FocusControl(null);
					}
					if (GUI.GetNameOfFocusedControl() == renameNodeField)
					{
						if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.Escape)
						{
							windowNameEdit = false;
							GUI.FocusControl(null);
							e.Use();
						}
					}
				}
				
				if (renameIconRect.Contains(e.mousePosition))
				{
					if (e.type == EventType.Used) //used by drag
					{
						windowNameEdit = true;
						GUI.FocusControl(renameNodeField);
						var te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
						if (te != null)
							te.SelectAll();
					}
					else if (e.type == EventType.MouseDown)
						windowNameEdit = false;
				}
			}
		}
	}
}
