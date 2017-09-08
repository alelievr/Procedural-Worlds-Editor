namespace PW.Core
{
	[System.SerializableAttribute]
	public class PWDependency
	{
		public int			nodeId;
		public int			anchorId;
		public int			connectedAnchorId;

		public PWDependency(int nodeId, int anchorId, int connectedAnchorId)
		{
			this.nodeId = nodeId;
			this.anchorId = anchorId;
			this.connectedAnchorId = connectedAnchorId;
		}
	}
}
