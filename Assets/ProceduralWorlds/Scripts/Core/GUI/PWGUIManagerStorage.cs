using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PW.Core
{
	[System.Serializable]
	public class PWGUIManagerStorage
	{
	
		[SerializeField]
		public List< PWGUISettings >	settingsStorage = new List< PWGUISettings >();
	
	}
}
	