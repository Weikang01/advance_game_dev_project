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

    public float ropeLength = 1.0F;                  //  The length of the rope.  Changing this value dynamically will cause the rope to break when it is too short (0 < ropeLength < ropeSegLen * 2)
    public int segments = 8;
    private float segmentRestingLength;
    public float ropeMass = 1.0F;                    //  The mass of each rope segment
    public float ropeWidth = 0.1F;                   //  Sets the width of the rope, this also changes the width of the collider
    public float stiffness = 4.0F;                  //  The higher the value, the less stretchy the rope will be
    public float damping = 0.4F;                    //  The higher the value, the less it will flap around

    [SerializeField]
    private Vector3[] segmentPos;                    //  DONT MESS!  This is for the Line Renderer's Reference and to set up the positions of the gameObjects
    [SerializeField]
    private Vector3[] segmentVelocity;             //  DONT MESS!  This is the velocity of each segment, used to make the rope swing
    [SerializeField] 
    private Vector3[] segmentForce;               //  DONT MESS!  This is the force of each segment, used to make the rope swing


    private LineRenderer line;                       //  Reference to the line renderer component

    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();

        rigidbodyEndA = endA.GetComponent<Rigidbody2D>();
        rigidbodyEndB = endB.GetComponent<Rigidbody2D>();

        segmentPos = new Vector3[segments + 1];
        segmentVelocity = new Vector3[segments + 1];
        segmentForce = new Vector3[segments + 1];

        segmentRestingLength = ropeLength / segments;

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
            segmentPos[i] = new Vector3(posX, posY, 0);
        }
    }

    private void ApplyPhysics()
    {
        for (int i = 0; i < segments + 1; i++)
        {
            if (i == 0)
            {
                Vector3 diff = segmentPos[i] - segmentPos[i + 1];
                segmentForce[i] -= (diff.magnitude - segmentRestingLength) * stiffness * diff / diff.magnitude;
                //rigidbodyEndA.AddForce(segmentForce[i]);
            }
            if (i != 0 && i != segments)
            {
                segmentForce[i] = Physics2D.gravity * ropeMass;

                // apply Hooks law
                Vector3 diff = segmentPos[i] - segmentPos[i - 1];
                Vector3 springForce = (diff.magnitude - segmentRestingLength) * stiffness * diff / diff.magnitude;

                diff = segmentPos[i] - segmentPos[i + 1];
                springForce += (diff.magnitude - segmentRestingLength) * stiffness * diff / diff.magnitude;

                segmentForce[i] -= springForce;
                segmentVelocity[i] += Time.deltaTime * (segmentForce[i] / ropeMass - damping * segmentVelocity[i]);
                // calculate damping
                //segmentVelocity[i] *= damping;

                segmentPos[i] += segmentVelocity[i] * Time.deltaTime;
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
        segmentPos[0] = endA.transform.position;
        segmentPos[segmentPos.Length - 1] = endB.transform.position;

        // apply physics to the rope
        ApplyPhysics();

        // draw the rope
        DrawRope();
    }
}
