using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Rope : MonoBehaviour
{
    public GameObject endA;
    public GameObject endB;

    [Header("Rope physics")]
    public int nr_segments = 8;
    public float width = 0.1F;                      //  The width of the rope segments
    public float ropeMass = 1.0F;                    //  The mass of each rope segment
    public float damping = 0.1F;                     //  The damping of each rope segment


    private LineRenderer line;                       //  Reference to the line renderer component
    private List<GameObject> segments_objects = new List<GameObject>();
    private Vector3[] segmentPos;

    void Start()
    {
        line = GetComponent<LineRenderer>();

        Initialize();
        DrawRope();
    }

    private void Initialize()
    {
        Rigidbody2D endA_rb = endA.GetComponent<Rigidbody2D>();
        Rigidbody2D endB_rb = endB.GetComponent<Rigidbody2D>();

        SpringJoint2D endA_sj, endB_sj;
        if (!endA.GetComponent<SpringJoint2D>())
            endA_sj = endA.AddComponent<SpringJoint2D>();
        else endA_sj = endA.GetComponent<SpringJoint2D>();
        if (!endB.GetComponent<SpringJoint2D>())
            endB_sj = endB.AddComponent<SpringJoint2D>();
        else endB_sj = endB.GetComponent<SpringJoint2D>();

        segmentPos = new Vector3[nr_segments + 1];
        segmentPos[0] = endA.transform.position;
        segmentPos[nr_segments] = endB.transform.position;

        //  Find the distance between each segment
        float segLenX = (endB.transform.transform.position.x - endA.transform.position.x) / nr_segments;
        float segLenY = (endB.transform.transform.position.y - endA.transform.position.y) / nr_segments;
        
        SpringJoint2D last_succ_sj = null;
        Rigidbody2D last_rb = null;
        float posX, posY;
        for (int i = 1; i < nr_segments; i++)
        {
            //  Find the each segments position using the slope from above
            posX = endA.transform.position.x + (segLenX * i);
            posY = endA.transform.position.y + (segLenY * i);

            //  Set each segments position
            segmentPos[i] = new Vector2(posX, posY);

            segments_objects.Add(new GameObject("segment" + i.ToString()));
            segments_objects[i - 1].transform.parent = transform;
            segments_objects[i - 1].layer = LayerMask.NameToLayer("OnlyInteractWithGround");

            segments_objects[i - 1].transform.position = (segmentPos[i] + segmentPos[i - 1]) * 0.5f;
            segments_objects[i - 1].transform.rotation = Quaternion.LookRotation((segmentPos[i] - segmentPos[i - 1]).normalized);

            Rigidbody2D cur_rb = segments_objects[i - 1].AddComponent<Rigidbody2D>();
            cur_rb.mass = ropeMass;
            SpringJoint2D cur_prev_sj = segments_objects[i - 1].AddComponent<SpringJoint2D>();
            SpringJoint2D cur_succ_sj = segments_objects[i - 1].AddComponent<SpringJoint2D>();

            cur_prev_sj.dampingRatio = damping;
            cur_succ_sj.dampingRatio = damping;

            CircleCollider2D cur_cc = segments_objects[i - 1].AddComponent<CircleCollider2D>();
            cur_cc.radius = width;

            if (i == 1)
            {
                endA_sj.connectedBody = cur_rb;
                cur_prev_sj.connectedBody = endA_rb;
            }
            else if (i == nr_segments - 1)
            {
                endB_sj.connectedBody = cur_rb;
                cur_prev_sj.connectedBody = last_rb;
                cur_succ_sj.connectedBody = endB_rb;
                last_succ_sj.connectedBody = cur_rb;
            }
            else
            {
                cur_prev_sj.connectedBody = last_rb;
                last_succ_sj.connectedBody = cur_rb;
            }

            last_rb = cur_rb;
            last_succ_sj = cur_succ_sj;
        }
    }

    private void DrawRope()
    {
        segmentPos[0] = endA.transform.position;
        segmentPos[nr_segments] = endB.transform.position;

        for (int i = 1; i < nr_segments; i++)
        {
            segmentPos[i] = segments_objects[i - 1].transform.position;

        }

        line.positionCount = nr_segments + 1;
        line.startWidth = width; line.endWidth = width;
        line.SetPositions(segmentPos);
    }


    // Update is called once per frame
    void Update()
    {
        DrawRope();
    }
}
