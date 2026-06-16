using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Random;

namespace Sts2RngFix.Patches;

/// <summary>
/// Shared mixing logic for the two <see cref="Rng"/>-constructor patches below. Rebuilds the
/// underlying <c>System.Random</c> from the avalanche-mixed seed and replays <see cref="Rng.Counter"/>
/// draws so the stream lands at the exact position the original constructor left it (keeping save/load
/// deterministic). The public <see cref="Rng.Seed"/> value is left untouched.
/// </summary>
internal static class RngMix
{
    private static readonly AccessTools.FieldRef<Rng, System.Random> RandomRef =
        AccessTools.FieldRefAccess<Rng, System.Random>("_random");

    internal static void Apply(Rng instance, uint finalSeed)
    {
        var r = new System.Random(unchecked((int)RngFixService.Mix(finalSeed)));
        int counter = instance.Counter;
        for (int i = 0; i < counter; i++) r.Next();
        RandomRef(instance) = r;
    }
}

/// <summary>
/// Patches <c>Rng(uint, int)</c> — the direct-seed constructor (e.g. <c>new Rng(runSeed + NetId)</c>,
/// <c>Rng.Chaotic</c>) and the chaining target of the string overload.
/// </summary>
[HarmonyPatch(typeof(Rng), MethodType.Constructor, new[] { typeof(uint), typeof(int) })]
internal static class Rng_UintInt_Ctor_Patch
{
    private static void Postfix(Rng __instance, uint seed) => RngMix.Apply(__instance, seed);
}

/// <summary>
/// Patches <c>Rng(uint, string)</c> — the name-derived constructor used by
/// <see cref="MegaCrit.Sts2.Core.Runs.RunRngSet"/> for its per-purpose streams. It normally chains into
/// <c>Rng(uint, int)</c>, but patching it directly guards against the JIT inlining the base constructor
/// into it before our patch is installed (which would otherwise leave every RunRngSet stream unmixed).
/// Both postfixes compute the same final seed, so when both fire the operation is idempotent — no
/// double-mixing.
/// </summary>
[HarmonyPatch(typeof(Rng), MethodType.Constructor, new[] { typeof(uint), typeof(string) })]
internal static class Rng_UintString_Ctor_Patch
{
    private static void Postfix(Rng __instance, uint seed, string name) =>
        RngMix.Apply(__instance, seed + (uint)StringHelper.GetDeterministicHashCode(name));
}
