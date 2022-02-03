using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using VinteR.Model.Gen;
using static Config;

public class NetworkManager : MonoBehaviour
{

    private IPEndPoint OptiTRackEndPoint;
    private UdpClient OptiTrackClient;
    private Thread OptiTrackListener;
    private HrriUtil.HrriMask hrriMask;

    public List<HoloRoomEntity> holoRoomEntities;

    private Dictionary<string, FixedJointPlaceholder> hrriToFixedJointPlaceholders = new Dictionary<string, FixedJointPlaceholder>();

    private List<MocapRigidBodyTransformer> mocapRigidBodies = new List<MocapRigidBodyTransformer>();
    private List<String> jointsToBeDestroyed = new List<String>();

    public HrriObject fakeJointDetection;

    public String cameraHrriAttribute;

    public GameObject cameraWrapper;

    private EventManager eventManager;
    private StudyConfig studyConfig;
    private DataLogger dataLogger;


    // Use this for initialization
    void Start()
    {
        GameObject gameManager = GameObject.FindGameObjectWithTag("GameManager");
        eventManager = gameManager.GetComponent<EventManager>();


        Config config = eventManager.config;
        hrriMask = new HrriUtil.HrriMask(config.hrriLocation, config.hrriGroup);
        OptiTRackEndPoint = new IPEndPoint(IPAddress.Any, config.clientPort);
        studyConfig = gameManager.GetComponent<StudyConfig>();
        dataLogger = gameManager.GetComponent<DataLogger>();

        Debug.Log("Starting OptiTrack Listener...");
        OptiTrackClient = new UdpClient(OptiTRackEndPoint);
        OptiTrackListener = new Thread(new ThreadStart(ReceiveOptiTrackData));
        OptiTrackListener.IsBackground = true;
        OptiTrackListener.Start();
        Debug.Log("Listening on port " + config.clientPort);
    }

    void LateUpdate()
    {
        mocapRigidBodies = GameObject.FindGameObjectsWithTag("MocapRigidBody").Select(mocapRigidBody => mocapRigidBody.gameObject.GetComponent<MocapRigidBodyTransformer>()).ToList();
        InstantiateGameObjects();
        InstantiateFakeJoints();
        RemoveUnusedJoints();
        FirePendingRemoteEvents();
    }

    private void InstantiateGameObjects()
    {
        List<String> keys = eventManager.hrriToGameObject.Keys.ToList();
        foreach (string hrri in keys)
        {
            var go = eventManager.hrriToGameObject[hrri];
            if (go == null)
            {
                // find prefab for HRRI object and HRRI attribute
                GameObject downstreamGo = CreateDownstreamGameObject(hrri);
                Debug.LogWarning("instantiating new remote object");
                eventManager.hrriToGameObject[hrri] = downstreamGo;
                eventManager.FireOnNewRemoteObject(downstreamGo);
            }

        }
    }

    private GameObject CreateDownstreamGameObject(String hrri)
    {
        GameObject go = null;
        (HrriLocation hrriLocation, HrriGroup HrriGroup, HrriObject hrriObject, String hrriAttribute) = HrriUtil.ParseHrri(hrri);
        HoloRoomEntity matchingEntity = holoRoomEntities.Find((entity) => EntityHrriMatchesIncomingHrri(entity, hrriObject, hrriAttribute));
        if (matchingEntity != null)
        {

            go = Instantiate(matchingEntity.gameObject);
            go.transform.parent = GameObject.FindGameObjectWithTag("Downstream").transform;
            HrriComponent hrriComponent = go.AddComponent<HrriComponent>();
            hrriComponent.hrri = hrri;
            Debug.Log("Created " + go.name + " object for key with HRRI: " + hrri);
            var mrbt = go.AddComponent<MocapRigidBodyTransformer>();
            mrbt.SetHrri(hrri);
            go.tag = "MocapRigidBody";
            ObjectRenderer or = go.GetComponent<ObjectRenderer>();
            if (or && HrriGroup == HrriGroup.ENVIRONMENT)
            {
                or.SetPhysical();
            }
            if (hrriObject == HrriObject.CIRCUIT_INSTRUCTION)
            {
                mrbt.hideOnTrackingLoss = true;
            }
            if (matchingEntity.hrriObject == HrriObject.HAND)
            {
                dataLogger.ToLog.Add(go);
            }
        }
        else
        {
            Debug.LogWarning("Could not find configured Prefab for " + hrri);
        }
        return go;
    }

    private bool EntityHrriMatchesIncomingHrri(HoloRoomEntity entity, HrriObject hrriObject, string hrriAttribute)
    {
        return hrriObject == entity.hrriObject
                && (hrriAttribute == entity.hrriAttribute || entity.hrriAttribute == "*");
    }

    private void InstantiateFakeJoints()
    {
        List<String> keys = hrriToFixedJointPlaceholders.Keys.ToList();
        foreach (string hrri in keys)
        {
            FixedJointPlaceholder fixedJointPlaceholder = hrriToFixedJointPlaceholders[hrri];
            if (fixedJointPlaceholder.hasFixedJoint) continue;
            String connectedHrri = fixedJointPlaceholder.connecetdHrri;
            InteractableHrriGameObject[] interactables = GameObject.FindGameObjectWithTag("Upstream").GetComponentsInChildren<InteractableHrriGameObject>();
            (HrriLocation hrriLocation, HrriGroup HrriGroup, HrriObject hrriObject, String hrriAttribute) = HrriUtil.ParseHrri(connectedHrri);
            foreach (InteractableHrriGameObject interactable in interactables)
            {
                if (interactable.hrriObject == hrriObject && interactable.hrriAttribute == hrriAttribute)
                {
                    GameObject downstreamGo = eventManager.hrriToGameObject[hrri];
                    MocapRigidBodyTransformer mocapRigidBodyTransformer = downstreamGo.GetComponent<MocapRigidBodyTransformer>();
                    // FakeJoint GameObject has to be moved at least once as otherwise the joint will be bound to the origin, leaving a gap between the FakeJointObject and the interactable object
                    if (mocapRigidBodyTransformer != null && mocapRigidBodyTransformer.getHasMoved())
                    {
                        FixedJoint fx = downstreamGo.AddComponent<FixedJoint>();
                        fx.breakForce = 20000;
                        fx.breakTorque = 20000;
                        fx.connectedBody = interactable.gameObject.GetComponentInChildren<Rigidbody>();
                        fixedJointPlaceholder.hasFixedJoint = true;
                        Debug.Log("Created fixed joint (" + hrri + " ~ " + connectedHrri + ")");
                    }
                    break;
                }
            }
        }
    }

    private void RemoveUnusedJoints()
    {
        foreach (String hrri in jointsToBeDestroyed)
        {
            if (eventManager.hrriToGameObject.Keys.Contains(hrri))
            {
                GameObject jointToDestroy = eventManager.hrriToGameObject[hrri];
                FixedJoint fixedJoint = jointToDestroy.GetComponent<FixedJoint>();
                FakeRigidBody rigidBody = jointToDestroy.GetComponentInChildren<FakeRigidBody>();
                GameObject connectedGo = fixedJoint.connectedBody.gameObject;
                Vector3 velocity = rigidBody.velocity;
                Destroy(jointToDestroy);
                connectedGo.GetComponentInChildren<Rigidbody>().velocity = velocity;
                eventManager.hrriToGameObject.Remove(hrri);
                hrriToFixedJointPlaceholders.Remove(hrri);
                Debug.Log("Destroyed FakeJoint " + hrri);
            }
        }
        jointsToBeDestroyed.Clear();
    }

    void ReceiveOptiTrackData()
    {
        while (true)
        {
            try
            {
                var data = OptiTrackClient.Receive(ref OptiTRackEndPoint);
                var mocapFrame = MocapFrame.Parser.ParseFrom(data);
                ParseMocapFrame(mocapFrame);
            }
            catch (Exception e)
            {
                Debug.Log("Receive data error " + e.Message);
                Debug.Log(e.StackTrace);
                OptiTrackClient.Close();
                return;
            }
        }

    }

    private void ParseMocapFrame(MocapFrame mocapFrame)
    {
        if (mocapFrame != null)
        {
            foreach (MocapFrame.Types.Body body in mocapFrame.Bodies)
            {

                if (body.Name == "")
                {
                    Debug.LogWarning("received body without name");
                    continue;
                }

                if (HrriUtil.MaskFitsHrri(hrriMask, body.Name))
                {
                    Debug.LogWarning("received local body in frame: " + body.Name);
                    continue;
                }

                if (DetectEvent(body))
                {
                    continue;
                }

                //Debug.Log("received body: " + body.Name);
                Vector3 vec = HoloRoomUtil.convertFromHoloRoomToUnity(new Vector3(body.Centroid.X, body.Centroid.Y, body.Centroid.Z));
                Quaternion rot = HoloRoomUtil.convertRotation(new Quaternion(body.Rotation.X, body.Rotation.Y, body.Rotation.Z, body.Rotation.W));
                mocapRigidBodies.ForEach((MocapRigidBodyTransformer mocapRigidBodyTransformer) =>
                {
                    if (mocapRigidBodyTransformer.GetHrri().StartsWith(body.Name))
                    {
                        mocapRigidBodyTransformer.UpdateTime();
                        mocapRigidBodyTransformer.Transform(vec, rot);
                    }
                });

                if (HrriUtil.HrriObjectEquals(body.Name, HrriUtil.HrriObjectToString(fakeJointDetection)))
                {
                    if (FakeJointConverter.isDeleteEvent(body))
                    {
                        // if mocapframe is detected as remove event store the object has to be removed
                        jointsToBeDestroyed.Add(body.Name);

                    }

                }

                if (body.Name == Config.pingHrri)
                {
                    PingLogger.pingReceived();
                }

                // real cloning of prefab is done in LateUpdate, because it cannot be done from background thread
                CreateNewGameObject(body);
            }
        }
    }

    private void CreateNewGameObject(MocapFrame.Types.Body body)
    {
        if (HrriUtil.HrriObjectEquals(body.Name, HrriUtil.HrriObjectToString(fakeJointDetection)))
        {
            FixedJointPlaceholder fixedJointPlaceholder = FakeJointConverter.convert(body);
            if (fixedJointPlaceholder != null)
            {
                CreateNewFixedJointPlaceholder(fixedJointPlaceholder);
                CreateNewHoloRoomEntity(body.Name);
            }
        }
        else if (HrriUtil.IsWellFormedHrri(body.Name))
        {
            CreateNewHoloRoomEntity(body.Name);
        }
    }

    private void CreateNewHoloRoomEntity(String hrri)
    {
        if (!eventManager.hrriToGameObject.ContainsKey(hrri))
        {
            eventManager.hrriToGameObject[hrri] = null;
        }
    }

    private void CreateNewFixedJointPlaceholder(FixedJointPlaceholder fixedJointPlaceholder)
    {
        if (!hrriToFixedJointPlaceholders.ContainsKey(fixedJointPlaceholder.hrri))
        {
            hrriToFixedJointPlaceholders[fixedJointPlaceholder.hrri] = fixedJointPlaceholder;
        }
    }

    private bool DetectEvent(MocapFrame.Types.Body body)
    {
        var (l, g, o, a) = HrriUtil.ParseHrri(body.Name);
        if (o != HrriObject.EVENT)
        {
            return false;
        }
        GameEvent e = GameEvent.Parse(body);
        if (IsNewEvent(e))
        {
            pendingRemoteEvents.Enqueue(e);
        }
        return true;
    }

    private void FirePendingRemoteEvents()
    {
        while (pendingRemoteEvents.Count > 0)
        {
            pendingRemoteEvents.TryDequeue(out GameEvent gameEvent);
            eventManager.FireOnRemoteEvent(gameEvent);
        }
    }

    ConcurrentQueue<GameEvent> pendingRemoteEvents = new ConcurrentQueue<GameEvent>();
    HashSet<string> knownEventIds = new HashSet<string>();
    private bool IsNewEvent(GameEvent e)
    {
        if (knownEventIds.Contains(e.ID))
        {
            return false;
        }
        knownEventIds.Add(e.ID);
        return true;
    }
}
