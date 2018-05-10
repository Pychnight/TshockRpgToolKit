using Corruption.PluginSupport;
using CustomQuests.Quests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace CustomQuests
{
	public partial class CustomQuestsPlugin : TerrariaPlugin
	{
		#region PartyCommands

		private void P(CommandArgs args)
		{
			var player = args.Player;
			if( args.Message.Length < 2 )
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}p <message>");
				return;
			}

			if( player.mute )
			{
				player.SendErrorMessage("You are muted.");
				return;
			}

			var session = GetSession(player);
			var party = session.Party;
			if( party == null )
			{
				player.SendErrorMessage("You are not in a party.");
				return;
			}

			var message = args.Message.Substring(2);
			party.SendMessage($"<{player.Name}> {message}", Main.teamColor[1]);
		}

		private void Party(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			var subcommand = parameters.Count == 0 ? "" : parameters[0];
			if( subcommand.Equals("form", StringComparison.OrdinalIgnoreCase) )
			{
				PartyForm(args);
			}
			else if( subcommand.Equals("invite", StringComparison.OrdinalIgnoreCase) )
			{
				PartyInvite(args);
			}
			else if( subcommand.Equals("kick", StringComparison.OrdinalIgnoreCase) )
			{
				PartyKick(args);
			}
			else if( subcommand.Equals("leave", StringComparison.OrdinalIgnoreCase) )
			{
				PartyLeave(args);
			}
			else if( subcommand.Equals("leader", StringComparison.OrdinalIgnoreCase) )
			{
				PartyLeader(args);
			}
			else
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}party form <name>.");
				player.SendErrorMessage($"Syntax: {Commands.Specifier}party invite <player>.");
				player.SendErrorMessage($"Syntax: {Commands.Specifier}party kick <player>.");
				player.SendErrorMessage($"Syntax: {Commands.Specifier}party leave.");
				player.SendErrorMessage($"Syntax: {Commands.Specifier}party leader <player>.");
			}
		}

		private void PartyForm(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			if( parameters.Count != 2 )
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}party form <name>.");
				return;
			}

			var session = GetSession(player);
			if( session.CurrentQuest != null )
			{
				player.SendErrorMessage("You cannot form a party while in a quest.");
				return;
			}
			if( session.Party != null )
			{
				player.SendErrorMessage("You are already in a party.");
				return;
			}

			var inputName = parameters[1];
			if( _parties.ContainsKey(inputName) )
			{
				player.SendErrorMessage($"A party with the name '{inputName}' already exists.");
				return;
			}

			var party = new Party(inputName, player);
			_parties[inputName] = party;
			session.Party = party;
			player.TPlayer.team = 1;
			player.SendData(PacketTypes.PlayerTeam, "", player.Index);
			player.TPlayer.team = 0;
			player.SendSuccessMessage($"Formed the '{inputName}' party.");
		}

		private void PartyInvite(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			if( parameters.Count != 2 )
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}party invite <player>.");
				return;
			}

			var session = GetSession(player);
			if( session.CurrentQuest != null )
			{
				player.SendErrorMessage("You cannot invite a player while in a quest.");
				return;
			}

			var party = session.Party;
			if( party == null )
			{
				player.SendErrorMessage("You are not in a party.");
				return;
			}

			var inputPlayer = parameters[1];
			var players = TShock.Utils.FindPlayer(inputPlayer);
			if( players.Count == 0 )
			{
				player.SendErrorMessage($"Invalid player '{inputPlayer}'.");
				return;
			}
			if( players.Count > 1 )
			{
				TShock.Utils.SendMultipleMatchError(player, players);
				return;
			}

			var player2 = players[0];
			var session2 = GetSession(player2);
			if( session2.Party != null )
			{
				player.SendErrorMessage($"{player2.Name} is already in a party.");
				return;
			}

			player2.SendInfoMessage($"You have been invited to join the '{party.Name}' party.");
			player2.SendInfoMessage("Use /accept or /decline.");
			player2.AwaitingResponse.Add("accept", args2 =>
			{
				if( session.CurrentQuest != null )
				{
					player.SendErrorMessage("You cannot accept the invite while the party is on a quest.");
					return;
				}

				session2.Party = party;
				party.SendInfoMessage($"{player2.Name} has joined the party.");
				party.Add(player2);
				foreach( var player3 in party )
				{
					player3.Player.TPlayer.team = 1;
					player2.SendData(PacketTypes.PlayerTeam, "", player3.Index);
					player3.Player.TPlayer.team = 0;
				}
				player2.TPlayer.team = 1;
				party.SendData(PacketTypes.PlayerTeam, "", player2.Index);
				player2.TPlayer.team = 0;
				player2.SendSuccessMessage($"Joined the '{party.Name}' party.");
				player2.AwaitingResponse.Remove("decline");
			});
			player2.AwaitingResponse.Add("decline", args2 =>
			{
				player.SendInfoMessage($"{player2.Name} has declined your invitation.");
				player2.SendSuccessMessage("Declined the invitation.");
				player2.AwaitingResponse.Remove("accept");
			});
			player.SendSuccessMessage($"Sent invitation to {player2.Name}.");
		}

		private void PartyKick(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			if( parameters.Count != 2 )
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}party kick <player>.");
				return;
			}

			var session = GetSession(player);
			var party = session.Party;
			if( party == null )
			{
				player.SendErrorMessage("You are not in a party.");
				return;
			}
			if( party.Leader.Player != player )
			{
				player.SendErrorMessage("You are not the leader of your party.");
				return;
			}
			//if (session.CurrentQuest != null)
			//{
			//    player.SendErrorMessage("You cannot kick a player while in a quest.");
			//    return;
			//}

			var inputPlayer = parameters[1];
			//var players = TShock.Utils.FindPlayer(inputPlayer);
			var targetIndex = party.IndexOf(inputPlayer);

			if( targetIndex < 0 )
			{
				player.SendErrorMessage($"Invalid player '{inputPlayer}'.");
				return;
			}
			if( targetIndex == 0 )
			{
				player.SendErrorMessage("You cannot kick yourself from the party.");
				return;
			}

			//if (players.Count > 1)
			//{
			//    TShock.Utils.SendMultipleMatchError(player, players);
			//    return;
			//}

			//var player2 = players[0];
			//if (!party.Contains(player2))
			//{
			//    player.SendErrorMessage($"{player2.Name} is not in the party.");
			//    return;
			//}

			var targetMember = party[targetIndex];
			var session2 = GetSession(targetMember);
			session2.Party = null;
			party.SendData(PacketTypes.PlayerTeam, "", targetMember.Index);
			party.Remove(targetMember.Player);
			party.SendInfoMessage($"{player.Name} kicked {targetMember.Name} from the party.");
			targetMember.SendInfoMessage("You have been kicked from the party.");
		}

		private void PartyLeave(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			if( parameters.Count != 1 )
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}party leave.");
				return;
			}

			var session = GetSession(player);
			var party = session.Party;
			if( party == null )
			{
				player.SendErrorMessage("You are not in a party.");
				return;
			}
			if( session.CurrentQuest != null )
			{
				player.SendErrorMessage("You cannot leave the party while in a quest.");
				return;
			}

			LeaveParty(player);
		}

		private void PartyLeader(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;

			if( parameters.Count != 2 )
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}party leader <player>.");
				return;
			}

			var session = GetSession(player);
			var party = session.Party;
			var newLeader = parameters[1];

			if( party == null )
			{
				player.SendErrorMessage("You are not in a party.");
				return;
			}
			if( player != party.Leader.Player )
			{
				player.SendErrorMessage("You are not the party leader.");
				return;
			}

			var newLeaderIndex = party.IndexOf(newLeader);
			if( newLeaderIndex == -1 )
			{
				player.SendErrorMessage($"{newLeader} is not a member of your party.");
				return;
			}

			var result = party.SetLeader(newLeaderIndex);
			if( result )
			{
				player.SendInfoMessage("You are no longer party leader.");
				party.SendInfoMessage($"{newLeader} is now the party leader.");
			}
			else
			{
				player.SendErrorMessage($"Failed to set party leader.");
				return;
			}
		}

		#endregion

		#region QuestCommands
		
		private void Quest(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			var isAdmin = player.HasPermission("customquests.quest.admin");
			var subcommand = parameters.Count == 0 ? "" : parameters[0];
			if( subcommand.Equals("abort", StringComparison.OrdinalIgnoreCase) )
			{
				QuestAbort(args);
			}
			else if( subcommand.Equals("accept", StringComparison.OrdinalIgnoreCase) )
			{
				QuestAccept(args);
			}
			else if( subcommand.Equals("list", StringComparison.OrdinalIgnoreCase) )
			{
				QuestList(args);
			}
			else if( subcommand.Equals("revoke", StringComparison.OrdinalIgnoreCase) )
			{
				if( !isAdmin )
				{
					player.SendErrorMessage("You do not have access to this command.");
					return;
				}

				QuestRevoke(args);
			}
			else if( subcommand.Equals("unlock", StringComparison.OrdinalIgnoreCase) )
			{
				if( !isAdmin )
				{
					player.SendErrorMessage("You do not have access to this command.");
					return;
				}

				QuestUnlock(args);
			}
			else if( subcommand.Equals("status", StringComparison.OrdinalIgnoreCase) )
			{
				QuestStatus(args);
			}
			else if( subcommand.Equals("clear", StringComparison.OrdinalIgnoreCase) )
			{
				QuestClear(args);
			}
			else
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}quest abort.");
				player.SendErrorMessage($"Syntax: {Commands.Specifier}quest accept <name>.");
				player.SendErrorMessage($"Syntax: {Commands.Specifier}quest list [page].");
				player.SendErrorMessage($"Syntax: {Commands.Specifier}quest status.");
				if( isAdmin )
				{
					player.SendErrorMessage($"Syntax: {Commands.Specifier}quest revoke [player] <name>.");
					player.SendErrorMessage($"Syntax: {Commands.Specifier}quest unlock [player] <name>.");
					player.SendErrorMessage($"Syntax: {Commands.Specifier}quest clear [player].");
				}
			}
		}

		private void QuestAbort(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			if( parameters.Count != 1 )
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}quest abort.");
				return;
			}

			var session = GetSession(player);
			var quest = session.CurrentQuest;
			if( quest == null )
			{
				player.SendErrorMessage("You are not in a quest.");
				return;
			}

			var party = session.Party;
			if( party != null )
			{
				if( party.Leader.Player != player )
				{
					player.SendErrorMessage("Only the party leader can abort the quest.");
					return;
				}

				foreach( var player2 in party )
				{
					var session2 = GetSession(player2);
					session2.IsAborting = true;
				}

				try
				{
					var bquest = session.CurrentQuest;
					bquest.Abort();
				}
				catch( Exception ex )
				{
					CustomQuestsPlugin.Instance.LogPrint(ex.ToString());
				}

				foreach( var player2 in party )
				{
					var session2 = GetSession(player2);
					session2.HasAborted = true;
					//session2.QuestStatusManager.Clear();
				}

				party.SendSuccessMessage("Aborted quest.");
			}
			else
			{
				session.IsAborting = true;

				try
				{
					var bquest = session.CurrentQuest;
					bquest.Abort();
				}
				catch( Exception ex )
				{
					CustomQuestsPlugin.Instance.LogPrint(ex.ToString());
				}

				session.HasAborted = true;

				player.SendSuccessMessage("Aborted quest.");
			}
		}

		private void QuestAccept(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			if( parameters.Count < 2 )
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}quest accept <name>.");
				return;
			}

			var session = GetSession(player);
			if( session.CurrentQuest != null )
			{
				player.SendErrorMessage("You are already in a quest. Either abort it or complete it.");
				return;
			}

			var availableQuests = session.AvailableQuestNames.Where(n => QuestLoader.Contains(n))
											.Select(n => QuestLoader[n])
											.Where(session.CanSeeQuest)
											.ToList();

			var inputName = string.Join(" ", parameters.Skip(1));
			var questInfo = availableQuests.FirstOrDefault(
				q => q.FriendlyName.Equals(inputName, StringComparison.OrdinalIgnoreCase) || q.Name == inputName);
			if( questInfo == null )
			{
				player.SendErrorMessage($"Invalid quest name '{inputName}'.");
				return;
			}

			var path = Path.Combine("quests", questInfo.ScriptPath ?? $"{questInfo.Name}.boo");
			if( !File.Exists(path) )
			{
				player.SendErrorMessage($"Quest '{questInfo.FriendlyName}' is corrupted.");
				CustomQuestsPlugin.Instance.LogPrint($"Failed to start quest '{questInfo.Name}'. Script file not found at '{path}'.", TraceLevel.Error);
				QuestLoader.InvalidQuests.Add(questInfo.Name);
				return;
			}

			var concurrentParties = _parties.Values.Select(p => GetSession(p.Leader))
				.Count(s => s.CurrentQuestName == inputName);
			if( concurrentParties >= questInfo.MaxConcurrentParties )
			{
				player.SendErrorMessage(
					$"There are too many parties currently performing the quest '{questInfo.FriendlyName}'.");
				return;
			}

			var party = session.Party;
			if( party != null )
			{
				if( party.Leader.Player != player )
				{
					player.SendErrorMessage("Only the party leader can accept a quest.");
					return;
				}
				if( party.Select(GetSession).Any(s => !s.AvailableQuestNames.Contains(questInfo.Name) &&
													  !s.CompletedQuestNames.Contains(questInfo.Name)) ||
					party.Select(GetSession).Any(s => !s.CanSeeQuest(questInfo)) )
				{
					player.SendErrorMessage($"Not everyone can start the quest '{questInfo.FriendlyName}'.");
					return;
				}
				if( party.Count < questInfo.MinPartySize )
				{
					player.SendErrorMessage($"Quest requires a larger party size of {questInfo.MinPartySize}.");
					return;
				}
				if( party.Count > questInfo.MaxPartySize )
				{
					player.SendErrorMessage($"Quest requires a smaller party size of {questInfo.MaxPartySize}.");
					return;
				}

				try
				{
					player.SendSuccessMessage($"Starting quest '{questInfo.FriendlyName}'!");
					//session.LoadQuest(questInfo);
					session.LoadQuestX(questInfo);

					foreach( var player2 in party.Where(p => p.Player != player) )
					{
						player2.SendSuccessMessage($"Starting quest '{questInfo.FriendlyName}'!");
						var session2 = GetSession(player2);
						//session2.QuestStatusManager.Clear();
						session2.CurrentQuest = session.CurrentQuest;
					}
				}
				catch( Exception ex )
				{
					player.SendErrorMessage($"Quest '{questInfo.FriendlyName}' is corrupted.");
					CustomQuestsPlugin.Instance.LogPrint(ex.ToString(), TraceLevel.Error);
					CustomQuestsPlugin.Instance.LogPrint(ex.InnerException?.ToString());
				}
			}
			else
			{
				if( questInfo.MinPartySize > 1 )
				{
					player.SendErrorMessage($"Quest requires a party size of {questInfo.MinPartySize}.");
					return;
				}

				try
				{
					player.SendSuccessMessage($"Starting quest '{questInfo.FriendlyName}'!");
					//session.LoadQuest(questInfo);
					session.LoadQuestX(questInfo);
				}
				catch( Exception ex )
				{
					player.SendErrorMessage($"Quest '{questInfo.FriendlyName}' is corrupted.");
					CustomQuestsPlugin.Instance.LogPrint(ex.ToString(), TraceLevel.Error);
					CustomQuestsPlugin.Instance.LogPrint(ex.InnerException?.ToString());
				}
			}
		}

		private void QuestList(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			if( parameters.Count > 2 )
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}quest list [page].");
				return;
			}

			var session = GetSession(player);

			var quests = session.AvailableQuestNames.Where(s => QuestLoader[s] != null).Select(s => QuestLoader[s]);
			var availableQuests = quests.Where(q => session.CanSeeQuest(q) || session.CurrentQuestName == q.Name).ToList();

			//...because sometimes quests are renamed, or removed...
			var validCompletedQuests = session.CompletedQuestNames.Where(q => QuestLoader[q] != null);
			var completedQuests = validCompletedQuests.Select(s => QuestLoader[s]).ToList();

			var totalQuestCount = availableQuests.Count + completedQuests.Count;
			var maxPage = ( totalQuestCount - 1 ) / QuestsPerPage + 1;
			var inputPageNumber = parameters.Count == 1 ? "1" : parameters[1];
			if( !int.TryParse(inputPageNumber, out var pageNumber) || pageNumber <= 0 || pageNumber > maxPage )
			{
				player.SendErrorMessage($"Invalid page number '{inputPageNumber}'");
				return;
			}

			if( totalQuestCount == 0 )
			{
				player.SendErrorMessage("No quests found.");
				return;
			}

			player.SendSuccessMessage($"Quests (page {pageNumber} out of {maxPage})");
			var offset = ( pageNumber - 1 ) * QuestsPerPage;
			for( var i = offset; i < offset + 5 && i < totalQuestCount; ++i )
			{
				if( i < availableQuests.Count )
				{
					var questInfo = availableQuests[i];
					player.SendInfoMessage(questInfo.Name == session.CurrentQuestName
						? $"{questInfo.FriendlyName} (IN PROGRESS): {questInfo.Description}"
						: $"{questInfo.FriendlyName} ({questInfo.Name}): {questInfo.Description}");
				}
				else
				{
					var questInfo = completedQuests[i - availableQuests.Count];
					player.SendSuccessMessage($"{questInfo.FriendlyName} (COMPLETED): {questInfo.Description}");
				}
			}
			if( pageNumber != maxPage )
			{
				player.SendSuccessMessage($"Type {Commands.Specifier}quest list {pageNumber + 1} for more.");
			}
		}

		private void QuestRevoke(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			if( parameters.Count != 2 && parameters.Count != 3 )
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}quest revoke [player] <name>.");
				return;
			}

			var player2 = args.Player;
			if( parameters.Count == 3 )
			{
				var inputPlayer = parameters[1];
				var players = TShock.Utils.FindPlayer(inputPlayer);
				if( players.Count == 0 )
				{
					player.SendErrorMessage($"Invalid player '{inputPlayer}'.");
					return;
				}
				if( players.Count > 1 )
				{
					TShock.Utils.SendMultipleMatchError(player, players);
					return;
				}

				player2 = players[0];
			}

			var session2 = GetSession(player2);
			var inputName = parameters.Last();
			if( !session2.AvailableQuestNames.Contains(inputName) && !session2.CompletedQuestNames.Contains(inputName) )
			{
				player.SendErrorMessage(
					$"{( player2 == player ? "You have" : $"{player2.Name} has" )} not unlocked quest '{inputName}'.");
				return;
			}
			if( QuestLoader.All(q => q.Name != inputName) )
			{
				player.SendErrorMessage($"Invalid quest name '{inputName}'.");
				return;
			}

			session2.RevokeQuest(inputName);
			var questInfo = QuestLoader.First(q => q.Name == inputName);
			player2.SendSuccessMessage($"Revoked quest '{questInfo.Name}'.");
			if( player2 != player )
			{
				player.SendSuccessMessage($"Revoked quest '{questInfo.Name}' for {player2.Name}.");
			}
		}

		private void QuestUnlock(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			if( parameters.Count != 2 && parameters.Count != 3 )
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}quest unlock [player] <name>.");
				return;
			}

			var player2 = args.Player;
			if( parameters.Count == 3 )
			{
				var inputPlayer = parameters[1];
				var players = TShock.Utils.FindPlayer(inputPlayer);
				if( players.Count == 0 )
				{
					player.SendErrorMessage($"Invalid player '{inputPlayer}'.");
					return;
				}
				if( players.Count > 1 )
				{
					TShock.Utils.SendMultipleMatchError(player, players);
					return;
				}

				player2 = players[0];
			}

			var session2 = GetSession(player2);
			var inputName = parameters.Last();
			if( session2.AvailableQuestNames.Contains(inputName) )
			{
				player.SendErrorMessage(
					$"{( player2 == player ? "You have" : $"{player2.Name} has" )} already unlocked quest '{inputName}'.");
				return;
			}
			if( session2.CompletedQuestNames.Contains(inputName) )
			{
				player.SendErrorMessage(
					$"{( player2 == player ? "You have" : $"{player2.Name} has" )} already completed quest '{inputName}'.");
				return;
			}
			if( QuestLoader.All(q => q.Name != inputName) )
			{
				player.SendErrorMessage($"Invalid quest name '{inputName}'.");
				return;
			}

			session2.UnlockQuest(inputName);
			var questInfo = QuestLoader.First(q => q.Name == inputName);
			player2.SendSuccessMessage($"Unlocked quest '{questInfo.Name}'.");
			if( player2 != player )
			{
				player.SendSuccessMessage($"Unlocked quest '{questInfo.Name}' for {player2.Name}.");
			}
		}

		private void QuestStatus(CommandArgs args)
		{
			var player = args.Player;
			var session = GetSession(player);

			if( session.CurrentQuest == null )
			{
				player.SendErrorMessage("You are not currently on a quest!");
			}
			else
			{
				//var isPartyLeader = player == session.Party.Leader.Player;
				var questName = session.CurrentQuest.QuestInfo.FriendlyName;
				var party = session.Party;
				var index = party.IndexOf(player);
								
				player.SendInfoMessage($"Current Quest: {questName}");
				
				if( index > -1)
				{
					var member = party[index];

					foreach( var qs in member.QuestStatuses )
						player.SendMessage(qs.Text ?? "", qs.Color);
				}
			}
		}

		private void QuestClear(CommandArgs args)
		{
			var parameters = args.Parameters;
			var player = args.Player;
			if( parameters.Count != 2 )
			{
				player.SendErrorMessage($"Syntax: {Commands.Specifier}quest clear player.");
				return;
			}

			var targetPlayerName = parameters[1];
			var targetPlayer = TShock.Utils.FindPlayer(targetPlayerName).FirstOrDefault();

			if( targetPlayer == null )
			{
				player.SendErrorMessage($"Unable to find player {targetPlayerName}.");
				return;
			}

			var session = GetSession(targetPlayer);
			if( session.CurrentQuest != null )
			{
				player.SendErrorMessage($"Unable to clear a player's data, while they are on a quest.");
				return;
			}

			session.Clear();

			player.SendInfoMessage($"Players quest data has been cleared.");
		}

		#endregion

		private void TileInfo(CommandArgs args)
		{
			var player = args.Player;
			player.AwaitingTempPoint = -1;
			player.SendInfoMessage("Hit a block to get the tile info.");
		}
	}
}
