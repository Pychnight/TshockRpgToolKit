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
	public class ClassExpRewardEvaluator : IRewardModifier
	{
		public decimal ModifyBaseRewardValue(RewardReason reason, string playerName, string currencyType, string itemName, decimal rewardValue)
		{
			if( reason != RewardReason.Killing || currencyType != "Exp" )
				return rewardValue;

			var player = TShockAPI.Utils.Instance.FindPlayer(playerName).FirstOrDefault();

			if( player == null )
				return rewardValue;

			var session = LevelingPlugin.Instance.GetOrCreateSession(player);
			var expValues = session.Class.Definition.ParsedNpcNameToExpValues;
			var result = rewardValue;
			
			if( expValues.TryGetValue(itemName, out var newResult) == true )
			{
				Debug.Print($"ClassExpReward adjusted to {newResult}(was {rewardValue}).");
				result = newResult;
			}
						
			var classExpMultiplier = (decimal)( session.Class.Definition.ExpMultiplierOverride ?? 1.0f );
			result *= classExpMultiplier;
			
			return result;
		}
	}
}
