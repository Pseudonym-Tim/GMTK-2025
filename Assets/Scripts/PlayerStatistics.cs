using UnityEngine;

/// <summary>
/// Handles everything to do with player statistics...
/// </summary>
public class PlayerStatistics : MonoBehaviour
{
    public int TotalKills { get; private set; } = 0;
    public int RecursionCount { get; private set; } = 0;
    public int NearMissCount { get; private set; } = 0;
    public int WavesComplete { get; private set; } = 0;
    public int StagesComplete { get; private set; } = 0;

    public void ResetStats() { TotalKills = 0; RecursionCount = 0; NearMissCount = 0; WavesComplete = 0; StagesComplete = 0; }
    public void RegisterKill() => TotalKills++;
    public void RegisterRecursion() => RecursionCount++;
    public void RegisterNearMiss() => NearMissCount++;
    public void RegisterWaveComplete(int waveAmount) => WavesComplete = waveAmount;
    public void RegisterStageComplete(int stageAmount) => StagesComplete = stageAmount;
}