# SPT 4.1 compatibility build

Targets SPT 4.1 and EFT 0.16.9.40743. Client type migrations use `E:\Downloads\4.1 mappings.txt` and the installed 4.1 assembly metadata. Server references resolve from `SPT_Runtime` and require the .NET 10 SDK to build.

- Removed the minimap, its hotkeys, layout/zoom state, and client/server settings. Full-screen maps and hold/toggle peek remain available.
- Out-of-raid quest markers now include all matching forced-spawn positions across every map and every target item. Like SPT-QuestMap, the lookup scans every template item after database modification has finished. It preserves Factory and Ground Zero variants for the existing map definitions to match.
- Completed objectives are filtered within their own quest. Duplicate positions are removed, and missing/invalid positions do not create marker records.
- Removed enemy PMC/scav/boss, backpack, wishlisted loot, BTR, airdrop, hidden stash, corpse, and helicopter crash markers, including their client/server options, progression requirements, colors, hooks, and dedicated assets. Friendly player markers and their death/despawn cleanup remain.
- The existing isolated SMAA map rendering and shader bundles are retained.

## Build and package

```powershell
pwsh -File Tools/Build-Release.ps1 -SptRoot D:\Tarkov-SPT-4.1
```

Produces `artifacts/DynamicMaps-1.0.5-SPT4.1.zip` with both client and server components. Normal builds do not deploy. The legacy client-only post-build deployment requires `-p:DeployOnBuild=true`.

Extract the archive into the SPT 4.1 game root. The server DLL, shared DLL, and config belong together in `SPT_Runtime/user/mods/DynamicMaps`; the client files belong in `BepInEx/plugins/DynamicMaps`. Replace the previous DynamicMaps installation instead of leaving duplicate copies in differently named mod folders. Preserve any desired server config customizations; removed minimap and marker options are no longer used.

## Validation

```powershell
dotnet run --project Tests/DynamicMaps.RegressionTests.csproj -c Release -- D:\Tarkov-SPT-4.1 --database
```

The suite checks multiple targets/positions/maps, map variants, completed objectives, ordinary-loot exclusion, invalid coordinates, serialization, and client Harmony/reflection targets. The optional database checks read the installed database without modifying it and confirm 23 positions for Getting Acquainted and four for You've Got Mail in the matched SPT 4.1 database. These exact counts can change with database updates or mods.

Builds and metadata checks do not validate live Unity behavior. In-game checks remain: out-of-raid map switching and quest markers, full-screen/peek transitions, raid markers, and SMAA with DLSS enabled and disabled.
