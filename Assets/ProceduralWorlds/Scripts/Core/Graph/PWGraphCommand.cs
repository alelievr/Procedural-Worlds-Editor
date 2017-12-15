using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PW.Core
{
	public enum PWGraphCommandType
	{
		NewNode,
		Link,
	}
	
	public class PWGraphCommand {
	
		public PWGraphCommandType	type;
		public Vector2				position;
		public string				name;
		public string				fromName;
		public string				toName;
	
	}
}