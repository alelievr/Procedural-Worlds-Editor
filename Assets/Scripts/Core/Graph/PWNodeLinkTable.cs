using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PWNodeLinkTable {

	[SerializeField]
    Dictionary< string, PWNodeLink >		linkTable = new Dictionary< string, PWnodeLink >();

	[SerializeField]
    Dictionary< string, List< string > >	anchorLinkTable = new Dictionary< string, List< string > >();

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

}
