using CustomNpcs.Npcs;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using TShockAPI;

namespace CustomNpcs.Npcs
{
	//delegates for custom npcs.
	//also, the returns are typed, which should work great when the boo scripts have the correct return type set, but should
	//die when they're not correct.
	//public delegate double CheckReplaceHandler(object npc);
	//public delegate int CheckSpawnHandler(object player, object x, object y);
	//public delegate bool SpawnHandler(object npc);
	//public delegate void CollisionHandler(object npc, object player);
	//public delegate void TileCollisionHandler(object npc, object tileHits);
	//public delegate void KilledHandler(object npc);
	//public delegate void TransformedHandler(object npc);
	//public delegate bool StrikeHandler(object npc, object player, object damage, object knockback, object critical);
	//public delegate bool AiUpdateHandler(object npc);

	public delegate double NpcCheckReplaceHandler(NPC npc);//does this need to be a CustomNpc??? original code uses NPC *shrugs*
	public delegate int NpcCheckSpawnHandler(TSPlayer player, int x, int y);
	public delegate bool NpcSpawnHandler(CustomNpc npc);
	public delegate void NpcCollisionHandler(CustomNpc npc, TSPlayer player);
	public delegate void NpcTileCollisionHandler(CustomNpc npc, List<Point> tileHits);
	public delegate void NpcKilledHandler(CustomNpc npc);
	public delegate void NpcTransformedHandler(CustomNpc npc);
	public delegate bool NpcStrikeHandler(CustomNpc npc, TSPlayer player, int damage, float knockback, bool critical);
	public delegate bool NpcAiUpdateHandler(CustomNpc npc);
}
