// #define DEBUG_WINDOW

using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace PW
{
	[System.SerializableAttribute]
	public class PWNode : ScriptableObject
	{
		public string	nodeTypeName;
		public Rect		windowRect;
		public int		windowId;
		public bool		renamable;
		public int		computeOrder;
		public int		viewHeight;
		public bool		specialButtonClick = false;
		public bool		isDragged = false;
		public Vector3	chunkPosition = Vector3.zero;
		public int		chunkSize = 16;
		public int		seed;

		public bool		seedHasChanged = false;
		public bool		positionHasChanged = false;
		public bool		chunkSizeHasChanged = false;
		public bool		inputHasChanged = false;
		public bool		outputHasChanged = false;
		public bool		justReloaded = false;
		public bool		notifyDataChanged = false;
		public bool		reloadRequested = false;
		public bool		needUpdate { get { return seedHasChanged || positionHasChanged || chunkSizeHasChanged || inputHasChanged || justReloaded || reloadRequested;}}

		[System.NonSerializedAttribute]
		public bool		unserializeInitialized = false;

		static Color	defaultAnchorBackgroundColor = new Color(.75f, .75f, .75f, 1);
		static GUIStyle	boxAnchorStyle = null;
		
		static Texture2D	disabledTexture = null;
		static Texture2D	highlightNewTexture = null;
		static Texture2D	highlightReplaceTexture = null;
		static Texture2D	highlightAddTexture = null;
		static Texture2D	errorIcon = null;

		[SerializeField]
		Vector2	graphDecal;
		[SerializeField]
		int		maxAnchorRenderHeight;
		[SerializeField]
		string	firstInitialization;

		bool	windowShouldClose = false;

		Vector3	oldChunkPosition;
		int		oldSeed;
		int		oldChunkSize;
		Pair< string, int >	lastAttachedLink = null;

		public static int	windowRenderOrder = 0;

		[SerializeField]
		List< PWLink >	links = new List< PWLink >();
		[SerializeField]
		//List< windowId, anchorId >
		List< PWNodeDependency >		depencendies = new List< PWNodeDependency >();

		[System.SerializableAttribute]
		public class PropertyDataDictionary : SerializableDictionary< string, PWAnchorData > {}
		[SerializeField]
		protected PropertyDataDictionary propertyDatas = new PropertyDataDictionary();

		[NonSerializedAttribute]
		protected Dictionary< string, FieldInfo > bakedNodeFields = new Dictionary< string, FieldInfo >();

		public void OnDestroy()
		{
			Debug.Log("node " + nodeTypeName + " detroyed !");
		}

		public void UpdateGraphDecal(Vector2 graphDecal)
		{
			this.graphDecal = graphDecal;
		}
		
		public void OnEnable()
		{
			Func< string, Texture2D > CreateTexture2DFromFile = (string ressourcePath) => {
				return Resources.Load< Texture2D >(ressourcePath);
			};
				
			Func< Color, Texture2D > CreateTexture2DColor = (Color c) => {
				Texture2D	ret;
				ret = new Texture2D(1, 1, TextureFormat.RGBA32, false);
				ret.SetPixel(0, 0, c);
				ret.Apply();
				return ret;
			};

			disabledTexture = CreateTexture2DColor(new Color(.4f, .4f, .4f, .5f));
			highlightNewTexture = CreateTexture2DColor(new Color(0, .5f, 0, .4f));
			highlightReplaceTexture = CreateTexture2DColor(new Color(1f, 0f, 0, .4f));
			highlightAddTexture = CreateTexture2DColor(new Color(0f, .0f, 0.5f, .4f));

			errorIcon = CreateTexture2DFromFile("error");

			LoadFieldAttributes();

			BakeNodeFields();
			
			//this will be true only if the object instance does not came from a serialized object.
			if (firstInitialization == null)
			{
				computeOrder = 0;
				windowRect = new Rect(400, 400, 200, 50);
				viewHeight = 0;
				renamable = false;
				maxAnchorRenderHeight = 0;

				OnNodeCreateOnce();

				firstInitialization = "initialized";
			}

			justReloaded = true;
		}

		public void RunNodeAwake()
		{
			OnNodeAwake();
			OnNodeCreate();
			unserializeInitialized = true;
		}

		public virtual void OnNodeAwake()
		{
		}

		public virtual void OnNodeCreate()
		{
		}

		public virtual void OnNodeCreateOnce()
		{
		}

		public void BeginFrameUpdate()
		{
			if (oldSeed != seed)
				seedHasChanged = true;
			if (oldChunkPosition != chunkPosition)
				positionHasChanged = true;
			if (oldChunkSize != chunkSize)
				chunkSizeHasChanged = true;
		}

		void ForeachPWAnchors(Action< PWAnchorData, PWAnchorData.PWAnchorMultiData, int > callback, bool showAdditional = false)
		{
			foreach (var PWAnchorData in propertyDatas)
			{
				var data = PWAnchorData.Value;
				if (data.multiple)
				{
					//update anchor instance if null:
					data.anchorInstance = bakedNodeFields[data.fieldName].GetValue(this);
					if (data.anchorInstance == null)
					{
						if (data.anchorInstance == null)
							continue ;
					}

					int anchorCount = Mathf.Max(data.minMultipleValues, data.multipleValueCount);
					if (data.displayHiddenMultipleAnchors || showAdditional)
						anchorCount++;
					for (int i = 0; i < anchorCount; i++)
					{
						//if multi-anchor instance does not exists, create it:
						if (data.displayHiddenMultipleAnchors && i == anchorCount - 1)
							data.multi[i].additional = true;
						else
							data.multi[i].additional = false;
						callback(data, data.multi[i], i);
					}
				}
				else
					callback(data, data.first, -1);
			}
		}

		void ForeachPWAnchorDatas(Action< PWAnchorData > callback)
		{
			foreach (var data in propertyDatas)
			{
				if (data.Value != null)
					callback(data.Value);
			}
		}

		void BakeNodeFields()
		{
			System.Reflection.FieldInfo[] fInfos = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

			bakedNodeFields.Clear();
			foreach (var fInfo in fInfos)
				bakedNodeFields[fInfo.Name] = fInfo;
		}

		void LoadFieldAttributes()
		{
			//get input variables
			System.Reflection.FieldInfo[] fInfos = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

			List< string > actualFields = new List< string >();
			foreach (var field in fInfos)
			{
				actualFields.Add(field.Name);
				if (!propertyDatas.ContainsKey(field.Name))
					propertyDatas[field.Name] = new PWAnchorData(field.Name, field.Name.GetHashCode());
				
				PWAnchorData	data = propertyDatas[field.Name];
				Color			backgroundColor = defaultAnchorBackgroundColor;
				PWAnchorType	anchorType = PWAnchorType.None;
				string			name = field.Name;
				Vector2			offset = Vector2.zero;

				data.anchorInstance = field.GetValue(this);
				System.Object[] attrs = field.GetCustomAttributes(true);
				foreach (var attr in attrs)
				{
					PWInput			inputAttr = attr as PWInput;
					PWOutput		outputAttr = attr as PWOutput;
					PWColor			colorAttr = attr as PWColor;
					PWOffset		offsetAttr = attr as PWOffset;
					PWMultiple		multipleAttr = attr as PWMultiple;
					PWGeneric		genericAttr = attr as PWGeneric;
					PWMirror		mirrorAttr = attr as PWMirror;
					PWNotRequired	notRequiredAttr = attr as PWNotRequired;

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
						var multipleValueInstance = field.GetValue(this) as PWValues;
						data.generic = true;
						data.multiple = true;
						data.allowedTypes = multipleAttr.allowedTypes;
						data.minMultipleValues = multipleAttr.minValues;
						data.maxMultipleValues = multipleAttr.maxValues;
					}
					if (genericAttr != null)
					{
						data.allowedTypes = genericAttr.allowedTypes;
						data.generic = true;
					}
					if (mirrorAttr != null)
						data.mirroredField = mirrorAttr.fieldName;
					if (notRequiredAttr != null)
						data.required = false;
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
					if (data.required && anchorType == PWAnchorType.Output)
						data.required = false;
					data.classAQName = GetType().AssemblyQualifiedName;
					data.fieldName = field.Name;
					data.anchorType = anchorType;
					data.type = (SerializableType)field.FieldType;
					data.first.color = (SerializableColor)backgroundColor;
					data.first.linkType = GetLinkTypeFromType(field.FieldType);
					data.first.name = name;
					data.offset = offset;
					data.windowId = windowId;

					//add missing values to instance of list:
					if (data.multiple && data.anchorInstance != null)
					{
						//add minimum number of anchors to render:
						if (data.multipleValueCount < data.minMultipleValues)
							for (int i = data.multipleValueCount; i < data.minMultipleValues; i++)
								data.AddNewAnchor(backgroundColor, field.Name.GetHashCode() + i + 1);

						var PWValuesInstance = data.anchorInstance as PWValues;

						if (PWValuesInstance != null)
							while (PWValuesInstance.Count < data.multipleValueCount)
								PWValuesInstance.Add(null);
					}
				}
			}

			//Check mirrored fields compatibility:
			foreach (var kp in propertyDatas)
				if (kp.Value.mirroredField != null)
				{
					if (propertyDatas.ContainsKey(kp.Value.mirroredField))
					{
						var type = propertyDatas[kp.Value.mirroredField].type;
						if (type != kp.Value.type)
						{
							Debug.LogWarning("incompatible mirrored type in " + GetType());
							kp.Value.mirroredField = null;
							continue ;
						}
					}
					else
						kp.Value.mirroredField = null;
				}


			//remove inhexistants dictionary entries:
			foreach (var kp in propertyDatas)
				if (!actualFields.Contains(kp.Key))
				{
					Debug.Log("removed " + kp.Key);
					propertyDatas.Remove(kp.Key);
				}
		}

		public static PWLinkType GetLinkTypeFromType(Type fieldType)
		{
			if (fieldType == typeof(Sampler2D))
				return PWLinkType.Sampler2D;
			if (fieldType == typeof(Sampler3D))
				return PWLinkType.Sampler3D;
			if (fieldType == typeof(Vector3) || fieldType == typeof(Vector3i))
				return PWLinkType.ThreeChannel;
			if (fieldType == typeof(Vector4))
				return PWLinkType.FourChannel;
			return PWLinkType.BasicData;
		}

		public void SetWindowId(int id)
		{
			windowId = id;
			ForeachPWAnchors((data, singleAnchor, i) => {
				data.windowId = id;
			});
		}

		public void OnGUI()
		{
			EditorGUILayout.LabelField("You are on the wrong window !");
		}

		#if UNITY_EDITOR
		public void OnWindowGUI(int id)
		{
			if (boxAnchorStyle == null)
			{
				boxAnchorStyle =  new GUIStyle(GUI.skin.box);
				boxAnchorStyle.padding = new RectOffset(0, 0, 1, 1);
			}

			// set the header of the window as draggable:
			int width = (int) windowRect.width;
			Rect dragRect = new Rect(0, 0, width, 20);
			if (Event.current.type == EventType.MouseDown && dragRect.Contains(Event.current.mousePosition))
				isDragged = true;
			if (Event.current.type == EventType.MouseUp)
				isDragged = false;
			if (id != -1)
				GUI.DragWindow(dragRect);

			int	debugViewH = 0;
			#if DEBUG_WINDOW
				GUIStyle debugstyle = new GUIStyle();
				debugstyle.normal.background = highlightAddTexture;

				EditorGUILayout.BeginVertical(debugstyle);
				EditorGUILayout.LabelField("Id: " + windowId + " | Compute order: " + computeOrder);
				EditorGUILayout.LabelField("Render order: " + windowRenderOrder++);
				EditorGUILayout.LabelField("Dependencies:");
				foreach (var dep in depencendies)
					EditorGUILayout.LabelField("    " + dep.windowId + " : " + dep.anchorId);
				EditorGUILayout.LabelField("Links:");
				foreach (var l in links)
					EditorGUILayout.LabelField("    " + l.distantWindowId + " : " + l.distantAnchorId);
				EditorGUILayout.EndVertical();
				debugViewH = (int)GUILayoutUtility.GetLastRect().height + 6; //add the padding and margin
			#endif

			GUILayout.BeginVertical();
			{
				RectOffset savedmargin = GUI.skin.label.margin;
				GUI.skin.label.margin = new RectOffset(2, 2, 5, 7);
				OnNodeGUI();
				GUI.skin.label.margin = savedmargin;
			}
			GUILayout.EndVertical();

			int viewH = (int)GUILayoutUtility.GetLastRect().height;
			if (Event.current.type == EventType.Repaint)
				viewHeight = viewH + debugViewH;

			viewHeight = Mathf.Max(viewHeight, maxAnchorRenderHeight);
				
			if (Event.current.type == EventType.Repaint)
				viewHeight += 24;
		}
		#endif
	
		public virtual void	OnNodeGUI()
		{
			EditorGUILayout.LabelField("empty node");
		}

		public void Process()
		{
			foreach (var kp in propertyDatas)
				if (kp.Value.mirroredField != null)
				{
					var val = kp.Value.anchorInstance;
					var mirroredProp = propertyDatas[kp.Value.mirroredField];
					bakedNodeFields[mirroredProp.fieldName].SetValue(this, val);
				}
				
			//send anchor connection events:
			if (lastAttachedLink != null)
			{
				OnNodeAnchorLink(lastAttachedLink.first, lastAttachedLink.second);
				lastAttachedLink = null;
			}

			OnNodeProcess();
		}

		public virtual void OnNodeProcess()
		{
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
				ret = new PWAnchorInfo(data.fieldName, anchorRect,
					singleAnchor.color, data.type,
					data.anchorType, windowId, singleAnchor.id,
					data.classAQName, index,
					data.generic, data.allowedTypes,
					singleAnchor.linkType, singleAnchor.linkCount);
			if (anchorRect.Contains(Event.current.mousePosition))
				ret.mouseAbove = true;
		}

		public PWAnchorInfo ProcessAnchors()
		{
			PWAnchorInfo ret = new PWAnchorInfo();
			
			int		anchorWidth = 38;
			int		anchorHeight = 16;

			Rect	winRect = windowRect;
			Rect	inputAnchorRect = new Rect(winRect.xMin - anchorWidth + 2, winRect.y + 20, anchorWidth, anchorHeight);
			Rect	outputAnchorRect = new Rect(winRect.xMax - 2, winRect.y + 20, anchorWidth, anchorHeight);
			ForeachPWAnchors((data, singleAnchor, i) => {
				//process anchor event and calcul rect position if visible
				if (singleAnchor.visibility != PWVisibility.Gone)
				{
					if (singleAnchor.visibility != PWVisibility.Gone && i <= 0)
					{
						if (data.anchorType == PWAnchorType.Input)
							inputAnchorRect.position += data.offset;
						else if (data.anchorType == PWAnchorType.Output)
							outputAnchorRect.position += data.offset;
					}
					if (singleAnchor.visibility == PWVisibility.Visible)
						ProcessAnchor(data, singleAnchor, ref inputAnchorRect, ref outputAnchorRect, ref ret, i);
					if (singleAnchor.visibility != PWVisibility.Gone)
					{
						if (data.anchorType == PWAnchorType.Input)
							inputAnchorRect.position += Vector2.up * 18;
						else if (data.anchorType == PWAnchorType.Output)
							outputAnchorRect.position += Vector2.up * 18;
					}
				}
			});
			maxAnchorRenderHeight = (int)Mathf.Max(inputAnchorRect.yMin - winRect.y - 20, outputAnchorRect.yMin - windowRect.y - 20);
			return ret;
		}
		
		void RenderAnchor(PWAnchorData data, PWAnchorData.PWAnchorMultiData singleAnchor, int index)
		{
			//if anchor have not been processed:
			if (singleAnchor == null)
				return ;

			string anchorName = (singleAnchor.name.Length > 4) ? singleAnchor.name.Substring(0, 4) : singleAnchor.name;

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
						GUI.DrawTexture(singleAnchor.anchorRect, highlightNewTexture);
						break ;
					case PWAnchorHighlight.AttachReplace:
						GUI.DrawTexture(singleAnchor.anchorRect, highlightReplaceTexture);
						break ;
					case PWAnchorHighlight.AttachAdd:
						GUI.DrawTexture(singleAnchor.anchorRect, highlightAddTexture);
						break ;
				}
			//reset the Highlight:
			singleAnchor.highlighMode = PWAnchorHighlight.None;

			if (data.required && singleAnchor.linkCount == 0
				&& (!data.multiple || (data.multiple && index < data.minMultipleValues)))
			{
				Rect errorIconRect = new Rect(singleAnchor.anchorRect);
				errorIconRect.size = Vector2.one * 17;
				errorIconRect.position += new Vector2(-10, -10);
				GUI.DrawTexture(errorIconRect, errorIcon);
			}


			//if window is renamable, render a text input above the window:
			if (renamable)
			{
				GUIStyle centeredText = new GUIStyle(GUI.skin.textField);
				centeredText.alignment = TextAnchor.UpperCenter;
				centeredText.margin.top += 2;

				Rect renameRect = windowRect;
				renameRect.position += graphDecal - Vector2.up * 18;
				renameRect.size = new Vector2(renameRect.size.x, 30);
				GUI.SetNextControlName("renameWindow");
				name = GUI.TextField(renameRect, name, centeredText);

				if (Event.current.type == EventType.MouseDown && !renameRect.Contains(Event.current.mousePosition))
					GUI.FocusControl(null);
				if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.Escape))
					GUI.FocusControl(null);
			}

			#if DEBUG_WINDOW
				Rect anchorSideRect = singleAnchor.anchorRect;
				if (data.anchorType == PWAnchorType.Input)
				{
					anchorSideRect.position += Vector2.left * 140;
					anchorSideRect.size += Vector2.right * 150;
				}
				else
				{
					anchorSideRect.position -= Vector2.left * 40;
					anchorSideRect.size += Vector2.right * 150;
				}
				GUI.Label(anchorSideRect, "id: " + (long)singleAnchor.id + " | links: " + singleAnchor.linkCount);
			#endif
		}
		
		public void RenderAnchors()
		{
			if (highlightAddTexture == null)
				OnEnable();
			
			ForeachPWAnchors((data, singleAnchor, i) => {
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

		public List< PWLink > GetLinks(int anchorId, int targetWindowId, int targetAnchorId)
		{
			return links.Where(l => l.localAnchorId == anchorId
				&& l.distantWindowId == targetWindowId
				&& l.distantAnchorId == targetAnchorId).ToList();
		}

		public List< PWNodeDependency >	GetDependencies()
		{
			return depencendies;
		}

		public List< PWNodeDependency > GetDependencies(int anchorId)
		{
			return depencendies.Where(d => d.connectedAnchorId == anchorId).ToList();
		}
		
		bool			AnchorAreAssignable(Type fromType, PWAnchorType fromAnchorType, bool fromGeneric, SerializableType[] fromAllowedTypes, PWAnchorInfo to, bool verbose = false)
		{
			if ((fromType != typeof(PWValues) && to.fieldType != typeof(PWValues)) //exclude PWValues to simple assignation (we need to check with allowedTypes)
				&& (fromType.IsAssignableFrom(to.fieldType) || fromType == typeof(object) || to.fieldType == typeof(object)))
			{
				if (verbose)
					Debug.Log(fromType.ToString() + " is assignable from " + to.fieldType.ToString());
				return true;
			}

			if (fromGeneric || to.generic)
			{
				if (verbose)
					Debug.Log("from type is generic");
				SerializableType[] types = (fromGeneric) ? fromAllowedTypes : to.allowedTypes;
				Type secondType = (fromGeneric) ? to.fieldType : fromType;
				foreach (Type firstT in types)
					if (fromGeneric && to.generic)
					{
						if (verbose)
							Debug.Log("to type is generic");
						foreach (Type toT in to.allowedTypes)
						{
							if (verbose)
								Debug.Log("checking assignable from " + firstT + " to " + toT);
							if (firstT.IsAssignableFrom(toT))
								return true;
						}
					}
					else
					{
						if (verbose)
							Debug.Log("checking assignable from " + firstT + " to " + secondType);
						if (firstT.IsAssignableFrom(secondType))
							return true;
					}
			}
			else
			{
				if (verbose)
					Debug.Log("checking assignable from " + fromType + " to " + to.fieldType);
				if (fromType.IsAssignableFrom(to.fieldType))
					return true;
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
				outputHasChanged = true;
				links.Add(new PWLink(
					to.windowId, to.anchorId, to.name, to.classAQName, to.propIndex,
					from.windowId, from.anchorId, from.name, from.classAQName, from.propIndex, from.anchorColor)
				);
				lastAttachedLink = new Pair< string, int>(from.name, from.propIndex);
				//mark local output anchors as linked:
				ForeachPWAnchors((data, singleAnchor, i) => {
					if (singleAnchor.id == from.anchorId)
						singleAnchor.linkCount++;
				});
			}
			else //input links are stored as depencencies:
			{
				inputHasChanged = true;
				ForeachPWAnchors((data, singleAnchor, i) => {
					if (singleAnchor.id == from.anchorId)
					{
						lastAttachedLink = new Pair< string, int>(from.name, from.propIndex);
						singleAnchor.linkCount++;
						//if data was added to multi-anchor:
						if (data.multiple && data.anchorInstance != null)
						{
							Debug.Log("added new anchor at PWValues: " + data.anchorInstance.GetHashCode());
							if (i == data.multipleValueCount)
								data.AddNewAnchor(data.fieldName.GetHashCode() + i + 1);
						}
						if (data.mirroredField != null)
						{
							//no need to check if anchorInstance is null because is is assigned from mirrored property.
							var mirroredProp = propertyDatas[data.mirroredField];
							if ((Type)mirroredProp.type == typeof(PWValues))
								mirroredProp.AddNewAnchor(mirroredProp.fieldName.GetHashCode() + i + 1);
						}
					}
				});
				depencendies.Add(new PWNodeDependency(to.windowId, to.anchorId, from.anchorId));
			}
		}

		public void AttachLink(string myAnchor, PWNode target, string targetAnchor)
		{
			if (!propertyDatas.ContainsKey(myAnchor) || !target.propertyDatas.ContainsKey(targetAnchor))
			{
				Debug.LogWarning("property not found: \"" + targetAnchor + "\" in " + target);
				return ;
			}

			PWAnchorData fromAnchor = propertyDatas[myAnchor];
			PWAnchorData toAnchor = target.propertyDatas[targetAnchor];

			PWAnchorInfo from = new PWAnchorInfo(
					fromAnchor.fieldName, new Rect(), Color.white,
					fromAnchor.type, fromAnchor.anchorType, fromAnchor.windowId,
					fromAnchor.first.id, fromAnchor.classAQName,
					(fromAnchor.multiple) ? 0 : -1, fromAnchor.generic, fromAnchor.allowedTypes,
					fromAnchor.first.linkType, fromAnchor.first.linkCount
			);
			PWAnchorInfo to = new PWAnchorInfo(
				toAnchor.fieldName, new Rect(), Color.white,
				toAnchor.type, toAnchor.anchorType, toAnchor.windowId,
				toAnchor.first.id, toAnchor.classAQName,
				(toAnchor.multiple) ? 0 : -1, toAnchor.generic, toAnchor.allowedTypes,
				toAnchor.first.linkType, toAnchor.first.linkCount
			);

			AttachLink(from, to);
		}

		int DeleteDependencies(Func< PWNodeDependency, bool > pred)
		{
			PWAnchorData.PWAnchorMultiData	singleAnchor;
			PWAnchorData.PWAnchorMultiData	multiAnchor;
			PWAnchorData					data;
			int								index;
			int								nDeleted = 0;

			depencendies.RemoveAll(d => {
				bool delete = pred(d);
				if (delete)
				{
					data = GetAnchorData(d.connectedAnchorId, out singleAnchor, out index);
					if (data == null)
						return delete;
					singleAnchor.linkCount--;
					nDeleted++;
					if (data.multiple)
					{
						PWValues vals = bakedNodeFields[data.fieldName].GetValue(this) as PWValues;
						vals.AssignAt(index, null, null);
						for (int i = vals.Count - 1; i != 0 && i >= data.minMultipleValues ; i--)
						{
							int id = data.fieldName.GetHashCode() + i;
							if (GetAnchorData(id, out multiAnchor) != null && multiAnchor.linkCount == 0)
							{
								vals.RemoveAt(i);
								data.multi.RemoveAt(i);
								data.multipleValueCount--;
								if (GetAnchorData(id + 1, out multiAnchor) != null)
									multiAnchor.id--;
							}
							else if (GetAnchorData(id, out multiAnchor) != null)
								break ;
						}
					}
					else
						bakedNodeFields[data.fieldName].SetValue(this, null);
				}
				return delete;
			});
			return nDeleted;
		}

		public void		DeleteAllLinkOnAnchor(int anchorId)
		{
			links.RemoveAll(l => {
				bool delete = l.localAnchorId == anchorId;
				if (delete)
					OnNodeAnchorUnlink(l.localName, l.localIndex);
				return delete;
			});
			if (DeleteDependencies(d => d.connectedAnchorId == anchorId) == 0)
			{
				PWAnchorData.PWAnchorMultiData singleAnchorData;
				GetAnchorData(anchorId, out singleAnchorData);
				singleAnchorData.linkCount = 0;
			}
		}

		public void		DeleteLink(int myAnchorId, PWNode distantWindow, int distantAnchorId)
		{
			links.RemoveAll(l => {
				bool delete = l.localAnchorId == myAnchorId && l.distantWindowId == distantWindow.windowId && l.distantAnchorId == distantAnchorId;
				if (delete)
					OnNodeAnchorUnlink(l.localName, l.localIndex);
				return delete;
			});
			//delete dependency and if it's not a dependency, decrement the linkCount of the link.
			if (DeleteDependencies(d => d.windowId == distantWindow.windowId && d.connectedAnchorId == myAnchorId && d.anchorId == distantAnchorId) == 0)
			{
				PWAnchorData.PWAnchorMultiData singleAnchorData;
				GetAnchorData(myAnchorId, out singleAnchorData);
				if (singleAnchorData != null)
					singleAnchorData.linkCount--;
			}
		}

		public void		DeleteDependency(int targetWindowId, int distantAnchorId)
		{
			DeleteDependencies(d => d.windowId == targetWindowId && d.anchorId == distantAnchorId);
		}
		
		public void		DeleteLinkByWindowTarget(int targetWindowId)
		{
			PWAnchorData.PWAnchorMultiData singleAnchorData;
			for (int i = 0; i < links.Count; i++)
				if (links[i].distantWindowId == targetWindowId)
				{
					OnNodeAnchorUnlink(links[i].localName, links[i].localIndex);
					GetAnchorData(links[i].localAnchorId, out singleAnchorData);
					singleAnchorData.linkCount--;
					links.RemoveAt(i--);
				}
		}

		public void		DeleteDependenciesByWindowTarget(int targetWindowId)
		{
			DeleteDependencies(d => d.windowId == targetWindowId);
		}

		public void		DeleteAllLinks()
		{
			foreach (var l in links)
				OnNodeAnchorUnlink(l.localName, l.localIndex);
			links.Clear();
			depencendies.Clear();
		}

		public List< Pair < int, int > >	GetAnchorConnections(int anchorId)
		{
			return depencendies.Where(d => d.connectedAnchorId == anchorId)
					.Select(d => new Pair< int, int >(d.windowId, d.anchorId))
					.Concat(links.Where(l => l.localAnchorId == anchorId)
						.Select(l => new Pair< int, int >(l.distantWindowId, l.distantAnchorId))
					).ToList();
		}

		public PWAnchorData	GetAnchorData(int id, out PWAnchorData.PWAnchorMultiData singleAnchorData)
		{
			int				index;
			
			return GetAnchorData(id, out singleAnchorData, out index);
		}

		public PWAnchorData	GetAnchorData(int id, out PWAnchorData.PWAnchorMultiData singleAnchorData, out int index)
		{
			PWAnchorData					ret = null;
			PWAnchorData.PWAnchorMultiData	s = null;
			int								retIndex = 0;

			ForeachPWAnchors((data, singleAnchor, i) => {
				if (singleAnchor.id == id)
				{
					s = singleAnchor;
					ret = data;
					retIndex = i;
				}
			}, true);
			index = retIndex;
			singleAnchorData = s;
			return ret;
		}

		public Rect?	GetAnchorRect(int id)
		{
			var matches =	from p in propertyDatas
							from p2 in p.Value.multi
							where p2.id == id
							select p2;

			if (matches.Count() == 0)
				return null;
			return matches.First().anchorRect;
		}

		public bool		CheckRequiredAnchorLink()
		{
			bool	ret = true;

			ForeachPWAnchors((data, singleAnchor, i) => {
			if (data.required && singleAnchor.linkCount == 0
					&& (!data.multiple || (data.multiple && i < data.minMultipleValues)))
				ret = false;
			});
			return ret;
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

			ForeachPWAnchors((data, singleAnchor, i) => {
				//Hide anchors and highlight when mouse hover
				// Debug.Log(data.windowId + ":" + data.fieldName + ": " + AnchorAreAssignable(data.type, data.anchorType, data.generic, data.allowedTypes, toLink, true));
				if (data.windowId != toLink.windowId
					&& data.anchorType == anchorType
					&& AnchorAreAssignable(data.type, data.anchorType, data.generic, data.allowedTypes, toLink, false))
				{
					if (data.multiple)
					{
						//display additional anchor to attach on next rendering
						data.displayHiddenMultipleAnchors = true;
					}
					if (singleAnchor.anchorRect.Contains(Event.current.mousePosition))
						if (singleAnchor.visibility == PWVisibility.Visible)
						{
							singleAnchor.highlighMode = PWAnchorHighlight.AttachNew;
							if (singleAnchor.linkCount > 0)
							{
								//if anchor is locked:
								if (data.anchorType == PWAnchorType.Input)
									singleAnchor.highlighMode = PWAnchorHighlight.AttachReplace;
								else
									singleAnchor.highlighMode = PWAnchorHighlight.AttachAdd;
							}
						}
				}
				else if (singleAnchor.visibility == PWVisibility.Visible
					&& singleAnchor.id != toLink.anchorId
					&& singleAnchor.linkCount == 0)
					singleAnchor.visibility = PWVisibility.InvisibleWhenLinking;
			});
		}

		public void		EndFrameUpdate()
		{
			//reset values at the end of the frame
			if (Event.current.type == EventType.Layout)
			{
				oldSeed = seed;
				oldChunkPosition = chunkPosition;
				oldChunkSize = chunkSize;
				seedHasChanged = false;
				positionHasChanged = false;
				chunkSizeHasChanged = false;
				inputHasChanged = false;
				outputHasChanged = false;
				reloadRequested = false;
				justReloaded = false;
			}
		}

		public virtual void	OnNodeAnchorLink(string propName, int index)
		{
		}

		public virtual void OnNodeAnchorUnlink(string propName, int index)
		{
		}

		public bool		WindowShouldClose()
		{
			return windowShouldClose;
		}

		public void		DisplayHiddenMultipleAnchors(bool display = true)
		{
			ForeachPWAnchorDatas((data)=> {
				if (data.multiple)
					data.displayHiddenMultipleAnchors = display;
			});
		}

		/* Utils function to manipulate PWnode variables */

		public void		UpdatePropEnabled(string propertyName, bool enabled, int index = 0)
		{
			if (propertyDatas.ContainsKey(propertyName))
			{
				var anchors = propertyDatas[propertyName].multi;
				if (anchors.Count <= index)
					return ;
				anchors[index].enabled = true;
			}
		}

		public void		UpdatePropName(string propertyName, string newName, int index = 0)
		{
			if (propertyDatas.ContainsKey(propertyName))
			{
				var anchors = propertyDatas[propertyName].multi;
				if (anchors.Count <= index)
					return ;
				anchors[index].name = newName;
			}
		}

		public void		UpdatePropBackgroundColor(string propertyName, Color newColor, int index = 0)
		{
			if (propertyDatas.ContainsKey(propertyName))
			{
				var anchors = propertyDatas[propertyName].multi;
				if (anchors.Count <= index)
					return ;
				anchors[index].color = (SerializableColor)newColor;
			}
		}

		public void		UpdatePropVisibility(string propertyName, PWVisibility visibility, int index = 0)
		{
			if (propertyDatas.ContainsKey(propertyName))
			{
				var anchors = propertyDatas[propertyName].multi;
				if (anchors.Count <= index)
					return ;
				anchors[index].visibility = visibility;
			}
		}

		public int		GetPropLinkCount(string propertyName, int index = 0)
		{
			if (propertyDatas.ContainsKey(propertyName))
			{
				var anchors = propertyDatas[propertyName].multi;
				if (anchors.Count <= index)
					return -1;
				return anchors[index].linkCount;
			}
			return -1;
		}

		public Rect?	GetAnchorRect(string propertyName, int index = 0)
		{
			if (propertyDatas.ContainsKey(propertyName))
			{
				var anchors = propertyDatas[propertyName].multi;
				if (anchors.Count <= index)
					return null;
				return anchors[index].anchorRect;
			}
			return null;
		}

		public PWAnchorData	GetAnchorData(string propName)
		{
			if (propertyDatas.ContainsKey(propName))
				return propertyDatas[propName];
			return null;
		}
    }
}