using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using System.Linq;

public class GrabInitiator : MonoBehaviour
{
    public static string INSTRUCTION_POSTFIX = "_INSTRUCTION";

    public SteamVR_Action_Boolean grabAction;
    private SteamVR_Behaviour_Pose pose;
    private List<GameObject> collidedInteractables = new List<GameObject>();
    private GameObject grabbedObject;

    private InstructionManager instructionManager;
    private EventManager eventManager;
    private StudyConfig studyConfig;

    void Awake()
    {
        pose = GetComponent<SteamVR_Behaviour_Pose>();
    }

    void Start()
    {
        GameObject gameManager = GameObject.FindGameObjectWithTag("GameManager");
        instructionManager = gameManager.GetComponent<InstructionManager>();
        eventManager = gameManager.GetComponent<EventManager>();
        studyConfig = gameManager.GetComponent<StudyConfig>();

    }

    void Update()
    {
        if (studyConfig.condition == StudyConfig.Condition.INSTRUCTION_BY_CONTROLLER)
        {
            // Oculus controller 
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
            {
                Debug.Log("Pickup!");
                Pickup();
            }
            if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch) || OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
            {
                Debug.Log("Drop!");
                Drop();
            }

            // Vive controller 
            if (grabAction != null && pose != null)
            {
                if (grabAction.GetStateDown(pose.inputSource))
                {
                    Pickup();
                }
                if (grabAction.GetStateUp(pose.inputSource))
                {
                    Drop();
                }
            }
        }

    }

    private void Pickup()
    {
        GameObject nearestObject = GetNearestCollidedObject();
        if (nearestObject != null && grabbedObject == null)
        {
            Interact(nearestObject);
        }
    }

    private void Interact(GameObject target)
    {
        Debug.Log("CreateJoint target: " + target);
        if (target.CompareTag("Tangible"))
        {
            grabbedObject = target;
            connectWithJoint(gameObject, grabbedObject);
        }

        if (target.CompareTag("MocapRigidBody"))
        {
            Debug.Log("grab extern object");

            InstructibleForeignRigidBody instructibleForeignRigidBody = target.GetComponent<InstructibleForeignRigidBody>();
            if (instructibleForeignRigidBody != null)
            {
                Debug.Log("instructed object is " + target.name);

                GameObject instructionObject = instructionManager.CreateLocalInstructionObject(target);
                grabbedObject = instructionObject;
                connectWithJoint(gameObject, instructionObject);
            }
            else
            {
                grabbedObject = target;
                GrabableForeignRigidBody isGrabable = grabbedObject.GetComponent<GrabableForeignRigidBody>();
                if (isGrabable != null)
                {
                    Debug.Log("grabbedObject is " + grabbedObject.name);
                    FakeJoint fk = gameObject.AddComponent<FakeJoint>();
                    fk.Init(grabbedObject);
                }
            }
        }
    }

    private static void connectWithJoint(GameObject o1, GameObject o2)
    {
        FixedJoint fx = o1.AddComponent<FixedJoint>();
        fx.breakForce = 20000;
        fx.breakTorque = 20000;
        fx.connectedBody = o2.GetComponent<Rigidbody>();
    }

    private void Drop()
    {
        RemoveJoint();
        grabbedObject = null;

    }

    private void RemoveJoint()
    {
        FixedJoint joint = GetComponent<FixedJoint>();
        if (joint)
        {
            joint.connectedBody = null;
            Destroy(joint);
            if (grabbedObject != null)
            {
                var rb = grabbedObject.GetComponent<Rigidbody>();
                rb.velocity = pose.GetVelocity();
                rb.angularVelocity = pose.GetAngularVelocity();
            }
        }

        FakeJoint fakeJoint = GetComponent<FakeJoint>();
        if (fakeJoint)
        {
            GameObject.FindGameObjectWithTag("GameManager").GetComponent<VinterUpstream>().SendFakeJointDestroyEvent(fakeJoint.GetHrri());
            Destroy(fakeJoint);
        }
    }

    private GameObject GetNearestCollidedObject()
    {
        if (collidedInteractables.Count <= 0)
            return null;

        var orderedCollidedInteractables = collidedInteractables
            .Where(o => o != null)
            .Select(o => (distance: (o.transform.position - transform.position).sqrMagnitude, go: o))
            .OrderBy(tuple => tuple.distance);

        return orderedCollidedInteractables.Count() == 0 ? null : orderedCollidedInteractables.First().go;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Tangible") || other.gameObject.CompareTag("MocapRigidBody"))
        {
            collidedInteractables.Add(other.gameObject);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        collidedInteractables.Remove(other.gameObject);
    }
}
