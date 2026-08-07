using UnityEngine;
using UnityEngine.Events;

public class Pickup : MonoBehaviour
{
    [SerializeField]
    protected UnityEvent pickupEvent;
    protected GameObject player;
    protected float collectDurationSeconds = 0.2f;
    protected float currentDropTimeCurve = 0f;
    protected float currentDropTime = 0f;
    protected float finishDropTime = 0.3f;
    [SerializeField]
    private Vector3[] dropPoints = new Vector3[3];
    [SerializeField] protected AudioClip audioClip;
    private bool playerInRange;

    void Awake()
    {
        player = GameObject.FindWithTag(Tags.PLAYER);
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
        Vector3 B = new Vector3(Mathf.Lerp(A.x, C.x, 0.5f), Random.Range(3f, 6f), Mathf.Lerp(A.z, C.z, 0.5f));
        return new Vector3[3] { A, B, C };
    }
    protected void Drop()
    {
        currentDropTime += Time.fixedDeltaTime / finishDropTime;
        currentDropTimeCurve = (Mathf.Cos((currentDropTime + 1) * Mathf.PI) + 1) / 2;
        Vector3 AB = Vector3.Lerp(dropPoints[0], dropPoints[1], currentDropTimeCurve / finishDropTime);
        Vector3 BC = Vector3.Lerp(dropPoints[1], dropPoints[2], currentDropTimeCurve / finishDropTime);
        Vector3 finalPoint = Vector3.Lerp(AB, BC, currentDropTimeCurve / finishDropTime);
        transform.position = finalPoint;
    }

    protected void FixedUpdate()
    {
        if (playerInRange) Collect();
        else if (currentDropTimeCurve / finishDropTime < 1f)
        {
            Drop();
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == Tags.PLAYER)
        {
            playerInRange = true;
            Invoke("PickUp", collectDurationSeconds);
        }
    }
    protected void Collect()
    {
        transform.position = Vector3.Lerp(transform.position, player.transform.position + (Vector3.up * 0.5f), Time.fixedDeltaTime / collectDurationSeconds);
        transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(0, 0, 0), Time.fixedDeltaTime / collectDurationSeconds);
    }
    protected virtual void PickUp()
    {
        AudioSource.PlayClipAtPoint(audioClip, transform.position);
        pickupEvent.Invoke();
        Destroy(gameObject);
    }
}
