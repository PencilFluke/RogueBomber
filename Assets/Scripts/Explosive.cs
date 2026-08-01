using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Explosive : MonoBehaviour
{

    [SerializeField] protected float explosionDelaySeconds = 5;
    [SerializeField] protected bool startOnSpawn;

    private float explosionRadius = 2;

    void Awake()
    {
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if (startOnSpawn)
        {
            Invoke("Explode", explosionDelaySeconds);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    protected virtual void Explode()
    {
        IEnumerable<Collider> enemies = Physics.OverlapSphere(transform.position, explosionRadius).Where((c) => c.gameObject.CompareTag("Enemy"));
        foreach (Collider enemyCollider in enemies)
        {
            enemyCollider.GetComponentInParent<Enemy>().TakeDamage(200);
        }
        Debug.Log("Bomb Exploded");
        Destroy(gameObject);
    }

}
