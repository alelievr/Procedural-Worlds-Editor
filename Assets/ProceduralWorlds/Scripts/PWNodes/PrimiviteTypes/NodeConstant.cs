using System.Collections.Generic;
using UnityEngine;
using ProceduralWorlds.Core;

namespace ProceduralWorlds.Nodes
{
	public class NodeConstant : BaseNode
	{

		public enum ConstantType
		{
			Int,
			Float,
			Vector2,
			Vector3,
			Vector4,
		}

		[Output("int")]
		public int		outi;

		[Output("float")]
		public float	outf;

		[Output("Vector2")]
		public Vector2	outv2;

		[Output("Vector3")]
		public Vector3	outv3;

		[Output("Vector4")]
		public Vector4	outv4;

		public ConstantType		selectedConstantType = ConstantType.Float;

		static Dictionary< ConstantType, string > properties = new Dictionary< ConstantType, string >() {
			{ConstantType.Int, "outi"},
			{ConstantType.Float, "outf"},
			{ConstantType.Vector2, "outv2"},
			{ConstantType.Vector3, "outv3"},
			{ConstantType.Vector4, "outv4"},
		};

		public override void OnNodeCreation()
		{
			renamable = true;
			name = "Constant";
		}

		public override void OnNodeEnable()
		{
			UpdateConstantType();
		}

		public void			UpdateConstantType()
		{
			foreach (var propKp in properties)
				if (propKp.Key == selectedConstantType)
					SetAnchorVisibility(propKp.Value, Visibility.Visible);
				else
				{
					SetAnchorVisibility(propKp.Value, Visibility.Gone);
					RemoveAllLinksFromAnchor(propKp.Value);
				}
		}
	}
}
