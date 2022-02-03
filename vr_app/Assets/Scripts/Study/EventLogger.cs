using System;
using System.IO;
using UnityEngine;
using static StudyConfig;

public class EventLogger : MonoBehaviour
{
    private string ParticipantID;
    private Condition InteractionMethod;

    private readonly string Delimiter = ",";
    private string _Directory;
    private string _FileName;
    StreamWriter sw;
    StudyConfig studyConfig;


    public enum LogEvent
    {
        NewInstruction,                             // instruction target
        CancelInstruction,                          // instruction
        FinishInstruction,                          // instruction
        CutEntity,                                  // blank target
        CopyEntity,                                 // blank target
        CancelBlankInteraction,                     // blank
        StartStudy,
        StopStudy,
        MovementAction
    }

    void Start()
    {
        ParticipantID = GetComponent<StudyConfig>().ParticipantID;
        InteractionMethod = GetComponent<StudyConfig>().condition;
        Config config = GameObject.FindGameObjectWithTag("GameManager").GetComponent<Config>();
        studyConfig = GameObject.FindGameObjectWithTag("GameManager").GetComponent<StudyConfig>();

        if (studyConfig.playground) return;

        // Use this for initialization
        _Directory = "study-remote-collab/";
        _FileName = config.hrriLocation + "_" + ParticipantID + "_" + InteractionMethod.ToString() + "_events_"+ Utils.GetTime() +".csv";
        if (!Directory.Exists(_Directory))
        {
            Directory.CreateDirectory(_Directory);
        }

        Debug.Log("Logging Directory: " + _Directory);
        string filePath = _Directory + _FileName;

        Debug.Log("Logging File Path: " + filePath);
        sw = new StreamWriter(filePath);

        string header = "timestamp" + Delimiter + "event_name" + Delimiter + "event_value";
        sw.WriteLine(header);
        sw.Flush();
    }

    public void AddLogEvent(LogEvent logEvent, string eventValue)
    {
        if (studyConfig.playground) return;

        long timestamp = Utils.GetTime();
        string row = timestamp + Delimiter + logEvent.ToString() + Delimiter + eventValue;
        sw.WriteLine(row);
        sw.Flush();
    }
}