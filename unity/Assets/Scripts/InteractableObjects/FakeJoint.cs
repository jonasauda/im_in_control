using UnityEngine;

public class FakeJoint : MonoBehaviour
{
    private string connectedHrri;

    private static int counter = 1;

    // HRRI for FakeJoints are like MUC-BLUE-FAKEJOINT-1
    private string hrri;

    void Start()
    {
        
    }

    public void Init(GameObject connectedTo)
    {
        HrriComponent hrriComponent = connectedTo.GetComponent<HrriComponent>();
        connectedHrri = hrriComponent.hrri;
        Config config = GameObject.FindGameObjectWithTag("GameManager").GetComponent<Config>();
        hrri = HrriUtil.CreateHrri(config.hrriLocation, config.hrriGroup, Config.HrriObject.FAKEJOINT, counter + "");
        counter += 1;
    }

    public string GetHrri()
    {
        return hrri;
    }

    public string GetConnectedHrri()
    {
        return connectedHrri;
    }


}
