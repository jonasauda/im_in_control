using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static StudyConfig;

public class DataLogger : MonoBehaviour
{

    private readonly string Delimiter = ",";
    private string _Directory;
    private string _FileName;
    StreamWriter sw;


    public List<GameObject> ToLog = new List<GameObject>();

    void Start()
    {

        string ParticipantID = GetComponent<StudyConfig>().ParticipantID;
        Condition InteractionMethod = GetComponent<StudyConfig>().condition;
        Config config = GameObject.FindGameObjectWithTag("GameManager").GetComponent<Config>();


        _Directory = "study-remote-collab/";
        _FileName = config.hrriLocation + "_" + ParticipantID + "_" + InteractionMethod.ToString() + "_paths_" + Utils.GetTime() + ".csv";
        if (!Directory.Exists(_Directory))
        {
            Directory.CreateDirectory(_Directory);
        }

        Debug.Log("Logging Directory: " + _Directory);
        string filePath = _Directory + _FileName;

        Debug.Log("Logging File Path: " + filePath);
        sw = new StreamWriter(filePath);

        string header =
            "timestamp" + Delimiter +
            "gameobject_name" + Delimiter +
            "pos_x" + Delimiter +
            "pos_y" + Delimiter +
            "pos_z" + Delimiter +
            "euler_x" + Delimiter +
            "euler_y" + Delimiter +
            "euler_z";

        sw.WriteLine(header);
        sw.Flush();
    }
    void Update()
    {
        long timestamp = Utils.GetTime();
        foreach (GameObject gameObject in ToLog)
        {
            Log(gameObject, timestamp);
        }
    }

    public void Log(GameObject gameObject, long timestamp)
    {
        if (gameObject)
        {
            Vector3 pos = gameObject.transform.position;
            Vector3 eulerAngles = gameObject.transform.rotation.eulerAngles;
            HrriComponent hrriComponent = gameObject.GetComponent<HrriComponent>();
            string name = hrriComponent ? hrriComponent.hrri : gameObject.name;
            string row =
                timestamp + Delimiter +
                name + Delimiter +
                pos.x + Delimiter +
                pos.y + Delimiter +
                pos.z + Delimiter +
                eulerAngles.x + Delimiter +
                eulerAngles.y + Delimiter +
                eulerAngles.z;
            //Debug.Log("logged: " + row);
            sw.WriteLine(row);
            sw.Flush();
        }
    }

}
