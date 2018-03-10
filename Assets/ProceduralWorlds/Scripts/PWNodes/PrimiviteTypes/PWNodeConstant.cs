using System.Collections.Generic;
using UnityEngine;
using PW.Core;

namespace PW.Node
{
	public class PWNodeConstant : PWNode
	{

		public enum ConstantType
		{
			Int,
			Float,
			Vector2,
			Vector3,
			Vector4,
		}

		[PWOutput("int")]
		public int		outi;

		[PWOutput("float")]
		public float	outf;

		[PWOutput("Vector2")]
		public Vector2	outv2;

		[PWOutput("Vector3")]
		public Vector3	outv3;

		[PWOutput("Vector4")]
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
					SetAnchorVisibility(propKp.Value, PWVisibility.Visible);
				else
				{
					SetAnchorVisibility(propKp.Value, PWVisibility.Gone);
					RemoveAllLinksFromAnchor(propKp.Value);
				}
		}
	}
}
