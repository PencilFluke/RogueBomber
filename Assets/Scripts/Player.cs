using System.Collections;
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

    private Vector3 lastLookDirection;
    #endregion

    #region Explosive
    [Header("Explosive")]
    [SerializeField] private GameObject explosive;
    private float minExplosiveSize = 0.2f;
    [SerializeField] private float explosiveReloadCooldown = 1f;
    private float currentCharge = 0f;
    private float maxChargeTime = 0.5f;
    private GameObject explosiveInstance;
    private bool reloadInProgress;
    #endregion

    private GameObject previewMesh;
    private List<GameObject> explosivePreviewAmmo = new List<GameObject>();
    [SerializeField] private Transform ammoPreviewPosition;
    [Header("Explosive modifiers")]
    static public float baseDamageMultiplier = 1f;
    static public float explosionRadiusMultiplier = 1f;
    static public float explosionDelaySecondsMultiplier = 1f;


    [Header("Constraints")]
    [SerializeField] private float screenBoundsX = 14.5f;
    [SerializeField] private float screenBoundsZ = 10.0f;

    #region Components
    private Rigidbody playerRigidBody;
    private SphereCollider pickupCollider;
    [SerializeField] private Animator healthAnimator;
    [SerializeField] private Animator animator;
    private AudioSource audioSource;
    [SerializeField] private AudioClip ammoReloadAudio;
    #endregion

    private readonly int speed_f = Animator.StringToHash("speed_f");
    private readonly int isWalking_b = Animator.StringToHash("isWalking_b");
    private readonly int isRunning_b = Animator.StringToHash("isRunning_b");
    private readonly int health_f = Animator.StringToHash("health_f");

    #region Initialisation
    void Awake()
    {
        InitialiseInput();
        GetComponentReferences();
        InitialiseStats();
    }

    private void OnEnable()
    {
        actions.FindActionMap(Tags.PLAYER).Enable();
    }

    private void OnDisable()
    {
        actions.FindActionMap(Tags.PLAYER).Disable();
    }
    void Start()
    {
        previewMesh = Resources.Load<GameObject>("prefabs/BombMesh");
        StartCoroutine(ReloadExplosive());
    }
    #endregion
    void FixedUpdate()
    {
        if (canMove) Move();

        if (explosiveInstance) ChargeExplosive();
    }
    #region Health
    public void Heal(float healAmount)
    {
        if (Health == maxHealth) return;
        else if (maxHealth - Health <= healAmount) Health = maxHealth;
        else if (maxHealth - Health > healAmount) Health += healAmount;
        healthAnimator.SetFloat(health_f, 1 - (Health / maxHealth));
    }
    public void TakeDamage(float damage)
    {
        if (isVulnerable && Health > damage)
        {
            Health -= damage;
            isVulnerable = false;
            Invoke("MakeVulnerable", invulnerabilityTime);
            healthAnimator.SetFloat(health_f, 1 - (Health / maxHealth));
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
        Vector3 direction = GetDirection();
        animator.SetFloat(speed_f, direction.magnitude);
        playerRigidBody.AddForce(direction * moveSpeed * sprintSpeedMultiplier);
        lastLookDirection = direction.magnitude == 0f ? lastLookDirection : direction;
        transform.forward = Vector3.Slerp(transform.forward, direction.normalized, Time.deltaTime * rotateSpeed);
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
        animator.SetBool(isWalking_b, true);
    }
    private void StopMove(InputAction.CallbackContext context)
    {
        canMove = false;
        animator.SetBool(isWalking_b, false);
        animator.SetFloat(speed_f, Vector3.zero.magnitude);
    }
    private void StartSprint(InputAction.CallbackContext context)
    {
        playerRigidBody.maxLinearVelocity = maxVelocity * sprintSpeedMultiplier;
        animator.SetBool(isRunning_b, true);
    }
    private void StopSprint(InputAction.CallbackContext context)
    {
        playerRigidBody.maxLinearVelocity = maxVelocity;
        animator.SetBool(isRunning_b, false);
    }
    #endregion
    #region Explosive
    private void StartFire(InputAction.CallbackContext context)
    {
        if (explosivePreviewAmmo.Count > 0)
        {
            SpawnExplosive();
            if (!reloadInProgress) StartCoroutine(ReloadExplosive());
            RemoveAmmo();
        }
    }
    private void StopFire(InputAction.CallbackContext context)
    {
        currentCharge = 0;
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
        explosiveInstance.transform.localScale = Vector3.Lerp(Vector3.one * minExplosiveSize, Vector3.one, currentCharge);
    }
    #endregion
    #region Ammo
    private void AddAmmo()
    {
        explosivePreviewAmmo.Add(Instantiate(previewMesh, ammoPreviewPosition.transform));
        SetEachAmmoPositions();
        AudioSource.PlayClipAtPoint(ammoReloadAudio, transform.root.position);
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
    private IEnumerator ReloadExplosive()
    {
        yield return new WaitForSeconds(explosiveReloadCooldown);
        if (explosivePreviewAmmo.Count < maxAmmo)
        {
            reloadInProgress = true;
            AddAmmo();
            StartCoroutine("ReloadExplosive");
        }
        else if (explosivePreviewAmmo.Count >= maxAmmo) reloadInProgress = false;
    }
    private void SetEachAmmoPositions()
    {
        for (int i = 0; i < explosivePreviewAmmo.Count; i++)
        {
            float height = explosivePreviewAmmo[i].GetComponent<Collider>().bounds.extents.y * 2f;
            explosivePreviewAmmo[i].transform.localPosition = new Vector3(0, height * i, 0);
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
        audioSource = GetComponent<AudioSource>();
    }
    private void InitialiseStats()
    {
        Health = maxHealth;
        Stamina = maxStamina;
        playerRigidBody.maxLinearVelocity = maxVelocity;
        pickupCollider.radius = PickupRadius;
        lastLookDirection = transform.forward;
    }
    private void InitialiseInput()
    {
        moveAction = actions.FindAction("Move", true);
        sprintAction = actions.FindAction("Sprint", true);
        fireAction = actions.FindAction("Attack", true);

        moveAction.started += StartMove;
        moveAction.canceled += StopMove;

        sprintAction.started += StartSprint;
        sprintAction.canceled += StopSprint;

        fireAction.started += StartFire;
        fireAction.canceled += StopFire;
    }
    #endregion
}
