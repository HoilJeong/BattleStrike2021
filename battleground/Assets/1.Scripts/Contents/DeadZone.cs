using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadZone : MonoBehaviour
{
    public GameObject player;
    public PlayerHealth playerHealth;

    private void OnCollisionEnter(Collision collision)
    {
        playerHealth = player.GetComponent<PlayerHealth>();
        if (collision.gameObject.name == "PlayerCharacter")
        {
            playerHealth.health -= 100;
        }
    }
}
