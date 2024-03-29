﻿using System.Collections.Generic;

namespace Banking.Currency
{
	/// <summary>
	/// Holds and provides fast access to ValueOverride objects, which can alter default Currency values. 
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	public class ValueOverrideList<TKey> : List<ValueOverride<TKey>>
	{
		//for fast access. Must call Initialize() prior to using.
		Dictionary<TKey, ValueOverride<TKey>> map;

		/// <summary>
		/// Creates a Dictionary of the current ValueOverrides for fast access, stored in the Map property. 
		/// </summary>
		public void Initialize(CurrencyDefinition currency)
		{
			var map = new Dictionary<TKey, ValueOverride<TKey>>(Count);

			foreach (var vo in this)
			{
				vo.Initialize(currency);
				map[vo.Key] = vo;
			}

			this.map = map;
		}

		/// <summary>
		/// Provides fast access to ValueOverrides for the specified key. Back by a Dictionary internally.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryGetValue(TKey key, out ValueOverride<TKey> value) => map.TryGetValue(key, out value);
	}
}
