using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class RandomWalkPauses : MonoBehaviour
{
    [Header("Monitoring Values: Do not edit")]
    public int steps_since_change;
    public int changes_since_180;
    private int max_pause_time = 1000;
    private int pause_iter;
    private float pause_prob = 0.001f;
    private bool is_paused;

    [Header("Random Walk Hyperparameters: Set before running sim")]
    private float max_angle = 360.0f;
    private int steps_before_direction_change = 400;
    private int changes_before_180 = 10;
    private Rigidbody rb;    
    private float max_move_speed;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        steps_since_change = 0;
        changes_since_180 = 0;
        max_move_speed = GetComponent<EndUser>().max_move_speed;
        is_paused = false;
        pause_iter = 0;
    }

    // Update is called once per frame
    void Update()
    {   

        if (!is_paused){
            if (Random.Range(0f, 100f)/100.0f < pause_prob){
                is_paused = true;
            }

            if (changes_since_180 >= changes_before_180){
                ChangeDirection(true);
                changes_since_180 = 0;
            }
            else if (steps_since_change >= steps_before_direction_change){
                ChangeDirection();
                changes_since_180++;
            }

            rb.velocity = transform.forward * max_move_speed;

            steps_since_change++;
        }
        else {
            if (pause_iter > max_pause_time){
                is_paused = false;
                pause_iter = 0;
            }
            else {
                pause_iter = pause_iter + 1;
            }
        }
        
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
        // the *= operation rotates by an angle, rather than assigning. This is specific to the
        // Unity Engine implementation.
        rb.rotation *= quat;

        steps_since_change = 0;
    }
}
