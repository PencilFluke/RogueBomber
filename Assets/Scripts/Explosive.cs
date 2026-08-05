using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Explosive : MonoBehaviour
{
    [Header("Player Derived Stats")]
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] protected float explosionDelaySeconds = 3f;
    [SerializeField] private float explosionRadius = 2;


    [Header("Prefab Defining Stats")]
    [SerializeField] protected bool startOnSpawn;
    private float explosionForce = 1000f;

    [SerializeField] private GameObject explosiveMesh;
    [SerializeField] private GameObject indicatorPrefab;
    private Animator animator;
    private GameObject radiusRenderer;
    private GameObject explosionTimeRenderer;
    [SerializeField] private float flashStartDelay = 0f;
    private float maxFlashSpeed = 5f;
    private float animSpeed = 0f;

    [SerializeField]
    private bool preview = false;
    [SerializeField] protected AudioClip audioClip;
    private float startTime;

    public enum ExplosiveType
    {
        Bomb = 0,
        Mine = 1,
        Sticky = 2
    }

    void Awake()
    {
        radiusRenderer = Instantiate(indicatorPrefab, transform.position, Quaternion.identity);
        radiusRenderer.GetComponent<ShapeRenderer>().RenderCircle(20, explosionRadius, true, Color.red, 0.2f);

        explosionTimeRenderer = Instantiate(indicatorPrefab, transform.position, Quaternion.identity);
        explosionTimeRenderer.GetComponent<ShapeRenderer>().RenderCircle(20, explosionRadius, true, Color.orange, 0.2f);
        animator = GetComponent<Animator>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (startOnSpawn && !preview)
        {
            startTime = Time.time;
            explosionTimeRenderer.transform.localScale = Vector3.zero;
            Invoke("Explode", explosionDelaySeconds);
            InvokeRepeating("UpdateAnimationSpeed", flashStartDelay, explosionDelaySeconds / maxFlashSpeed);
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        radiusRenderer.transform.position = transform.position;
        radiusRenderer.transform.localScale = transform.localScale;

        explosionTimeRenderer.transform.position = transform.position;
        Vector3 explosionTimeSize = Vector3.one * ((Time.time - startTime) / explosionDelaySeconds);
        explosionTimeRenderer.transform.localScale = explosionTimeSize * transform.localScale.x;
    }

    protected virtual void Explode()
    {
        float scaleMultiplier = transform.localScale.x;
        IEnumerable<Collider> enemies = Physics.OverlapSphere(transform.position, explosionRadius * scaleMultiplier)
        .Where((c) => c.gameObject.CompareTag("Enemy") || c.gameObject.CompareTag("Player"));
        foreach (Collider collider in enemies)
        {
            float distanceToExplosive = (transform.position - collider.gameObject.transform.position).magnitude;
            float damageModifier = 1f - (Mathf.Clamp(distanceToExplosive, 0f, explosionRadius) / explosionRadius);
            float finalDamage = baseDamage * scaleMultiplier * damageModifier;

            if (finalDamage > baseDamage * 0.05f)
            {
                if (collider.gameObject.tag == "Enemy")
                {
                    collider.GetComponent<Enemy>().TakeDamage(finalDamage);
                }
                if (collider.gameObject.tag == "Player")
                {
                    collider.GetComponent<Player>().TakeDamage(finalDamage);
                }
                collider.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, explosionRadius * scaleMultiplier, explosionForce / 2f, ForceMode.Impulse);
            }

        }

        AudioSource.PlayClipAtPoint(audioClip, transform.position, transform.localScale.magnitude * 2f);
        animator.SetBool("explode", true);

        HideMainElements();
        Invoke("DestroyBomb", 1f);
    }

    void HideMainElements()
    {
        explosiveMesh.SetActive(false);
        radiusRenderer.SetActive(false);
        explosionTimeRenderer.SetActive(false);
    }

    void DestroyBomb()
    {
        Destroy(gameObject);
        Destroy(radiusRenderer.gameObject);
        Destroy(explosionTimeRenderer.gameObject);
    }

    private void UpdateAnimationSpeed()
    {
        animator.SetFloat("speed_f", animSpeed);
        animSpeed++;
    }

}
