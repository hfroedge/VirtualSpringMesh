using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class RandomWalk : MonoBehaviour
{
    [Header("Monitoring Values: Do not edit")]
    public int steps_since_change;
    public int changes_since_180;

    [Header("Random Walk Hyperparameters: Set before running sim")]
    public float max_angle;
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
    }

    // Update is called once per frame
    void Update()
    {   
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
