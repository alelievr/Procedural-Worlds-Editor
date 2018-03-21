using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProceduralWorlds.Core
{
	[System.Serializable]
	public class NodeLinkTable : ISerializationCallbackReceiver
	{
		[System.NonSerialized]
		public Dictionary< string, NodeLink > linkTable = new Dictionary< string, NodeLink >();
	
		[SerializeField]
		List< string >		linkGUIDs = new List< string >();
		[SerializeField]
		List< NodeLink >	linkInstancies = new List< NodeLink >();

		readonly bool		debug = false;
	
		//to be called, fromAnchor and toAnchor fields in NodeLink must be valid
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
		
		public bool ContainsLink(string linkGUID)
		{
			return linkTable.ContainsKey(linkGUID);
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			linkTable.Clear();

			//fill node link dictionary for fast access
			for (int i = 0; i < linkGUIDs.Count; i++)
				linkTable[linkGUIDs[i]] = linkInstancies[i];
			
			if (debug)
				Debug.Log("after serialization: dict keys: " + linkTable.Count);
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			//set back dictionary values into lists for serialization
			linkGUIDs = linkTable.Keys.ToList();
			linkInstancies = linkTable.Values.ToList();

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