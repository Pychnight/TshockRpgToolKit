-- Utility function for spawning after any mechanical boss is defeated.
function AfterAnyMechBoss()
    return NPC.downedMechBossAny
end

-- Utility function for spawning after Golem is defeated.
function AfterGolem()
    return NPC.downedGolemBoss
end

-- Utility function for spawning after the Moon Lord is defeated.
function AfterMoonLord()
    return NPC.downedMoonlord
end

-- Utility function for spawning after Plantera is defeated.
function AfterPlantera()
    return NPC.downedPlantBoss
end

-- Utility function for spawning at the cavern level.
function AtCavernLevel(player)
    return player.TPlayer.ZoneRockLayerHeight
end

-- Utility function for spawning at the sky level.
function AtSkyLevel(player)
    return player.TPlayer.ZoneSkyHeight
end

-- Utility function for spawning at the surface level.
function AtSurfaceLevel(player)
    return player.TPlayer.ZoneOverworldHeight
end

-- Utility function for spawning at the underground level.
function AtUndergroundLevel(player)
    return player.TPlayer.ZoneDirtLayerHeight
end

-- Utility function for spawning at the underworld level.
function AtUnderworldLevel(player)
    return player.TPlayer.ZoneUnderworldHeight
end

-- Utility function for spawning during a blood moon.
function DuringBloodMoon()
    return Main.bloodMoon
end

-- Utility function for spawning during day.
function DuringDay()
    return Main.dayTime
end

-- Utility function for spawning during an eclipse.
function DuringEclipse()
    return Main.eclipse
end

-- Utility function for spawning during hardmode.
function DuringHardmode()
    return Main.hardMode
end

-- Utility function for spawning during night.
function DuringNight()
    return not Main.dayTime
end

-- Utility function for spawning during rain.
function DuringRain()
    return Main.raining
end

-- Utility function for spawning in a corruption biome.
function InCorruption(player)
	return player.TPlayer.ZoneCorrupt
end

-- Utility function for spawning in a crimson biome.
function InCrimson(player)
	return player.TPlayer.ZoneCrimson
end

-- Utility function for spawning in a desert biome.
function InDesert(player)
	return player.TPlayer.ZoneDesert
end

-- Utility function for spawning in the dungeon.
function InDungeon(player)
	return player.TPlayer.ZoneDungeon
end

-- Utility function for spawning in a forest biome (not a special biome).
function InForest(player)
    return not player.TPlayer.ZoneCorrupt and not player.TPlayer.ZoneCrimson and not player.TPlayer.ZoneDesert
       and not player.TPlayer.ZoneDungeon and not player.TPlayer.ZoneHoly    and not player.TPlayer.ZoneSnow
       and not player.TPlayer.ZoneJungle  and not player.TPlayer.ZoneMeteor
end

-- Utility function for spawning in a hallow biome.
function InHallow(player)
	return player.TPlayer.ZoneHoly
end

-- Utility function for spawning in an ice biome.
function InIce(player)
	return player.TPlayer.ZoneSnow
end

-- Utility function for spawning in a jungle biome.
function InJungle(player)
	return player.TPlayer.ZoneJungle
end

-- Utility function for spawning in a meteor.
function InMeteor(player)
	return player.TPlayer.ZoneMeteor
end