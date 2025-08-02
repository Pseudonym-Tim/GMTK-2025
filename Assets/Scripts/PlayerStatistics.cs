using UnityEngine;

/// <summary>
/// Handles everything to do with player statistics...
/// </summary>
public class PlayerStatistics : MonoBehaviour
{
    public int TotalKills { get; private set; }
    public int RecursionCount { get; private set; }
    public int NearMissCount { get; private set; }
    public int WavesComplete { get; private set; }
    public int StagesComplete { get; private set; }

    public void ResetStats() { TotalKills = 0; RecursionCount = 0; NearMissCount = 0; WavesComplete = 0; StagesComplete = 0; }
    public void RegisterKill() => TotalKills++;
    public void RegisterRecursion() => RecursionCount++;
    public void RegisterNearMiss() => NearMissCount++;
    public void RegisterWaveComplete() => WavesComplete++;
    public void RegisterStageComplete() => StagesComplete++;
}