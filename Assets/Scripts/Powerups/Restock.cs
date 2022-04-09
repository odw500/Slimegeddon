using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Restock : Powerup
{
    protected override void Activate()
    {
        playerController.playSound(3);
        playerController.missileCount += Random.Range(3, 6);
        Destroy(gameObject);
    }
}