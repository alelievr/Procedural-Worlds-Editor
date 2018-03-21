using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ProceduralWorlds.Core
{
	public enum BaseGraphCommandType
	{
		NewNode,
		NewNodePosition,
		Link,
		LinkAnchor,
		LinkAnchorName,
		GraphAttribute,
	}
	
	public class BaseGraphCommand : IEquatable< BaseGraphCommand >
	{
	
		public readonly BaseGraphCommandType	type;
		public readonly Vector2				position;
		public readonly bool					forcePositon;
		public readonly string				name;
		public readonly Type					nodeType;
		public readonly string				fromNodeName;
		public readonly string				toNodeName;

		public readonly string				attributes;

		public readonly int					fromAnchorIndex;
		public readonly int					toAnchorIndex;
		public readonly string				fromAnchorFieldName;
		public readonly string				toAnchorFieldName;

		public readonly string				graphFieldName;
		public readonly object				graphFieldValue;

		//New node constructor
		public BaseGraphCommand(Type nodeType, string name, string attributes = null)
		{
			this.type = BaseGraphCommandType.NewNode;
			this.nodeType = nodeType;
			this.name = name;
			this.forcePositon = false;
			this.attributes = attributes;
		}

		//New node with position constructor
		public BaseGraphCommand(Type nodeType, string name, Vector2 position, string attributes = null)
		{
			this.type = BaseGraphCommandType.NewNodePosition;
			this.nodeType = nodeType;
			this.name = name;
			this.position = position;
			this.forcePositon = true;
			this.attributes = attributes;
		}

		//new link constructor
		public BaseGraphCommand(string fromNodeName, string toNodeName)
		{
			this.type = BaseGraphCommandType.Link;
			this.fromNodeName = fromNodeName;
			this.toNodeName = toNodeName;
		}

		//new link with anchor index constructor
		public BaseGraphCommand(string fromNodeName, int fromAnchorIndex, string toNodeName, int toAnchorIndex)
		{
			this.type = BaseGraphCommandType.LinkAnchor;
			this.fromNodeName = fromNodeName;
			this.fromAnchorIndex = fromAnchorIndex;
			this.toNodeName = toNodeName;
			this.toAnchorIndex = toAnchorIndex;
		}

		public BaseGraphCommand(string fromNodeName, string fromAnchorFieldName, string toNodeName, string toAnchorFieldName)
		{
			this.type = BaseGraphCommandType.LinkAnchorName;
			this.fromNodeName = fromNodeName;
			this.fromAnchorFieldName = fromAnchorFieldName;
			this.toNodeName = toNodeName;
			this.toAnchorFieldName = toAnchorFieldName;
		}

		//Graph attribute
		public BaseGraphCommand(string fieldName, object value)
		{
			this.type = BaseGraphCommandType.GraphAttribute;
			this.graphFieldName = fieldName;
			this.graphFieldValue = value;
		}

		public static bool operator ==(BaseGraphCommand cmd1, BaseGraphCommand cmd2)
		{
			return	cmd1.type == cmd2.type
					&& cmd1.forcePositon == cmd2.forcePositon
					&& cmd1.fromNodeName == cmd2.fromNodeName
					&& cmd1.toNodeName == cmd2.toNodeName
					&& cmd1.name == cmd2.name
					&& cmd1.position == cmd2.position;
		}

		public static bool operator !=(BaseGraphCommand cmd1, BaseGraphCommand cmd2)
		{
			return !(cmd1 == cmd2);
		}

		public override bool Equals(object cmd)
		{
			if (!(cmd is BaseGraphCommand))
				return false;
			
			return ((cmd as BaseGraphCommand) == this);
		}

		public bool Equals(BaseGraphCommand other)
		{
			return this.Equals((object)other);
		}

		public override int GetHashCode()
		{
			return position.GetHashCode()
				+ type.GetHashCode()
				+ forcePositon.GetHashCode()
				+ name.GetHashCode()
				+ nodeType.GetHashCode()
				+ fromNodeName.GetHashCode()
				+ toNodeName.GetHashCode();
		}
	}
}