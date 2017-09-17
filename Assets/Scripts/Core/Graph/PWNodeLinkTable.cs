using System.Collections;
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
	
		//to be called, fmorAnchor and toAnchor fields in PWNodeLink must be valid
		public void				AddLink(PWNodeLink link)
		{
			if (link.fromAnchor == null || link.toAnchor == null)
			{
				Debug.LogWarning("[LinkTable] Failed to add link because these anchors are null");
				return ;
			}
			if (linkTable.ContainsKey(link.GUID))
			{
				Debug.LogWarning("[LinkTable] Attempt to add a link already in tha linkTable");
				return ;
			}

			linkTable[link.GUID] = link;

			if (!anchorLinkTable.ContainsKey(link.fromAnchor.GUID) || anchorLinkTable[link.fromAnchor.GUID] == null)
				anchorLinkTable[link.fromAnchor.GUID] = new List< string >();
			if (!anchorLinkTable.ContainsKey(link.toAnchor.GUID) || anchorLinkTable[link.toAnchor.GUID] == null)
				anchorLinkTable[link.toAnchor.GUID] = new List< string >();
			
			anchorLinkTable[link.fromAnchor.GUID].Add(link.GUID);
			anchorLinkTable[link.toAnchor.GUID].Add(link.GUID);
		}

		//this method will be called twice per link removed, one per link's anchor.
		public void				RemoveLink(PWNodeLink link)
		{
			if (!linkTable.Remove(link.GUID))
				Debug.LogWarning("[LinkTable] Attempt to remove inexistent link at GUID: " + link.GUID);
			if (!anchorLinkTable[link.fromAnchor.GUID].Remove(link.GUID))
				Debug.LogWarning("[LiknTable] Attempt to remove inexistent link " + link.GUID + " in anchor " + link.fromAnchor);
			if (!anchorLinkTable[link.toAnchor.GUID].Remove(link.GUID))
				Debug.LogWarning("[LiknTable] Attempt to remove inexistent link " + link.GUID + " in anchor " + link.toAnchor);
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