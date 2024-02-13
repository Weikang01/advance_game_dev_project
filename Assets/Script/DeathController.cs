using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathController : MonoBehaviour
{
    [SerializeField] GameObject spawnLocation;
    
    void Start()
    {
        this.transform.position = spawnLocation.transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Deadly"))
        {
            this.transform.position = spawnLocation.transform.position;
        }
    }
}
