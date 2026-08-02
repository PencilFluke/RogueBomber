using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Explosive : MonoBehaviour
{

    [SerializeField] protected float explosionDelaySeconds = 5;
    [SerializeField] protected bool startOnSpawn;

    [SerializeField] private float explosionRadius = 2;
    [SerializeField] private GameObject indicatorPrefab;
    private GameObject indicator;

    void Awake()
    {
        indicator = Instantiate(indicatorPrefab, transform.position, Quaternion.identity);
        indicator.GetComponent<ShapeRenderer>().RenderCircle(20, explosionRadius, true, Color.red);
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
        indicator.transform.position = transform.position;
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
        Destroy(indicator);
    }

}
