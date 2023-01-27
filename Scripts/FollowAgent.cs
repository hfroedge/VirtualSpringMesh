using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowAgent : Blimp
{
    public GameObject target_agent;
    public Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {   
        Vector3 target_position = new Vector3(0, height_offset, 0) + target_agent.transform.position;
        rb.MovePosition(target_position);
    }
}
