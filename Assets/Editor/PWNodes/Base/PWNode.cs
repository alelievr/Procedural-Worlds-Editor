using System.Linq;
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

		int		computeOrder; //to define an order for computing result
		Vector2	scrollPos;
		int		viewHeight;
		Vector2	graphDecal = Vector2.zero;
		int		maxAnchorRenderHeight;

		[SerializeField]
		List< PWLink > links = new List< PWLink >();

		[System.NonSerializedAttribute]
		Dictionary< string, PWAnchorData >	propertyDatas = new Dictionary< string, PWAnchorData >();

		public PWNode()
		{
			windowId = staticWindowId++;
			computeOrder = 0;
			windowRect = new Rect(400, 400, 200, 300);
			viewHeight = 0;
		}

		public void UpdateGraphDecal(Vector2 graphDecal)
		{
			this.graphDecal = graphDecal;
		}

		void ForeachPWAnchors(Action< string, PWAnchorData, PWAnchorData.PWAnchorMultiData, int > callback)
		{
			foreach (var PWAnchorData in propertyDatas)
			{
				PWAnchorData data = PWAnchorData.Value;
				if (data.multiple)
				{
					int anchorCount = Mathf.Max(data.minMultipleValues, data.multipleValueInstance.Count);
					if (data.displayHiddenMultipleAnchors)
						anchorCount++;
					for (int i = 0; i < anchorCount; i++)
					{
						//if multi-anchor instance does not exists, create it:
						if (data.displayHiddenMultipleAnchors && i == anchorCount - 1)
							data.multi[i].additional = true;
						else
							data.multi[i].additional = false;
						callback(PWAnchorData.Key, data, data.multi[i], i);
					}
				}
				else
					callback(PWAnchorData.Key, data, data.first, 0);
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
					PWGeneric	genericAttr = attr as PWGeneric;

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
							data.generic = true;
							data.multiple = true;
							data.allowedTypes = multipleAttr.allowedTypes;
							data.minMultipleValues = multipleAttr.minValues;
							data.maxMultipleValues = multipleAttr.maxValues;

							//add minimum number of anchors to render:
							for (int i = 1; i <= data.minMultipleValues; i++)
								data.AddNewAnchor(backgroundColor);
						}
					}
					if (genericAttr != null)
					{
						data.allowedTypes = genericAttr.allowedTypes;
						data.generic = true;
					}
				}
				if (anchorType == PWAnchorType.None) //field does not have a PW attribute
					propertyDatas.Remove(field.Name);
				else
				{
					if (anchorType == PWAnchorType.Output && data.multiple)
					{
						Debug.LogWarning("PWMultiple attribute is only valid on input variables");
						data.multiple = false;
					}
					data.name = name;
					data.anchorType = anchorType;
					data.type = field.FieldType;
					data.first.color = (SerializableColor)backgroundColor;
					data.first.name = name;
					data.first.offset = offset;
					data.windowId = windowId;
				}
			}
		}

		public void OnEnable()
		{
			name = "basic node";
			maxAnchorRenderHeight = 0;
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
			EditorGUILayout.LabelField("You are on the wrong window !");
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

			viewHeight = Mathf.Max(viewHeight, maxAnchorRenderHeight);
			windowRect.height = viewHeight + 24; //add the window header and footer size
		}
	
		public virtual void	OnNodeGUI()
		{
			EditorGUILayout.LabelField("empty node");
		}

		void ProcessAnchor(
			PWAnchorData data,
			PWAnchorData.PWAnchorMultiData singleAnchor,
			ref Rect inputAnchorRect,
			ref Rect outputAnchorRect,
			ref PWAnchorInfo ret,
			int index = -1)
		{
			Rect anchorRect = (data.anchorType == PWAnchorType.Input) ? inputAnchorRect : outputAnchorRect;
			anchorRect.position += graphDecal;

			singleAnchor.anchorRect = anchorRect;

			if (!ret.mouseAbove)
				ret = new PWAnchorInfo(data.name, anchorRect, singleAnchor.color, data.type, data.anchorType, windowId, singleAnchor.id, data.generic, data.allowedTypes);
			if (anchorRect.Contains(Event.current.mousePosition))
				ret.mouseAbove = true;
		}

		public PWAnchorInfo ProcessAnchors()
		{
			PWAnchorInfo ret = new PWAnchorInfo();
			
			int		anchorWidth = 38;
			int		anchorHeight = 16;

			Rect	inputAnchorRect = new Rect(windowRect.xMin - anchorWidth + 2, windowRect.y + 20, anchorWidth, anchorHeight);
			Rect	outputAnchorRect = new Rect(windowRect.xMax - 2, windowRect.y + 20, anchorWidth, anchorHeight);
			ForeachPWAnchors((fieldName, data, singleAnchor, i) => {
				//process anchor event and calcul rect position if visible
				if (singleAnchor.visibility != PWVisibility.Gone)
				{
					if (singleAnchor.visibility == PWVisibility.Visible)
						ProcessAnchor(data, singleAnchor, ref inputAnchorRect, ref outputAnchorRect, ref ret, i);
					if (data.anchorType == PWAnchorType.Input)
						inputAnchorRect.position += singleAnchor.offset + Vector2.up * 18;
					else if (data.anchorType == PWAnchorType.Output)
						outputAnchorRect.position += singleAnchor.offset + Vector2.up * 18;
				}
			});
			maxAnchorRenderHeight = (int)Mathf.Max(inputAnchorRect.yMin - windowRect.y - 20, outputAnchorRect.yMin - windowRect.y - 20);
			return ret;
		}
		
		void RenderAnchor(PWAnchorData data, PWAnchorData.PWAnchorMultiData singleAnchor, int index)
		{
			string anchorName = (data.name.Length > 4) ? data.name.Substring(0, 4) : data.name;

			if (data.multiple)
			{
				//TODO: better
				if (singleAnchor.additional)
					anchorName = "+";
				else
					anchorName += index;
			}
			Color savedBackground = GUI.backgroundColor;
			GUI.backgroundColor = singleAnchor.color;
			GUI.Box(singleAnchor.anchorRect, anchorName, boxAnchorStyle);
			GUI.backgroundColor = savedBackground;
			if (!singleAnchor.enabled)
				GUI.DrawTexture(singleAnchor.anchorRect, disabledTexture);
			else
				switch (singleAnchor.highlighMode)
				{
					case PWAnchorHighlight.AttachNew:
						GUI.DrawTexture(singleAnchor.anchorRect, highlightTexture);
						break ;
				}
			//reset the Highlight:
			singleAnchor.highlighMode = PWAnchorHighlight.None;
		}
		
		public void RenderAnchors()
		{
			ForeachPWAnchors((fieldName, data, singleAnchor, i) => {
				//draw anchor:
				if (singleAnchor.visibility != PWVisibility.Gone)
				{
					if (singleAnchor.visibility == PWVisibility.Visible)
						RenderAnchor(data, singleAnchor, i);
					if (singleAnchor.visibility == PWVisibility.InvisibleWhenLinking)
						singleAnchor.visibility = PWVisibility.Visible;
				}
			});
		}

		public List< PWLink > GetLinks()
		{
			return links;
		}
		
		bool			AnchorAreAssignable(Type fromType, PWAnchorType fromAnchorType, bool fromGeneric, Type[] fromAllowedTypes, PWAnchorInfo to, bool verbose = false)
		{
			if (fromType.IsAssignableFrom(to.fieldType) || fromType == typeof(object) || to.fieldType == typeof(object))
			{
				if (verbose)
					Debug.Log(fromType.ToString() + " is assignable from " + to.fieldType.ToString());
				return true;
			}
			
			if (fromAnchorType == PWAnchorType.Input)
			{
				if (fromGeneric)
				{
					if (verbose)
						Debug.Log("Generic variable, check all allowed types:");
					foreach (var t in fromAllowedTypes)
					{
						if (verbose)
							Debug.Log("check castable from " + to.fieldType + " to " + t);
						if (to.fieldType.IsAssignableFrom(t))
						{
							if (verbose)
								Debug.Log(fromType + " is castable from " + t);
							return true;
						}
					}
				}
			}
			else
			{
				if (to.generic)
				{
					foreach (var t in to.allowedTypes)
					{
						if (verbose)
							Debug.Log("check castable from " + fromType + " to " + t);
						if (fromType.IsAssignableFrom(t))
						{
							if (verbose)
								Debug.Log(fromType + " is castable from " + t);
							return true;
						}
					}
				}
			}
			return false;
		}

		bool			AnchorAreAssignable(PWAnchorInfo from, PWAnchorInfo to, bool verbose = false)
		{
			return AnchorAreAssignable(from.fieldType, from.anchorType, from.generic, from.allowedTypes, to, verbose);
		}

		public void		AttachLink(PWAnchorInfo from, PWAnchorInfo to)
		{
			//from is othen me and with an anchor type of Output.

			//quit if types are not compatible
			if (!AnchorAreAssignable(from, to))
				return ;
			if (from.anchorType == to.anchorType)
				return ;
			if (from.windowId == to.windowId)
				return ;

			//we store output links:
			if (from.anchorType == PWAnchorType.Output)
			{
				links.Add(new PWLink(to.windowId, to.anchorId, from.windowId, from.anchorId, from.anchorColor));
				//mark local output anchors as linked:
				ForeachPWAnchors((fieldName, data, singleAnchor, i) => {
					if (singleAnchor.id == from.anchorId)
						singleAnchor.linkCount++;
				});
			}
			else //input links are stored as depencencies:
			{
				ForeachPWAnchors((fieldName, data, singleAnchor, i) => {
					if (singleAnchor.id == from.anchorId)
					{
						//if data was added to multi-anchor:
						if (data.multiple)
							if (i == data.multipleValueInstance.Count)
								data.AddNewAnchor();
					}
				});
				//TODO: create dependency for the window to.windowId (for fast computeOrder calcul)
			}
		}

		public void		RemoveLink(int anchorId)
		{
			links.RemoveAll(l => l.localAnchorId == anchorId);
			PWAnchorData.PWAnchorMultiData singleAnchorData;
			var data = GetAnchorData(anchorId, out singleAnchorData);
			singleAnchorData.linkCount--;
			//TODO: decrement the linkCount and update locked if needed.
		}

		public PWAnchorData	GetAnchorData(int id, out PWAnchorData.PWAnchorMultiData singleAnchorData)
		{
			PWAnchorData					ret = null;
			PWAnchorData.PWAnchorMultiData	s = null;

			ForeachPWAnchors((fieldName, data, singleAnchor, i) => {
				if (singleAnchor.id == id)
				{
					s = singleAnchor;
					ret = data;
				}
			});
			singleAnchorData = s;
			return ret;
		}

		public Rect?	GetAnchorRect(int id)
		{
			var matches =	from p in propertyDatas
							from p2 in p.Value.multi
							where p2.Value.id == id
							select p2.Value;

			if (matches.Count() == 0)
				return null;
			return matches.First().anchorRect;
		}

		PWAnchorType	InverAnchorType(PWAnchorType type)
		{
			if (type == PWAnchorType.Input)
				return PWAnchorType.Output;
			else if (type == PWAnchorType.Output)
				return PWAnchorType.Input;
			return PWAnchorType.None;
		}

		public void		HighlightLinkableAnchorsTo(PWAnchorInfo toLink)
		{
			PWAnchorType anchorType = InverAnchorType(toLink.anchorType);

			ForeachPWAnchors((fieldName, data, singleAnchor, i) => {
				//Hide anchors and highlight when mouse hover
				// Debug.Log(data.name + ": " + AnchorAreAssignable(data.type, data.anchorType, data.generic, data.allowedTypes, toLink, true));
				if (data.windowId != toLink.windowId
					&& data.anchorType == anchorType
					&& AnchorAreAssignable(data.type, data.anchorType, data.generic, data.allowedTypes, toLink))
				{
					if (data.multiple)
					{
						//display additional anchor to attach on next rendering
						data.displayHiddenMultipleAnchors = true;
					}
					if (singleAnchor.anchorRect.Contains(Event.current.mousePosition))
						if (singleAnchor.visibility == PWVisibility.Visible)
						{
							//TODO: another color for locked anchors.
							singleAnchor.highlighMode = PWAnchorHighlight.AttachNew;
						}
				}
				else if (singleAnchor.visibility == PWVisibility.Visible && singleAnchor.id != toLink.anchorId)
					singleAnchor.visibility = PWVisibility.InvisibleWhenLinking;
			});
		}

		public void		DisplayHiddenMultipleAnchors(bool display = true)
		{
			ForeachPWAnchors((fieldName, data, singleAnchor, i)=> {
				if (data.multiple)
					data.displayHiddenMultipleAnchors = display;
			});
		}

		/* Utils function to manipulate PWnode variables */

		public void		UpdatePropEnabled(string propertyName, bool enabled)
		{
			if (propertyDatas.ContainsKey(propertyName))
				propertyDatas[propertyName].first.enabled = true;
		}

		public void		UpdatePropName(string propertyName, string newName)
		{
			if (propertyDatas.ContainsKey(propertyName))
				propertyDatas[propertyName].first.name = newName;
		}

		public void		UpdatePropBackgroundColor(string propertyName, Color newColor)
		{
			if (propertyDatas.ContainsKey(propertyName))
				propertyDatas[propertyName].first.color = (SerializableColor)newColor;
		}

		public void		UpdatePropVisibility(string propertyName, PWVisibility visibility)
		{
			if (propertyDatas.ContainsKey(propertyName))
				propertyDatas[propertyName].first.visibility = visibility;
		}

		public bool		IsPropLocked(string propertyName)
		{
			if (propertyDatas.ContainsKey(propertyName))
				return propertyDatas[propertyName].first.locked;
			return false;
		}
    }
}