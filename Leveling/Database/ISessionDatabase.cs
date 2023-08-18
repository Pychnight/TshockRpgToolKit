using Leveling.Sessions;

namespace Leveling.Database
{
	public interface ISessionDatabase
	{
		string ConnectionString { get; }

		SessionDefinition Load(string userName);
		void Save(string userName, SessionDefinition sessionDefinition);
	}
}