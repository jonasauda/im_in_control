using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeRigidBody : MonoBehaviour
{

    private Vector3 oldPosition;
    [HideInInspector]
    public Vector3 velocity; 

    // Start is called before the first frame update
    void Start()
    {
        oldPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newPosition = transform.position;
        Vector3 deltaPos = newPosition - oldPosition;
        velocity = deltaPos / Time.deltaTime;
        oldPosition = newPosition;
    }
}
