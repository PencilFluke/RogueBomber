using System;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float damage = 10;
    [SerializeField] private float moveSpeed = 16f;
    public float maxHealth = 30f;
    private Rigidbody enemyRigidBody;
    private bool isTouchingPlayer;
    private bool isGrounded = true;
    private float rotateTime = 3f;
    private GameObject player;
    private float health;
    private bool isDead;
    public static int maxCount = 10;
    private Animator animator;
    [SerializeField] private AudioClip audioClip;
    [SerializeField] public List<Drop> drops = new List<Drop>();

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
        player = GameObject.Find("Player");
        animator = GetComponentInChildren<Animator>();
        InitialiseStats();
    }



    // Update is called once per frame
    void FixedUpdate()
    {
        if (player && (isGrounded || enemyRigidBody.linearVelocity.magnitude < 1f))
            MoveTowardPlayer();
    }

    private void MoveTowardPlayer()
    {
        Vector3 playerDirection = (player.transform.position - transform.position).normalized;
        Debug.DrawLine(transform.position, player.transform.position, Color.red);
        if (!isTouchingPlayer && !isDead) enemyRigidBody.AddForce(new Vector3(playerDirection.x, 0f, playerDirection.z) * moveSpeed);
        gameObject.transform.forward = Vector3.Slerp(gameObject.transform.forward, playerDirection, Time.deltaTime * rotateTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isTouchingPlayer = true;
        }

        if (collision.gameObject.CompareTag("Ground"))
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
        if (collision.gameObject.CompareTag("Player"))
        {
            isTouchingPlayer = false;
        }

        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
    private void InitialiseStats()
    {
        health = maxHealth;

    }
    public void TakeDamage(float amount)
    {
        if (health > amount)
        {
            health -= amount;
            animator.SetFloat("health_f", 1 - (health / maxHealth));
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
