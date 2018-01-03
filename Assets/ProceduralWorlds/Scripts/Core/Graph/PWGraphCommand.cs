using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PW.Core
{
	public enum PWGraphCommandType
	{
		NewNode,
		NewNodePosition,
		Link,
		LinkAnchor,
	}
	
	public class PWGraphCommand
	{
	
		public PWGraphCommandType	type;
		public Vector2				position;
		public bool					forcePositon;
		public string				name;
		public Type					nodeType;
		public string				fromName;
		public string				toName;
		public string				attributes;

		//New node constructor
		public PWGraphCommand(Type nodeType, string name, string attributes = null)
		{
			this.type = PWGraphCommandType.NewNode;
			this.nodeType = nodeType;
			this.name = name;
			this.forcePositon = false;
			this.attributes = attributes;
		}

		//New node with position constructor
		public PWGraphCommand(Type nodeType, string name, Vector2 position, string attributes = null)
		{
			this.type = PWGraphCommandType.NewNodePosition;
			this.nodeType = nodeType;
			this.name = name;
			this.position = position;
			this.forcePositon = true;
			this.attributes = attributes;
		}

		//new link constructor
		public PWGraphCommand(string fromNodeName, string toNodeName)
		{
			this.type = PWGraphCommandType.Link;
			this.fromName = fromNodeName;
			this.toName = toNodeName;
		}

		//new link with anchor index constructor
		public PWGraphCommand(string fromNodeName, int fromAnchorFieldIndex, string toNodeName, int toAnchorFieldIndex)
		{
			this.type = PWGraphCommandType.Link;
			this.toName = toNodeName;
			this.fromName = fromNodeName;
		}

		public static bool operator ==(PWGraphCommand cmd1, PWGraphCommand cmd2)
		{
			return	cmd1.type == cmd2.type
					&& cmd1.forcePositon == cmd2.forcePositon
					&& cmd1.fromName == cmd2.fromName
					&& cmd1.toName == cmd2.toName
					&& cmd1.name == cmd2.name
					&& cmd1.position == cmd2.position;
		}

		public static bool operator !=(PWGraphCommand cmd1, PWGraphCommand cmd2)
		{
			return !(cmd1 == cmd2);
		}

		public override bool Equals(object cmd)
		{
			if (cmd as PWGraphCommand == null)
				return false;
			
			return ((cmd as PWGraphCommand) == this);
		}

		public override int GetHashCode()
		{
			return position.GetHashCode()
				+ type.GetHashCode()
				+ forcePositon.GetHashCode()
				+ name.GetHashCode()
				+ nodeType.GetHashCode()
				+ fromName.GetHashCode()
				+ toName.GetHashCode();
		}
	
	}
}