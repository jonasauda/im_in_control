using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static StudyConfig;

public class AudioLogger : MonoBehaviour
{

    AudioClip myAudioClip;
    private string _Directory;
    private string _FileName;

    void Start()
    {
        string ParticipantID = GetComponent<StudyConfig>().ParticipantID;
        Condition InteractionMethod = GetComponent<StudyConfig>().condition;
        Config config = GameObject.FindGameObjectWithTag("GameManager").GetComponent<Config>();
        EventManager eventManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<EventManager>();
        eventManager.OnStartStudy += HandleOnStartStudy;
        eventManager.OnStopStudy += HandleOnStopStudy;

        _Directory = "study-remote-collab/";
        _FileName = config.hrriLocation + "_" + ParticipantID + "_" + InteractionMethod.ToString() + "_audio_" + Utils.GetTime() + ".wav";
        if (!Directory.Exists(_Directory))
        {
            Directory.CreateDirectory(_Directory);
        }
    }

    void Update() { }

    void HandleOnStartStudy(bool remote)
    {
        myAudioClip = Microphone.Start(null, false, 600, 44100);
    }

    void HandleOnStopStudy(bool remote)
    {
        string filePath = _Directory + _FileName;
        Debug.Log("Saving audio to " + filePath);
        SaveWav.Save(filePath, myAudioClip);
    }
}