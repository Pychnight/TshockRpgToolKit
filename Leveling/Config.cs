﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Corruption.PluginSupport;
using Newtonsoft.Json;
using TerrariaApi.Server;

namespace Leveling
{
    /// <summary>
    ///     Represents the configuration. This class is a singleton.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class Config : JsonConfig
    {
		/// <summary>
		///     Gets the configuration instance.
		/// </summary>
		public static Config Instance { get; internal set; } = new Config();

		[JsonProperty(Order = 0)]
		public DatabaseConfig DatabaseConfig { get; private set; } = new DatabaseConfig();

		/// <summary>
		///     Gets the default class name.
		/// </summary>
		[JsonProperty("DefaultClass", Order = 1)]
		public string DefaultClassName { get; private set; } // = "ranger";

		//TODO should this use generic units or use currency strings? This needs an overhaul.
		/// <summary>
		///     Gets the mapping of NPC names to EXP rewards.
		/// </summary>
		[JsonProperty("NpcToExpReward", Order = 2)]
        public IDictionary<string, long> NpcNameToExpReward = new Dictionary<string, long>();
		
        /// <summary>
        ///     Gets the global death penalty minimum.
        /// </summary>
        [JsonProperty("DeathPenaltyMinimum", Order = 5)]
        public long DeathPenaltyMinimum { get; private set; }

        /// <summary>
        ///     Gets or sets the global death penalty multiplier.
        /// </summary>
        [JsonProperty("DeathPenaltyMultiplier", Order = 4)]
        public double DeathPenaltyMultiplier { get; set; } = 0.33;

        /// <summary>
        ///     Gets or sets the global death penalty multiplier from PvP.
        /// </summary>
        [JsonProperty("DeathPenaltyPvPMultiplier", Order = 6)]
        public double DeathPenaltyPvPMultiplier { get; set; } = 0.10;
		        
        /// <summary>
        ///     Gets or sets the global EXP multiplier.
        /// </summary>
        [JsonProperty("ExpMultiplier", Order = 3)]
        public double ExpMultiplier { get; set; } = 1.0;

		public override void Validate()
		{
			if(string.IsNullOrWhiteSpace(DefaultClassName))
			{
				throw new Exception("DefaultClass is not set.");
			}
		}
	}
}
