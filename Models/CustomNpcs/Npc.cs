using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
//using Terraria;
//using CustomNpcs.Projectiles;
using System.Reflection;
//using TShockAPI;
using System.Diagnostics;
using System.Windows.Forms.Design;
using RpgToolsEditor.Controls;

namespace RpgToolsEditor.Models.CustomNpcs
{
	/// <summary>
	///     Represents an NPC definition.
	/// </summary>
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class Npc : IModel
	{
		public event PropertyChangedEventHandler PropertyChanged;

		string name = "New Npc";

		/// <summary>
		///     Gets the internal name.
		/// </summary>
		[Category("Basic Properties")]
		[JsonProperty(Order = 0)]
		public string Name
		{
			get { return name; }
			set
			{
				name = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
			}
		}

		/// <summary>
		///     Gets the base type.
		/// </summary>
		[Category("Basic Properties")]
		[JsonProperty(Order = 1)]
		public int BaseType { get; set; }

		[Category("Basic Properties")]
		[Editor(typeof(FileNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
		[JsonProperty(Order = 2)]
		public string ScriptPath { get; set; }

		[JsonProperty("BaseOverride", Order = 3)]
		internal NpcBaseOverride baseOverride = new NpcBaseOverride();

		[JsonProperty("Loot", Order = 4)]
		internal LootDefinition loot = new LootDefinition();

		[JsonProperty("Spawning", Order = 5)]
		internal SpawningDefinition spawning = new SpawningDefinition();
		
		[Category("Override Properties")]
		public int? AiStyle
		{
			get => baseOverride.AiStyle;
			set => baseOverride.AiStyle = value;
		}
		
		[Category("Override Properties")]
		public int[] BuffImmunities
		{
			get => baseOverride.BuffImmunities;
			set => baseOverride.BuffImmunities = value;
		}

		[Category("Override Properties")]
		public int? Defense
		{
			get => baseOverride.Defense;
			set => baseOverride.Defense = value;
		}

		[Category("Override Properties")]
		public bool? HasNoCollision
		{
			get => baseOverride.HasNoCollision;
			set => baseOverride.HasNoCollision = value;
		}

		[Category("Override Properties")]
		public bool? HasNoGravity
		{
			get => baseOverride.HasNoGravity;
			set => baseOverride.HasNoGravity = value;
		}

		[Category("Override Properties")]
		public bool? IsBoss
		{
			get => baseOverride.IsBoss;
			set => baseOverride.IsBoss = value;
		}

		[Category("Override Properties")]
		public bool? IsImmortal
		{
			get => baseOverride.IsImmortal;
			set => baseOverride.IsImmortal = value;
		}

		[Category("Override Properties")]
		public bool? IsImmuneToLava
		{
			get => baseOverride.IsImmuneToLava;
			set => baseOverride.IsImmuneToLava = value;
		}

		[Category("Override Properties")]
		public bool? IsTrapImmune
		{
			get => baseOverride.IsTrapImmune;
			set => baseOverride.IsTrapImmune = value;
		}

		[Category("Override Properties")]
		public bool? DontTakeDamageFromHostiles
		{
			get => baseOverride.DontTakeDamageFromHostiles;
			set => baseOverride.DontTakeDamageFromHostiles = value;
		}

		[Category("Override Properties")]
		public float? KnockbackMultiplier
		{
			get => baseOverride.KnockbackMultiplier;
			set => baseOverride.KnockbackMultiplier = value;
		}

		[Category("Override Properties")]
		public int? MaxHp
		{
			get => baseOverride.MaxHp;
			set => baseOverride.MaxHp = value;
		}

		[Category("Override Properties")]
		[DisplayName("Name")]
		public string xName
		{
			get => baseOverride.Name;
			set => baseOverride.Name = value;
		}

		[Category("Override Properties")]
		public float? NpcSlots
		{
			get => baseOverride.NpcSlots;
			set => baseOverride.NpcSlots = value;
		}

		[Category("Override Properties")]
		public float? Value
		{
			get => baseOverride.Value;
			set => baseOverride.Value = value;
		}

		[Category("Override Properties")]
		public bool? BehindTiles
		{
			get => baseOverride.BehindTiles;
			set => baseOverride.BehindTiles = value;
		}

		/// <summary>
		///     Gets the loot entries.
		/// </summary>
		[Browsable(false)]
		[Category("Loot")]
		public List<LootEntry> LootEntries
		{
			get => loot.Entries;
			set => loot.Entries = value;
		}
		
		///// <summary>
		/////     Gets a value indicating whether the NPC should aggressively update due to unsynced changes with clients.
		///// </summary>
		//public bool ShouldAggressivelyUpdate =>
		//	_baseOverride.AiStyle != null || _baseOverride.BuffImmunities != null ||
		//	_baseOverride.IsImmuneToLava != null || _baseOverride.HasNoCollision != null ||
		//	_baseOverride.HasNoGravity != null;

		///// <summary>
		/////     Gets a value indicating whether loot should be overriden.
		///// </summary>
		//public bool ShouldOverrideLoot => _loot.IsOverride;

		///// <summary>
		/////     Gets a value indicating whether the NPC should spawn.
		///// </summary>
		//public bool ShouldReplace => _spawning.ShouldReplace;

		///// <summary>
		/////     Gets a value indicating whether the NPC should spawn.
		///// </summary>
		//public bool ShouldSpawn => _spawning.ShouldSpawn;

		///// <summary>
		/////     Gets an optional value that overrides the global spawnrate, if present.
		///// </summary>
		//public int? SpawnRateOverride => _spawning.SpawnRateOverride;

		///// <summary>
		/////     Gets a value indicating whether the NPC should have kills tallied.
		///// </summary>
		//public bool ShouldTallyKills => _loot.TallyKills;

		///// <summary>
		/////     Gets a value indicating whether the NPC should update on hit.
		///// </summary>
		//public bool ShouldUpdateOnHit =>
		//	_baseOverride.Defense != null || _baseOverride.IsImmortal != null ||
		//	_baseOverride.KnockbackMultiplier != null;

		[Category("Spawning")]
		public bool ShouldReplace
		{
			get => spawning.ShouldReplace;
			set => spawning.ShouldReplace = value;
		}

		[Category("Spawning")]
		public bool ShouldSpawn
		{
			get => spawning.ShouldSpawn;
			set => spawning.ShouldSpawn = value;
		}

		[Category("Spawning")]
		public int? SpawnRateOverride
		{
			get => spawning.SpawnRateOverride;
			set => spawning.SpawnRateOverride = value;
		}

		public Npc() 
		{
		}

		public Npc(Npc other)
		{
			Name = other.Name;
			BaseType = other.BaseType;
			ScriptPath = other.ScriptPath;

			baseOverride = new NpcBaseOverride(other.baseOverride);
			loot = new LootDefinition(other.loot);
			spawning = new SpawningDefinition(other.spawning);
		}
		
		object ICloneable.Clone()
		{
			return new Npc(this);
		}
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class NpcBaseOverride // : IValidator
	{
		[JsonProperty]
		public int? AiStyle { get; set; }

		[JsonProperty]
		public int[] BuffImmunities { get; set; }

		[JsonProperty]
		public int? Defense { get; set; }

		[JsonProperty]
		public bool? HasNoCollision { get; set; }

		[JsonProperty]
		public bool? HasNoGravity { get; set; }

		[JsonProperty]
		public bool? IsBoss { get; set; }

		[JsonProperty]
		public bool? IsImmortal { get; set; }

		[JsonProperty]
		public bool? IsImmuneToLava { get; set; }

		[JsonProperty]
		public bool? IsTrapImmune { get; set; }

		[JsonProperty]
		public float? KnockbackMultiplier { get; set; }

		[JsonProperty]
		public int? MaxHp { get; set; }

		[JsonProperty]
		public string Name { get; set; }

		[JsonProperty]
		public float? NpcSlots { get; set; }

		[JsonProperty]
		public float? Value { get; set; }

		[JsonProperty]
		public bool? BehindTiles { get; set; }

		[JsonProperty]
		public bool? DontTakeDamageFromHostiles { get; set; }

		public NpcBaseOverride()
		{
		}

		public NpcBaseOverride(NpcBaseOverride other)
		{
			AiStyle = other.AiStyle;
			BuffImmunities = other.BuffImmunities?.ToArray();
			Defense = other.Defense;
			HasNoCollision = other.HasNoCollision;
			HasNoGravity = other.HasNoGravity;
			IsBoss = other.IsBoss;
			IsImmortal = other.IsImmortal;
			IsImmuneToLava = other.IsImmuneToLava;
			IsTrapImmune = other.IsTrapImmune;
			KnockbackMultiplier = other.KnockbackMultiplier;
			MaxHp = other.MaxHp;
			Name = other.Name;
			NpcSlots = other.NpcSlots;
			Value = other.Value;
			BehindTiles = other.BehindTiles;
			DontTakeDamageFromHostiles = other.DontTakeDamageFromHostiles;
		}
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class LootDefinition //: IValidator
	{
		[JsonProperty(Order = 2)]
		public List<LootEntry> Entries { get; set; } = new List<LootEntry>();

		[JsonProperty(Order = 1)]
		public bool IsOverride { get; set; }

		[JsonProperty(Order = 0)]
		public bool TallyKills { get; set; }

		public LootDefinition()
		{
		}

		public LootDefinition(LootDefinition other)
		{
			TallyKills = other.TallyKills;
			IsOverride = other.IsOverride;
			Entries = other.Entries.Select(e => new LootEntry(e)).ToList();
		}
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class SpawningDefinition
	{
		[JsonProperty(Order = 1)]
		public bool ShouldReplace { get; set; }

		[JsonProperty(Order = 0)]
		public bool ShouldSpawn { get; set; }

		[JsonProperty(Order = 2)]
		public int? SpawnRateOverride { get; set; }

		public SpawningDefinition()
		{
		}

		public SpawningDefinition(SpawningDefinition other)
		{
			ShouldSpawn = other.ShouldSpawn;
			ShouldReplace = other.ShouldReplace;
			SpawnRateOverride = other.SpawnRateOverride;
		}
	}
}
