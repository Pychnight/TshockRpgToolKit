using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomQuests.Sessions
{
    internal abstract class SessionRepository
    {
        internal abstract SessionInfo Load(string userName);
		internal abstract void Save(SessionInfo sessionInfo, string userName);
		internal abstract void Save(Session session, string userName);
	}
}
