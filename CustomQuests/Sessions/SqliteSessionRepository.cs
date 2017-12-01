using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomQuests.Sessions
{
    internal class SqliteSessionRepository : SessionRepository
    {
        internal override SessionInfo Load(string userName)
        {
            throw new NotImplementedException();
        }

		internal override void Save(SessionInfo sessionInfo, string userName)
		{
			throw new NotImplementedException();
		}

		internal override void Save(Session session, string userName)
        {
            throw new NotImplementedException();
        }
	}
}
