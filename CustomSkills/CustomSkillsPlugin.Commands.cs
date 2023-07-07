using Corruption;
using Corruption.PluginSupport;
using System;
using System.Diagnostics;
using TerrariaApi.Server;
using TShockAPI;

namespace CustomSkills
{
	public sealed partial class CustomSkillsPlugin : TerrariaPlugin
	{
		internal const string SyntaxSkillSub = "skill <name>";
		internal const string SyntaxSkillLearnSub = "skill learn <name>";
		internal const string SyntaxSkillListSub = "skill list [category]";
		internal const string SyntaxSkillInfoSub = "skill info <name>";
		internal const string SyntaxSkillCancelSub = "skill cancel";

		private void SkillCommand(CommandArgs args)
		{
			var parameters = args.Parameters;
			var subCommand = args.GetSafeParam(0);
			var player = args.Player;
			string skillName = null;
			string categoryName = null;

			if (parameters.Count >= 1)
			{
				switch (subCommand)
				{
					case "learn":
						ParseCategoryAndSkill(args, 1, out skillName, out categoryName);
						SkillLearnSubCommand(player, skillName, categoryName);
						return;

					case "list":
						SkillListSubCommand(player, args.GetSafeParam(1));
						return;

					case "info":
						ParseCategoryAndSkill(args, 1, out skillName, out categoryName);
						SkillInfoSubCommand(player, skillName, categoryName);
						return;

					case "cancel":
						//ParseCategoryAndSkill(args, 1, out skillName, out categoryName);
						SkillCancelSubCommand(player);
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

			if ((args.Parameters.Count - parameterStartIndex) == 2)
			{
				categoryName = args.GetSafeParam(parameterStartIndex + 0);
				skillName = args.GetSafeParam(parameterStartIndex + 1);
			}
			else
			{
				skillName = args.GetSafeParam(parameterStartIndex + 0);
			}
		}

		private bool GetCategoryAndSkill(TSPlayer player, string skillName, string categoryName, out CustomSkillCategory category, out CustomSkillDefinition skillDef)
		{
			category = null;
			skillDef = null;

			if (string.IsNullOrWhiteSpace(skillName))
			{
				player.SendErrorMessage("Expected skill name.");
				player.SendSyntaxMessage(SyntaxSkillSub);
				return false;
			}

			if (!string.IsNullOrWhiteSpace(categoryName))
				category = CustomSkillDefinitionLoader.TryGetCategory(categoryName);
			else
				category = CustomSkillDefinitionLoader.TryGetCategory();//get default "uncategorized" category...

			if (category == null)
			{
				player.SendErrorMessage($"No such category '{categoryName}'.");
				return false;
			}

			category.TryGetValue(skillName, out skillDef);

			if (skillDef == null)
			{
				player.SendErrorMessage($"No such skill as '{skillName}'.");
				return false;
			}

			return true;
		}

		private void SkillSubCommand(TSPlayer player, string skillName, string categoryName = null)
		{
			if (!GetCategoryAndSkill(player, skillName, categoryName, out var category, out var definition))
				return;

			var session = Session.GetOrCreateSession(player);

			if (!session.HasLearned(skillName))
			{
				player.SendErrorMessage($"You have not learned {skillName}.");
				return;
			}

			//are we allowed to use this skill?
			if (definition.PermissionsToUse != null && !PlayerFunctions.PlayerHasPermission(player, definition.PermissionsToUse))
			{
				player.SendInfoMessage($"You are not allowed to use {skillName}.");
				return;
			}

			//do we have enough "funds" to use this skill?
			if (!session.CanAffordCastingSkill(skillName))
			{
				player.SendInfoMessage($"You cannot afford to use {skillName}.");
				return;
			}

			if (!session.IsSkillReady(definition.Name))
			{
				player.SendInfoMessage($"{skillName} is not ready yet.");
				return;
			}

			session.PlayerSkillInfos.TryGetValue(skillName, out var playerSkillInfo);

			var skillAdded = CustomSkillRunner.AddActiveSkill(player, definition, playerSkillInfo.CurrentLevel);
			if (!skillAdded)
			{
				player.SendInfoMessage($"You cannot use {skillName} right now.");
				return;
			}
		}

		private void SkillLearnSubCommand(TSPlayer player, string skillName, string categoryName = null)
		{
			if (!GetCategoryAndSkill(player, skillName, categoryName, out var category, out var definition))
				return;

			var session = Session.GetOrCreateSession(player);

			if (session.HasLearned(skillName))
			{
				player.SendInfoMessage($"You have already learned {skillName}.");
				return;
			}

			//can we learn this skill?
			if (definition.PermissionsToLearn != null && !PlayerFunctions.PlayerHasPermission(player, definition.PermissionsToLearn))
			{
				player.SendInfoMessage($"You try, but are unable to learn {skillName}.");
				return;
			}

			if (session.LearnSkill(skillName))
			{
				player.SendInfoMessage($"You have learned {skillName}.");

				//try to run the first OnLevelUp
				try
				{
					definition.Levels?[0]?.OnLevelUp?.Invoke(player);
				}
				catch (Exception ex)
				{
					CustomSkillsPlugin.Instance.LogPrint(ex.ToString(), TraceLevel.Error);
				}
			}
			else
			{
				player.SendErrorMessage($"You try to learn {skillName}, but nothing happens. ( This is a bug. )");
			}
		}

		private void SkillListSubCommand(TSPlayer player, string categoryName = null)
		{
			CustomSkillCategory category = null;

			if (!string.IsNullOrWhiteSpace(categoryName))
			{
				category = CustomSkillDefinitionLoader.TryGetCategory(categoryName);

				if (category != null)
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
				category = CustomSkillDefinitionLoader.TryGetCategory(null);

				foreach (var def in category.Values)
					player.SendInfoMessage(def.Name);
			}
		}

		private void SkillInfoSubCommand(TSPlayer player, string skillName, string categoryName = null)
		{
			if (!GetCategoryAndSkill(player, skillName, categoryName, out var category, out var definition))
				return;

			var session = Session.GetOrCreateSession(player);

			if (!session.HasLearned(skillName))
			{
				player.SendErrorMessage($"You have not learned {skillName}.");
				return;
			}

			if (!session.PlayerSkillInfos.TryGetValue(skillName, out var levelInfo))
			{
				player.SendErrorMessage($"Failed to retrieve skill info for '{skillName}'. Please notify the server admin.");
				return;
			}

			var level = definition.Levels[levelInfo.CurrentLevel];
			var damageRange = level.DamageRangeEstimate ?? "??";
			var castingCost = level.CastingCost?.ToString() ?? "None";
			var chargingCost = level.ChargingCost?.ToString() ?? "None";

			player.SendInfoMessage($"{skillName}, level {levelInfo.CurrentLevel} - {definition.Description ?? ""}");
			player.SendInfoMessage($"Damage Range: {damageRange}, Casting Cost: {castingCost}");
			player.SendInfoMessage($"Charge Time: {level.ChargingDuration}, Charging Cost: {chargingCost}");
		}

		private void SkillCancelSubCommand(TSPlayer player)
		{
			var canceledSkill = CustomSkillRunner.RemoveActiveSkill(player.Name);
			if (canceledSkill == null)
				player.SendErrorMessage("There is nothing to cancel!");
		}

		private void SendSkillSyntax(TSPlayer player)
		{
			player.SendSyntaxMessage(SyntaxSkillSub);
			player.SendSyntaxMessage(SyntaxSkillLearnSub);
			player.SendSyntaxMessage(SyntaxSkillListSub);
			player.SendSyntaxMessage(SyntaxSkillInfoSub);
			player.SendSyntaxMessage(SyntaxSkillCancelSub);
		}
	}
}
