namespace Byoe.Core
{
	/// <inheritdoc cref="T:Byoe.Core.ISessionContext" />
	public class SessionContext : ISessionContext
	{
		/// <inheritdoc cref="P:Byoe.Core.ISessionContext.UserName" />
		public string UserName { get; set; }

		/// <inheritdoc cref="P:Byoe.Core.ISessionContext.ClientId" />
		public string ClientId { get; set; }

		/// <inheritdoc cref="P:Byoe.Core.ISessionContext.Tag" />
		public object Tag { get; set; }
	}
}
