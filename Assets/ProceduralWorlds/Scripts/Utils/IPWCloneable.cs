using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralWorlds.Core
{
	public interface IPWCloneable< T > {
	
		T Clone(T reuseExisting);
	
	}
}