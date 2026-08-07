using System.Collections.Generic;
using UnityEngine;

public class PlayerFXController : MonoBehaviour
{
    [SerializeField] private List<AudioClip> stepSounds = new List<AudioClip>();
    public void PlayStepSound()
    {
        int clipIndex = Random.Range(0, stepSounds.Count);
        AudioSource.PlayClipAtPoint(stepSounds[clipIndex], transform.root.position);
    }
}
