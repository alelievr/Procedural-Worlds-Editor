using UnityEngine;
using PW.Core;

namespace PW.Editor
{
	public interface IPWLayoutSeparator
	{
		void Initialize(PWGraphEditor graphEditor);

		void UpdateLayoutSettings(PWLayoutSettings layoutSettings);

		Rect Begin();

		Rect Split();

		Rect End();
	}
}