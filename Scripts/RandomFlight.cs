using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomFlight : MonoBehaviour
{
    [Header("Monitoring Values: Do not edit")]
    public int steps_since_change;
    public int changes_since_180;

    [Header("Random Walk Hyperparameters: Set before running sim")]
    public float max_angle = 360;
    private int steps_before_direction_change = 400;
    private int changes_before_180 = 10;
    private float max_move_speed;
    Blimp this_blimp;
    
    [Header("Variables related to flight algorithm")]
    public Vector3 ref_point = new Vector3(250, 15, 250);
    public int steps_since_last_contact = 0;
    // after which, will return to reference point
    private int max_steps_since_last_contact = 1500;

    // amount to increase edge length of spiral after each 90 degree turn
    private int incr_rotation_edge = 50;
    public int steps_since_rotation = 0;
    public int edge_steps_length = 25;
    public bool ref_point_set = false;
    public bool random_walk = true;
    public bool return_to_ref = false;




    
    private Rigidbody rb;   
    // Start is called before the first frame update
    void Start()
    {
        this_blimp = GetComponent<Blimp>();
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        steps_since_change = 0;
        changes_since_180 = 0;
        max_move_speed = this_blimp.max_move_speed;
    }

    // Update is called once per frame
    void Update()
    {   
        // if contact has recently been made with another blimp, use the random walk algorithm below.
        // if no contact has been made recently, return to reference point
        // if no contact is made on the way to the reference point, begin searching for a new one.


        if (this_blimp.blimp_connections.Count < 1){
            steps_since_last_contact = steps_since_last_contact + 1;
        }
        else {
            steps_since_last_contact = 0;
            ref_point = transform.position;
            ref_point_set = true;
        }

        if (steps_since_last_contact < max_steps_since_last_contact){
            RandomWalk();
            random_walk = true;
        }
        else {
            random_walk = false;
            if (ref_point_set){
                // return to reference point
                
                // rb.MovePosition(ref_point * Time.deltaTime * 5f);
                // transform.position = ref_point;
                
                float dist = Vector3.Distance(transform.position, ref_point);
                if (dist < 10 || dist > 250) {
                    // at this distance, returning to a reference point is likely too late. Also catches cases where blimp wanders
                    // too far. Begin spiraling.
                    ref_point_set = false;
                    return_to_ref = false;
                }
                else {
                    MoveTowardsPoint(ref_point);
                    return_to_ref = true;
                }
                
            }
            else {
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
    }

    private void MoveTowardsPoint(Vector3 ref_point){
        
        transform.LookAt(ref_point);
        rb.velocity = transform.forward * 5.0f;
    }

    private void RandomWalk(){
        if (changes_since_180 >= changes_before_180){
            ChangeDirection(true);
            changes_since_180 = 0;
        }
        else if (steps_since_change >= steps_before_direction_change){
            ChangeDirection();
            changes_since_180++;
        }

        float move_speed;
        if (this_blimp.eu_connections.Count > 1){
            move_speed = 1.0f;
        }
        else {
            move_speed = 5.0f;
        }

        rb.velocity = transform.forward * move_speed;

        if (transform.position.y <= this_blimp.height_offset || transform.position.y > (this_blimp.height_offset+ 3) ){
            transform.position = new Vector3(transform.position.x, this_blimp.height_offset+2, transform.position.z);
        } 

        steps_since_change++;
    }

    private void ChangeDirection(bool reverse = false) {
    // https://answers.unity.com/questions/552674/make-a-character-walk-around-randomly.html
    float neg_or_pos = Random.Range(0f, 100f);
    float angle;

    if(reverse){
        angle = 180f;
    } else {

        if (neg_or_pos < 50f){
            angle = -Random.Range(0f, max_angle);
        } else {
            angle = Random.Range(0f, max_angle);
        }

    }
    
    Quaternion quat = Quaternion.AngleAxis(angle, Vector3.up);
    // * rotates by an angle, rather than assigning.
    rb.rotation *= quat;

    steps_since_change = 0;
    }
}
