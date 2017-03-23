namespace PW
{
	[System.SerializableAttribute]
	public class PWNodeDependency
	{
		public int		windowId;
		public int		anchorId;
		public int		connectedAnchorId;

		public PWNodeDependency(int windowId, int anchorId, int connectedAnchorId)
		{
			this.windowId = windowId;
			this.anchorId = anchorId;
			this.connectedAnchorId = connectedAnchorId;
		}
	}
}