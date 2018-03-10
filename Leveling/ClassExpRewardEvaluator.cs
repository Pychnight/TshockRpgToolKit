using Banking.Rewards;
using Leveling.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leveling
{
	public class ClassExpRewardEvaluator : IRewardEvaluator
	{
		public decimal GetRewardValue(RewardReason reason, string playerName, string currencyType, string itemName, decimal rewardValue)
		{
			if( reason != RewardReason.Killing )
				return rewardValue;

			var player = TShockAPI.Utils.Instance.FindPlayer(playerName).FirstOrDefault();

			if( player == null )
				return rewardValue;

			var session = LevelingPlugin.Instance.GetOrCreateSession(player);
			var expValues = session.Class.Definition.ParsedNpcNameToExpValues;

			if( expValues.TryGetValue(itemName, out var result) == true )
			{
				Debug.Print($"ClassExpReward adjusted to {result}(was {rewardValue}).");
				return result;
			}
			else
				return rewardValue;
		}
	}
}
