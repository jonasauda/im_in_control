using UnityEngine;

public class VRSystemSettings : MonoBehaviour
{

    public enum VirtualRealiySystem { Oculus, Vive, UNDEFINDED };

    public VirtualRealiySystem virtualRealiySystem = VirtualRealiySystem.UNDEFINDED;

    private GameObject vrCamera;

    // Start is called before the first frame update
    void Awake()
    {
        switch (virtualRealiySystem)
        {
            case VirtualRealiySystem.Oculus:
                GameObject.Find("ViveCameraRig").SetActive(false);
                vrCamera = GameObject.Find("CenterEyeAnchor");
                break;

            case VirtualRealiySystem.Vive:
                GameObject.Find("OculusTransformer").SetActive(false);
                vrCamera = GameObject.FindWithTag("ViveCamera");
                break;

            default:
                break;
        }
    }


    void Start()
    {
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<DataLogger>().ToLog.Add(vrCamera);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
