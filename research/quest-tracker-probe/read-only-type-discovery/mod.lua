-- Run with EML's *export* capability on a disposable, legally owned
-- game/server installation. No patches, no runtime hooks, no save access.
-- Outputs only type names and counts; does not export proprietary resource data.
if not loader or not loader.features or not loader.features.export then
    print("[Quest type probe] Export capability unavailable; nothing done.")
    return
end
if not game or not game.assets or not io.export then
    print("[Quest type probe] Required EML interfaces unavailable.")
    return
end

local list = game.assets.get_resource_types()
if not list then
    print("[Quest type probe] No type registry available.")
    return
end
local matches = {}
local phrases = {
    "quest", "objective", "journal", "lore", "loca", "marker",
    "map", "progress", "challenge", "knowledge"
}
for _, type_name in ipairs(list) do
    if type(type_name) == "string" then
        local lower = type_name:lower()
        for _, word in ipairs(phrases) do
            if lower:find(word, 1, true) then
                table.insert(matches, type_name)
                break
            end
        end
    end
end
table.sort(matches)
local lines = {
    "GAME_VERSION=" .. tostring(game.version or "unknown"),
    "MODE=READ_ONLY_TYPE_DISCOVERY",
    "TYPE_MATCHES=" .. tostring(#matches),
}
-- Do not inspect arbitrary resource contents, play data or user identifiers.
for _, name in ipairs(matches) do
    -- Avoid embedded newlines or filesystem special characters in the report.
    if #name <= 200 and not name:find("[\r\n]") then
        table.insert(lines, name)
    end
end
io.export("shroudnav-quest-resource-type-names.txt", table.concat(lines, "\n") .. "\n")
print("[Quest type probe] Exported " .. tostring(#matches) ..
      " matching type names. No resources were modified.")
