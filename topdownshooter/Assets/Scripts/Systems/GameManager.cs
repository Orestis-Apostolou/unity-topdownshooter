using System.Collections;
using Unity.MLAgents;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Agent References")]
    [Tooltip("RL Agent / Player")]
    public GameObject playerAgent;
    [SerializeField] private RLAgent playerRLScript;
    [SerializeField] private RLAgent enemyRLScript;

    [Tooltip("Traditional Agent")]
    public GameObject enemyAgent;

    [Header("Score (read-only in Inspector)")]
    [SerializeField] private int playerWins = 0;
    [SerializeField] private int enemyWins  = 0;

    public int PlayerWins => playerWins;
    public int EnemyWins  => enemyWins;

    [Header("Reset Settings")]
    public float resetDelay = 2.5f;

    public Vector3 playerSpawn { get; private set; }
    private Quaternion playerRot;

    public Vector3 enemySpawn { get; private set; }
    private Quaternion enemyRot;

    public bool roundOver {get; private set;} = false;

    private void Awake()
    {
        // Get spawnpoints
        playerSpawn = playerAgent.transform.position;
        enemySpawn = enemyAgent.transform.position;

        playerRot = playerAgent.transform.rotation;
        enemyRot = enemyAgent.transform.rotation;
        
        playerRLScript = playerAgent.GetComponent<RLAgent>();
        enemyRLScript = enemyAgent.GetComponent<RLAgent>();

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
        if (roundOver) return;
        roundOver = true;

        if (deadAgent == playerAgent)
        {
            // Enemy won
            enemyWins++;
            enemyRLScript?.AddReward(+1f);
            playerRLScript?.AddReward(-1f);
        }
        else if (deadAgent == enemyAgent)
        {
            // Player won
            playerWins++;
            playerRLScript?.AddReward(+1f);
            enemyRLScript?.AddReward(-1f);
        }

        MetricsManager.Instance.OnRoundEnd(deadAgent == enemyAgent);
        StartCoroutine(ResetSequence());
    }

    public void OnAgentDamaged(GameObject agent)
    {
        if(agent == playerAgent)
        {
            // Player took damage
            enemyRLScript?.AddReward(+0.15f);
            playerRLScript?.AddReward(-0.15f);
        }
        else if (agent == enemyAgent)
        {
            // Enemy took damage
            enemyRLScript?.AddReward(-0.15f);
            playerRLScript?.AddReward(+0.15f);          
        }

        MetricsManager.Instance.OnShotHit(agent);
    }

    private IEnumerator ResetSequence()
    {
        yield return null; // exit physics callback context first

        foreach (GameObject bullet in GameObject.FindGameObjectsWithTag("Bullet"))
            Destroy(bullet);

        ArenaGenerator.Instance.DestroyLayout();
        playerAgent.SetActive(false);
        enemyAgent.SetActive(false);


        yield return StartCoroutine(ArenaGenerator.Instance.GenerateLayout());

        ResetAgent(playerAgent, playerSpawn, playerRot);
        ResetAgent(enemyAgent, enemySpawn, enemyRot);

        roundOver = false;
        if (Academy.Instance.IsCommunicatorOn)
        {
            playerRLScript?.EndEpisode();
            enemyRLScript?.EndEpisode();
        }
    }

    private void ResetAgent(GameObject agent, Vector3 spawnPoint, Quaternion spawnRot)
    {
        if (agent == null) return;

        // Reset Position and Rotation
        agent.transform.position = spawnPoint;
        agent.transform.rotation = spawnRot;

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

        // Re-enable defeated agent
        agent.SetActive(true);
    }  

    // public string GetScoreString() => $"Player: {playerWins}  |  Enemy: {enemyWins}";
}