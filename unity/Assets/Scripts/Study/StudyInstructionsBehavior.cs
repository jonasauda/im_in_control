using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StudyInstructionsBehavior : MonoBehaviour
{
    public Config.HrriLocation location;

    void Awake()
    {
        var gameMangager = GameObject.FindGameObjectWithTag("GameManager");
        var config = gameMangager.GetComponent<Config>();
        var studyConfig = gameMangager.GetComponent<StudyConfig>();
        if (location != config.hrriLocation || studyConfig.playground)
        {
            gameObject.SetActive(false);
        }
    }

}
