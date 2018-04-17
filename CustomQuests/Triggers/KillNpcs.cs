using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Terraria;
using TerrariaApi.Server;
using System.Diagnostics;
using TShockAPI.Localization;

namespace CustomQuests.Triggers
{
    /// <summary>
    ///     Represents a kill NPCs trigger.
    /// </summary>
    [UsedImplicitly]
    public sealed class KillNpcs : Trigger
    {
        private static readonly Dictionary<int, int> LastStrucks = new Dictionary<int, int>();

		private HashSet<string> npcNames;
		//private HashSet<int> npcIds;
		private readonly Next.Party party;

        private int _amount;
		
		public KillNpcs(Next.Party party, string npcName, int amount)
		{
			this.party = party ?? throw new ArgumentNullException(nameof(party));
			npcNames = new HashSet<string>();

			npcNames.Add(npcName);

			_amount = amount;
		}
		
		/// <summary>
		///     Initializes a new instance of the <see cref="KillNpcs" /> class with the specified party, NPC name, and amount.
		/// </summary>
		/// <param name="party">The party, which must not be <c>null</c>.</param>
		/// <param name="npcNames">Object containing NPC names, or <c>null</c> for any NPC.</param>
		/// <param name="amount">The amount, which must be positive.</param>
		/// <exception cref="ArgumentNullException">
		///     Either <paramref name="party" /> or <paramref name="npcName" /> is <c>null</c>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="amount" /> is not positive.</exception>
		public KillNpcs(Next.Party party, int amount, params string[] npcNames)
		{
			this.party = party ?? throw new ArgumentNullException(nameof(party));

			//{
			//	var name = GetNPCName(npcNames);
			//	if( name == null )
			//		throw new ArgumentException(nameof(npcNames), "Must be a string, double, or LuaTable.");

			//	this.npcNames = new HashSet<string>() { name };
			//}

			this.npcNames = new HashSet<string>(npcNames);

			Debug.Print("KillNpcList:");
			foreach( var n in this.npcNames )
			{
				Debug.Print(n);
			}

			_amount = amount > 0
				? amount
				: throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");
		}

		string GetNPCName(object value)
		{
			if(value is string)
			{
				return value as string;
			}
			else if (value is double)
			{
				var id = (int)(double)value;
				var name = EnglishLanguage.GetNpcNameById(id);//think id should be renamed 'type'
				return name;
			}

			return null;
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
        protected override bool UpdateImpl() => _amount <= 0;

        private void OnNpcKilled(NpcKilledEventArgs args)
        {
			var npc = args.npc;

			Debug.Print($"NpcKilled name: {npc.GivenOrTypeName}");
			Debug.Print($"NpcKilled id: {npc.whoAmI}");
			Debug.Print($"NpcKilled TypeName: {npc.TypeName}");
			Debug.Print($"NpcKilled type: {npc.type}");
			Debug.Print($"Contains name? {npcNames.Contains(npc.GivenOrTypeName)}");
			
			//if (LastStrucks.TryGetValue(npc.whoAmI, out var lastStruck) && _party.Any(p => p.Index == lastStruck) &&
			//             (_npcName?.Equals(npc.GivenOrTypeName, StringComparison.OrdinalIgnoreCase) ?? true))

			int lastStruck = 0;

			//boo path
			if(party!=null)
			{
				if( LastStrucks.TryGetValue(npc.whoAmI, out lastStruck) &&
				party.Any(m => m.Player.Index == lastStruck) &&
				npcNames.Contains(npc.GivenOrTypeName) )
				{
					Debug.Print("Kill counted!");

					LastStrucks.Remove(npc.whoAmI);
					--_amount;
				}
				
				return;
			}
        }

        private void OnNpcStrike(NpcStrikeEventArgs args)
        {
            var playerIndex = args.Player.whoAmI;

			//boo path
			if( args.Handled || party.All(m => m.Player.Index != playerIndex) )
			{
				return;
			}
						
            var npc = args.Npc;
            //if (!_npcName?.Equals(npc.GivenOrTypeName, StringComparison.OrdinalIgnoreCase) ?? false)
			if (!npcNames.Contains(npc.GivenOrTypeName))
			{
                return;
            }
			
			Debug.Print($"NpcStruck name: {npc.GivenOrTypeName}");
			Debug.Print($"NpcStruck id: {npc.whoAmI}");

			var npcId = npc.whoAmI;
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
