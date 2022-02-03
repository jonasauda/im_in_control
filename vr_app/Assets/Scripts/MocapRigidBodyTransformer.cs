using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Config;
using System;

public class MocapRigidBodyTransformer : CorpseDetection
{
    public HrriLocation hrriLocation;
    public HrriGroup hrriGroup;
    public HrriObject hrriObject;
    public string hrriAttribute;
    private string hrri;
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private bool hasMoved = false;
    private EventManager eventManager;
    private bool isCorpse = false;

    public bool hideOnTrackingLoss = false;

    void Start()
    {
        Init();
        hrri = HrriUtil.CreateHrri(hrriLocation, hrriGroup, hrriObject, hrriAttribute);
        eventManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<EventManager>();
    }

    void Update()
    {
        transform.position = lastPosition;
        transform.rotation = lastRotation;

        if (hideOnTrackingLoss)
        {
            if (!isCorpse && IsCorpse())
            {
                isCorpse = true;
                OnCorpsed();
                //eventManager.FireOnRemoveRemoteObject(gameObject);
            }

            if (isCorpse && !IsCorpse())
            {
                isCorpse = false;
                OnRevived();
                //eventManager.FireOnNewRemoteObject(gameObject);
            }

            if (!transform.position.Equals(Vector3.zero))
            {
                hasMoved = true;
            }
        }
    }

    protected void OnCorpsed()
    {
        transform.localScale = new Vector3(0, 0, 0);
    }

    protected void OnRevived()
    {
        transform.localScale = new Vector3(1, 1, 1);
    }

    public string GetHrri()
    {
        return hrri;
    }

    public void SetHrri(string hrri)
    {
        this.hrri = hrri;
        (hrriLocation, hrriGroup, hrriObject, hrriAttribute) = HrriUtil.ParseHrri(this.hrri);
    }

    public void Transform(Vector3 position, Quaternion rotation)
    {
        lastRotation = rotation;
        lastPosition = position;
    }

    public bool getHasMoved()
    {
        return hasMoved;
    }
}
