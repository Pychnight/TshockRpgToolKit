using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace Corruption
{
	public static class PlayerCommandFunctions
	{
		public static bool ExecuteCommand(TSPlayer player, string text)
		{
			if( player == null || text == null )
				return false;

			return HandleCommand(player, text);
		}

		//Code below this point grabbed from TShock and modified where needed.
		public static bool HandleCommand(TSPlayer player, string text)
		{
			string text2 = text.Remove(0, 1);
			string text3 = text[0].ToString();
			bool flag = false;
			if( text3 == Commands.SilentSpecifier )
			{
				flag = true;
			}
			int num = -1;
			for( int i = 0; i < text2.Length; i++ )
			{
				if( IsWhiteSpace(text2[i]) )
				{
					num = i;
					break;
				}
			}
			if( num == 0 )
			{
				player.SendErrorMessage("Invalid command entered. Type {0}help for a list of valid commands.", new object[]
				{
					Commands.Specifier
				});
				return true;
			}
			string cmdName;
			if( num < 0 )
			{
				cmdName = text2.ToLower();
			}
			else
			{
				cmdName = text2.Substring(0, num).ToLower();
			}
			List<string> list;
			if( num < 0 )
			{
				list = new List<string>();
			}
			else
			{
				list = ParseParameters(text2.Substring(num));
			}
			IEnumerable<Command> enumerable = Commands.ChatCommands.FindAll((Command c) => c.HasAlias(cmdName));
			//if( PlayerHooks.OnPlayerCommand(player, cmdName, text2, list, ref enumerable, text3) )
			//{
			//	return true;
			//}
			if( enumerable.Count<Command>() != 0 )
			{
				foreach( Command current in enumerable )
				{
					//if( !current.CanRun(player) )
					//{
					//	TShock.Utils.SendLogs(string.Format("{0} tried to execute {1}{2}.", player.Name, Commands.Specifier, text2), Color.PaleVioletRed, player);
					//	player.SendErrorMessage("You do not have access to this command.");
					//}
					//else if( !current.AllowServer && !player.RealPlayer )
					//{
					//	player.SendErrorMessage("You must use this command in-game.");
					//}
					//else
					{
						if( current.DoLog )
						{
							TShock.Utils.SendLogs(string.Format("{0} executed: {1}{2}.", player.Name, flag ? Commands.SilentSpecifier : Commands.Specifier, text2), Color.PaleVioletRed, player);
						}
						current.Run(text2, flag, player, list);
					}
				}
				return true;
			}
			if( player.AwaitingResponse.ContainsKey(cmdName) )
			{
				Action<CommandArgs> arg_137_0 = player.AwaitingResponse[cmdName];
				player.AwaitingResponse.Remove(cmdName);
				arg_137_0(new CommandArgs(text2, player, list));
				return true;
			}
			player.SendErrorMessage("Invalid command entered. Type {0}help for a list of valid commands.", new object[]
			{
				Commands.Specifier
			});
			return true;
		}

		private static bool IsWhiteSpace(char c)
		{
			return c == ' ' || c == '\t' || c == '\n';
		}

		public static List<string> ParseParameters(string str)
		{
			List<string> list = new List<string>();
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			for( int i = 0; i < str.Length; i++ )
			{
				char c = str[i];
				if( c == '\\' && ++i < str.Length )
				{
					if( str[i] != '"' && str[i] != ' ' && str[i] != '\\' )
					{
						stringBuilder.Append('\\');
					}
					stringBuilder.Append(str[i]);
				}
				else if( c == '"' )
				{
					flag = !flag;
					if( !flag )
					{
						list.Add(stringBuilder.ToString());
						stringBuilder.Clear();
					}
					else if( stringBuilder.Length > 0 )
					{
						list.Add(stringBuilder.ToString());
						stringBuilder.Clear();
					}
				}
				else if( IsWhiteSpace(c) && !flag )
				{
					if( stringBuilder.Length > 0 )
					{
						list.Add(stringBuilder.ToString());
						stringBuilder.Clear();
					}
				}
				else
				{
					stringBuilder.Append(c);
				}
			}
			if( stringBuilder.Length > 0 )
			{
				list.Add(stringBuilder.ToString());
			}
			return list;
		}
	}
}
