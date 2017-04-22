using UnityEngine;
using System.Collections;


/// Helper script to show what a skin actually looks like
/// To use, throw on a GameObject and edit your skin asset while the game is running.
public class ShowSkin : MonoBehaviour {
	public GUISkin skin;
	public float elemWidth = 100, elemHeight = 30;
	public Texture2D testIcon;
	bool testBool;
	int selection;
	void OnGUI () {
		// Assign this skin to the GUI. If it is null, the GUI will use the builtin skin
		GUI.skin = skin;

		// Read back which skin the GUI is actually using
		GUISkin sk = GUI.skin;
		
		// Make a group that contains all the elements.
		GUI.BeginGroup (new Rect (30,20, Screen.width - 60, Screen.height - 40), sk.name, "window");
		
		GUIStyle window = GUI.skin.GetStyle ("window");
		int x = 0, y = 0;
		// Go over all GUIStyles inside the skin.
		foreach (GUIStyle s in sk) {
			// Display them as a toggle button (toggle buttons use all background images, and you can click to toggle it).
			testBool = GUI.Toggle (new Rect (x * (elemWidth + 20) + window.padding.left, y * (elemHeight + 15) + window.padding.top, elemWidth, elemHeight), testBool, new GUIContent (s.name.ToUpper(), testIcon), s);

			// Advance & "wordwrap" the elements
			x++;
			if (x * (elemWidth + 20) > Screen.width - elemWidth - 40 -window.padding.right) {
				x = 0;
				y++;
			}
		}
		GUI.EndGroup();
	}
}
