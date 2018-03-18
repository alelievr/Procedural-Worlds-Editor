
namespace ProceduralWorlds.Core
{
	public enum GraphProcessMode
	{
		Normal,		//output a disaplayable terrain (with isosurface / oth)
		Once,		//initialization pass
		Geologic,	//output a structure containing all maps for a chunk (terrain, wet, temp, biomes, ...)
	}
}