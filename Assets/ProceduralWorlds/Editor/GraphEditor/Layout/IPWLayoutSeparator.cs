using UnityEngine;
using PW.Core;

namespace PW.Editor
{
	public interface IPWLayoutSeparator
	{
		void Initialize(PWGraphEditor graphEditor);

		PWLayoutSetting UpdateLayoutSetting(PWLayoutSetting layoutSettings);

		Rect Begin();

		Rect Split();

		Rect End();
	}
}