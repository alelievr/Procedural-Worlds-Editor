using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPWArray
{
	int Count { get; }

	List< object > GetGenericValues();
	List< string > GetNames();

	object At(int index);
	string NameAt(int index);

	int FindName(string name);

	void GenericAdd(object val);
	void GenericAdd(object val, string name);

	bool GenericAssignAt(int index, object val, string name, bool force = false);
	bool RemoveAt(int index);

	void Clear();
}
