using System;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float damage = 10;
    [SerializeField] private float moveSpeed = 1000.0f;
    private float maxVelocity = 6f;
    public float maxHealth = 30f;
    private Rigidbody enemyRigidBody;
    private bool isTouchingPlayer;
    private bool isGrounded = true;
    private float rotateTime = 3f;
    private GameObject player;
    private float health;
    private bool isDead;
    public static int maxCount = 10;
    [SerializeField] private Animator animator;
    [SerializeField] private Animator healthAnimator;
    [SerializeField] private AudioClip audioClip;
    [SerializeField] public List<Drop> drops = new List<Drop>();


    private readonly int speed_f = Animator.StringToHash("speed_f");
    private readonly int isWalking_b = Animator.StringToHash("isWalking_b");
    private readonly int health_f = Animator.StringToHash("health_f");

    [Serializable]
    public class Drop
    {
        public GameObject prefab;
        public int amount = 1;
        public float chance = 1f;

    }

    void Awake()
    {
        enemyRigidBody = GetComponent<Rigidbody>();
        player = GameObject.Find(Tags.PLAYER);
        InitialiseStats();
    }
    void FixedUpdate()
    {
        if (player && (isGrounded || enemyRigidBody.linearVelocity.magnitude < 1f))
            MoveTowardPlayer();

        SetAnimationState();
    }

    private void MoveTowardPlayer()
    {

        Vector3 playerDirection = (player.transform.position - transform.position).normalized;
        Debug.DrawLine(transform.position, player.transform.position, Color.red);
        Vector3 target = new Vector3(playerDirection.x, 0f, playerDirection.z);
        if (!isTouchingPlayer && !isDead) enemyRigidBody.AddForce(target * moveSpeed);
        gameObject.transform.forward = Vector3.Slerp(gameObject.transform.forward, target, Time.deltaTime * rotateTime);
    }

    void SetAnimationState()
    {
        float enemyVelocity = enemyRigidBody.linearVelocity.magnitude;

        if (enemyVelocity > 0.1f) animator.SetBool(isWalking_b, true);
        float walkSpeed = 0.5f + (enemyVelocity / maxVelocity * 2f);
        if (!isGrounded) walkSpeed = 0f;

        animator.SetFloat(speed_f, walkSpeed);

    }


    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(Tags.PLAYER))
        {
            isTouchingPlayer = true;
        }

        if (collision.gameObject.CompareTag(Tags.GROUND))
        {
            isGrounded = true;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (isTouchingPlayer && player)
        {
            player.GetComponent<Player>().TakeDamage(damage);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag(Tags.PLAYER))
        {
            isTouchingPlayer = false;
        }

        if (collision.gameObject.CompareTag(Tags.GROUND))
        {
            isGrounded = false;
        }
    }
    private void InitialiseStats()
    {
        float variation = UnityEngine.Random.Range(0f, 0.2f);
        float increase = 1 + variation;
        float decrease = 1 - variation;
        maxHealth *= decrease;
        health = maxHealth;
        moveSpeed *= increase;
        enemyRigidBody.maxLinearVelocity = maxVelocity;
        enemyRigidBody.maxLinearVelocity *= decrease;
        enemyRigidBody.mass *= decrease;
        transform.localScale = Vector3.one * decrease;
    }
    public void TakeDamage(float amount)
    {
        if (health > amount)
        {
            health -= amount;
            healthAnimator.SetFloat(health_f, 1 - (health / maxHealth));
        }
        else
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
        if (drops.Count > 0) DropItems();
        AudioSource.PlayClipAtPoint(audioClip, transform.position);
    }

    void DropItems()
    {
        foreach (Drop drop in drops)
        {
            for (int i = 0; i < drop.amount; i++)
            {
                float chanceResult = UnityEngine.Random.Range(0f, 1f);
                if (drop.chance >= chanceResult)
                    Instantiate(drop.prefab, transform.position, Quaternion.identity);
            }
        }
    }

}
