using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Rope : MonoBehaviour
{
    public GameObject endA;
    public GameObject endB;
    private SpringJoint2D endA_sj, endB_sj;

    [Header("Rope physics")]
    public int nr_segments = 4;
    public float width = 0.1F;                       //  The width of the rope segments
    public float ropeMass = 1.0F;                    //  The mass of each rope segment
    public float damping = 0.1F;                     //  The damping of each rope segment


    private LineRenderer line;                       //  Reference to the line renderer component
    private List<GameObject> segments_objects = new List<GameObject>();
    private Vector3[] segmentPos;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        segmentPos = new Vector3[nr_segments + 1];



        if (!endA.GetComponent<SpringJoint2D>())
            endA_sj = endA.AddComponent<SpringJoint2D>();
        else endA_sj = endA.GetComponent<SpringJoint2D>();
        if (!endB.GetComponent<SpringJoint2D>())
            endB_sj = endB.AddComponent<SpringJoint2D>();
        else endB_sj = endB.GetComponent<SpringJoint2D>();


        if (!endA)
            CreateEmptyEnd("A");
        if (!endB)
            CreateEmptyEnd("B");

        Initialize();
        DrawRope();
    }

    private void Initialize()
    {
        Rigidbody2D endA_rb = endA.GetComponent<Rigidbody2D>();
        Rigidbody2D endB_rb = endB.GetComponent<Rigidbody2D>();

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

            cur_prev_sj.enableCollision = true;
            cur_succ_sj.enableCollision = true;

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

    public void DestroyRope()
    {
        Destroy(endA_sj);
        Destroy(endB_sj);
        foreach (GameObject segment in segments_objects)
        {
            Destroy(segment);
        }
        Destroy(gameObject);
    }

    private void CreateEmptyEnd(string which_end="A")
    {
        GameObject end = new GameObject("end" + which_end);
        Rigidbody2D rb = end.AddComponent<Rigidbody2D>();
        rb.mass = ropeMass;

        if (which_end == "A")
        {
            if (segments_objects.Count != 0)
            {
                end.transform.position = segments_objects[0].transform.position;
                segments_objects[0].GetComponent<SpringJoint2D>().connectedBody = rb;
                for (int i = 1; i < segments_objects.Count; i++)
                    foreach (SpringJoint2D cur_sj in segments_objects[i].GetComponents<SpringJoint2D>())
                        cur_sj.distance = 0;
            }
            else
                end.transform.position = transform.position;
        }
        else if (which_end == "B")
        {
            if (segments_objects.Count != 0)
            {
                end.transform.position = segments_objects[segments_objects.Count - 1].transform.position;
                segments_objects[segments_objects.Count - 1].GetComponents<SpringJoint2D>()[1].connectedBody = rb;
                for (int i = 1; i < segments_objects.Count; i++)
                    foreach (SpringJoint2D cur_sj in segments_objects[i].GetComponents<SpringJoint2D>())
                        cur_sj.distance = 0;
            }
            else
                end.transform.position = transform.position;
        }

        CircleCollider2D cc = end.AddComponent<CircleCollider2D>();
        cc.radius = width;

        if (which_end == "A")
        {
            endA = end;
            endA_sj = end.AddComponent<SpringJoint2D>();
            endA_sj.connectedBody = rb;
            endA_sj.enableCollision = true;
            endA_sj.distance = 0;
        }
        else if (which_end == "B")
        {
            endB = end;
            endB_sj = end.AddComponent<SpringJoint2D>();
            endB_sj.connectedBody = rb;
            endB_sj.enableCollision = true;
            endB_sj.distance = 0;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (!endA)
        {
            CreateEmptyEnd("A");
            if (!endB)
                CreateEmptyEnd("B");
            Initialize();
        }
        if (!endB)
        {
            CreateEmptyEnd("B");
            Initialize();
        }


        DrawRope();
    }
}
