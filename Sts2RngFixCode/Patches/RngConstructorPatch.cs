using HarmonyLib;
using MegaCrit.Sts2.Core.Random;

namespace Sts2RngFix.Patches;

/// <summary>
/// Postfix on <see cref="Rng"/>'s primary constructor <c>Rng(uint seed, int counter)</c> — the single
/// place the underlying <c>System.Random</c> is created (<c>_random = new System.Random((int)seed)</c>).
/// The <c>Rng(uint, string)</c> overload chains into this one, so this patch also covers name-derived
/// streams.
///
/// We rebuild <c>_random</c> from the avalanche-mixed seed and then replay <c>Counter</c> draws so the
/// stream lands at the exact same position the original constructor left it at (the base constructor
/// runs <c>FastForwardCounter(counter)</c> before this postfix, setting <see cref="Rng.Counter"/>).
/// This keeps save/load deterministic: a reloaded run reconstructs the identical mixed stream.
///
/// We do not touch the public <see cref="Rng.Seed"/> value, so any code that further derives seeds
/// from it (e.g. <c>seed + NetId + hash(id)</c>) keeps using the vanilla values — those get mixed in
/// turn when they construct their own <see cref="Rng"/>.
/// </summary>
[HarmonyPatch(typeof(Rng), MethodType.Constructor, new[] { typeof(uint), typeof(int) })]
internal static class Rng_Constructor_Patch
{
    private static readonly AccessTools.FieldRef<Rng, System.Random> RandomRef =
        AccessTools.FieldRefAccess<Rng, System.Random>("_random");

    private static void Postfix(Rng __instance, uint seed)
    {
        uint mixed = RngFixService.Mix(seed);
        var r = new System.Random(unchecked((int)mixed));

        int counter = __instance.Counter;
        for (int i = 0; i < counter; i++) r.Next();

        RandomRef(__instance) = r;
    }
}
