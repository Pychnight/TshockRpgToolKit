﻿using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using TerrariaApi.Server;
using System.Diagnostics;
using TShockAPI.Localization;
using CustomQuests.Quests;
using System.Collections.Concurrent;

namespace CustomQuests.Triggers
{
    /// <summary>
    ///     Represents a kill NPCs trigger.
    /// </summary>
	public sealed class KillNpcs : TallyTrigger
    {
		private Dictionary<int, int> LastStrucks = new Dictionary<int, int>();
		private HashSet<string> npcTypes;
		private IEnumerable<PartyMember> partyMembers;
		private int _amount;
		private bool AnyNcpType => npcTypes == null;

		/// <summary>
		///     Initializes a new instance of the <see cref="KillNpcs" /> class with the specified party, NPC name, and amount.
		/// </summary>
		/// <param name="partyMembers">The party, which must not be <c>null</c>.</param>
		/// <param name="tallyChangedAction"></param>
		/// <param name="amount">The amount, which must be positive.</param>
		/// <param name="npcTypes">Object containing NPC type names or type ids, or <c>null</c> for any NPC.</param>
		/// <exception cref="ArgumentNullException">
		///     Either <paramref name="party" /> or <paramref name="npcName" /> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="amount" /> is not positive.</exception>
		public KillNpcs(IEnumerable<PartyMember> partyMembers, Action<PartyMember, int> tallyChangedAction, int amount, params object[] npcTypes)
		{
			this.partyMembers = partyMembers ?? throw new ArgumentNullException(nameof(partyMembers));
			this.npcTypes = BuildNpcNameHashSet(npcTypes);

			_amount = amount > 0
				? amount
				: throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");

			SetTallyChangedAction(tallyChangedAction);
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="KillNpcs" /> class with the specified party, NPC name, and amount.
		/// </summary>
		/// <param name="party">The party, which must not be <c>null</c>.</param>
		/// <param name="npcTypes">Object containing NPC names, or <c>null</c> for any NPC.</param>
		/// <param name="amount">The amount, which must be positive.</param>
		/// <exception cref="ArgumentNullException">
		///     Either <paramref name="party" /> or <paramref name="npcName" /> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="amount" /> is not positive.</exception>
		public KillNpcs(PartyMember partyMember, int amount, params object[] npcTypes) : this(partyMember.ToEnumerable(), amount, npcTypes)
		{
		}
		
		/// <summary>
		///     Initializes a new instance of the <see cref="KillNpcs" /> class with the specified party, NPC name, and amount.
		/// </summary>
		/// <param name="party">The party, which must not be <c>null</c>.</param>
		/// <param name="npcTypes">Object containing NPC type names or type ids, or <c>null</c> for any NPC.</param>
		/// <param name="amount">The amount, which must be positive.</param>
		/// <exception cref="ArgumentNullException">
		///     Either <paramref name="party" /> or <paramref name="npcName" /> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="amount" /> is not positive.</exception>
		public KillNpcs(IEnumerable<PartyMember> partyMembers, int amount, params object[] npcTypes) : this(partyMembers,null,amount,npcTypes)
		{
		}
		
		/// <summary>
		///		Initializes a new instance of the <see cref="KillNpcs" /> class with the specified party and amount, accepting any NPC type.
		/// </summary>
		/// <param name="partyMembers"></param>
		/// <param name="amount"></param>
		public KillNpcs(IEnumerable<PartyMember> partyMembers, int amount)
		{
			this.partyMembers = partyMembers ?? throw new ArgumentNullException(nameof(partyMembers));
			this.npcTypes = null;

			_amount = amount > 0
				? amount
				: throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
		}

		public KillNpcs(PartyMember partyMember) : this(partyMember.ToEnumerable(), 1)
		{
		}
		
		/// <inheritdoc />
		protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NpcKilled.Deregister(CustomQuestsPlugin.Instance, OnNpcKilled);
                ServerApi.Hooks.NpcStrike.Deregister(CustomQuestsPlugin.Instance, OnNpcStrike);
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc />
        protected override void Initialize()
        {
            ServerApi.Hooks.NpcKilled.Register(CustomQuestsPlugin.Instance, OnNpcKilled);
            ServerApi.Hooks.NpcStrike.Register(CustomQuestsPlugin.Instance, OnNpcStrike);
        }

		/// <inheritdoc />
		protected internal override TriggerStatus UpdateImpl()
		{
			TryProcessTallyChanges();

			return (_amount <= 0).ToTriggerStatus();
		}

        private void OnNpcKilled(NpcKilledEventArgs args)
        {
			var npc = args.npc;

			//Debug.Print($"DebugId: {debugId}");
			Debug.Print($"NpcKilled name: {npc.GivenOrTypeName}");
			Debug.Print($"NpcKilled id: {npc.whoAmI}");
			Debug.Print($"NpcKilled TypeName: {npc.TypeName}");
			Debug.Print($"NpcKilled type: {npc.type}");
			//Debug.Print($"Contains name? {npcTypes.Contains(npc.GivenOrTypeName)}");

			if(!AnyNcpType && !npcTypes.Contains(npc.GivenOrTypeName) )
				return;

			//if (LastStrucks.TryGetValue(npc.whoAmI, out var lastStruck) && _party.Any(p => p.Index == lastStruck) &&
			//             (_npcName?.Equals(npc.GivenOrTypeName, StringComparison.OrdinalIgnoreCase) ?? true))

			int lastStruck = 0;
						
			if( LastStrucks.TryGetValue(npc.whoAmI, out lastStruck) && partyMembers.Any(m => m.Player.Index == lastStruck) )
			{
				//Debug.Print("Kill counted!");
				
				if(HasTallyChangedAction)
				{
					var member = partyMembers.FirstOrDefault(pm => pm.Player.Index == lastStruck);
					if (member != null)
						TryEnqueueTallyChange(member,1);
				}

				LastStrucks.Remove(npc.whoAmI);
				--_amount;
			}
				
			return;
		}

        private void OnNpcStrike(NpcStrikeEventArgs args)
        {
            var playerIndex = args.Player.whoAmI;
			var npc = args.Npc;
			var npcId = npc.whoAmI;

			//if (!_npcName?.Equals(npc.GivenOrTypeName, StringComparison.OrdinalIgnoreCase) ?? false)
			if( args.Handled || (!AnyNcpType && !npcTypes.Contains(npc.GivenOrTypeName )) )
				return;
			
			if( partyMembers.All(m => m.Player.Index != playerIndex) )
				return;
						
			Debug.Print($"NpcStruck name: {npc.GivenOrTypeName}");
			Debug.Print($"NpcStruck id: {npc.whoAmI}");
						
            LastStrucks[npcId] = playerIndex;

			Debug.Print($"Strike Player index: {playerIndex}");

			//DISABLED becuase the bottom clause is removing all registered strikes, and thus kills are failing.
			//no idea why this stuff is here, other than the commit that said "Fix KillNpcs"...
			//var defense = npc.defense;
   //         if (npc.ichor)
   //         {
   //             defense -= 20;
   //         }
   //         if (npc.betsysCurse)
   //         {
   //             defense -= 40;
   //         }
   //         defense = Math.Max(0, defense);
   //         var damage = Main.CalculateDamage(args.Damage, defense);
   //         if (args.Critical)
   //         {
   //             damage *= 2.0;
   //         }
   //         damage *= Math.Max(1.0, npc.takenDamageMultiplier);

   //         var actualNpc = npc.realLife >= 0 ? Main.npc[npc.realLife] : npc;
   //         if (actualNpc.life - damage <= 0)
   //         {
   //             LastStrucks.Remove(npcId);
   //             --_amount;
   //         }
        }
    }
}
