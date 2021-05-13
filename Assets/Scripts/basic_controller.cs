using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class basic_controller : MonoBehaviour
{
    public float speed = 1.0f;
    private Rigidbody rigidBody;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate() {
        if (rigidBody.velocity.magnitude < speed){
            float value = Input.GetAxis("Vertical");
            if(value != 0){
                rigidBody.AddForce(0, 0, value * Time.fixedDeltaTime * 1000f);
            }
        }
    }
    
}
