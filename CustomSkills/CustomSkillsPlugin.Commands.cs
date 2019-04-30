using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariaApi.Server;
using TShockAPI;

namespace CustomSkills
{
	public sealed partial class CustomSkillsPlugin : TerrariaPlugin
	{
		private void SkillCommand(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;

			//if (parameters.Count != 1)
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}skill <name>");
				player.SendErrorMessage($"Syntax: {Commands.Specifier}skill learn <name>");
				player.SendErrorMessage($"Syntax: {Commands.Specifier}skill list [category]");
				player.SendErrorMessage($"Syntax: {Commands.Specifier}skill info <name>");
				return;
			}
		}
	}
}
