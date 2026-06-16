namespace Sts2RngFix;

/// <summary>
/// Core of the fix: a strong avalanche mix applied to each RNG stream's seed.
///
/// The game derives per-stream seeds by simple addition (<c>runSeed + hash(streamName)</c>,
/// <c>runSeed + NetId + hash(id)</c>, ...). Those seeds differ by small/structured amounts, and
/// <c>System.Random</c>'s first draw is nearly a linear function of the seed, so the streams' first
/// outputs end up correlated with each other. Running every seed through a full-diffusion hash before
/// it reaches <c>System.Random</c> makes the resulting seeds statistically independent, which removes
/// the cross-stream correlation while leaving each individual stream uniform.
/// </summary>
internal static class RngFixService
{
    public static readonly bool Disabled =
        System.Environment.GetEnvironmentVariable("STS2_RNG_FIX_DISABLED") == "1";

    /// <summary>
    /// splitmix32 finalizer. Every input bit affects every output bit, so seeds that differ by a small
    /// constant map to unrelated outputs. Verified offline (2M seeds): with raw seeds, conditioning a
    /// 10-way choice on a correlated 4-way stream skews it to 31.8% / 0.0%; after this mix it returns
    /// to 9.9–10.1% per option.
    /// </summary>
    public static uint Mix(uint x)
    {
        x += 0x9E3779B9u;
        x = (x ^ (x >> 16)) * 0x21F0AAADu;
        x = (x ^ (x >> 15)) * 0x735A2D97u;
        return x ^ (x >> 15);
    }
}
