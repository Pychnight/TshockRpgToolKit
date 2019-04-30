using Corruption.PluginSupport;
using Microsoft.Xna.Framework;
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
		internal const string SyntaxSkillSub		= "skill <name>";
		internal const string SyntaxSkillLearnSub	= "skill learn <name>";
		internal const string SyntaxSkillListSub	= "skill list [category]";
		internal const string SyntaxSkillInfoSub	= "skill info <name>";

		private void SkillCommand(CommandArgs args)
		{
			var parameters = args.Parameters;
			var subCommand = args.GetSafeParam(0);
			var player = args.Player;
			string skillName = null;
			string categoryName = null;

			if (parameters.Count >= 1)
			{
				switch(subCommand)
				{
					case "learn":
						ParseCategoryAndSkill(args, 1, out skillName, out categoryName);
						SkillLearnSubCommand(player,skillName,categoryName);
						return;

					case "list":
						SkillListSubCommand(player, args.GetSafeParam(1));
						return;

					case "info":
						ParseCategoryAndSkill(args, 1, out skillName, out categoryName);
						SkillInfoSubCommand(player,skillName,categoryName);
						return;

					default:
						ParseCategoryAndSkill(args, 0, out skillName, out categoryName);
						SkillSubCommand(player, skillName, categoryName);
						return;
				}
			}
			
			SendSkillSyntax(player);
		}

		private void ParseCategoryAndSkill(CommandArgs args, int parameterStartIndex, out string skillName, out string categoryName)
		{
			skillName = null;
			categoryName = null;

			if((args.Parameters.Count - parameterStartIndex ) == 2)
			{
				categoryName = args.GetSafeParam(parameterStartIndex+0);
				skillName = args.GetSafeParam(parameterStartIndex+1);
			}
			else
			{
				skillName = args.GetSafeParam(parameterStartIndex+0);
			}
		}

		private bool GetCategoryAndSkill(TSPlayer player, string skillName, string categoryName, out CustomSkillCategory category, out CustomSkillDefinition skillDef)
		{
			category = null;
			skillDef = null;

			if(string.IsNullOrWhiteSpace(skillName))
			{
				player.SendErrorMessage("Expected skill name.");
				player.SendSyntaxMessage(SyntaxSkillSub);
				return false;
			}

			if(!string.IsNullOrWhiteSpace(categoryName))
				category = CustomSkillManager.TryGetCategory(categoryName);
			else
				category = CustomSkillManager.TryGetCategory();//get default "uncategorized" category...

			if(category == null)
			{
				player.SendErrorMessage($"No such category '{categoryName}'.");
				return false;
			}

			category.TryGetValue(skillName, out skillDef);

			if(skillDef == null)
			{
				player.SendErrorMessage($"No such skill as '{skillName}'.");
				return false;
			}

			return true;
		}
		
		private void SkillSubCommand(TSPlayer player, string skillName, string categoryName = null)
		{
			if(!GetCategoryAndSkill(player, skillName, categoryName, out var category, out var definition))
				return;

			player.SendInfoMessage($"You invoke {skillName}, but nothing happens.");
		}

		private void SkillLearnSubCommand(TSPlayer player, string skillName, string categoryName = null)
		{
			if (!GetCategoryAndSkill(player, skillName, categoryName, out var category, out var definition))
				return;

			player.SendInfoMessage($"You try to learn {skillName}, but nothing happens.");
		}

		private void SkillListSubCommand(TSPlayer player, string categoryName = null)
		{
			CustomSkillCategory category = null;
			
			if(!string.IsNullOrWhiteSpace(categoryName))
			{
				category = CustomSkillManager.TryGetCategory(categoryName);

				if(category!=null)
				{
					//const int itemsPerPage = 4;

					//var lines = category.Select(d => d.Value.Name).ToList(); 
					//var pageCount = lines.PageCount(itemsPerPage);

					//if (pageNumber < 1 || pageNumber > pageCount)
					//	pageNumber = 1;

					//var page = lines.GetPage(pageNumber - 1, itemsPerPage);//we display based off of 1

					//player.SendInfoMessage($"Page #{pageNumber} of {pageCount}.");

					//foreach (var l in page)
					//{
					//	player.SendInfoMessage(l);
					//}

					//player.SendMessage("Use /bank bal <page> or /bank bal <currency> to see more.", Color.Green);
					//player.SendInfoMessage("Use /bank bal <page> or /bank bal <currency> to see more.");

					foreach (var kvp in category)
						player.SendInfoMessage(kvp.Value.Name);
				}
				else
				{
					player.SendErrorMessage($"No such category as '{categoryName}'.");
					return;
				}
			}
			else
			{
				category = CustomSkillManager.TryGetCategory(null);

				foreach (var def in category.Values)
					player.SendInfoMessage(def.Name);
			}
		}

		private void SkillInfoSubCommand(TSPlayer player, string skillName, string categoryName = null)
		{
			if(!GetCategoryAndSkill(player, skillName, categoryName, out var category, out var definition))
				return;

			player.SendInfoMessage($"You find no information on {skillName}.");
		}

		private void SendSkillSyntax(TSPlayer player)
		{
			player.SendSyntaxMessage(SyntaxSkillSub);
			player.SendSyntaxMessage(SyntaxSkillLearnSub);
			player.SendSyntaxMessage(SyntaxSkillListSub);
			player.SendSyntaxMessage(SyntaxSkillInfoSub);
		}
	}
}
