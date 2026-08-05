using UnityEngine;
using UnityEngine.Events;

public class HealthPickup : Pickup
{
    protected float healAmount = 10f;

    void Awake()
    {
        player = GameObject.FindWithTag("Player");
        pickupEvent.AddListener(() => player.GetComponent<Player>().Heal(healAmount));
    }
}
