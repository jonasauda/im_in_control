using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class OptitrackHmdTracking : MonoBehaviour
{

    private Camera camera;
    private Vector3 startPos = Vector3.zero;
    private Quaternion startRot = Quaternion.identity;

    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponentInChildren<Camera>();
        InputTracking.disablePositionalTracking = true;
        camera.transform.localPosition = Vector3.zero;
        startRot = camera.transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position = -InputTracking.GetLocalPosition(XRNode.CenterEye);
        transform.localRotation = Quaternion.Inverse(InputTracking.GetLocalRotation(XRNode.CenterEye));
    }

    void LateUpdate()
    {
    }
}
