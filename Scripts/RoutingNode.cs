using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoutingNode : MonoBehaviour
{

    // range of node given in meters
    [Header("Internal Node Parameters")]
    public Color color = Color.black;
    public List<Blimp> blimp_connections;
    public List<EndUser> eu_connections;
    public float transmitter_power;
    
    // for now, completely arbitrary. These just needed for calculation.
    private float max_signal_range;
    public float max_move_speed;

    protected string node_type;

    // graph_label is an id unqiue to a given graph.
    private int graph_label;

    void OnDrawGizmos(){
        Gizmos.color = this.color;
        Gizmos.DrawWireSphere(transform.position, max_signal_range);
    }

    public string get_type(){
        return this.node_type;
    }

    public float get_max_signal_range(){
        return this.max_signal_range;
    }

    /**
    sets max signal range to a value measured in meters.
    */
    public void set_max_signal_range(float range){
        this.max_signal_range = range;
    }

    public void set_node_color(Color color){
        this.color = color;
    }

    public void set_graph_label(int i){
        this.graph_label = i;
    }

    public void remove_connection(Blimp unconnected_node){
        if (blimp_connections.Contains(unconnected_node)){
            blimp_connections.Remove(unconnected_node);
        }

    }
    public void remove_connection(EndUser unconnected_node){
        if (eu_connections.Contains(unconnected_node)){
            eu_connections.Remove(unconnected_node);
        }
    }

    public void add_connection(Blimp connected_node){
        if (this.get_type() == "Blimp"){
        }

        if (! blimp_connections.Contains(connected_node)){
            blimp_connections.Add(connected_node);
        }

    }
    public void add_connection(EndUser connected_node){
        if (! eu_connections.Contains(connected_node)){
            eu_connections.Add(connected_node);
        }
    }
}
