using System;
using Corruption.PluginSupport;
using Newtonsoft.Json;
using Terraria.ID;

namespace CustomNpcs.Npcs
{
    /// <summary>
    ///     Represents a loot entry definition.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class LootEntryDefinition : IValidator
    {
		/// <summary>
		///     Gets the name.
		/// </summary>
		[JsonProperty(Order = 0)]
		public string Name { get; internal set; } = "Dirt Block";

		/// <summary>
		///     Gets the minimum stack size.
		/// </summary>
		[JsonProperty(Order = 1)]
		public int MinStackSize { get; private set; }
		
		/// <summary>
		///     Gets the maximum stack size.
		/// </summary>
		[JsonProperty(Order = 2)]
		public int MaxStackSize { get; private set; }

		/// <summary>
		///     Gets the prefix.
		/// </summary>
		[JsonProperty(Order = 3)]
		public int Prefix { get; private set; }

		/// <summary>
		///     Gets the chance.
		/// </summary>
		[JsonProperty(Order = 4)]
        public double Chance { get; private set; }
		
		//[Obsolete]
  //      internal void ThrowIfInvalid()
  //      {
  //          if (Name == null)
  //          {
  //              throw new FormatException($"{nameof(Name)} is null.");
  //          }
  //          if (MinStackSize < 0)
  //          {
  //              throw new FormatException($"{nameof(MinStackSize)} is negative.");
  //          }
  //          if (MaxStackSize < MinStackSize)
  //          {
  //              throw new FormatException($"{nameof(MaxStackSize)} is less than {nameof(MinStackSize)}.");
  //          }
  //          if (Chance <= 0)
  //          {
  //              throw new FormatException($"{nameof(Chance)} is not positive.");
  //          }
  //          if (Chance > 1)
  //          {
  //              throw new FormatException($"{nameof(Chance)} is greater than 1.");
  //          }
  //          if (Prefix <= -2)
  //          {
  //              throw new FormatException($"{nameof(Prefix)} is too small.");
  //          }
  //          if (Prefix >= PrefixID.Count)
  //          {
  //              throw new FormatException($"{nameof(Prefix)} is too large.");
  //          }
  //      }

		public ValidationResult Validate()
		{
			var result = new ValidationResult();

			if( Name == null )
			{
				//throw new FormatException($"{nameof(Name)} is null.");
				result.AddError($"{nameof(Name)} is null.");
			}
			if( MinStackSize < 0 )
			{
				//throw new FormatException($"{nameof(MinStackSize)} is negative.");
				result.AddError($"{nameof(MinStackSize)} is negative.");
			}
			if( MaxStackSize < MinStackSize )
			{
				//throw new FormatException($"{nameof(MaxStackSize)} is less than {nameof(MinStackSize)}.");
				result.AddError($"{nameof(MaxStackSize)} is less than {nameof(MinStackSize)}.");
			}
			if( Chance <= 0 )
			{
				//throw new FormatException($"{nameof(Chance)} is not positive.");
				result.AddError($"{nameof(Chance)} is not positive.");
			}
			if( Chance > 1 )
			{
				//throw new FormatException($"{nameof(Chance)} is greater than 1.");
				result.AddError($"{nameof(Chance)} is greater than 1.");
			}
			if( Prefix <= -2 )
			{
				//throw new FormatException($"{nameof(Prefix)} is too small.");
				result.AddError($"{nameof(Prefix)} is too small.");
			}
			if( Prefix >= PrefixID.Count )
			{
				//throw new FormatException($"{nameof(Prefix)} is too large.");
				result.AddError($"{nameof(Prefix)} is too large.");
			}

			return result;
		}
	}
}
