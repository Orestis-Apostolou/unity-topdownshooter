using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Agent References")]
    [Tooltip("RL Agent / Player")]
    public GameObject playerAgent;

    [Tooltip("Traditional Agent")]
    public GameObject enemyAgent;

    [Header("Breakable Walls")]
    [Tooltip("Drag all breakable wall GameObjects here.")]
    public List<GameObject> breakableWalls;

    [Header("Score (read-only in Inspector)")]
    [SerializeField] private int playerWins = 0;
    [SerializeField] private int enemyWins  = 0;

    public int PlayerWins => playerWins;
    public int EnemyWins  => enemyWins;

    [Header("Reset Settings")]
    public float resetDelay = 1.5f;

    private Vector3 playerSpawn;
    private Quaternion playerRot;

    private Vector3 enemySpawn;
    private Quaternion enemyRot;

    private bool roundOver = false;

    private void Awake()
    {
        // Get spawnpoints
        if(playerAgent != null && playerAgent != null)
        {
            playerSpawn = playerAgent.transform.position;
            enemySpawn = enemyAgent.transform.position;
        }

        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        roundOver = false;
    }

    public void OnAgentDied(GameObject deadAgent)
    {
        if (roundOver) return; // guard against double-calls in same frame
        roundOver = true;

        if (deadAgent == playerAgent)
        {
            enemyWins++;
            Debug.Log($"[GameManager] Enemy wins the round! Score → Player:{playerWins} Enemy:{enemyWins}");
        }
        else if (deadAgent == enemyAgent)
        {
            playerWins++;
            Debug.Log($"[GameManager] Player wins the round! Score → Player:{playerWins} Enemy:{enemyWins}");
        }
        else
        {
            Debug.LogWarning($"[GameManager] Unknown agent died: {deadAgent.name}. No score awarded.");
        }

        StartCoroutine(ResetAfterDelay());
    }

    private IEnumerator ResetAfterDelay()
    {
        Time.timeScale = 0.15f;
        yield return new WaitForSecondsRealtime(resetDelay);
        Time.timeScale = 1f;
        ResetGame();
    }

    public void ResetGame()
    {
        roundOver = false;

        ResetAgent(playerAgent, playerSpawn, playerRot);
        ResetAgent(enemyAgent,  enemySpawn, enemyRot);
        ResetWalls();

        Debug.Log("[GameManager] Arena reset. New round started.");
    }

    private void ResetAgent(GameObject agent, Vector3 spawnPoint, Quaternion spawnRot)
    {
        if (agent == null) return;

        // Re-enable defeated agent
        agent.SetActive(true);

        // Reposition
        if (spawnPoint != null)
        {
            agent.transform.position = spawnPoint;
            agent.transform.rotation = spawnRot;
        }

        // Reset HealthSystem
        HealthSystem health = agent.GetComponent<HealthSystem>();
        health.ResetHealth();

        // Reset HeatSystem
        HeatSystem heat = agent.GetComponent<HeatSystem>();
        heat.ResetHeat();

        // Reset Rigidbody velocity so agents don't carry momentum into next round
        Rigidbody2D rb = agent.GetComponent<Rigidbody2D>();
        rb.linearVelocity  = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    private void ResetWalls()
    {
        foreach(GameObject wall in breakableWalls)
        {
            HealthSystem hs = wall.GetComponent<HealthSystem>();
            hs.ResetHealth();
            wall.SetActive(true);
        }
    }   

    // public string GetScoreString() => $"Player: {playerWins}  |  Enemy: {enemyWins}";
}