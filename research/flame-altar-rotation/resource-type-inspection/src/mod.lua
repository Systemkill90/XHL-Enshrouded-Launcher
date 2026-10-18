-- Resource type-name discovery only. No game hooks, no writes, no saves.
-- Use only in a disposable legally-owned game copy with EML export capability.
if not loader or not loader.features or not loader.features.export then
    print("[Altar type probe] Export capability unavailable. No change.")
    return
end
if not game or not game.assets or not io.export then
    print("[Altar type probe] EML type-inspection interfaces unavailable. No change.")
    return
end
local types = game.assets.get_resource_types()
if not types then
    print("[Altar type probe] Resource type list unavailable.")
    return
end

local keywords = {
    "altar", "flame", "claim", "base", "buildarea", "buildzone",
    "territory", "placement", "rotation", "voxel", "persist", "volume", "bounding"
}
local output = {
    "ShroudForge Altar Resource Type Names",
    "version=" .. tostring(game.version or "unknown"),
    "mode=type-names-only",
    "no-client-mod=true",
    "no-resource-patch=true",
}
local count = 0
for _, name in ipairs(types) do
    if type(name) == "string" and #name <= 200 and not name:find("[\r\n]") then
        local lowered = name:lower()
        for _, keyword in ipairs(keywords) do
            if lowered:find(keyword, 1, true) then
                table.insert(output, name)
                count = count + 1
                break
            end
        end
    end
end
table.sort(output)
io.export("shroudforge-altar-type-names.txt", table.concat(output, "\n") .. "\n")
print("[Altar type probe] Exported " .. tostring(count) .. " type names. No writes or patches.")
