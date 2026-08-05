using UnityEngine;
using UnityEngine.Events;

public class Pickup : MonoBehaviour
{
    [SerializeField]
    protected UnityEvent pickupEvent;
    protected GameObject player;
    protected bool pickedUp = false;

    protected float dropProgress = 0f;
    protected float dropTime = 0f;
    protected float finishDropTime = 3f;
    protected float finishTimeModifier;
    [SerializeField]
    private Vector3[] dropPoints = new Vector3[3];
    [SerializeField] private bool debug = false;
    [SerializeField] protected AudioClip audioClip;

    void Awake()
    {
        player = GameObject.FindWithTag("Player");
        dropPoints = GetDropPoints();
    }

    void OnEnable()
    {
        if (debug) dropPoints = GetDropPoints();
    }

    private Vector3[] GetDropPoints()
    {
        finishTimeModifier = Random.Range(3f, 6f);
        finishDropTime = 0.1f * finishTimeModifier;

        Vector3 A = transform.position;
        Vector3 C = transform.position + LocationHelper.GetRandomPointInXZCircle(-4f, 4f, 0);
        Vector3 B = new Vector3(Mathf.Lerp(A.x, C.x, 0.5f), Random.Range(3f, 6f), Mathf.Lerp(A.z, C.z, 0.5f));
        return new Vector3[3] { A, B, C };
    }
    protected void Drop()
    {
        dropTime += Time.fixedDeltaTime / finishDropTime;
        dropProgress = (Mathf.Cos((dropTime + 1) * Mathf.PI) + 1) / 2;
        Vector3 AB = Vector3.Lerp(dropPoints[0], dropPoints[1], dropProgress);
        Vector3 BC = Vector3.Lerp(dropPoints[1], dropPoints[2], dropProgress);
        Vector3 finalPoint = Vector3.Lerp(AB, BC, dropProgress);
        transform.position = finalPoint;
    }

    protected void FixedUpdate()
    {
        if (player)
        {
            Vector3 distanceToPlayer = transform.position - player.transform.position;
            if (player && distanceToPlayer.magnitude < player.GetComponent<Player>().PickupRadius)
            {
                Collect();
                if (!pickedUp) Invoke("PickUp", 0.5f);
            }
            else if (dropProgress < 0.95f && !pickedUp)
            {
                Drop();
            }
        }

    }
    protected void Collect()
    {
        transform.position = Vector3.Lerp(transform.position, player.transform.position + (Vector3.up * 0.5f), 0.05f);
        transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(0, 0, 0), 0.05f);
    }

    protected virtual void PickUp()
    {
        AudioSource.PlayClipAtPoint(audioClip, transform.position);
        pickupEvent.Invoke();
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        if (debug)
        {
            Gizmos.DrawSphere(dropPoints[0], 0.1f);
            Gizmos.DrawSphere(dropPoints[1], 0.1f);
            Gizmos.DrawSphere(dropPoints[2], 0.1f);
            Gizmos.DrawLine(dropPoints[0], dropPoints[1]);
            Gizmos.DrawLine(dropPoints[1], dropPoints[2]);
        }

    }
}
