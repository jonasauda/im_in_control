using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Config : MonoBehaviour
{

    public enum HrriLocation
    {
        NONE, LOCATION2, LOCATION1
    }

    public enum HrriGroup
    {
        NONE, BLUE, RED, YELLOW, ENVIRONMENT
    }

    public enum HrriObject
    {
        NONE, HMD, HAND, SKELETON, OBJ01, OBJ02, CONTROLLER, TABLE, CUBEINTERACTABLE, SPHEREINTERACTABLE, FAKEJOINT, PING, MUG, MUG_INSCTRUCTION, OBJ, CIRCUIT_INSTRUCTION, EVENT
    }

    public static Dictionary<HrriObject, HrriObject> HrriObjectToInstructionObject = new Dictionary<HrriObject, HrriObject>()
    {
        { HrriObject.MUG, HrriObject.MUG_INSCTRUCTION },
        { HrriObject.OBJ, HrriObject.CIRCUIT_INSTRUCTION }
    };
    public static Dictionary<HrriObject, HrriObject> InstructionObjectToHrriObject = HrriObjectToInstructionObject.ToDictionary((i) => i.Value, (i) => i.Key);

    public static string pingHrri = "REMOTE-YELLOW-PING";

    [HideInInspector]
    public HrriLocation hrriLocation;

    [HideInInspector]
    public HrriGroup hrriGroup;

    [HideInInspector]
    public string vinterIp = "127.0.0.1";

    [HideInInspector]
    public int vinterPort = 7282;

    [HideInInspector]
    public int clientPort = 2020;

    public bool enableSelfInstructions = false;

    private void Awake()
    {

//#if UNITY_EDITOR
//        UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
//#endif

        LoadConfig();
    }

    private void LoadConfig()
    {
        ConfigLoader.ClientConfiguration config = ConfigLoader.GetConfig();
        if (config.Enabled)
        {
            var hrriElements = HrriUtil.ParseHrri(config.ClientHrri);
            this.hrriLocation = hrriElements.Item1;
            this.hrriGroup = hrriElements.Item2;
            this.vinterIp = config.ServerIp;
            this.vinterPort = config.ServerPort;
            this.clientPort = config.ClientPort;
        }
    }
}
