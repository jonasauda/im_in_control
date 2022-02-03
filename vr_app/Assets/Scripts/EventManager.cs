using System;
using System.Collections.Generic;
using UnityEngine;

class EventManager : MonoBehaviour
{
    public Dictionary<string, GameObject> hrriToGameObject = new Dictionary<string, GameObject>();

    public Config config { get; private set; }
    public EventLogger eventLogger { get; private set; }
    public DataLogger dataLogger { get; private set; }

    void Awake()
    {
        config = gameObject.GetComponent<Config>();
        eventLogger = gameObject.GetComponent<EventLogger>();
        dataLogger = gameObject.GetComponent<DataLogger>();
    }

    public delegate void OnNewRemoteObjectDelegate(GameObject go);
    public event OnNewRemoteObjectDelegate OnNewRemoteObject;
    public void FireOnNewRemoteObject(GameObject go)
    {
        OnNewRemoteObject(go);
        dataLogger.ToLog.Add(go);
    }

    public delegate void OnStartBlankInteractionDelegate(GameObject blank, GameObject target);
    public event OnStartBlankInteractionDelegate OnStartBlankInteraction;
    public void FireOnStartBlankInteraction(GameObject blank, GameObject target)
    {
        OnStartBlankInteraction(blank, target);
    }

    public delegate void OnRemoteEventDelegate(GameEvent gameEvent);
    public event OnRemoteEventDelegate OnRemoteEvent;
    public void FireOnRemoteEvent(GameEvent gameEvent)
    {
        OnRemoteEvent(gameEvent);
    }

    public delegate void OnStartStudyDelegate(bool remote);
    public event OnStartStudyDelegate OnStartStudy;
    public void FireOnStartStudy(bool remote)
    {
        OnStartStudy(remote);
        eventLogger.AddLogEvent(EventLogger.LogEvent.StartStudy, "");
    }

    public delegate void OnStopStudyDelegate(bool remote);
    public event OnStopStudyDelegate OnStopStudy;
    public void FireOnStopStudy(bool remote)
    {
        OnStopStudy(remote);
        eventLogger.AddLogEvent(EventLogger.LogEvent.StopStudy, "");
    }
}