using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawn : MonoBehaviour
{
    // these are measured as deviations from the center: (250, 250).
    private float max_x_range = 150f;
    private float max_z_range = 150f;
    // Start is called before the first frame update
    void Start()
    {
        float new_x = Random.Range(250f - max_x_range, 250f + max_x_range);
        float new_z = Random.Range(250f - max_z_range, 250f + max_z_range);

        transform.position = new Vector3(new_x, 12.5f, new_z);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
