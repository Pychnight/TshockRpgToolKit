using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Banking;
using Banking.Rewards;
using Corruption.PluginSupport;
using Leveling.Levels;
using TShockAPI;

namespace Leveling.Classes
{
    /// <summary>
    ///     Represents a class.
    /// </summary>
    public sealed class Class
    {
        private readonly ClassDefinition _definition;

		public ClassDefinition Definition => _definition;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Class" /> class with the specified definition.
        /// </summary>
        /// <param name="definition">The definition, which must not be <c>null</c>.</param>
        internal Class(ClassDefinition definition)
        {
            Debug.Assert(definition != null, "Definition must not be null.");

            _definition = definition;
            Levels = _definition.LevelDefinitions.Select(ld => new Level(this, ld)).ToList().AsReadOnly();
        }

        /// <summary>
        ///     Gets a value indicating whether to allow switching classes. This will override the
        ///     <see cref="AllowSwitchingBeforeMastery" /> property.
        /// </summary>
        public bool AllowSwitching => _definition.AllowSwitching;

        /// <summary>
        ///     Gets a value indicating whether to allow switching classes before mastery.
        /// </summary>
        public bool AllowSwitchingBeforeMastery => _definition.AllowSwitchingBeforeMastery;

        /// <summary>
        ///     Gets the death penalty multiplier override.
        /// </summary>
        public double? DeathPenaltyMultiplierOverride => _definition.DeathPenaltyMultiplierOverride;

        /// <summary>
        ///     Gets the display name.
        /// </summary>
        public string DisplayName => _definition.DisplayName;

        /// <summary>
        ///     Gets the EXP multiplier override.
        /// </summary>
        public double? ExpMultiplierOverride => _definition.ExpMultiplierOverride;

        /// <summary>
        ///     Gets the set of item names allowed.
        /// </summary>
        public ISet<string> ItemNamesAllowed { get; } = new HashSet<string>();

        /// <summary>
        ///     Gets the levels.
        /// </summary>
        public IList<Level> Levels { get; }

        /// <summary>
        ///     Gets the name.
        /// </summary>
        public string Name => _definition.Name;

        /// <summary>
        ///     Gets the set of permissions granted.
        /// </summary>
        public ISet<string> PermissionsGranted { get; } = new HashSet<string>();

        /// <summary>
        ///     Gets the prerequisite levels.
        /// </summary>
        public IList<Level> PrerequisiteLevels { get; private set; }

        /// <summary>
        ///     Gets the prerequisite classes.
        /// </summary>
        public IList<string> PrerequisitePermissions => _definition.PrerequisitePermissions;

		/// <summary>
		/// Gets the CurrencyDefinition this Class uses for leveling purposes.
		/// </summary>
		public CurrencyDefinition LevelingCurrency => _definition.LevelingCurrency;
		
		/// <summary>
		///		Gets the Exp Currency cost to enter this class.
		/// </summary>
		public string CostString => _definition.CostString;

		/// <summary>
		/// Gets the parsed equivalent of CostString.
		/// </summary>
		public decimal Cost => _definition.Cost;

		/// <summary>
		/// Gets the Currency used to enter this class, if one exists.
		/// </summary>
		public CurrencyDefinition CostCurrency => _definition.CostCurrency;

		/// <summary>
		///		Gets the list of Commands to run on first change to the class.
		/// </summary>
		public IList<string> CommandsOnClassChangeOnce => _definition.CommandsOnClassChangeOnce;
		
		public override string ToString() => DisplayName;

		/// <summary>
		/// Safe wrapper for script hook OnLevelUp.
		/// </summary>
		/// <param name="player">TSPlayer instance.</param>
		/// <param name="levelIndex">Int level index.</param>		
		internal void OnLevelUp(TSPlayer player, int levelIndex)
		{
			try
			{
				Definition.OnLevelUp?.Invoke(player, this, levelIndex);
			}
			catch(Exception ex)
			{
				LevelingPlugin.Instance.LogPrint(ex.ToString(), TraceLevel.Error);
			}
		}

		/// <summary>
		/// Safe wrapper for script hook OnLevelUp.
		/// </summary>
		/// <param name="player">TSPlayer instance.</param>
		/// <param name="levelIndex">Int level index.</param>
		internal void OnLevelDown(TSPlayer player, int levelIndex)
		{
			try
			{
				Definition.OnLevelDown?.Invoke(player, this, levelIndex);
			}
			catch( Exception ex )
			{
				LevelingPlugin.Instance.LogPrint(ex.ToString(), TraceLevel.Error);
			}
		}

		/// <summary>
		/// Safe wrapper for script hook OnClassChange.
		/// </summary>
		/// <param name="player">TSPlayer instance.</param>
		/// <param name="previousClass">Previous Class instance.</param>
		internal void OnClassChange(TSPlayer player, Class previousClass)
		{
			try
			{
				Definition.OnClassChange?.Invoke(player, this, previousClass);
			}
			catch( Exception ex )
			{
				LevelingPlugin.Instance.LogPrint(ex.ToString(), TraceLevel.Error);
			}
		}

		/// <summary>
		/// Safe wrapper for script hook OnClassMastered.
		/// </summary>
		/// <param name="player">TSPlayer instance.</param>
		internal void OnClassMastered(TSPlayer player)
		{
			try
			{
				Definition.OnClassMastered?.Invoke(player, this);
			}
			catch( Exception ex )
			{
				LevelingPlugin.Instance.LogPrint(ex.ToString(), TraceLevel.Error);
			}
		}

		/// <summary>
		///     Resolves the class using the specified levels.
		/// </summary>
		/// <param name="levels">The levels, which must not be <c>null</c> or contain <c>null</c>.</param>
		/// <param name="stage">The resolution stage. All classes must be resolved at stage 0 before stage 1.</param>
		internal void Resolve(IList<Level> levels, int stage)
        {
            Debug.Assert(levels != null, "Levels must not be null.");
            Debug.Assert(!levels.Contains(null), "Levels must not contain null.");

            if (stage == 0)
            {
                PrerequisiteLevels = _definition.PrerequisiteLevelNames
                    .Select(pln => levels.First(l => l.Name == pln)).ToList();
            }
            else if (stage == 1)
            {
                var queue = new Queue<Level>();
                foreach (var level in PrerequisiteLevels)
                {
                    queue.Enqueue(level);
                }
                while (queue.Count > 0)
                {
                    var level = queue.Dequeue();
                    foreach (var level2 in level.Class.Levels)
                    {
                        ItemNamesAllowed.UnionWith(level2.ItemNamesAllowed);
                        PermissionsGranted.UnionWith(level2.PermissionsGranted);
                        if (level2 == level)
                        {
                            break;
                        }
                    }

                    foreach (var level2 in level.Class.PrerequisiteLevels)
                    {
                        queue.Enqueue(level2);
                    }
                }
            }
        }
    }
}
