using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using TShockAPI;

namespace CustomNpcs.Npcs
{
	//delegates for custom npcs.
	public delegate double NpcCheckReplaceHandler(NPC npc);//does this need to be a CustomNpc??? original code uses NPC *shrugs*
	public delegate int NpcCheckSpawnHandler(TSPlayer player, int x, int y);
	public delegate void NpcSpawnHandler(CustomNpc npc);
	public delegate void NpcCollisionHandler(CustomNpc npc, TSPlayer player);
	public delegate void NpcTileCollisionHandler(CustomNpc npc, List<Point> tileHits);
	public delegate void NpcKilledHandler(CustomNpc npc);
	public delegate void NpcTransformedHandler(CustomNpc npc);
	public delegate bool NpcStrikeHandler(CustomNpc npc, TSPlayer player, int damage, float knockback, bool critical);
	public delegate bool NpcAiUpdateHandler(CustomNpc npc);
}
