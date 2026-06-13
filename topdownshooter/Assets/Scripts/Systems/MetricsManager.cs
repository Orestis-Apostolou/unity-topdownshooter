using UnityEngine;
using Unity.MLAgents;

public class MetricsManager : MonoBehaviour
{
    public static MetricsManager Instance { get; private set; }

    [Header("References")]
    public GameObject playerAgent;
    public GameObject enemyAgent;
    private HeatSystem playerHeatSys;

    // Round Timer
    private float roundStartTime;

    // Accuracy
    private int playerShotsFired;
    private int playerShotsHit;

    private int enemyShotsFired;
    private int enemyShotsHit;

    // Health related metrics
    private float playerHealthOnWin;      // player HP when enemy dies
    private float enemyHealthOnLoss;      // enemy HP when player dies

    // Heat related metrics
    private float playerHeatAccumulator;  // sum of heat samples this round
    private int   playerHeatSamples;      // how many samples taken

    // Accumulated metrics (averaged across episodes)
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

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        playerHeatSys = playerAgent.GetComponent<HeatSystem>();
        ResetRoundMetrics();
    }

    private void FixedUpdate()
    {
        // Sample heat every physics step
        playerHeatAccumulator += playerHeatSys.HeatPercent();
        playerHeatSamples++;
    }

    // Called from FiringSystem script
    public void OnShotFired(GameObject shooter)
    {
        if (shooter == playerAgent) playerShotsFired++;
        else enemyShotsFired++;
    }

    // Called from GameManager script
    public void OnShotHit(GameObject victim)
    {
        if (victim == playerAgent) enemyShotsHit++;
        else playerShotsHit++;
    }

    // Called from GameManager script
    public void OnRoundEnd(bool playerWon)
    {
        float timeAlive = Time.time - roundStartTime;

        float playerAccuracy = playerShotsFired > 0 
            ? (float)playerShotsHit / playerShotsFired : 0f;
        float enemyAccuracy  = enemyShotsFired  > 0 
            ? (float)enemyShotsHit  / enemyShotsFired  : 0f;
        float avgHeat = playerHeatSamples > 0 
            ? playerHeatAccumulator / playerHeatSamples : 0f;

        // Accumulate for running averages
        totalAccuracyPlayer += playerAccuracy;
        totalAccuracyEnemy += enemyAccuracy;
        totalHeatLevel += avgHeat;
        totalShotsFiredPerRound += playerShotsFired;
        episodeCount++;

        if (playerWon)
        {
            playerHealthOnWin   = playerAgent.GetComponent<HealthSystem>().HealthPercent();
            totalHealthOnWin   += playerHealthOnWin;
            totalTimeAliveOnWin += timeAlive;
            playerWins++;
        }
        else
        {
            enemyHealthOnLoss      = enemyAgent.GetComponent<HealthSystem>().HealthPercent();
            totalEnemyHealthOnLoss += enemyHealthOnLoss;
            totalTimeAliveOnLoss   += timeAlive;
            enemyWins++;
        }

        Debug.Log($"[MetricsManager] Score: PLAYER {playerWins} | ENEMY {enemyWins}");

        LogToStatsRecorder();
        ResetRoundMetrics();
    }

    // Load metrics to TensorBoard
    private void LogToStatsRecorder()
    {
        if (!Academy.Instance.IsCommunicatorOn) return;

        var stats = Academy.Instance.StatsRecorder;

        // Per-episode metrics
        float playerAcc = playerShotsFired > 0 ? (float)playerShotsHit / playerShotsFired : 0f;
        float enemyAcc  = enemyShotsFired  > 0 ? (float)enemyShotsHit  / enemyShotsFired  : 0f;
        float avgHeat   = playerHeatSamples > 0 ? playerHeatAccumulator / playerHeatSamples : 0f;

        stats.Add("Metrics/Player Accuracy",       playerAcc);
        stats.Add("Metrics/Enemy Accuracy",        enemyAcc);
        stats.Add("Metrics/Avg Heat Level",        avgHeat);
        stats.Add("Metrics/Shots Fired Per Round", playerShotsFired);

        // Win/loss conditional metrics — only log when relevant
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
}