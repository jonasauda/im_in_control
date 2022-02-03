using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlankBehavior : MonoBehaviour
{

    private static readonly float INTERACTION_TIME_THRESHOLD = 1000f;
    private static readonly float RESET_IGNORE_TIME = 500f;
    private static readonly float RESET_IGNORE_DISTANCE = .05f;

    private StudyConfig studyConfig;
    private Config config;
    private EventManager eventManager;

    private GameObject collidingObject;
    private Vector3? initialRestPosition;
    private long? initalRestTimestamp;

    private long? beginCollisionTimestamp;
    private long startTimestamp;

    void Start()
    {
        var gameManager = GameObject.FindGameObjectWithTag("GameManager");
        config = gameManager.GetComponent<Config>();
        studyConfig = gameManager.GetComponent<StudyConfig>();
        eventManager = gameManager.GetComponent<EventManager>();

        initalRestTimestamp = Utils.GetTime();
        startTimestamp = Utils.GetTime();
    }

    private void Update()
    {
        long now = Utils.GetTime();

        if (initalRestTimestamp.HasValue)
        {
            if (now - initalRestTimestamp.Value > RESET_IGNORE_TIME)
            {
                initalRestTimestamp = null;
                initialRestPosition = transform.position;
            }
            return;
        }
        else if (initialRestPosition.HasValue)
        {
            var diffVector = transform.position - initialRestPosition.Value;
            float distance = diffVector.magnitude;
            if (distance > RESET_IGNORE_DISTANCE)
            {
                initalRestTimestamp = null;
                initialRestPosition = null;
            }
            return;
        }

        if (collidingObject)
        {
            if (!beginCollisionTimestamp.HasValue)
            {
                beginCollisionTimestamp = now;
            }
            else if (beginCollisionTimestamp.HasValue && now - beginCollisionTimestamp > INTERACTION_TIME_THRESHOLD)
            {
                StartInteraction();
                collidingObject = null;
                beginCollisionTimestamp = null;
                initalRestTimestamp = Utils.GetTime();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPhysicalObject(other.gameObject)) return;
        collidingObject = other.gameObject;
        beginCollisionTimestamp = Utils.GetTime();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject != collidingObject) return;
        collidingObject = null;
        beginCollisionTimestamp = null;
    }

    private bool IsPhysicalObject(GameObject go)
    {
        HrriComponent hrriComponent = go.GetComponent<HrriComponent>();
        if (!hrriComponent) return false;

        var (l, g, o, a) = HrriUtil.ParseHrri(hrriComponent.hrri);
        return o == Config.HrriObject.OBJ;
    }

    private void StartInteraction()
    {
        Debug.LogWarning("BlankBehavior.StartInteraction");
        eventManager.FireOnStartBlankInteraction(gameObject, collidingObject);
    }
}
