using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking.Rewards
{
	/// <summary>
	/// Reward for destroying or placing a tile. 
	/// </summary>
	public class MiningReward : Reward
	{
		public ITile Tile { get; set; }
		public int TileX { get; set; }
		public int TileY { get; set; }

		public MiningReward(string playerName, ITile tile, int tileX = 0, int tileY = 0, RewardReason rewardReason = RewardReason.Mining)
		{
			Debug.Assert(rewardReason == RewardReason.Mining || rewardReason == RewardReason.Placing,
							"RewardReason must be either Mining, or Placing.");

			PlayerName = playerName;
			RewardReason = rewardReason;
			Tile = tile;
			TileX = tileX;
			TileY = tileY;
		}

		protected internal override decimal OnEvaluate(CurrencyDefinition currency, IRewardModifier rewardEvaluator = null)
		{
			var value = 1.0m;

			if(rewardEvaluator!=null)
			{
				value = rewardEvaluator.ModifyBaseRewardValue(RewardReason, PlayerName, currency.InternalName, Tile.type.ToString(), value);
			}
			
			value *= (decimal)currency.Multiplier;
						
			return value;
		}
	}
}
