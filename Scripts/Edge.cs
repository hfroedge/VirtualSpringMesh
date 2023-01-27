using System.Collections;
using System.Collections.Generic;

public class Edge
{
    public RoutingNode node1;
    public RoutingNode node2;

    public Edge(RoutingNode node1, RoutingNode node2){
        this.node1 = node1;
        this.node2 = node2;
    }

    // public int get_label(){
    //     return ;
    // }
}
