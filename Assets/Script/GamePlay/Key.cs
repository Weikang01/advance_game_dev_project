using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : MonoBehaviour
{
    CharacterController2D playerController = null;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (playerController == null && collision.gameObject.GetComponent<CharacterController2D>())
        {
            playerController = collision.gameObject.GetComponent<CharacterController2D>();
            gameObject.tag = "NotInteractable";
        }

        if (collision.gameObject.CompareTag("GoldGate"))
        {
            gameObject.SetActive(false);
            collision.gameObject.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        if (playerController)
        {
            float x = playerController.gameObject.transform.position.x + (playerController.FacingRight ? 1 : -1);
            float y = playerController.gameObject.transform.position.y;
            this.transform.position = new Vector3(x, y, 0);
        }
    }
}
