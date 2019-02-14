using CustomQuests.Sessions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomQuests.Database
{
	public interface IDatabase //: IDisposable
	{
		string ConnectionString { get; set; }
		
		SessionInfo Read(string playerName);
		void Write(SessionInfo sessionInfo, string playerName);
	}
}
