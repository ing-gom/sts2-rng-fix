# StS2 RNG Fix

A Slay the Spire 2 mod that removes the **correlated-randomness bias** in the game's RNG.

> 한국어 설명은 [README.ko.md](README.ko.md).

## The bug

Slay the Spire 2 builds ~12 separate RNG streams for a run (map, shuffle, card generation, rewards, "niche", …). Each is seeded by **simple addition**:

```csharp
// RunRngSet.CreateRng
new Rng(runSeed + hash(streamName))   // -> new System.Random((int)thatSeed)
```

`System.Random`'s **first output is almost a linear function of its seed**. So two streams seeded from the same `runSeed` (e.g. the map stream and the Neow's-Bones curse stream) produce **correlated first draws**.

Unconditionally everything looks uniform — which is why it's easy to miss. But the moment you **condition on something you can observe** (your starting map, your first reward…), another result collapses onto a biased subset:

- **Neow's Bones** gives a curse whose distribution depends on your starting map — e.g. ~54% Debt on one start, ~73% Writhe on another, with some curses appearing near 0% of the time.
- **Act 2 transform cards** lean heavily toward a fixed sub-pool, with map-dependent weights.

This is the same **"Correlated Randomness"** issue documented for StS1 ([forgottenarbiter analysis](https://forgottenarbiter.github.io/Correlated-Randomness/), which inspired StS1's *RNG Fix* mod). StS2 tried to mitigate it by adding `hash(streamName)` offsets, but that isn't enough because the first-draw correlation survives small/structured seed differences.

## The fix

A single Harmony patch on the `Rng` constructor runs each stream's seed through a **splitmix32 avalanche hash** before it reaches `System.Random`:

```
System.Random( mix(runSeed + hash(streamName)) )
```

Full bit-diffusion makes the per-stream seeds statistically independent, so the streams decorrelate. Each individual stream stays perfectly uniform.

**Verified offline (2,000,000 seeds):** conditioning a 10-way choice on a correlated 4-way stream skews it to **31.8% / 0.0%** with raw seeds, and back to **9.9–10.1% per option** after the mix.

**Verified in the real game engine** (headless boot, real `CurseCardPool`, 120k seeds): the actual Neow's Bones curse, conditioned on the starting map, has several curses at **0.00%** and others at **24–26%** in vanilla — and a flat **~10% each** with the mod. Full per-curse table: [docs/curse_distribution.md](docs/curse_distribution.md).

## Important: what changes and what doesn't

- ✅ **Determinism is preserved.** The same seed still always produces the same run. The mod is a deterministic function.
- ✅ **Modded seeds reproduce among mod users.** The mix is a fixed transform, so a seed shared between two players who both run **the same mod version** plays out identically. Seed-sharing still works — the compatible group is just "same-version mod users" instead of "vanilla players". (When sharing a seed, mention the version, e.g. *Sts2RngFix v0.1.0*; a future change to the mix would form a new compatibility group.)
- ⚠️ **Vanilla seed compatibility is not.** A given seed produces a *different* (now-unbiased) outcome than vanilla, so seeds shared with non-modded players / daily seeds won't reproduce.
- ⚠️ **Multiplayer:** since outcomes differ from vanilla, all players in a lobby must run the mod (or it desyncs). **Single-player is recommended.**

## Scope — what changes vs what's preserved

The fix is a single patch on the `Rng` constructor, which is the **one chokepoint every gameplay random draw passes through** (verified across the whole decompiled assembly). So the boundary is clean: anything that goes through `Rng` is affected; anything that doesn't is untouched.

**One-line summary:** the mod changes the *dice rolls*, never the *rules*. Every probability, pool, weight, and pity system stays exactly as vanilla — only (a) which outcome a given seed lands on and (b) the correlation between streams change.

### ✅ Preserved

- **Determinism** — same seed → same modded run, every time. Save/load restores the exact stream position.
- **All probability rules / distributions / pools / weights / pity** — not a single game rule is changed. Card rarity odds, the curse pool, map node frequencies, shop pricing, Defect orb ratios, etc. are all vanilla.
- **Unconditional (marginal) distributions** — overall stats like "card rewards are 60% common" or "overall curse appearance rate" are identical (vanilla was already uniform *unconditionally*).
- **Within-stream quality** — each stream is still one `System.Random`; its internal sequence quality was already fine and is unchanged.
- **Displayed seed string and all non-RNG logic** — card effects, damage formulas, fixed rewards/events, fixed treasure nodes, etc.
- **Cosmetic/visual randomness** — VFX scale, poison-smoke direction, shader noise, color jitter, etc. live *outside* `Rng` and are deliberately left alone (they're meant to be non-deterministic visual flavor).

### 🔄 Changed

1. **Concrete outcomes per seed** — which map/monsters/cards/rewards/curse/shop a given seed produces differs from vanilla (seeds are re-mixed). This is why vanilla seed-sharing / daily seeds no longer reproduce.
2. **Cross-stream correlation removed** — you can no longer predict one result from another (e.g. "starting map → Neow's Bones curse"). This is the whole point of the fix.
3. **Adjacent-seed correlation removed** — related side effects like consecutive-NetId enemy skin/power correlation are cleared up too.

### Gameplay streams affected (all re-rolled; rules preserved)

- **RunRngType (12):** UpFront, Shuffle (combat deck order), UnknownMapPoint (? room type), CombatCardGeneration, CombatPotionGeneration, CombatCardSelection, CombatEnergyCosts, CombatTargets, MonsterAi (enemy intents), Niche (Neow's Bones curse, …), CombatOrbs (Defect), TreasureRoomRelics
- **PlayerRngType (3):** Rewards (card/relic rewards), Shops (shop stock), Transformations (**includes Act 2 transform cards**)
- **Plus** map generation, monster encounters, and per-entity RNG — all flow through `Rng` and are mixed too.

## Install

1. Download `Sts2RngFix-vX.Y.Z.zip` from [Releases](../../releases).
2. Extract the `Sts2RngFix/` folder into your Slay the Spire 2 `mods/` directory:
   ```
   Slay the Spire 2/mods/Sts2RngFix/Sts2RngFix.dll
   Slay the Spire 2/mods/Sts2RngFix/Sts2RngFix.json
   ```
3. Launch the game.

To turn the fix off without uninstalling, set the environment variable `STS2_RNG_FIX_DISABLED=1`.

## Build from source

```
dotnet build -c Release
```

Requires Slay the Spire 2 installed (the build auto-discovers `sts2.dll` / `0Harmony.dll`). The DLL + manifest are copied into the game's `mods/Sts2RngFix/` folder on build.

## License

MIT — see [LICENSE](LICENSE).
