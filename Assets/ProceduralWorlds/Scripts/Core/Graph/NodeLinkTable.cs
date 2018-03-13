using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.Core
{
	[System.Serializable]
	public class NodeLinkTable : ISerializationCallbackReceiver
	{
	
		[System.Serializable]
		public class LinkTable : SerializableDictionary< string, NodeLink > {}
		[System.Serializable]
		public class AnchorLinkTable : SerializableDictionary< string, List< string > > {}
	
		[SerializeField]
		LinkTable		linkTable = new LinkTable();

		readonly bool			debug = false;
	
		//to be called, fmorAnchor and toAnchor fields in NodeLink must be valid
		public void				AddLink(NodeLink link)
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
		public void				RemoveLink(NodeLink link)
		{
			if (!linkTable.Remove(link.GUID))
				Debug.LogWarning("[LinkTable] Attempt to remove inexistent link at GUID: " + link.GUID);
		}
	
		public NodeLink		GetLinkFromGUID(string linkGUID)
		{
			NodeLink	ret = null;
	
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

		public IEnumerable< NodeLink > GetLinks()
		{
			foreach (var linkKp in linkTable)
				yield return linkKp.Value;
		}
	}
}