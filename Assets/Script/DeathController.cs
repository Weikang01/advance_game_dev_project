using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathController : MonoBehaviour
{
    [SerializeField] GameObject spawnLocation;
    [SerializeField] GameObject ConnectedPlayer;
    [SerializeField] Rope RopeScript;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        this.transform.position = spawnLocation.transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Deadly"))
        {
            this.transform.position = spawnLocation.transform.position;
            ConnectedPlayer.transform.position = spawnLocation.transform.position;
        }
    }
}
