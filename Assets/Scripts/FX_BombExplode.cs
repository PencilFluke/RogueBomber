using UnityEngine;

public class FX_BombExplode : MonoBehaviour
{
    private AudioSource audioSource;
    private Light pointLight;
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        pointLight = GetComponentInChildren<Light>();
    }
    void Start()
    {
        audioSource.volume = transform.root.localScale.x;
        pointLight.range = pointLight.range * transform.root.localScale.x;
    }
    void PlayExplosiveFlash()
    {
        audioSource.Play();
        Invoke("DestroyOnComplete", audioSource.clip.length);
    }

    void DestroyOnComplete()
    {
        Destroy(gameObject);
    }

}
