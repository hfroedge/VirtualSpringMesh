using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndUser : RoutingNode
{

    // Start is called before the first frame update
    void Start()
    {
        this.transmitter_power = 1000.0f;
        this.max_move_speed = 5.0f;
        this.set_max_signal_range(40);
        this.node_type = "EndUser";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
