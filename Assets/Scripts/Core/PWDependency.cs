namespace PW
{
	[System.SerializableAttribute]
	public class PWNodeDependency
	{
		public int			nodeId;
		public int			anchorId;
		public int			connectedAnchorId;
		public PWLinkMode	mode;

		public PWNodeDependency(int nodeId, int anchorId, int connectedAnchorId)
		{
			this.nodeId = nodeId;
			this.anchorId = anchorId;
			this.connectedAnchorId = connectedAnchorId;
			this.mode = PWLinkMode.AutoProcess;
		}
	}
}