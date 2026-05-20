using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private HeatSystem heatSystem;
    private HealthSystem healthSystem;
    private FiringSystem firingSystem;

    [Header("Movement Settings")]
    public float movespeed = 6;
    public float rotspeed = 130.0f;

    private PlayerControls controls;
    private Vector2 moveInput;
    private float rotInput;
    private Rigidbody2D rb;

    private bool isFiring = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controls = new PlayerControls();

        // Movement input listeners
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        // Rotation input listeners
        controls.Player.Rotate.performed += ctx => rotInput = ctx.ReadValue<float>();
        controls.Player.Rotate.canceled += ctx => rotInput = 0.0f;

        // Fire input
        controls.Player.Fire.performed += ctx => isFiring = true;
        controls.Player.Fire.canceled += ctx => isFiring = false;

        heatSystem = GetComponent<HeatSystem>();
        healthSystem = GetComponent<HealthSystem>();
        firingSystem = GetComponent<FiringSystem>();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    void FixedUpdate()
    {
        // Apply potential overheat slow
        float effectiveMoveSpeed = movespeed * (heatSystem.IsOverheated ? heatSystem.overheatSlow : 1f);

        Vector2 movement = new Vector2(moveInput.x, moveInput.y);
        rb.MovePosition(rb.position + movement * movespeed * Time.fixedDeltaTime);
        rb.MoveRotation(rb.rotation + rotspeed * Time.fixedDeltaTime * -rotInput);
    }

    void Update()
    {
        if (firingSystem.CanFire() && heatSystem.CanFire() && isFiring)
        {
            firingSystem.Fire();
            heatSystem.Fire();
        }
    }
}
