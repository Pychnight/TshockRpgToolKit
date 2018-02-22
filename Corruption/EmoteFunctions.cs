using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.Localization;

namespace Corruption
{
	public static class EmoteFunctions
	{
		public const int AnchorTypeNpc = 0;
		internal const int AnchorTypePlayer = 1;
		internal const int AnchorTypeProjectile = 2;

		public static int AttachEmote(int anchorType, int anchorId, int emoteId, int lifeTime)
		{
			int ID = EmoteBubble.AssignNewID();

			//int anchorType = 0; // 0 = npc, 1 = player, 2 = projectile
			//int anchorId = this.Index;
			//ushort meta1 = 0; //id of the above
			//byte lifeTime = 255;
			//int emoteId = EmoteID.WeatherRain;
			//int emote = 1;

			//TSPlayer.All.SendData(PacketTypes.EmoteBubble, "", emoteId, anchorType, meta1, lifeTime, emote, meta1);
			NetMessage.SendData(91, -1, -1, NetworkText.Empty, ID, anchorType, anchorId, lifeTime, emoteId);

			return ID;
		}

		//public void AttachEmote()
		//{
		//	int ID = EmoteBubble.AssignNewID();

		//	int anchorType = 0; // 0 = npc, 1 = player, 2 = projectile
		//	int anchorId = this.Index;
		//	//ushort meta1 = 0; //id of the above
		//	byte lifeTime = 255;
		//	int emoteId = EmoteID.WeatherRain;
		//	int emote = 1;

		//	//TSPlayer.All.SendData(PacketTypes.EmoteBubble, "", emoteId, anchorType, meta1, lifeTime, emote, meta1);
		//	NetMessage.SendData(91, -1, -1, NetworkText.Empty, ID, anchorType, anchorId, 600, emoteId);
		//}
	}
}
