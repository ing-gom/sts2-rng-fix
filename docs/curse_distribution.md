# Neow's Bones curse distribution — vanilla vs fixed

Measured in the **real game engine** (headless boot, real `CurseCardPool`, real `RunRngSet`), replicating Neow's Bones exactly:

```csharp
var curses = ModelDb.CardPool<CurseCardPool>()
    .GetUnlockedCards(UnlockState.all, CardMultiplayerConstraint.None)
    .Where(c => c.CanBeGeneratedByModifiers);
var curse = runState.Rng.Niche.NextItem(curses);   // the curse you receive
```

- **120,000 seeds**, curse pool = 10 curses.
- **Conditioning variable** = the Act 1 map RNG's first draw, bucketed into 4 (`new Rng(runSeed, "act_1_map").NextInt(4)`). This stands in for "the starting map you can see before the curse is decided" — the same kind of observable the community conditioned on (선착장 / 과성장). Coarser than the real biome, so the skew here (~0–26%) is *milder* than the community's biome-conditioned numbers (e.g. Debt 54%); finer conditioning makes it worse.
- **vanilla** = mod off (`STS2_RNG_FIX_DISABLED=1`), **fixed** = `Sts2RngFix` active. Auto-detected per boot via `new Rng(12345).NextInt(1000)` (66 = vanilla, 142 = fixed).

## Unconditional — both uniform (this is why the bug hides)

Across all seeds, every curse is ~10% in **both** vanilla and fixed. The bias is invisible until you condition on something observable.

| Curse | vanilla | fixed |
|---|---|---|
| CLUMSY (서투름) | 10.06% | 10.20% |
| DEBT (빚) | 10.06% | 10.00% |
| DECAY (부패) | 9.94% | 9.91% |
| DOUBT (의심) | 10.00% | 10.07% |
| GUILTY (죄책감) | 9.91% | 9.86% |
| INJURY (상처) | 9.96% | 9.98% |
| NORMALITY (규칙 준수) | 9.99% | 9.86% |
| REGRET (후회) | 10.01% | 9.95% |
| SHAME (수치) | 10.06% | 10.22% |
| WRITHE (몸부림) | 10.02% | 9.95% |

## Conditional on the starting-map bucket — vanilla is heavily skewed, fixed is flat

Each cell = P(curse | that map bucket). **Uniform target = 10%.** `fixed` is 9.65–10.43% in *every* cell, so only the vanilla columns are shown per bucket below (fixed ≈ 10% everywhere).

### Bucket 0
| Curse | vanilla | fixed |
|---|---|---|
| CLUMSY (서투름) | **0.00%** | 10.16% |
| DEBT (빚) | **0.00%** | 10.05% |
| DECAY (부패) | **0.00%** | 9.99% |
| DOUBT (의심) | **0.00%** | 10.07% |
| GUILTY (죄책감) | 2.11% | 9.96% |
| INJURY (상처) | 13.87% | 9.75% |
| NORMALITY (규칙 준수) | 15.68% | 9.88% |
| REGRET (후회) | **26.17%** | 10.04% |
| SHAME (수치) | 24.14% | 10.43% |
| WRITHE (몸부림) | 18.02% | 9.65% |

### Bucket 1
| Curse | vanilla | fixed |
|---|---|---|
| CLUMSY (서투름) | 2.11% | 10.31% |
| DEBT (빚) | **0.00%** | 9.79% |
| DECAY (부패) | **0.00%** | 10.02% |
| DOUBT (의심) | **0.00%** | 10.21% |
| GUILTY (죄책감) | 6.17% | 9.67% |
| INJURY (상처) | 23.65% | 10.02% |
| NORMALITY (규칙 준수) | **24.26%** | 9.80% |
| REGRET (후회) | 13.97% | 9.91% |
| SHAME (수치) | 16.18% | 10.13% |
| WRITHE (몸부림) | 13.65% | 10.13% |

### Bucket 2
| Curse | vanilla | fixed |
|---|---|---|
| CLUMSY (서투름) | 13.56% | 10.25% |
| DEBT (빚) | 15.89% | 9.95% |
| DECAY (부패) | **25.94%** | 9.65% |
| DOUBT (의심) | 24.48% | 10.03% |
| GUILTY (죄책감) | 17.98% | 9.92% |
| INJURY (상처) | **0.00%** | 10.08% |
| NORMALITY (규칙 준수) | **0.00%** | 9.80% |
| REGRET (후회) | **0.00%** | 10.19% |
| SHAME (수치) | **0.00%** | 10.10% |
| WRITHE (몸부림) | 2.14% | 10.01% |

### Bucket 3
| Curse | vanilla | fixed |
|---|---|---|
| CLUMSY (서투름) | **24.51%** | 10.07% |
| DEBT (빚) | 24.29% | 10.20% |
| DECAY (부패) | 13.79% | 9.98% |
| DOUBT (의심) | 15.46% | 9.96% |
| GUILTY (죄책감) | 13.32% | 9.87% |
| INJURY (상처) | 2.31% | 10.09% |
| NORMALITY (규칙 준수) | **0.00%** | 9.96% |
| REGRET (후회) | **0.00%** | 9.65% |
| SHAME (수치) | **0.00%** | 10.21% |
| WRITHE (몸부림) | 6.32% | 10.02% |

## Takeaway

In vanilla, **which curse Neow's Bones gives is largely decided by your map** — in each bucket several curses are *impossible* (0.00%) and one or two dominate (24–26%). Unconditionally it still looks like a fair 10% each, which is why it goes unnoticed. With `Sts2RngFix` every curse is ~10% in every bucket — the curse is no longer predictable from the map.

The same structure (event picks a reward via `base.Rng.NextItem(...)`) drives the **TrashHeap (쓰레기장)** and **DollRoom (인형)** relic events, which are skewed the same way and fixed the same way; see the project README.
