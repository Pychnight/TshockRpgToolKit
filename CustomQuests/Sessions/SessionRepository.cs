using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaApi.Server;

namespace CustomQuests.Sessions
{
    internal abstract class SessionRepository : IDisposable
    {
		internal bool IsDisposed { get; private set; }
		protected internal TerrariaPlugin plugin;

		internal abstract SessionInfo Load(string userName);
		internal abstract void Save(SessionInfo sessionInfo, string userName);
		internal abstract void Save(Session session, string userName);

		internal virtual void OnDispose(bool isDisposing)
		{
		}

		protected void Dispose(bool isDisposing)
		{
			if(IsDisposed)
				return;

			OnDispose(isDisposing);

			IsDisposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
		}
	}
}
