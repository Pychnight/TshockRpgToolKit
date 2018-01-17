-- currently only works for CustomNpcs
function CustomIDContains(txt)
	if __npcNameContainer==nil then
		return false
	else
		return __npcNameContainer:CustomIdContains(txt) 
	end
end

 -- not currently working
function NameContains(txt)
	if __npcNameContainer==nil then
		return false
	else
		return __npcNameContainer:NameContains(txt)
	end
end
