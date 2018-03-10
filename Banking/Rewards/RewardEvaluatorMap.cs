using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Banking.Rewards
{
	public class RewardEvaluatorMap : Dictionary<RewardReason,IRewardEvaluator>
	{
	}
}
