using System.Collections;
using System;
using System.Text;
using System.Collections.Generic;
using UnityEngine;

public class VirtualSpringMesh : MonoBehaviour
{
    [Header("Internal Parameters: do not edit")]
    public int steps_since_update;

    [Header("Hyperparameters")]
    // minimum force threshold for R, as described in Felice 2014.
    public float min_force_r_thresh = 1;
    public int steps_before_update = 5;

    Blimp this_blimp;
    private Rigidbody rb;   

    [Header("Variables related to flight algorithm")]
    public int steps_since_last_contact = 0;
    // after which, will return to reference point
    private int max_steps_since_last_contact = 1500;

    // amount to increase edge length of spiral after each 90 degree turn
    private int incr_rotation_edge = 50;
    public int steps_since_rotation = 0;
    public int edge_steps_length = 25;
    public bool part_of_network_spine = true;
    
    // Start is called before the first frame update
    void Start()
    {
        this_blimp = GetComponent<Blimp>();
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

    }

    // Update is called once per frame
    void Update()
    {        
        Vector3 net_force = sum_forces();

        if (this_blimp.blimp_connections.Count < 1){
            steps_since_last_contact = steps_since_last_contact + 1;
        }
        else {
            steps_since_last_contact = 0;
            rb.velocity = new Vector3(0,0,0);
        }

        if (transform.position.y <= this_blimp.height_offset || transform.position.y > (this_blimp.height_offset+ 3) ){
            net_force.y = this_blimp.height_offset;
        }

        if (steps_since_last_contact < max_steps_since_last_contact){
            this_blimp.transform.position = Vector3.MoveTowards(transform.position, 
            transform.position + net_force, 
            this_blimp.max_move_speed*Time.deltaTime);
            part_of_network_spine = true;
        }
        else {
            part_of_network_spine = false;
            
            //searching_for_new_ref: begin spiraling.
            if (steps_since_rotation > edge_steps_length){
                // rotate
                Quaternion quat = Quaternion.AngleAxis(90f, Vector3.up);
                rb.rotation *= quat;
                steps_since_rotation = 0;
                edge_steps_length = edge_steps_length + incr_rotation_edge;
            }
            else{
                rb.velocity = transform.forward * 5.0f;
                steps_since_rotation = steps_since_rotation + 1;
            }
        }
    }

    /**
    Simulates power receiption of a RoutingNode receiver by treating signals and transmitters as isotropic radiators.
    The power of a signal emitted from an isotropic radiator follows the inverse square law with-respect-to its displacement
    (Jim Sinclair, "How Radio Signals Work", Ch 3.4). Using this as basic approximation of received power.

    param other_node: may be another blimp or an end_user
    */
    private float inverse_square_intensity(RoutingNode other_node){
        
        Vector3 l = this_blimp.transform.position;
        Vector3 x = other_node.transform.position;
        float intensity;
        float radius = (l - x).magnitude;
        
        if (other_node.get_type() == "Blimp"){
            if (radius > 100){
                intensity = other_node.transmitter_power/(4*Mathf.PI*Mathf.Pow(radius, 2.0f));
            }
            else {
                // don't want Blimps to clump together, so anything less than the number above will be pushed back.
                // at radius of 10, intensity = - 0.008f. At 1, 0.07f
                
                intensity = -other_node.transmitter_power/(4*Mathf.PI*Mathf.Pow(radius, 2.0f));
                // at this constant, blimps may overlap - only slight push back.
                // intensity = -0.005f; 
            }
        }
        else {
            if (radius > 1){
                // for math reasons (don't want to divide by fractions or by zero)
                intensity = other_node.transmitter_power/(4*Mathf.PI*Mathf.Pow(radius, 2.0f));
                // intensity = other_node.transmitter_power*(4*Mathf.PI*Mathf.Pow(radius, 2.0f));
            }
            else {
                // for engineering reasons.
                intensity = -0.0f;
            }
        }
        

        return intensity;
    }

    /**
    Calculates attraction weight for the blimp towards a ground agent node.
    Works as intended when set to constant 0.5f, with strong attraction to ground nodes
    */
    private float calculate_k_atg(RoutingNode end_user){
        int eu_connections = this_blimp.eu_connections.Count;

        int max_neighbor_connectivity = 1; 
        int connectivity;

        foreach(Blimp neighbor_ru in this_blimp.blimp_connections){

            connectivity = neighbor_ru.blimp_connections.Count;

            if (connectivity > max_neighbor_connectivity){
                max_neighbor_connectivity = connectivity;
            }
        }

        float k_atg = 0.4f*((float)eu_connections) / (float)max_neighbor_connectivity;
        
        return k_atg;
    }

    /**
    Calculate attraction weight for the blimp towards another blimp.
    Changed this by a factor of 100 to reduce tendency to clump together
    */
    private float calculate_k_ata(RoutingNode other_blimp){

        float k_ata = 0.005f;
        return k_ata;
    }

    private Vector3 sum_forces(){
        Vector3 net_force = new Vector3(0, 0, 0);

        foreach(Blimp other_blimp in this_blimp.blimp_connections){
            float magnitude = calculate_k_ata(other_blimp)*inverse_square_intensity(other_blimp);
            Vector3 direction = other_blimp.transform.position - this_blimp.transform.position;
            net_force = net_force + magnitude * direction;
        }

        
        foreach(EndUser eu in this_blimp.eu_connections){
            float magnitude = calculate_k_atg(eu)*inverse_square_intensity(eu);
            Vector3 direction = eu.transform.position - this_blimp.transform.position;
            net_force = net_force + magnitude * direction.normalized;
        }  
        
        return net_force;
    }

    
}
