using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Text;
using System.IO;


public class NetworkGraph : MonoBehaviour
{

    // these are measured as deviations from the center: (250, 250).
    private float max_x_range = 150f;
    private float max_z_range = 150f;

    public RoutingNode[] nodes;
    private List<Edge> edges;
    public int iter = 0;
    private int experiment_iter_max = 20000;
    private int experiment_iter = 0;
    private StringBuilder csv = new StringBuilder();
    // Start is called before the first frame update
    void Start()
    {
        csv = begin_csv();
    }

    void Update()
    {
        StringBuilder connections = new StringBuilder("", 200);
        
        this.edges = determine_edges();
        color_subgraphs_green();
        
        foreach(RoutingNode node in nodes){
            if (node.get_type() == "EndUser"){
                connections.AppendFormat(", {0}", node.blimp_connections.Count + node.eu_connections.Count);
            }
        }

        csv.AppendLine(string.Format("{0}, {1}{2}", iter, get_connectivity_index(), connections.ToString()));
        if (iter == experiment_iter_max){
            File.WriteAllText(string.Format("./vsm_testn{0}.csv", experiment_iter), csv.ToString());
            Debug.Log("saved file");
            Debug.Log(experiment_iter);
            csv = begin_csv();
            experiment_iter = experiment_iter + 1;
            iter = 0;

            foreach(RoutingNode node in nodes){
                float new_x = UnityEngine.Random.Range(250f - max_x_range, 250f + max_x_range);
                float new_z = UnityEngine.Random.Range(250f - max_z_range, 250f + max_z_range);
                node.transform.position = new Vector3(new_x, 12.5f, new_z);  
            }
        }

        iter++;
    }

    private StringBuilder begin_csv(){
        StringBuilder local_header = new StringBuilder("", 600);
        StringBuilder local_csv = new StringBuilder();
        local_header.Append("Time Step, Connectivity Index");
        
        int num_eus = 30;

        for (int i = 1; i <= num_eus; i = i + 1){
            local_header.AppendFormat(", connections to EU{0}", i);
        }

        local_csv.AppendLine(local_header.ToString());

        return local_csv;
    }

    private float get_connectivity_index(){
        float connected_eus = 0.0f;
        float total_eus = 0.0f;
        foreach(RoutingNode rn in nodes){
            if (rn.get_type() == "EndUser"){
                total_eus = total_eus + 1.0f;

                if ((rn.eu_connections.Count > 0) || (rn.blimp_connections.Count > 0)){
                    connected_eus = connected_eus + 1.0f;
                }
            }
        }  
        return connected_eus / total_eus;
    }

    /**
    Update internal states of two nodes to indicate if they are connected

    Isn't there a better way than this type-check/casting/4-way overloading method?
    */
    private void declare_connection(RoutingNode n1, RoutingNode n2, bool connected){
        
        if(n1.get_type() == "Blimp" && n2.get_type() == "Blimp" ){
            declare_connection((Blimp) n1, (Blimp) n2, connected);
        }
        else if(n1.get_type() == "EndUser"  && n2.get_type() == "Blimp" ){
            declare_connection((EndUser) n1, (Blimp) n2, connected);
        }
        else if(n1.get_type() == "Blimp" && n2.get_type() == "EndUser" ){
            declare_connection((Blimp) n1, (EndUser) n2, connected);
        }
        else if(n1.get_type() == "EndUser"  && n2.get_type() == "EndUser" ){
            declare_connection((EndUser) n1, (EndUser) n2, connected);
        }
    }
    private void declare_connection(Blimp n1, Blimp n2, bool connected){
        
        if (connected){
            n1.add_connection(n2);
            n2.add_connection(n1);
        }
        else {
            n1.remove_connection(n2);
            n2.remove_connection(n1);
        }
    }
    private void declare_connection(EndUser n1, Blimp n2, bool connected){
        
        if (connected){
            n1.add_connection(n2);
            n2.add_connection(n1);
        }
        else {
            n1.remove_connection(n2);
            n2.remove_connection(n1);
        }
    }
    private void declare_connection(Blimp n1, EndUser n2, bool connected){
        
        if (connected){
            n1.add_connection(n2);
            n2.add_connection(n1);
        }
        else {
            n1.remove_connection(n2);
            n2.remove_connection(n1);
        }
    }
    private void declare_connection(EndUser n1, EndUser n2, bool connected){
        
        if (connected){
            n1.add_connection(n2);
            n2.add_connection(n1);
        }
        else {
            n1.remove_connection(n2);
            n2.remove_connection(n1);
        }
    }

    /**
    Checks distance between nodes and returns list of node pairs that are bilaterally connected.
    Uses node.max_signal_range to determine if connection is possible.
    */
    private List<Edge> determine_edges(){

        List<Edge> edges = new List<Edge>();

        for (int i = 0; i < nodes.Length-1; i++){
            for (int j = i+1; j < nodes.Length; j++){
                RoutingNode n1 = nodes[i];
                RoutingNode n2 = nodes[j];
                
                float distance = Vector3.Distance(n1.transform.position, n2.transform.position);

                if (distance < n1.get_max_signal_range() && distance < n2.get_max_signal_range()){
                    edges.Add(new Edge(n1, n2));
                    declare_connection(n1, n2, true);
                }
                else {
                    declare_connection(n1, n2, false);
                }
            }
        }

        return edges;
    }

    /**
    If node in edge, returns the other node. Returns null otherwise.
    */
    private RoutingNode node_in_edge(RoutingNode node, Edge edge){
        if (node.Equals(edge.node1)) {
            return edge.node2;
        } 
        else if (node.Equals(edge.node2)){
            return edge.node1;
        }
        else {
            return null;
        }
    }

    /**
    https://stackoverflow.com/questions/354330/how-to-determine-if-two-nodes-are-connected

    Returns true if a path exists between source and destination.
    */
    public bool nodes_connected(RoutingNode source, RoutingNode destination){
        
        Stack<RoutingNode> to_do = new Stack<RoutingNode>();
        List<RoutingNode> done = new List<RoutingNode>();

        to_do.Push(source);

        while (to_do.Count != 0){
            RoutingNode removed_node = to_do.Pop();

            foreach(Edge edge in this.edges){
                RoutingNode reachable_node = node_in_edge(removed_node, edge);
                if (reachable_node.Equals(destination)){
                    return true;
                }
                if (!done.Contains(reachable_node) && !reachable_node.Equals(null)){
                    to_do.Push(reachable_node);
                }
            }
        }

        return false;
    }

    /**
    Colors any node which is connected to another node green. Red otherwise.
    */
    public void color_subgraphs_green(){
        foreach(RoutingNode node in nodes){
            if( (node.blimp_connections.Count + node.eu_connections.Count) > 0){
                node.color = Color.green;
            }
            else {
                node.color = Color.red;
            }
        }
    }

    // /**
    // Colors nodes of this graph a given color.
    // */
    // private void color_graph(Color color){
    //     foreach (RoutingNode node in this.nodes){
    //         node.set_node_color(color);
    //     }
    // }

    // public void color_subgraphs(){
    //     foreach (NetworkGraph subgraph in get_subgraphs()){
    //         if (subgraph.edges.Equals(null)){
    //             color_graph(Color.black);
    //         }
    //         else {
    //             color_graph(Color.green);
    //         }
    //     }
    // }

    // /**
    // Uses subgraph labels (set in mark_subgraphs) 
    // */
    // public List<NetworkGraph> get_subgraphs(){
    //     return new List<NetworkGraph>();
    // }

    // /**
    // Marks nodes with a label indicating their subgraph. Single isolated nodes are subgraphs with no edges.
    // First call with no arguments
    // DFS Floodfill
    // */
    // private void mark_subgraphs(Stack<RoutingNode> unlabeled_nodes, int label){

    //     RoutingNode current_node = unlabeled_nodes.Pop();
    //     while(unlabeled_nodes.Count > 0){
    //         foreach(List<(RoutingNode, RoutingNode)>)
    //     }

    // }



}
