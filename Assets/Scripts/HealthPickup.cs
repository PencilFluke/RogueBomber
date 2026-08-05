using UnityEngine;
using UnityEngine.Events;

public class HealthPickup : Pickup
{
    protected float healAmount = 10f;

    protected override void ConfigurePickupEvent()
    {
        base.ConfigurePickupEvent();
        pickupEvent.AddListener(() => player.GetComponent<Player>().Heal(healAmount));
    }
}
