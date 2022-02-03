using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StudyConfig : MonoBehaviour
{
    public enum Condition { UNDEFINED, VR_BASELINE, INSTRUCTIONS, CUT, COPY, INSTRUCTION_BY_CONTROLLER }

    public string ParticipantID = "Please Enter";
    public Condition condition = Condition.UNDEFINED;
    private bool studyRunning = false;

	[HideInInspector]
    public bool playground = false;

    EventManager eventManager;

    private void Start()
    {
        GameObject gameManager = GameObject.FindGameObjectWithTag("GameManager");
        eventManager = gameManager.GetComponent<EventManager>();
        eventManager.OnStartStudy += HandleOnStartStudy;
    }

    private void Update()
    {
        if (playground) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (studyRunning)
            {
                Debug.LogWarning("Stopped Study");
                eventManager.FireOnStopStudy(false);
            }
            else
            {
                Debug.LogWarning("Started Study");
                eventManager.FireOnStartStudy(false);
            }
        }
    }

    private void HandleOnStartStudy(bool remote)
    {
        studyRunning = true;
    }
}
