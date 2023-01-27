using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blimp : RoutingNode
{
    public float height_offset = 12;
    // for now, completely arbitrary. Just needed for calculation.

    // Start is called before the first frame update
    void Start()
    {
        this.transmitter_power = 1000.0f;
        this.set_max_signal_range(175);
        this.node_type = "Blimp";
        // 2 meters per second
        this.max_move_speed = 5.0f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
