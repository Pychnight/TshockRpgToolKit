-- Utility function for spawning or replacing after any mechanical boss is defeated.
function AfterAnyMechBoss()
    return NPC.downedMechBossAny
end

-- Utility function for spawning or replacing after Golem is defeated.
function AfterGolem()
    return NPC.downedGolemBoss
end

-- Utility function for spawning or replacing after the Moon Lord is defeated.
function AfterMoonLord()
    return NPC.downedMoonlord
end

-- Utility function for spawning or replacing after Plantera is defeated.
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

-- Utility function for spawning or replacing during a blood moon.
function DuringBloodMoon()
    return Main.bloodMoon
end

-- Utility function for spawning or replacing during the day.
function DuringDay()
    return Main.dayTime
end

-- Utility function for spawning or replacing during an eclipse.
function DuringEclipse()
    return Main.eclipse
end

-- Utility function for spawning or replacing during hardmode.
function DuringHardmode()
    return Main.hardMode
end

-- Utility function for spawning or replacing during the night.
function DuringNight()
    return not Main.dayTime
end

-- Utility function for spawning or replacing during rain.
function DuringRain()
    return Main.raining
end

-- Utility function for spawning in a beach biome.
function InBeach(player)
	return player.TPlayer.ZoneBeach
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
    local tplayer = player.TPlayer
    return not tplayer.ZoneBeach and not tplayer.ZoneCorrupt and not tplayer.ZoneCrimson and not tplayer.ZoneDesert and
           not tplayer.ZoneDungeon and not tplayer.ZoneGlowshroom and not tplayer.ZoneHoly and not tplayer.ZoneSnow and 
           not tplayer.ZoneJungle and not tplayer.ZoneMeteor and not tplayer.ZoneOldOneArmy and
           not tplayer.ZoneTowerSolar and not tplayer.ZoneTowerVortex and not tplayer.ZoneTowerNebula and
           not tplayer.ZoneTowerStardust
end

-- Utility function for spawning in a glowing mushroom biome.
function InGlowshroom(player)
	return player.TPlayer.ZoneGlowshroom
end

-- Utility function for spawning in a hallow biome.
function InHallow(player)
	return player.TPlayer.ZoneHoly
end

-- Utility function for spawning in an ice biome.
function InIce(player)
	return player.TPlayer.ZoneSnow
end

-- Utility function for spawning in a region.
function InRegion(x, y, name)
    local region = GetRegion(name)
    return region ~= nil and region:InArea(x, y)
end

-- Utility function for spawning in a jungle biome.
function InJungle(player)
	return player.TPlayer.ZoneJungle
end

-- Utility function for spawning in a meteor.
function InMeteor(player)
	return player.TPlayer.ZoneMeteor
end

-- Utility function for spawning in water.
function InWater(x, y)
	local tile = GetTile(x, y)
	return tile.liquid > 0 and tile:liquidType() == 0
end

-- Utility function for replacing with a type.
function IsType(baseNpc, type)
    return baseNpc.netID == type
end