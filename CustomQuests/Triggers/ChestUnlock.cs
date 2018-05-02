using CustomQuests.Quests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomQuests.Triggers
{
	public class ChestUnlock : Trigger
	{
		private int chestX, chestY;
		private bool unlocked;
		
		public ChestUnlock(int chestX, int chestY)
		{
			this.chestX = chestX;
			this.chestY = chestY;
		}

		protected override void Dispose(bool disposing)
		{
			if( disposing )
				CustomQuestsPlugin.Instance.ChestUnlocked -= chestUnlocked;

			base.Dispose(disposing);
		}

		/// <inheritdoc />
		protected override void Initialize()
		{
			CustomQuestsPlugin.Instance.ChestUnlocked += chestUnlocked;
		}

		/// <inheritdoc />
		protected internal override bool UpdateImpl() => unlocked;

		private void chestUnlocked(object sender, ChestUnlockedEventArgs args)
		{
			if( args.ChestX == chestX && args.ChestY == chestY-1 ) //this -1 offset stems from MarioE's original PlaceChest function. Not sure why its done like this.
				unlocked = true;
		}
	}
}
