using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.Core
{
	[System.Serializable]
	public class ProceduralWorldsGUIStorage
	{
	
		[SerializeField]
		public List< PWGUISettings >	settingsStorage = new List< PWGUISettings >();
	
	}
}
	