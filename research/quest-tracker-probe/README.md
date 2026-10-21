# ShroudNav — vanilla in-game quest tracker feasibility probe

**Not a mod release. Not tested on Enshrouded 1.0. Not installed on Velaris.**

This is a narrowly scoped research kit for the **actual** user requirement: improve the game client's existing quest tracker, map markers and journal with **NO client-side installation**. The external browser quest guide was rejected and PR #22 was closed unmerged. The road-data research is irrelevant until a server-only client-facing modification works.

## What open source already proves

1. [Brabb3l / kfc-parser (GPL-3.0)](https://github.com/Brabb3l/kfc-parser) documents parsing typed KFC game assets, reflection metadata, unpack/repack, and the Lua Enshrouded Mod Loader (EML) `game.assets` APIs.
2. [EML resource API documentation](https://brabb3l.github.io/kfc-parser/eml/develop/getting_started.html) documents enumerating resource types and reading/writing typed resources. The currently supported mode is *pre-start asset patching*, not arbitrary runtime UI controls.
3. [lucastucious/enshrouded-better-loc (CC0)](https://github.com/lucastucious/enshrouded-better-loc) provides a concrete example of reading `keen::LocaTagCollectionResource`, reading `keen::LocaTagCollectionResourceData`, finding a localization tag ID, creating replacement content with `buffer:write_resource` and redirecting `loc.dataHash`.
4. [Enshrouded dedicated-server configuration](https://enshrouded.zendesk.com/hc/en-us/articles/16055441447709-Dedicated-Server-Configuration) does **not** expose creating custom quest tracker elements.
5. This only proves it is possible to rebuild a typed localization resource in a modded game installation. It **does not prove** that server-side localization edits are transmitted to clients or that quest objective locations can be overridden.

**Critical:** EML may require the `dbghelp.dll` proxy loader on the server clone. It must never be introduced into the owner's working server where alternate loaders were previously found incompatible.

## Deliverables

- `read-only-type-discovery/`: EML export-only mod that enumerates names of resource types containing Quest/Objective/Journal/Lore/Marker/Loca/Map, not their private/game data. Use only in an isolated clone.
- `probe.py`: **actual compiler** that turns a LOCAL `En_Us` localization export from a legally owned game copy into an exact-build, one-tag EML proof package. It embeds an expected original string and refuses build mismatches or tag ambiguity. Outputs original mod code and a ZIP for a disposable clone; never writes to game directories.
- `probe.lua.template`: the compiled EML script's source. It recreates just one language-content resource and changes one string from exact original to original plus ` [SF SERVER TEST]`.
- `tests/`: synthetic tests, with no game data, network, Steam or Windows services.
- GitHub Actions synthetic verification. **The GitHub workflow cannot decompile proprietary Enshrouded binaries because we do not have them.**

## How to use, when an isolated test install is available

1. Make a **separate copy** of a legally obtained Enshrouded 1.0 server and keep an entirely independent, disposable world. Save SHA-256 hashes of the executable, `.kfc`, and `.kfc_resources`. Never load the Velaris save or copy its settings/keys.
2. From the documented [EML CLI](https://brabb3l.github.io/kfc-parser/eml/usage.html), using the disposable copy only, inspect resource types with the `read-only-type-discovery` mod and the **export** capability. Follow EML's documented mod folder/manifest. No patch capability is necessary for this discovery step.
3. With a source localization export created locally by EML (see [existing CC0 open-source exporter](https://github.com/lucastucious/enshrouded-better-loc)), inspect likely quest strings:

   ```powershell
   python research/quest-tracker-probe/probe.py --export "C:\temp\En_Us.json" --scan quest journal marker
   ```

   This scan only reads an existing local file; it **does not extract Enshrouded game files by itself**.
4. Choose a **real numeric localization ID** used on an existing quest line, from that same exact-build export. Ideally use a quest where you can capture the old text in-game. Compile an exact-build single-ID test package:

   ```powershell
   python research/quest-tracker-probe/probe.py --export "C:\temp\En_Us.json" --tag-id 123456 --output-dir "C:\temp\quest-proof-output"
   ```

   Replace the example `123456` with a tag ID actually present in the export. Output ZIP is for an isolated game/server clone. All other original strings remain untouched. Build ID is derived from the export and rechecked at EML execution.
5. Record the original quest text on the vanilla client **before** the patch. Stop the isolated server, install **only the proof** and **one compatible EML loader** on that cloned server; never alter the client. Start and capture the EML console success/refusal.
6. Join with the **same unmodified vanilla client**. Check the on-screen quest tracker, map marker text and journal. If the test string appears, record **exact evidence** (client screenshot, affected ID, build hashes) and check another unmodified client. If it does **not** appear, mark the **server-only localization path NO-GO**, don't invent a fake fix.
7. **Discard the disposable server** after the trial. Do not change quest progress, world saves or production game resources. There is no deployment path in this PR.

### Why test a text tag first?

The smallest falsifiable experiment is a harmless, visibly marked string. If the vanilla client uses its own local localization resources and never displays the server-edited string, a larger server-only quest guide will not become viable just by extracting more roads. Quest-state and positional marker changes are more intrusive and must not be attempted before we know the client consumes any server-edited quest data.

### Limitations

- We cannot and did not decompile or recompile actual proprietary game binaries here: the dedicated-server files and 1.0 reflection exports are not available in this conversation. The work uses **open-source tooling and documented interfaces** to compile a safe test mod from a separately supplied lawful extraction.
- The test can confirm a string delivery path, not a full tracker replacement. Even if it passes, we must separately discover quest/objective marker data, serialization direction, build compatibility, and multiplayer behavior.
- No manual browser or second-screen guide is an acceptable replacement for this user's intended feature.
- Do not merge or deploy based on synthetic tests. If the one-tag server-only proof fails, **stop** pursuing the no-install in-game quest-tracker modification unless a different concrete server-to-client mechanism is demonstrated.
