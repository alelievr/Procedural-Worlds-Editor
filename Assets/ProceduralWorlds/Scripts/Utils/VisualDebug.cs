using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.Core
{
	public class VisualDebug
	{
		public List < Frame >	frames = new List< Frame >();

		Frame					currentFrame;
		Color					currentColor;

		public class Frame
		{
			public string			name;
			public List< View >		infos = new List< View >();

			public Frame(string name)
			{
				this.name = name;
			}
		}

		public abstract class View
		{
			public Vector3	position;
			public Color	color;
	
			public View(Vector3 position)
			{
				this.position = position;
			}

			public View() {}
		}
		
		public class PointView : View
		{
			public float size;

			public PointView(Vector3 pos, float size, Color color) : base(pos)
			{
				this.size = size;
				this.color = color;
			}
		}

		public class LabelView : View
		{
			public GUIStyle style;
			public string text;

			public LabelView(Vector3 pos, string text, GUIStyle style = null) : base(pos)
			{
				this.text = text;
				this.style = style;
			}
		}

		public class TriangleView : View
		{
			public Vector3	p1;
			public Vector3	p2;
			public Vector3	p3;

			public TriangleView(Vector3 p1, Vector3 p2, Vector3 p3, Color color)
			{
				this.color = color;
				this.p1 = p1;
				this.p2 = p2;
				this.p3 = p3;
			}
		}

		public class LineView : View
		{
			public Vector3 p1;
			public Vector3 p2;

			public LineView(Vector3 p1, Vector3 p2, Color color)
			{
				this.p1 = p1;
				this.p2 = p2;
				this.color = color;
			}
		}

		public VisualDebug()
		{
			Initialize();
		}

		public void Initialize()
		{
			frames.Clear();
			BeginFrame("Initial state");
			currentColor = Color.white;
		}

		public void SetColor(Color color)
		{
			currentColor = color;
		}
		
		public void BeginFrame(string name)
		{
			currentFrame = new Frame(name);
			frames.Add(currentFrame);
		}

		public void DrawPoint(Vector3 pos, float size, Color color)
		{
			currentFrame.infos.Add(new PointView(pos, size, color));
		}

		public void DrawPoint(Vector3 pos, float size = .1f)
		{
			DrawPoint(pos, size, currentColor);
		}

		public void DrawTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Color color)
		{
			currentFrame.infos.Add(new TriangleView(p1, p2, p3, color));
		}

		public void DrawTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
		{
			DrawTriangle(p1, p2, p3, currentColor);
		}

		public void DrawLabel(Vector3 pos, string text, GUIStyle style = null)
		{
			currentFrame.infos.Add(new LabelView(pos, text, style));
		}

		public void DrawLine(Vector3 p1, Vector3 p2, Color color)
		{
			currentFrame.infos.Add(new LineView(p1, p2, color));
		}
		
		public void DrawLine(Vector3 p1, Vector3 p2)
		{
			DrawLine(p1, p2, currentColor);
		}

		public void EndFrame()
		{
			currentFrame = null;
		}
	}
}