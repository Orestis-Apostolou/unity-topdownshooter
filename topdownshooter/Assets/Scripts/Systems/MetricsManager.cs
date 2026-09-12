using UnityEngine;
using Unity.MLAgents;
using System.Collections.Generic;
using System.IO;

public class MetricsManager : MonoBehaviour
{
    public static MetricsManager Instance { get; private set; }

    [Header("Evaluation Settings")]
    public int totalEpisodesToRun = 100; // Πόσα επεισόδια θα κρατήσει το benchmark
    [Range(1f, 50f)]
    public float simulationSpeed = 10f;  // Επιτάχυνση simulation
    public string csvFileName = "Inference_Results.csv";

    [Header("References")]
    public GameObject playerAgent;
    public GameObject enemyAgent;
    private HeatSystem playerHeatSys;

    // List tracking for precise CSV Export & Statistics
    private List<float> episodeRewards = new List<float>();
    private List<float> episodeDurations = new List<float>();

    // Round Timer
    private float roundStartTime;

    // Accuracy
    private int playerShotsFired;
    private int playerShotsHit;
    private int enemyShotsFired;
    private int enemyShotsHit;

    // Health related metrics
    private float playerHealthOnWin;
    private float enemyHealthOnLoss;

    // Heat related metrics
    private float playerHeatAccumulator;
    private int playerHeatSamples;

    // Accumulated metrics
    private float totalAccuracyPlayer;
    private float totalAccuracyEnemy;
    private float totalHealthOnWin;
    private float totalEnemyHealthOnLoss;
    private float totalHeatLevel;
    private float totalShotsFiredPerRound;
    private float totalTimeAliveOnWin;
    private float totalTimeAliveOnLoss;
    private int episodeCount;
    private int playerWins;
    private int enemyWins;
    private int winBit;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        playerHeatSys = playerAgent.GetComponent<HeatSystem>();

        // 1. Επιτάχυνση του χρόνου κατά την έναρξη του Inference
        Time.timeScale = simulationSpeed;
        Time.fixedDeltaTime = 0.02f / simulationSpeed;

        ResetRoundMetrics();
    }

    private void FixedUpdate()
    {
        playerHeatAccumulator += playerHeatSys.HeatPercent();
        playerHeatSamples++;
    }

    public void OnShotFired(GameObject shooter)
    {
        if (shooter == playerAgent) playerShotsFired++;
        else enemyShotsFired++;
    }

    public void OnShotHit(GameObject victim)
    {
        if (victim == playerAgent) enemyShotsHit++;
        else playerShotsHit++;
    }

    public void OnRoundEnd(bool playerWon)
    {
        float timeAlive = Time.time - roundStartTime;

        float playerAccuracy = playerShotsFired > 0 
            ? (float)playerShotsHit / playerShotsFired : 0f;
        float enemyAccuracy  = enemyShotsFired  > 0 
            ? (float)enemyShotsHit  / enemyShotsFired  : 0f;
        float avgHeat = playerHeatSamples > 0 
            ? playerHeatAccumulator / playerHeatSamples : 0f;

        // 2. Συλλογή του RLAgent Cumulative Reward
        float currentReward = 0f;
        var rlAgentScript = playerAgent.GetComponent<RLAgent>();
        if (rlAgentScript != null)
        {
            currentReward = rlAgentScript.GetCumulativeReward();
        }

        episodeRewards.Add(currentReward);
        episodeDurations.Add(timeAlive);

        // Accumulate for running averages
        totalAccuracyPlayer += playerAccuracy;
        totalAccuracyEnemy += enemyAccuracy;
        totalHeatLevel += avgHeat;
        totalShotsFiredPerRound += playerShotsFired;
        episodeCount++;

        if (playerWon)
        {
            playerHealthOnWin = playerAgent.GetComponent<HealthSystem>().HealthPercent();
            totalHealthOnWin += playerHealthOnWin;
            totalTimeAliveOnWin += timeAlive;
            winBit = 1;
            playerWins++;
        }
        else
        {
            enemyHealthOnLoss = enemyAgent.GetComponent<HealthSystem>().HealthPercent();
            totalEnemyHealthOnLoss += enemyHealthOnLoss;
            totalTimeAliveOnLoss += timeAlive;
            winBit = 0;
            enemyWins++;
        }

        Debug.Log($"[MetricsManager] Episode {episodeCount}/{totalEpisodesToRun} | Score: PLAYER {playerWins} - ENEMY {enemyWins} | Reward: {currentReward:F2}");

        LogToStatsRecorder();

        // 3. Έλεγχος αν ολοκληρώθηκαν τα επεισόδια του Evaluation
        if (episodeCount >= totalEpisodesToRun)
        {
            FinishEvaluation();
        }
        else
        {
            ResetRoundMetrics();
        }
    }

    private void LogToStatsRecorder()
    {
        if (!Academy.Instance.IsCommunicatorOn) return;

        var stats = Academy.Instance.StatsRecorder;

        float playerAcc = playerShotsFired > 0 ? (float)playerShotsHit / playerShotsFired : 0f;
        float enemyAcc  = enemyShotsFired  > 0 ? (float)enemyShotsHit  / enemyShotsFired  : 0f;
        float avgHeat   = playerHeatSamples > 0 ? playerHeatAccumulator / playerHeatSamples : 0f;

        stats.Add("Metrics/Player Accuracy",        playerAcc);
        stats.Add("Metrics/Enemy Accuracy",         enemyAcc);
        stats.Add("Metrics/Avg Heat Level",         avgHeat);
        stats.Add("Metrics/Shots Fired Per Round",  playerShotsFired);
        stats.Add("Metrics/Win Rate",               winBit);

        if (playerHealthOnWin > 0)
        {
            stats.Add("Metrics/Avg Health On Win",         playerHealthOnWin);
            stats.Add("Metrics/Avg Time Alive On Win",     Time.time - roundStartTime);
        }
        if (enemyHealthOnLoss > 0)
        {
            stats.Add("Metrics/Enemy Health On Loss",      enemyHealthOnLoss);
            stats.Add("Metrics/Avg Time Alive On Loss",    Time.time - roundStartTime);
        }
    }

    public void ResetRoundMetrics()
    {
        roundStartTime        = Time.time;
        playerShotsFired      = 0;
        playerShotsHit        = 0;
        enemyShotsFired       = 0;
        enemyShotsHit         = 0;
        playerHealthOnWin     = 0f;
        enemyHealthOnLoss     = 0f;
        playerHeatAccumulator = 0f;
        playerHeatSamples     = 0;
    }

    // 4. Υπολογισμός τελικών στατιστικών & Εξαγωγή σε CSV
    private void FinishEvaluation()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        float winRate = ((float)playerWins / totalEpisodesToRun) * 100f;

        // Mean Reward & Standard Deviation
        float sumReward = 0f;
        foreach (float r in episodeRewards) sumReward += r;
        float meanReward = sumReward / totalEpisodesToRun;

        float sumSquares = 0f;
        foreach (float r in episodeRewards) sumSquares += Mathf.Pow(r - meanReward, 2);
        float stdDevReward = Mathf.Sqrt(sumSquares / totalEpisodesToRun);

        Debug.Log("=========================================");
        Debug.Log($"=== EVALUATION COMPLETED ({totalEpisodesToRun} Episodes) ===");
        Debug.Log($"Win Rate: {winRate:F2}%");
        Debug.Log($"Mean Reward: {meanReward:F2} (±{stdDevReward:F2})");
        Debug.Log($"Avg Player Accuracy: {(totalAccuracyPlayer / totalEpisodesToRun) * 100f:F1}%");
        Debug.Log("=========================================");

        SaveToCSV(winRate, meanReward, stdDevReward);

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    private void SaveToCSV(float winRate, float meanReward, float stdDev)
    {
        string filePath = Path.Combine(Application.dataPath, csvFileName);
        using (StreamWriter writer = new StreamWriter(filePath, false))
        {
            writer.WriteLine("Episode,Reward,DurationSec");
            for (int i = 0; i < episodeRewards.Count; i++)
            {
                writer.WriteLine($"{i + 1},{episodeRewards[i].ToString("F2", System.Globalization.CultureInfo.InvariantCulture)},{episodeDurations[i].ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}");
            }
            writer.WriteLine();
            writer.WriteLine("Summary Metric,Value");
            writer.WriteLine($"Win Rate (%),{winRate.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}");
            writer.WriteLine($"Mean Reward,{meanReward.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}");
            writer.WriteLine($"Std Dev Reward,{stdDev.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}");
            writer.WriteLine($"Avg Player Accuracy (%),{((totalAccuracyPlayer / totalEpisodesToRun) * 100f).ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}");
            writer.WriteLine($"Avg Shots Fired,{ (totalShotsFiredPerRound / totalEpisodesToRun).ToString("F2", System.Globalization.CultureInfo.InvariantCulture)}");
        }

        Debug.Log($"[CSV SAVED] File saved successfully at: {filePath}");
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}