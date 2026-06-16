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

## Important: what changes and what doesn't

- ✅ **Determinism is preserved.** The same seed still always produces the same run. The mod is a deterministic function.
- ⚠️ **Vanilla seed compatibility is not.** A given seed produces a *different* (now-unbiased) outcome than vanilla, so seeds shared with non-modded players / daily seeds won't reproduce.
- ⚠️ **Multiplayer:** since outcomes differ from vanilla, all players in a lobby must run the mod (or it desyncs). **Single-player is recommended.**

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
