using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomSkills
{
	/// <summary>
	/// Tracks the cost for various CustomSkill phases.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public class CustomSkillCost
	{
		/// <summary>
		/// Gets or sets the amount of HP this costs.
		/// </summary>
		[JsonProperty(Order = 1)]
		public int Hp { get; set; } = 0;

		/// <summary>
		/// Gets or sets the amount of MP this costs.
		/// </summary>
		[JsonProperty(Order = 2)]
		public int Mp { get; set; } = 1;

		/// <summary>
		/// Gets or sets the amount of EXP this costs.
		/// </summary>
		[JsonProperty(Order = 3)]
		public int Exp { get; set; } = 0;

		/// <summary>
		/// Gets or sets the type and amount of Banking.Currency this costs.
		/// </summary>
		[JsonProperty(Order = 4)]
		public string Currency { get; set; } = "";

		//helpers
		public bool RequiresHp => Hp > 0;
		public bool RequiresMp => Mp > 0;
		public bool RequiresExp => Exp > 0;
		public bool RequiresCurrency => !string.IsNullOrWhiteSpace(Currency);

		public override string ToString()
		{
			var sb = new StringBuilder(128);
			
			if(RequiresHp)
			{
				sb.Append($"Hp: {Hp}");
			}

			if(RequiresMp)
			{
				AppendSeparator(sb);
				sb.Append($"Mp: {Mp}");
			}

			if(RequiresExp)
			{
				AppendSeparator(sb);
				sb.Append($"Exp: {Exp}");
			}

			if(RequiresCurrency)
			{
				AppendSeparator(sb);
				sb.Append($"Currency: {Currency}");
			}

			//return $"Hp: {Hp}, Mp: {Mp}, Exp: {Exp},"
			return sb.ToString();
		}

		private void AppendSeparator(StringBuilder sb)
		{
			if(sb.Length > 0)
				sb.Append(", ");
		}
	}
}
