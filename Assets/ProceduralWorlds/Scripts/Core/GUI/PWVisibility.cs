
namespace PW.Core
{
	[System.SerializableAttribute]
	public enum PWVisibility
	{
		Visible,				//anchor is visible
		Invisible,				//anchor is invisible but the positions will be calulated as if it was not
		Gone,					//anchor is invisible and his size is ignored
	}
}