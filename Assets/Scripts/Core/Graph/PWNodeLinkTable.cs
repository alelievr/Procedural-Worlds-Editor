﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PW.Core
{
	[System.Serializable]
	public class PWNodeLinkTable : ISerializationCallbackReceiver {
	
		[System.Serializable]
		public class LinkTable : SerializableDictionary< string, PWNodeLink > {}
		[System.Serializable]
		public class AnchorLinkTable : SerializableDictionary< string, List< string > > {}
	
		[SerializeField]
		LinkTable		linkTable = new LinkTable();
	
		[SerializeField]
		AnchorLinkTable	anchorLinkTable = new AnchorLinkTable();
	
		public void AddLink(string anchorGUID, PWNodeLink link)
		{
			linkTable[link.GUID] = link;

			if (!anchorLinkTable.ContainsKey(anchorGUID) || anchorLinkTable[anchorGUID] == null)
				anchorLinkTable[anchorGUID] = new List< string >();
			
			anchorLinkTable[anchorGUID].Add(link.GUID);
		}

		public void RemoveLink(string anchorGUID, PWNodeLink link)
		{
			if (!linkTable.Remove(link.GUID))
				Debug.LogWarning("[LinkTable] Attempt to remove inexistent link at GUID: " + link.GUID);
			if (!anchorLinkTable[anchorGUID].Remove(link.GUID))
				Debug.LogWarning("[LiknTable] Attempt to remove inexistent link " + link.GUID + " in anchor " + anchorGUID);
		}
	
		public List< string >    GetLinkGUIDsFromAnchorGUID(string anchorGUID)
		{
			List< string >	ret = null;
	
			anchorLinkTable.TryGetValue(anchorGUID, out ret);
			return ret;
		}
	
		public PWNodeLink		GetLinkFromGUID(string linkGUID)
		{
			PWNodeLink	ret = null;
	
			linkTable.TryGetValue(linkGUID, out ret);
			return ret;
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			Debug.Log("after serialization: dict keys: " + linkTable.Count);
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			Debug.Log("Before serialization: dict keys: " + linkTable.Count);
		}
	}
}