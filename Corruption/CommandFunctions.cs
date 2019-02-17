using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace Corruption
{
	public static class CommandFunctions
	{
		/// <summary>
		/// Executes a command as the given TSPlayer.
		/// </summary>
		/// <param name="player">TSPlayer.</param>
		/// <param name="command">Command string.</param>
		/// <returns>Status.</returns>
		public static bool ExecuteCommand(TSPlayer player, string command)
		{
			if( player == null || command == null )
				return false;
			
			return Commands.HandleCommand(TSPlayer.Server, command);
		}

		/// <summary>
		/// Executes a command as TSPlayer.Server.
		/// </summary>
		/// <param name="command">Command string</param>
		/// <returns>Status.</returns>
		public static bool ExecuteCommand(string command)
		{
			return ExecuteCommand(TSPlayer.Server, command);
		}
	}
}
