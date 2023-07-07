namespace Banking
{
	public class StrikeInfo
	{
		//public DateTime LastStrikeTime { get; private set; }
		//public int Strikes { get; private set; }
		public int Damage { get; private set; }
		public int DamageDefended { get; private set; }
		public string WeaponName { get; private set; } //since were only concerned with what weapon was equipped that killed the npc, we can overwrite this.

		public void AddStrike(int damage, int damageDefended, string weaponName)
		{
			//LastStrikeTime = DateTime.Now;
			//Strikes += 1;
			Damage += damage;
			DamageDefended += damageDefended;
			WeaponName = weaponName;
		}
	}
}
