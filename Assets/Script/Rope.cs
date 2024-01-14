using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    public GameObject endA;
    public GameObject endB;

    private Rigidbody2D rigidbodyEndA;
    private Rigidbody2D rigidbodyEndB;

    public int segments = 8;
    public float elasticity = 0.2f;                  //  The higher the value, the more elastic the rope will be
    public float ropeMass = 1.0F;                    //  The mass of each rope segment
    public float stiffness = 4.0F;                  //  The higher the value, the less stretchy the rope will be
    [Range(0,1)]public float damping = 0.4F;         //  The higher the value, the less it will flap around

    [SerializeField]
    private Vector3[] segmentPos;                    //  DONT MESS!  This is for the Line Renderer's Reference and to set up the positions of the gameObjects
    [SerializeField]
    private Vector3[] segmentVelocity;             //  DONT MESS!  This is the velocity of each segment, used to make the rope swing
    [SerializeField] 
    private Vector3[] segmentForce;               //  DONT MESS!  This is the force of each segment, used to make the rope swing

    private LineRenderer line;                       //  Reference to the line renderer component
    private float segmentRestingLength;
    private float segmentMaxLength;
    private float segmentMinLength;

    private Vector2 oldVelocityEndA;
    private Vector2 oldVelocityEndB;

    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();

        rigidbodyEndA = endA.GetComponent<Rigidbody2D>();
        rigidbodyEndB = endB.GetComponent<Rigidbody2D>();

        segmentPos = new Vector3[segments + 1];
        segmentVelocity = new Vector3[segments + 1];
        segmentForce = new Vector3[segments + 1];

        segmentRestingLength = (endA.transform.position - endB.transform.position).magnitude / segments;
        segmentMaxLength = segmentRestingLength  + segmentRestingLength * elasticity;
        segmentMinLength = segmentRestingLength - segmentRestingLength * elasticity;

        oldVelocityEndA = rigidbodyEndA.velocity;
        oldVelocityEndB = rigidbodyEndB.velocity;


        // apply physics to the rope
        Initialize();

        // draw the rope
        DrawRope();
    }

    private void Initialize()
    {
        //  Find the distance between each segment
        float segLenX = (endB.transform.transform.position.x - endA.transform.position.x) / segments;
        float segLenY = (endB.transform.transform.position.y - endA.transform.position.y) / segments;
        for (int i = 0; i < segments + 1; i++)
        {
            //  Find the each segments position using the slope from above
            float posX = endA.transform.position.x + (segLenX * i);
            float posY = endA.transform.position.y + (segLenY * i);
            //  Set each segments position
            segmentPos[i] = new Vector2(posX, posY);
        }
    }

    private Vector3 CalculateSpringForce(Vector3 diff)
    {
        return (diff.magnitude - segmentRestingLength) * (diff.magnitude - segmentRestingLength) * stiffness * diff.normalized;
    }

    private void ApplyPhysics()
    {
        segmentPos[0] = endA.transform.position;
        segmentPos[segmentPos.Length - 1] = endB.transform.position;

        for (int i = 0; i < segments + 1; i++)
        {
            if (i == 0)
            {
                Vector3 diff = segmentPos[i + 1] - segmentPos[i];

                segmentForce[i] = CalculateSpringForce(diff);
                rigidbodyEndA.AddForce(segmentForce[i], ForceMode2D.Impulse);
            }
            else
            {
                Vector3 diff;
                if (i == segments)
                {
                    diff = segmentPos[i - 1] - segmentPos[i];
                    segmentForce[i] = CalculateSpringForce(diff);
                    rigidbodyEndB.AddForce(segmentForce[i], ForceMode2D.Impulse);
                }
                else  // i != 0 && i != segments
                {
                    segmentForce[i] = Physics2D.gravity * ropeMass;

                    // apply Hooks law
                    diff = segmentPos[i - 1] - segmentPos[i];
                    Vector3 springForce = CalculateSpringForce(diff);

                    diff = segmentPos[i + 1] - segmentPos[i];
                    springForce += CalculateSpringForce(diff);

                    segmentForce[i] += springForce;

                    segmentVelocity[i] += Time.deltaTime * (segmentForce[i] / ropeMass - damping * segmentVelocity[i]);

                    // calculate damping
                    segmentPos[i] += segmentVelocity[i] * Time.deltaTime;

                    // TODO: check for collision
                    RaycastHit2D hitPrev = Physics2D.Raycast(segmentPos[i - 1], (segmentPos[i] - segmentPos[i - 1]).normalized, (segmentPos[i] - segmentPos[i - 1]).magnitude);
                    RaycastHit2D hitAft = Physics2D.Raycast(segmentPos[i + 1], (segmentPos[i] - segmentPos[i + 1]).normalized, (segmentPos[i] - segmentPos[i + 1]).magnitude);

                    if (hitPrev.collider != null && hitPrev.collider.CompareTag("Ground") &&
                        hitAft.collider != null && hitAft.collider.CompareTag("Ground"))
                    {
                        // draw x and y coordinates of the normal
                        Debug.DrawLine(hitPrev.point, hitPrev.point + new Vector2(hitPrev.normal.x, 0), Color.red);
                        Debug.DrawLine(hitPrev.point, hitPrev.point + new Vector2(0, hitPrev.normal.y), Color.red);
                        // draw the hit line
                        Debug.DrawLine(segmentPos[i - 1], segmentPos[i], Color.red);

                        Vector2 normal = hitPrev.normal.normalized;

                        // Calculate the reflection direction
                        Vector2 reflection = Vector2.Reflect(segmentVelocity[i], normal);

                        // Apply the reflection direction as the new velocity
                        segmentVelocity[i] = reflection;

                        // Adjust the position to avoid penetration
                        segmentPos[i] = hitPrev.point + (normal * 0.01f); // Adjust by a small offset to avoid continuous collision
                    }
                }

                diff = segmentPos[i - 1] - segmentPos[i];
                

                float distanceError = diff.magnitude > segmentMaxLength ? diff.magnitude - segmentMaxLength : 0;

                if (distanceError > 0.0f)
                {
                    segmentPos[i] = segmentPos[i - 1] - diff.normalized * segmentMaxLength;
                }

                if (i == segments)
                {
                    segmentPos[i] = endB.transform.position;

                    for (int j = segments; j >= 1; j--)
                    {
                        diff = segmentPos[j - 1] - segmentPos[j];

                        distanceError = diff.magnitude > segmentMaxLength ? diff.magnitude - segmentMaxLength : 0;

                        if (distanceError > 0.0f)
                        {
                            segmentPos[j - 1] = segmentPos[j] + diff.normalized * segmentMaxLength;
                        }

                    }
                }
            }

        }
    }

    private void ApplyPhysics2()
    {
        // static forces
        Vector3 prevDiff, succDiff;
        for (int i = 0, stride = segments, dir = 1; stride >= 0; i += dir * stride, stride--, dir *= -1)
        {
            if (i == 0)
            {
                //prevDiff = new Vector3(endA.transform.position.x, endA.transform.position.y) - segmentPos[i];
                //succDiff = segmentPos[i + 1] - segmentPos[i];
                // add constrains
                segmentPos[0] = endA.transform.position;
                succDiff = segmentPos[i + dir] - segmentPos[i];
                   
                if (succDiff.magnitude > segmentMaxLength)
                {
                    //segmentPos[i + dir] = segmentPos[i] + succDiff.normalized * segmentMaxLength;
                }
            }
            else if (i == segments)
            {
                //prevDiff = segmentPos[i - 1] - segmentPos[i];
                //succDiff = new Vector3(endB.transform.position.x, endB.transform.position.y) - segmentPos[i];
                // add constrains
                segmentPos[segments] = endB.transform.position;
                succDiff = segmentPos[i + dir] - segmentPos[i];

                if (succDiff.magnitude > segmentMaxLength)
                {
                    //segmentPos[i + dir] = segmentPos[i] + succDiff.normalized * segmentMaxLength;
                }
            }
            else
            {
                prevDiff = segmentPos[i - 1] - segmentPos[i];
                succDiff = segmentPos[i + 1] - segmentPos[i];

                segmentForce[i] = Physics2D.gravity;
                segmentForce[i] += CalculateSpringForce(prevDiff);
                segmentForce[i] += CalculateSpringForce(succDiff);

                segmentVelocity[i] += Time.deltaTime * (segmentForce[i] / ropeMass - damping * segmentVelocity[i]);
                segmentPos[i] += segmentVelocity[i] * Time.deltaTime;

                // add constrains
                //succDiff = segmentPos[i + dir] - segmentPos[i];
                //if (succDiff.magnitude > segmentMaxLength)
                //{
                //    segmentPos[i + dir] = segmentPos[i] + succDiff.normalized * segmentMaxLength;
                //}

                if (stride == 0)
                {
                    for (int j = i, inner_stride=1, inner_dir = -dir; inner_stride <= segments - 2 ;j+=inner_stride * inner_dir, inner_stride++, inner_dir = -inner_dir)
                    {
                        succDiff = segmentPos[j + inner_dir] - segmentPos[j];
                        prevDiff = segmentPos[j - inner_dir] - segmentPos[j];
                        if (succDiff.magnitude > segmentMaxLength)
                        {
                            segmentPos[j + inner_dir] = segmentPos[j] + succDiff.normalized * segmentMaxLength;
                        }
                    }
                }
            }
        }




    }

    private void DrawRope()
    {
        line.positionCount = segmentPos.Length;

        line.SetPositions(segmentPos);
    }


    // Update is called once per frame
    void Update()
    {
        // apply physics to the rope
        ApplyPhysics();
        //ApplyPhysics2();

        // draw the rope
        DrawRope();
    }
}
