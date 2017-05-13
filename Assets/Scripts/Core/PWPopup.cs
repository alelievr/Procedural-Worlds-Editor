using UnityEngine;
using System.Collections.Generic;
using System;

//popup system used to display ColorField's ColorPicker and settings windows.
namespace PW
{
	public static class PWPopup {

		private class PWPopupData
		{
			public Vector2	position;
			public Action	windowGUI;

			public PWPopupData(Vector2 position, Action windowGUI)
			{
				this.position = position;
				this.windowGUI = windowGUI;
			}
		}

		private static Queue< PWPopupData > toRender = new Queue< PWPopupData >();

		public static void AddToRender(PWGUISettings s, Action windowGUI)
		{
			toRender.Enqueue(new PWPopupData(s.windowPosition, windowGUI));
		}

		public static void RenderAll()
		{
			while (toRender.Count != 0)
			{
				RenderPopup(toRender.Dequeue());
			}
		}

		private static void RenderPopup(PWPopupData data)
		{
			//TODO: render a draggable window (not a GUI.Window, it will not work)
			//and call the windowGUI when the window context is well defined (Area)
		}
	}
}