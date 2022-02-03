using UnityEngine;
//using Valve.VR;

public class ResetCameraRig : MonoBehaviour
{
    [Tooltip("Desired head position of player when seated")]
    public Transform cameraRig;
    public Transform cameraTransform;

    public float manualCalibrationX;
    public float manualCalibrationY;
    public float manualCalibrationZ;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetOrigin();
        }
    }

    private void ResetOrigin()
    {
        if ((cameraTransform != null) && (cameraRig != null))
        {

            cameraRig.Rotate(new Vector3(0f, -cameraTransform.rotation.eulerAngles.y, 0f));
            cameraRig.position = new Vector3(cameraRig.position.x - cameraTransform.position.x, cameraRig.position.y, cameraRig.position.z - cameraTransform.position.z);
            cameraRig.position += new Vector3(manualCalibrationX, manualCalibrationY, manualCalibrationZ);

            Debug.Log("reset origin");
        }
        else
        {
            Debug.Log("Error: SteamVR objects not found!");
        }
    }
}