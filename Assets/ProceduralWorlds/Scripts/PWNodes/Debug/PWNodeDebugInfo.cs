using UnityEngine;
using System;
using PW.Core;
using PW.Biomator;

namespace PW.Node
{
	public class PWNodeDebugInfo : PWNode
	{
		[PWInput]
		public object		obj;

		public override void OnNodeCreation()
		{
			name = "Debug Info";
			renamable = true;
			obj = "null";
		}
	}
}
