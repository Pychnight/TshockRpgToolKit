using Banking.Currency;
using Banking.TileTracking;
using OTAPI.Tile;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace Banking.Rewards
{
	/// <summary>
	/// Reward for destroying or placing a tile. 
	/// </summary>
	public class MiningReward : Reward
	{
		public TileSubTarget TileSubTarget { get; set; }
		public ushort TileOrWallId { get; set; }
				
		public MiningReward(TSPlayer player, ushort tileOrWallId, TileSubTarget tileSubTarget, RewardReason rewardReason = RewardReason.Mining)
		{
			Debug.Assert(rewardReason == RewardReason.Mining || rewardReason == RewardReason.Placing,
							"RewardReason must be either Mining, or Placing.");

			PlayerName = player.Name;
			PlayerGroup = player.Group.Name;
			RewardReason = rewardReason;
			TileSubTarget = tileSubTarget;
			TileOrWallId = tileOrWallId;
			//tile.wallFrameNumber();
		}

		protected internal override decimal OnEvaluate(CurrencyDefinition currency)//, IRewardModifier rewardEvaluator = null)
		{
			decimal value;

			if(RewardReason == RewardReason.Mining)
				value = currency.GetBaseMiningValue(TileOrWallId, TileSubTarget, PlayerGroup);
			else
				value = currency.GetBasePlacingValue(TileOrWallId, TileSubTarget, PlayerGroup);

			//if(rewardEvaluator!=null)
			//{
			//	value = rewardEvaluator.ModifyBaseRewardValue(RewardReason, PlayerName, currency.InternalName, Tile.type.ToString(), value);
			//}

			value *= (decimal)currency.Multiplier;
						
			return value;
		}
	}
}
