using CustomNpcs.Npcs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace CustomNpcs
{
	//delegates for custom npcs.

	//TODO Ideally, we can use these typed delegates for better performance. Right now we use object for parameters,
	//to be compatible with boo's duck typing.
	//public delegate int CheckSpawnHandler(TSPlayer player, int x, int y);
	//public delegate bool SpawnHandler(CustomNpc npc);
	//public delegate void CollisionHandler(CustomNpc npc, TSPlayer player);

	//also, the returns are typed, which should work great when the boo scripts have the correct return type set, but should
	//die when they're not correct.
	public delegate double CheckReplaceHandler(object npc);
	public delegate int CheckSpawnHandler(object player, object x, object y);
	public delegate bool SpawnHandler(object npc);
	public delegate void CollisionHandler(object npc, object player);
	public delegate void TileCollisionHandler(object npc, object tileHits);
	public delegate void KilledHandler(object npc);
	public delegate void TransformedHandler(object npc);
	public delegate bool StrikeHandler(object npc, object player, object damage, object knockback, object critical);
	public delegate bool AiUpdateHandler(object npc);
}
