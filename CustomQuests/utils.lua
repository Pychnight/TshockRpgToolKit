-- Counts the number of blocks in the area matching the ID.
function CountBlocks(x, y, x2, y2, id)
	local count = 0
	for i = x, x2 do
		for j = y, y2 do
			if MatchesBlock(x, y, id) then
				count = count + 1
			end
		end
	end
	return count
end

-- Counts the number of blocks in the area matching the ID and frames.
function CountBlocksWithFrames(x, y, x2, y2, id, frameX, frameY)
	local count = 0
	for i = x, x2 do
		for j = y, y2 do
			if MatchesBlockWithFrames(x, y, id, frameX, frameY) then
				count = count + 1
			end
		end
	end
	return count
end

-- Counts the number of walls in the area matching the ID.
function CountWalls(x, y, x2, y2, id)
	local count = 0
	for i = x, x2 do
		for j = y, y2 do
			if MatchesWall(x, y, id) then
				count = count + 1
			end
		end
	end
	return count
end

-- Finds an NPC by name. (Uses GivenOrTypeName)
function FindNpcByName(name)
	for i = 0, 200 do
		local npc = Main.npc[i]
		if npc.active and npc.GivenOrTypeName == name then
			return npc
		end
	end
end

-- Finds an NPC by type.
function FindNpcByType(type)
	for i = 0, 200 do
		local npc = Main.npc[i]
		if npc.active and npc.netID == type then
			return npc
		end
	end
end

-- Runs a callback for each player in a party.
function ForEachPlayer(callback)
	local enumerator = party:GetEnumerator()
	while enumerator:MoveNext() do
		callback(enumerator.Current)
	end
end

-- Determines if the block at the coordinates matches the ID.
function MatchesBlock(x, y, id)
	local tile = GetTile(x, y)
	if id == "air" then
		return not tile:active() and tile.liquid == 0
	elseif id == "water" then
		return not tile:active() and tile.liquid > 0 and tile:liquidType() == 0
	elseif id == "lava" then
		return not tile:active() and tile.liquid > 0 and tile:liquidType() == 1
	elseif id == "honey" then
		return not tile:active() and tile.liquid > 0 and tile:liquidType() == 2
	else
		return tile:active() and GetTileType(tile) == id
	end
end

-- Determines if the block at the coordinates matches the ID and frames.
function MatchesBlockWithFrames(x, y, id, frameX, frameY)
	local tile = GetTile(x, y)
	return tile:active() and GetTileType(tile) == id and tile.frameX == frameX and tile.frameY == frameY
end

-- Determines if the wall at the coordinates matches the ID.
function MatchesWall(x, y, id)
	local tile = GetTile(x, y)
	return tile.type == id
end

-- Adds a chat response trigger.
function QuickChatResponse(response, onlyLeader, callback)
	local trigger = ChatResponse(party, response, onlyLeader)
	trigger.Callback = callback
	AddTrigger(trigger)
end

-- Adds a condition trigger.
function QuickCondition(condition, callback)
	local trigger = Condition(condition)
	trigger.Callback = callback
	AddTrigger(trigger)
end

-- Adds a drop items trigger.
function QuickDropItems(name, amount, callback)
	local trigger = DropItems(party, name, amount)
	trigger.Callback = callback
	AddTrigger(trigger)
end

-- Adds a gather items trigger.
function QuickGatherItems(name, amount, callback)
	local trigger = GatherItems(party, name, amount)
	trigger.Callback = callback
	AddTrigger(trigger)
end

-- Adds an in area trigger.
function QuickInArea(x, y, x2, y2, isEveryone, callback)
	local trigger = InArea(party, x, y, x2, y2, isEveryone)
	trigger.Callback = callback
	AddTrigger(trigger)
end

-- Adds a kill NPCs trigger.
function QuickKillNpcs(name, amount, callback)
	local trigger = KillNpcs(party, name, amount)
	trigger.Callback = callback
	AddTrigger(trigger)
end

-- Adds a wait trigger.
function QuickWait(seconds, callback)
	local trigger = Wait(TimeSpan.FromSeconds(seconds))
	trigger.Callback = callback
	AddTrigger(trigger)
end

-- Sets the block at the coordinates to the ID.
function SetBlock(x, y, id)
	local tile = GetTile(x, y)
	if id == "air" then
		tile:active(false)
		tile.liquid = 0
		tile.type = 0
	elseif id == "water" then
		tile:active(false)
		tile.liquid = 255
		tile:liquidType(0)
		tile.type = 0
	elseif id == "lava" then
		tile:active(false)
		tile.liquid = 255
		tile:liquidType(1)
		tile.type = 0
	elseif id == "honey" then
		tile:active(false)
		tile.liquid = 255
		tile:liquidType(2)
		tile.type = 0
	else
		tile:active(true)
		tile.liquid = 0
		SetTileType(tile, id)
	end
end

-- Sets the blocks in the area.
function SetBlocks(x, y, x2, y2, id)
	for i = x, x2 do
		for j = y, y2 do
			SetBlock(i, j, id)
		end
	end
end

-- Sets the walls at the coordinates.
function SetWall(x, y, id)
	local tile = GetTile(x, y)
	tile.wall = id
end

-- Sets the walls in the area.
function SetWalls(x, y, x2, y2, id)
	for i = x, x2 do
		for j = y, y2 do
			SetWall(i, j, id)
		end
	end
end