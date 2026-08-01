using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 100f;
    private Rigidbody enemyRigidBody;
    private bool isTouchingPlayer;
    private float rotateTime = 3f;
    private GameObject player;
    private float health;
    private float maxHealth = 100f;
    private bool isDead;


    void Awake()
    {
        enemyRigidBody = GetComponent<Rigidbody>();
        player = GameObject.Find("Player");
        InitialiseStats();
    }



    // Update is called once per frame
    void FixedUpdate()
    {
        MoveTowardPlayer();
    }

    private void MoveTowardPlayer()
    {
        Vector3 playerDirection = (player.transform.position - transform.position).normalized;
        Debug.DrawLine(transform.position, player.transform.position, Color.red);
        if (!isTouchingPlayer && !isDead) enemyRigidBody.linearVelocity = playerDirection * moveSpeed;
        gameObject.transform.forward = Vector3.Slerp(gameObject.transform.forward, playerDirection, Time.deltaTime * rotateTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isTouchingPlayer = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isTouchingPlayer = false;
        }
    }
    private void InitialiseStats()
    {
        health = maxHealth;
    }
    public void TakeDamage(float amount)
    {
        if (health > amount)
            health -= amount;
        else
        {
            Die();
        }
        Debug.Log("Took Damage");
    }

    void Die()
    {
        Debug.Log("Enemy Died");
        Destroy(gameObject);
    }

    void DropItems()
    {

    }

}
