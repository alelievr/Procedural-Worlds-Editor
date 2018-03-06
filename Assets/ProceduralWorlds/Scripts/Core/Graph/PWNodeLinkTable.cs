using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PW.Core
{
	[System.Serializable]
	public class PWNodeLinkTable : ISerializationCallbackReceiver
	{
	
		[System.Serializable]
		public class LinkTable : SerializableDictionary< string, PWNodeLink > {}
		[System.Serializable]
		public class AnchorLinkTable : SerializableDictionary< string, List< string > > {}
	
		[SerializeField]
		LinkTable		linkTable = new LinkTable();

		readonly bool			debug;
	
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
		}

		//this method will be called twice per link removed, one per link's anchor.
		public void				RemoveLink(PWNodeLink link)
		{
			if (!linkTable.Remove(link.GUID))
				Debug.LogWarning("[LinkTable] Attempt to remove inexistent link at GUID: " + link.GUID);
		}
	
		public PWNodeLink		GetLinkFromGUID(string linkGUID)
		{
			PWNodeLink	ret = null;
	
			linkTable.TryGetValue(linkGUID, out ret);
			return ret;
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (debug)
				Debug.Log("after serialization: dict keys: " + linkTable.Count);
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			if (debug)
				Debug.Log("Before serialization: dict keys: " + linkTable.Count);
		}

		public IEnumerable< PWNodeLink > GetLinks()
		{
			foreach (var linkKp in linkTable)
				yield return linkKp.Value;
		}
	}
}