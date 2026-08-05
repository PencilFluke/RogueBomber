using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Explosive : MonoBehaviour
{
    [Header("Player Affected Stats")]
    [SerializeField] private float baseDamage = 10f;
    private float playerModifiedDamage;
    [SerializeField] private float explosionRadius = 2;
    private float playerModifiedRadius;
    [SerializeField] protected float explosionDelaySeconds = 1f;
    protected float playerModifiedExplosionDelay;

    [Header("Prefab Defining Stats")]
    [SerializeField] private GameObject explosiveMesh;
    [SerializeField] private GameObject explosiveFX;
    [SerializeField] private float explosionForce = 1000f;
    [SerializeField] protected bool startOnSpawn;
    [SerializeField] protected AudioClip explodeAudioClip;

    private float startTime;
    private Animator animator;
    private float flashStartDelay = 0f;
    private float maxFlashSpeed = 5f;
    private float animSpeed = 0f;

    private GameObject circleRenderer;
    private ShapeRenderer radiusRenderer;
    private ShapeRenderer explosionTimeRenderer;

    public enum ExplosiveType
    {
        Bomb = 0,
        Mine = 1,
        Sticky = 2
    }

    void Awake()
    {
        ApplyModifiers();
        circleRenderer = Resources.Load<GameObject>("Prefabs/CircleRenderer");
        radiusRenderer = Instantiate(circleRenderer, transform.position, Quaternion.identity).GetComponent<ShapeRenderer>();
        explosionTimeRenderer = Instantiate(circleRenderer, transform.position, Quaternion.identity).GetComponent<ShapeRenderer>();
        radiusRenderer.RenderCircle(20, playerModifiedRadius, true, Color.red, 0.2f);
        explosionTimeRenderer.RenderCircle(20, playerModifiedRadius, true, Color.orange, 0.2f);
        animator = GetComponent<Animator>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (startOnSpawn)
        {
            startTime = Time.time;
            explosionTimeRenderer.transform.localScale = Vector3.zero;
            Invoke("Explode", playerModifiedExplosionDelay);
            InvokeRepeating("UpdateAnimationSpeed", flashStartDelay, playerModifiedExplosionDelay / maxFlashSpeed);
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        SetIndicators();
    }

    private void SetIndicators()
    {
        radiusRenderer.transform.position = transform.position;
        radiusRenderer.transform.localScale = transform.localScale;

        explosionTimeRenderer.transform.position = transform.position;
        Vector3 explosionTimeSize = Vector3.one * ((Time.time - startTime) / playerModifiedExplosionDelay);
        explosionTimeRenderer.transform.localScale = explosionTimeSize * transform.localScale.x;
    }

    protected virtual void Explode()
    {
        float scaleMultiplier = transform.localScale.x;

        IEnumerable<Collider> enemies = Physics.OverlapSphere(transform.position, playerModifiedRadius * scaleMultiplier)
        .Where((c) => c.gameObject.CompareTag("Enemy") || c.gameObject.CompareTag("Player"));

        foreach (Collider collider in enemies)
        {
            float distanceToExplosive = (transform.position - collider.gameObject.transform.position).magnitude;
            float damageModifier = 1f - (Mathf.Clamp(distanceToExplosive, 0f, playerModifiedRadius) / playerModifiedRadius);
            float finalDamage = playerModifiedDamage * scaleMultiplier * damageModifier;

            if (finalDamage > playerModifiedDamage * 0.05f)
            {
                if (collider.gameObject.tag == "Enemy")
                {
                    collider.GetComponent<Enemy>().TakeDamage(finalDamage);
                }
                if (collider.gameObject.tag == "Player")
                {
                    collider.GetComponent<Player>().TakeDamage(finalDamage);
                }
                collider.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, playerModifiedRadius * scaleMultiplier, explosionForce / 2f, ForceMode.Impulse);
            }

        }

        Instantiate(explosiveFX, transform.position, Quaternion.identity).transform.localScale = transform.localScale;
        DestroyInstance();
    }

    void DestroyInstance()
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

    public virtual void ApplyModifiers()
    {
        playerModifiedDamage = baseDamage * Player.baseDamageMultiplier;
        playerModifiedRadius = explosionRadius * Player.explosionRadiusMultiplier;
        playerModifiedExplosionDelay = explosionDelaySeconds * Player.explosionDelaySecondsMultiplier;
    }

}
