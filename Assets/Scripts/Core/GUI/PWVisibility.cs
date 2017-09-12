
namespace PW.Core
{
	[System.SerializableAttribute]
	public enum PWVisibility
	{
		Visible,
		Invisible,
		InvisibleWhenLinking,	//anchor is invisible while user is linking two anchors:
		Gone,					//anchor is invisible and his size is ignored
	}
}