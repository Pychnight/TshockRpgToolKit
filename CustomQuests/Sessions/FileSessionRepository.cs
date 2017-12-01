using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomQuests.Sessions
{
    internal class FileSessionRepository : SessionRepository
    {
        internal string BasePath { get; private set; }

        internal FileSessionRepository(string basePath)
        {
			BasePath = basePath;       
        }

        internal override SessionInfo Load(string userName)
        {
			Console.WriteLine($"FileSessionRepo.Load({userName})");
			SessionInfo sessionInfo = null;
			var path = Path.Combine(BasePath, $"{userName}.json");
			
			if(File.Exists(path))
			{
				sessionInfo = JsonConvert.DeserializeObject<SessionInfo>(File.ReadAllText(path));
			}

			return sessionInfo;
		}

        internal override void Save(Session session, string userName)
        {
			Console.WriteLine($"FileSessionRepo.Save [session]({userName})");
			var path = Path.Combine(BasePath, $"{userName}.json");
            File.WriteAllText(path, JsonConvert.SerializeObject(session, Formatting.Indented));
        }

		internal override void Save(SessionInfo sessionInfo, string userName)
		{
			Console.WriteLine($"FileSessionRepo.Save [sessionInfo]({userName})");
			var path = Path.Combine(BasePath, $"{userName}.json");
			File.WriteAllText(path, JsonConvert.SerializeObject(sessionInfo, Formatting.Indented));
		}
	}
}
