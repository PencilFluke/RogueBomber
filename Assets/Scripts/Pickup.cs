using UnityEngine;
using UnityEngine.Events;

public class Pickup : MonoBehaviour
{
    [SerializeField]
    protected UnityEvent pickupEvent;
    protected GameObject player;
    protected float currentDropTimeCurve = 0f;
    protected float currentDropTime = 0f;
    protected float finishDropTime = 0.3f;
    [SerializeField]
    private Vector3[] dropPoints = new Vector3[3];
    [SerializeField] protected AudioClip audioClip;

    void Awake()
    {
        player = GameObject.FindWithTag("Player");
        dropPoints = GetDropPoints();
        ConfigurePickupEvent();
    }

    protected virtual void ConfigurePickupEvent()
    {
    }

    private Vector3[] GetDropPoints()
    {
        float dropTimeModifier = Random.Range(0f, 0.3f);
        finishDropTime += dropTimeModifier;

        Vector3 A = transform.position;
        Vector3 C = transform.position + LocationHelper.GetRandomPointInXZCircle(-4f, 4f, 0);
        Vector3 B = new Vector3(Mathf.Lerp(A.x, C.x, 0.5f), Random.Range(0f, 2f), Mathf.Lerp(A.z, C.z, 0.5f));
        return new Vector3[3] { A, B, C };
    }
    protected void Drop()
    {
        currentDropTime += Time.fixedDeltaTime / finishDropTime;
        currentDropTimeCurve = (Mathf.Cos((currentDropTime + 1) * Mathf.PI) + 1) / 2;
        Vector3 AB = Vector3.Lerp(dropPoints[0], dropPoints[1], currentDropTimeCurve);
        Vector3 BC = Vector3.Lerp(dropPoints[1], dropPoints[2], currentDropTimeCurve);
        Vector3 finalPoint = Vector3.Slerp(AB, BC, currentDropTimeCurve);
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
                Invoke("PickUp", 0.5f);
            }
            else if (currentDropTime < 0.95f)
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
}
