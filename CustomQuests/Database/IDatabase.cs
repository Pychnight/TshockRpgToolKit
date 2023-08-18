using CustomQuests.Sessions;

namespace CustomQuests.Database
{
	public interface IDatabase //: IDisposable
	{
		string ConnectionString { get; set; }

		SessionInfo Read(string playerName);
		void Write(SessionInfo sessionInfo, string playerName);
	}
}
