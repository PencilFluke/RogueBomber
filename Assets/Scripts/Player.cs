using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    #region Input
    [Header("Player Inputs")]
    [SerializeField] private InputActionAsset actions;
    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction fireAction;
    private InputAction loadAction;
    #endregion

    #region Player Stats
    [Header("Player Stats")]
    [SerializeField] private float maxHealth = 100.0f;
    private float _health;
    public float Health
    {
        get { return _health; }
        set { _health = value; }
    }
    [SerializeField] private float maxStamina = 100.0f;
    private float _stamina;
    public float Stamina
    {
        get { return _stamina; }
        set { _stamina = value; }
    }
    private float moveSpeed = 1000.0f;
    private bool canMove = false;
    private float rotateSpeed = 600.0f;
    private float maxVelocity = 5f;
    private float sprintSpeedMultiplier = 1.50f;
    [SerializeField] private int maxAmmo = 3;
    [SerializeField] private float _pickupRadius;
    public float PickupRadius
    {
        get { return _pickupRadius + 0.5f; }
        set { _pickupRadius = value + 0.5f; }
    }
    private float invulnerabilityTime = 0.5f;
    private bool isVulnerable = true;
    private bool _isDead = false;
    public bool IsDead { get; }
    #endregion

    #region Explosive
    [Header("Explosive")]
    [SerializeField] private GameObject explosive;
    private float minExplosiveSize = 0.2f;
    [SerializeField] private float explosiveReloadCooldown = 1f;
    private float explodeTime = 3f;
    private float currentCharge = 0f;
    private float maxChargeTime = 0.5f;
    private GameObject explosiveInstance;
    private bool canHold = true;
    #endregion

    private GameObject previewMesh;
    private List<GameObject> explosivePreviewAmmo = new List<GameObject>();
    private Vector3 ammoPreviewPosition = Vector3.up * 2f;
    [Header("Explosive modifiers")]
    static public float baseDamageMultiplier = 2f;
    static public float explosionRadiusMultiplier = 2f;
    static public float explosionDelaySecondsMultiplier = 2f;


    [Header("Constraints")]
    [SerializeField] private float screenBoundsX = 14.5f;
    [SerializeField] private float screenBoundsZ = 10.0f;

    #region Components
    private Rigidbody playerRigidBody;
    private SphereCollider pickupCollider;
    private Animator animator;
    private AudioSource audioSource;
    #endregion

    #region Initialisation
    void Awake()
    {
        InitialiseInput();
        GetComponentReferences();
        InitialiseStats();
    }

    private void OnEnable()
    {
        actions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        actions.FindActionMap("Player").Disable();
    }
    void Start()
    {
        previewMesh = Resources.Load("prefabs/BombMesh").ConvertTo<GameObject>();
        InvokeRepeating("ReloadExplosive", 0f, explosiveReloadCooldown);
    }
    #endregion
    void FixedUpdate()
    {
        if (canMove) Move();

        if (explosiveInstance && canHold) ChargeExplosive();

        if (!fireAction.IsPressed())
        {
            currentCharge = 0;
            canHold = false;
        }
    }
    //Stats
    #region Health
    public void Heal(float healAmount)
    {
        if (Health == maxHealth) return;
        else if (maxHealth - Health <= healAmount) Health = maxHealth;
        else if (maxHealth - Health > healAmount) Health += healAmount;
        animator.SetFloat("health_f", 1 - (Health / maxHealth));
    }
    public void TakeDamage(float damage)
    {
        if (isVulnerable && Health > damage)
        {
            Health -= damage;
            isVulnerable = false;
            Invoke("MakeVulnerable", invulnerabilityTime);
            animator.SetFloat("health_f", 1 - (Health / maxHealth));
            audioSource.PlayOneShot(audioSource.clip);
        }
        else if (isVulnerable && Health <= damage)
        {
            Health = 0;
            Die();
        }
    }
    void MakeVulnerable()
    {
        isVulnerable = true;
    }
    void Die()
    {
        Destroy(gameObject);
    }
    #endregion
    #region Movement
    private void Move()
    {
        playerRigidBody.AddForce(GetDirection() * moveSpeed * sprintSpeedMultiplier);
        transform.forward = Vector3.Slerp(transform.forward, GetDirection().normalized, Time.deltaTime * rotateSpeed);
        ConstrainPositionToXZBounds(transform.position, screenBoundsX, screenBoundsZ);
    }
    private Vector3 GetDirection()
    {
        return new Vector3(moveAction.ReadValue<Vector2>().x, 0.0f, moveAction.ReadValue<Vector2>().y);
    }
    public void UseStamina(float staminaCost)
    {
        Stamina -= staminaCost;
    }
    private void StartMove(InputAction.CallbackContext context)
    {
        canMove = true;
    }
    private void StopMove(InputAction.CallbackContext context)
    {
        canMove = false;
    }
    private void StartSprint(InputAction.CallbackContext context)
    {
        playerRigidBody.maxLinearVelocity = maxVelocity * sprintSpeedMultiplier;
    }
    private void StopSprint(InputAction.CallbackContext context)
    {
        playerRigidBody.maxLinearVelocity = maxVelocity;
    }
    #endregion
    #region Explosive
    private void StartFire(InputAction.CallbackContext context)
    {
        if (explosivePreviewAmmo.Count > 0)
        {
            SpawnExplosive();
            RemoveAmmo();
        }
    }
    private void HoldFire(InputAction.CallbackContext context)
    {
        canHold = context.performed;

    }
    private void StopFire(InputAction.CallbackContext context)
    {
        canHold = !context.canceled && !context.performed;
        explosiveInstance = null;
    }
    private void SpawnExplosive()
    {
        Vector3 spawnPosition = (-transform.forward * 1f) + Vector3.up;
        explosiveInstance = explosivePreviewAmmo.Count > 0 ? Instantiate(explosive, transform.position + spawnPosition, Quaternion.identity) : explosiveInstance;
        explosiveInstance.transform.localScale = Vector3.one * minExplosiveSize;
        explosiveInstance.transform.position = transform.position + spawnPosition;
    }
    private void ChargeExplosive()
    {
        Vector3 holdPosition = (-transform.forward * 1f) + Vector3.up;
        explosiveInstance.transform.position = transform.position + holdPosition;
        currentCharge += Time.deltaTime / maxChargeTime;
        explosiveInstance.transform.localScale = Vector3.Lerp(Vector3.one * minExplosiveSize, Vector3.one, currentCharge / explodeTime);
    }
    #endregion
    #region Ammo
    private void AddAmmo()
    {
        if (explosivePreviewAmmo.Count >= maxAmmo)
            return;
        explosivePreviewAmmo.Add(Instantiate(previewMesh, gameObject.transform));
        SetAmmoPositions();
    }
    private void RemoveAmmo()
    {
        if (explosivePreviewAmmo.Count > 0)
        {
            Destroy(explosivePreviewAmmo.Last());
            explosivePreviewAmmo.Remove(explosivePreviewAmmo.Last());
        }
    }
    private void SetAmmoCount(int count)
    {
        for (int i = 0; i < count; i++)
        {
            AddAmmo();
        }
    }
    private void ReloadExplosive()
    {
        AddAmmo();
    }
    private void SetAmmoPositions()
    {
        for (int i = 0; i < explosivePreviewAmmo.Count; i++)
        {
            float height = explosivePreviewAmmo[i].GetComponent<Collider>().bounds.extents.y;
            explosivePreviewAmmo[i].transform.localPosition = ammoPreviewPosition + Vector3.up * (height * i);
        }
    }
    #endregion

    //Helpers
    #region Constrain to Screen
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
    #endregion
    #region Initialisation Methods
    private void GetComponentReferences()
    {
        playerRigidBody = GetComponent<Rigidbody>();
        pickupCollider = GetComponent<SphereCollider>();
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
    }
    private void InitialiseStats()
    {
        Health = maxHealth;
        Stamina = maxStamina;
        playerRigidBody.maxLinearVelocity = maxVelocity;
        pickupCollider.radius = PickupRadius;
    }
    private void InitialiseInput()
    {
        moveAction = actions.FindAction("Move");
        sprintAction = actions.FindAction("Sprint");
        fireAction = actions.FindAction("Attack");

        moveAction.started += StartMove;
        moveAction.canceled += StopMove;

        sprintAction.started += StartSprint;
        sprintAction.canceled += StopSprint;

        fireAction.started += StartFire;
        fireAction.performed += HoldFire;
        fireAction.canceled += StopFire;
    }
    #endregion
}
