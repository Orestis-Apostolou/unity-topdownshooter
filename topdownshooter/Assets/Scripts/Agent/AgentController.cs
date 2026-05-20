using UnityEngine;
using UnityEngine.AI;

public class AgentController : MonoBehaviour
{
    public StateMachine stateMachine = new StateMachine();

    public HeatSystem heatSystem { get; private set; }
    public HealthSystem healthSystem { get; private set; }
    public FiringSystem firingSystem { get; private set; }
    public NavMeshAgent navAgent { get; private set; }

    public Rigidbody2D rb { get; private set; }
    public GameObject player;

    public float rotspeed { get; private set; }
    public float movespeed { get; private set; }

    public float EffectiveMoveSpeed => movespeed * (heatSystem.IsOverheated ? heatSystem.overheatSlow : 1f);

    // At what (+ -) angle is the agent considered to be looking at the player
    public float aimAngle = 2.0f;
    public Vector2 playerVelocity { get; private set; }
    protected Vector2 lastPlayerPos;

    protected virtual void Awake()
    {
        heatSystem = GetComponent<HeatSystem>();
        healthSystem = GetComponent<HealthSystem>();
        firingSystem = GetComponent<FiringSystem>();

        navAgent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody2D>();

        if (player  == null)
        {
            Debug.LogError("No target reference set for agent");
        }
        else
        {
            // Match player movement parameters
            PlayerController controller = player.GetComponent<PlayerController>();
            rotspeed = controller.rotspeed;
            movespeed = controller.movespeed;
        }
    }

    protected virtual void Start()
    {
        lastPlayerPos = player.transform.position;
    }

    protected virtual void Update() => stateMachine.Update();
    protected virtual void FixedUpdate()
    {
        playerVelocity = ((Vector2)player.transform.position - lastPlayerPos) / Time.fixedDeltaTime;
        lastPlayerPos = player.transform.position;
        stateMachine.FixedUpdate();
    }
}
