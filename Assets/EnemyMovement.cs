using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private float speed = 2f;
    private bool turned = false;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Rigidbody2D rb2;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wall1;
    [SerializeField] private Transform wall2;
    [SerializeField] private LayerMask groundLayer;

    void Update()
    {
        if (ReachedEnd() && turned == false)
        {
            speed = speed * -1;
            turned = true;
            StartCoroutine(waitForMove());
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.velocity = new Vector2(speed, rb.velocity.y);
        rb2.velocity = new Vector2(speed, rb2.velocity.y);

    }

    bool ReachedEnd()
    {
        
        if(Physics2D.OverlapCircle(wall1.position, 0.01f, groundLayer) || Physics2D.OverlapCircle(wall2.position, 0.2f, groundLayer))
        {
            return true;
        }
        else if (Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    IEnumerator waitForMove()
    {
        yield return new WaitForSeconds(0.2f);
        turned = false;
    }
}
