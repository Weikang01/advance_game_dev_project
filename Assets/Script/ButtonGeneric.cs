using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonGeneric : MonoBehaviour
{
    //door function and the value for the currentbutton
    public DoorControllerGeneric genricFunction;
    public int currentButton = 0;

    //when the player steps on the button it will return the currentbutton and true to the door function
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Collider2D button_base = gameObject.GetComponentInChildren<BoxCollider2D>();

        if (collision.gameObject.CompareTag("Player") 
            && collision.otherCollider.gameObject.name != button_base.gameObject.name)
        {
            genricFunction.doorFunc(true, currentButton);
        }
    }
    //when the player steps off the button it will return the currentbutton and true to the door function
    private void OnCollisionExit2D(Collision2D collision)
    {
        Collider2D button_base = gameObject.GetComponentInChildren<BoxCollider2D>();

        if (collision.gameObject.CompareTag("Player")
            && collision.otherCollider.gameObject.name != button_base.gameObject.name)
        {
            genricFunction.doorFunc(false, currentButton);
        }
    }
}
