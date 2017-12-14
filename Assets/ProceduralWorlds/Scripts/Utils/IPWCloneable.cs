using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PW.Core
{
	public interface IPWCloneable< T > {
	
		T Clone(T reuseExisting);
	
	}
}