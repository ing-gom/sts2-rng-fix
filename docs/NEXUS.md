# Nexus Mods listing copy

Reference text for the Nexus Mods page. Nexus uses BBCode in the long
description; the summary field is plain text. The blocks below are
ready to paste. The Nexus listing is English-only; Korean is available
via the linked README.ko.md in the repo.

---

## Summary (≤200 chars)

Removes the "correlated randomness" RNG bias inherited from StS1 — e.g. Neow's Bones curse skewing with your starting map. Avalanche-mixes each seed so the streams decorrelate. Determinism kept.

## Description (BBCode, English)

```bbcode
[size=4][b]What it fixes[/b][/size]

Slay the Spire 2 builds about a dozen separate random-number streams for a run (map, shuffle, card generation, rewards, "niche", and so on). Each is seeded by simple addition — [i]runSeed + hash(streamName)[/i] — and fed straight into .NET's [i]System.Random[/i], whose [b]first output is almost a linear function of its seed[/b]. So two streams seeded from the same run seed (for example the map stream and the Neow's Bones curse stream) end up with [b]correlated first draws[/b].

Unconditionally everything still looks uniform, which is why it's easy to miss. But the moment you [b]condition on something you can observe[/b] — your starting map, your first reward — another result collapses onto a biased subset:
[list]
[*][b]Neow's Bones[/b] hands you a curse whose distribution depends on your starting map (e.g. ~54% Debt on one start, ~73% Writhe on another, with some curses appearing nearly 0% of the time).
[*][b]Act 2 transform cards[/b] lean heavily toward a fixed sub-pool, with map-dependent weights.
[/list]
This is the same [b]"Correlated Randomness"[/b] issue documented for Slay the Spire 1 (the one that inspired StS1's [i]RNG Fix[/i] mod). StS2 tried to mitigate it with the [i]hash(streamName)[/i] offsets, but that isn't enough — the first-draw correlation survives small, structured seed differences.

[size=4][b]How it works[/b][/size]
A single Harmony patch on the [i]Rng[/i] constructor runs each stream's seed through a [b]splitmix32 avalanche hash[/b] before it reaches [i]System.Random[/i]. Full bit-diffusion makes the per-stream seeds statistically independent, so the streams decorrelate. Each individual stream stays perfectly uniform.

[b]Verified.[/b] Offline against the game's own Harmony on the game runtime (the postfix fires through constructor chaining, the field is replaced, the counter is restored, and the same seed stays stable). A 2,000,000-seed simulation: conditioning a 10-way choice on a correlated stream skews it to [b]31.8% / 0.0%[/b] with raw seeds, and back to [b]9.9–10.1% per option[/b] after the mix.

[size=4][b]Scope — what changes vs what's preserved[/b][/size]
The mod changes the [i]dice rolls[/i], never the [i]rules[/i]. Every probability, pool, weight, and pity system stays exactly as vanilla.

[b]Preserved:[/b]
[list]
[*][b]Determinism[/b] — same seed still always produces the same run; save/load is exact.
[*][b]All probability rules / pools / weights[/b] — card rarity odds, the curse pool, map node frequencies, shop pricing, Defect orb ratios — all vanilla.
[*][b]Unconditional (marginal) distributions[/b] — overall rates like "60% common rewards" are identical.
[*][b]Displayed seed string and all non-RNG logic[/b] — card effects, damage, fixed rewards/events.
[*][b]Cosmetic/visual randomness[/b] — VFX, shader noise, color jitter live outside the RNG and are left alone.
[/list]
[b]Changed:[/b]
[list]
[*][b]Concrete outcomes per seed[/b] — which map/monsters/cards/rewards/curse a given seed produces differs from vanilla.
[*][b]Cross-stream correlation removed[/b] — you can no longer predict one result from another (e.g. starting map → curse).
[*][b]Adjacent-seed correlation removed[/b] — e.g. consecutive-enemy skin/power correlation is cleared too.
[/list]

[size=4][b]Important[/b][/size]
[list]
[*]Outcomes differ from vanilla, so [b]seeds shared with non-modded players and daily seeds will not reproduce[/b].
[*][b]Single-player is recommended.[/b] In multiplayer, every player in the lobby must run the mod or the run desyncs.
[/list]

[size=4][b]Installation[/b][/size]
[list=1]
[*]Download the latest [b]Sts2RngFix-vX.Y.Z.zip[/b] from the [url=https://github.com/ing-gom/sts2-rng-fix/releases]GitHub Releases page[/url] (or the Files tab here).
[*]Extract the [i]Sts2RngFix/[/i] folder into [i]<Slay the Spire 2 install>/mods/[/i].
[*]Launch the game.
[/list]

You should end up with:
[code]
<Slay the Spire 2>/mods/Sts2RngFix/Sts2RngFix.dll
<Slay the Spire 2>/mods/Sts2RngFix/Sts2RngFix.json
[/code]

[size=4][b]Configuration[/b][/size]
[list]
[*][b]Disable the mod[/b] — set [i]STS2_RNG_FIX_DISABLED=1[/i] before launching the game (no need to uninstall).
[/list]

[size=4][b]Caveats[/b][/size]
[list]
[*]The mod patches the [i]Rng[/i] constructor. If a future game patch changes that class, the source may need an update.
[*]Because outcomes diverge from vanilla, do not use this if you need seed parity with other players or with the base game.
[/list]

[size=4][b]Credits / Source[/b][/size]
[list]
[*]MegaCrit — Slay the Spire 2.
[*]HarmonyX — runtime patching library used by this mod (bundled with the game; not redistributed).
[*]Background reading: [url=https://forgottenarbiter.github.io/Correlated-Randomness/]"Correlated Randomness in Slay the Spire" (forgottenarbiter)[/url].
[*]Source: [url=https://github.com/ing-gom/sts2-rng-fix]github.com/ing-gom/sts2-rng-fix[/url] · [url=https://github.com/ing-gom/sts2-rng-fix/blob/main/LICENSE]MIT License[/url]
[*]한국어 설명: [url=https://github.com/ing-gom/sts2-rng-fix/blob/main/README.ko.md]README.ko.md[/url]
[/list]
```
