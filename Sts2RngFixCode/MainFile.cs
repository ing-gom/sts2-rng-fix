using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace Sts2RngFix;

/// <summary>
/// Entry point. Installs a single Harmony patch on the <c>Rng</c> constructor that decorrelates the
/// game's per-purpose RNG streams.
///
/// The game seeds every stream as <c>new System.Random((int)(runSeed + hash(streamName)))</c>
/// (<c>RunRngSet.CreateRng</c>). Because <c>System.Random</c>'s first output is a near-linear function
/// of its seed, two streams seeded from the same run seed (e.g. the map stream and the Neow's-Bones
/// curse stream) produce correlated first draws. Unconditionally the distribution looks uniform, but
/// conditioning on an observable (the starting map) collapses another result (the curse) onto a biased
/// subset — the "Correlated Randomness" bug carried over from StS1.
///
/// The fix runs each stream's seed through a splitmix32 avalanche before it reaches
/// <c>System.Random</c>, so the streams become statistically independent. Determinism is preserved
/// (same seed → same result); only the seed→outcome mapping changes versus vanilla.
///
/// Disable at runtime via env var <c>STS2_RNG_FIX_DISABLED=1</c>.
/// </summary>
[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "Sts2RngFix";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; }
        = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        try
        {
            if (RngFixService.Disabled)
            {
                Logger.Info($"[{ModId}] disabled via STS2_RNG_FIX_DISABLED=1; no patches applied.");
                return;
            }

            var harmony = new Harmony(ModId);
            harmony.PatchAll(typeof(MainFile).Assembly);
            Logger.Info($"[{ModId}] Harmony patches applied.");
            Logger.Info($"[{ModId}] initialized (v0.1.0).");
        }
        catch (Exception ex)
        {
            Logger.Warn($"[{ModId}] init failed: {ex.Message}");
        }
    }
}
