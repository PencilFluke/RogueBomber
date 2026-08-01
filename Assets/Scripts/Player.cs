using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Player Inputs")]
    public InputAction moveAction;
    public InputAction sprintAction;

    [Header("Stats")]

    public float maxHealth = 100.0f;
    public float maxStamina = 100.0f;
    private float health;
    private float stamina;
    public float moveSpeed = 1000.0f;
    public float rotateSpeed = 600.0f;
    public float sprintSpeedMultiplier = 1.5f;
    public bool isSprinting = false;

    //Components
    private Rigidbody playerRigidBody;
    [Header("Constraints")]
    public float screenBoundsX = 16.5f;
    public float screenBoundsZ = 12.0f;

    void Start()
    {
        GetComponentReferences();
        InitialiseInput();
        InitialiseStats();
    }

    void FixedUpdate()
    {
        float speed = IsPlayerSprinting() ? moveSpeed * sprintSpeedMultiplier : moveSpeed;


        playerRigidBody.AddForce(GetDirection() * speed * Time.deltaTime, ForceMode.VelocityChange);
        transform.forward = Vector3.Slerp(transform.forward, GetDirection(), Time.deltaTime * rotateSpeed);
        ConstrainPositionToXZBounds(transform.position, screenBoundsX, screenBoundsZ);
    }


    //Initialisation
    private void InitialiseStats()
    {
        health = maxHealth;
        stamina = maxStamina;
    }

    private void InitialiseInput()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        sprintAction = InputSystem.actions.FindAction("Sprint");
    }
    private void GetComponentReferences()
    {
        playerRigidBody = GetComponent<Rigidbody>();
    }

    //Movement
    private Vector3 GetDirection()
    {
        Vector2 moveDirection = moveAction.ReadValue<Vector2>();
        return new Vector3(moveDirection.x, 0.0f, moveDirection.y);
    }

    private bool IsPlayerSprinting()
    {
        return sprintAction.IsPressed() && stamina > 0;
    }

    private void ConstrainPositionToXZBounds(Vector3 pos, float x, float z)
    {
        ConstrainToScreenBoundsX(pos, x);
        ConstrainToScreenBoundsZ(pos, z);
    }

    private void ConstrainToScreenBoundsX(Vector3 pos, float x)
    {
        if (pos.x > x) { transform.position = new Vector3(x, pos.y, pos.z); }
        else if (pos.x < -x) { transform.position = new Vector3(-x, pos.y, pos.z); }
    }

    private void ConstrainToScreenBoundsZ(Vector3 pos, float z)
    {
        if (pos.z > z) { transform.position = new Vector3(pos.x, pos.y, z); }
        else if (pos.z < -z) { transform.position = new Vector3(pos.x, pos.y, -z); }
    }

    //Stat Manipulation
    private void UseStamina(float staminaCost)
    {
        stamina -= staminaCost;
    }

    private void TakeDamage(float damage)
    {
        health -= damage;
    }



}
