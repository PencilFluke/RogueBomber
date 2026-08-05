using UnityEngine;

public class FX_BombExplode : MonoBehaviour
{
    private AudioSource audio;
    private Animator animator;
    private Light pointLight;
    void Awake()
    {
        audio = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        pointLight = GetComponentInChildren<Light>();
    }
    void Start()
    {
        audio.PlayOneShot(audio.clip, Mathf.Min(transform.localScale.magnitude, 1f));
        Debug.Log("Volume: " + Mathf.Min(transform.localScale.magnitude, 1f));
        pointLight.range = pointLight.range * gameObject.GetComponentInParent<Transform>().localScale.magnitude;
        Invoke("DestroyOnComplete", audio.clip.length);
    }

    void DestroyOnComplete()
    {
        Destroy(gameObject);
    }

}
