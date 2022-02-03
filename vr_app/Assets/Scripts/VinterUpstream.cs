using UnityEngine;
using Aristo;

using System;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using VinteR.Model.Gen;
using Google.Protobuf;
using static Config;
using static System.Linq.Enumerable;
using static VinteR.Model.Gen.MocapFrame.Types;


public class VinterUpstream : MonoBehaviour
{
    public bool handLeftTransmissionIsEnabled = true;
    public bool handRightTransmissionIsEnabled = true;
    public bool controllerTransmissionIsEnabled;
    public GameObject controllerLeft;
    public GameObject controllerRight;

    private string vinterIp;
    private int vinterPort;
    private IPEndPoint remoteEndPoint;
    private UdpClient client;

    private HrriLocation hrriLocation = HrriLocation.NONE;
    private HrriGroup hrriGroup = HrriGroup.NONE;

    public bool measureRoundTripTime = false;

    private Config config;
    EventManager eventManager;

    // Start is called before the first frame update
    void Start()
    {
        config = gameObject.GetComponent<Config>();
        eventManager = gameObject.GetComponent<EventManager>();
        vinterIp = config.vinterIp;
        vinterPort = config.vinterPort;
        hrriLocation = config.hrriLocation;
        hrriGroup = config.hrriGroup;
        remoteEndPoint = new IPEndPoint(IPAddress.Parse(vinterIp), vinterPort);
        client = new UdpClient();
        Debug.Log("created UDP client for socket address " + vinterIp + ":" + vinterPort);

        config = gameObject.GetComponent<Config>();
        eventManager.OnStartBlankInteraction += HandleOnStartBlankInteraction;
        eventManager.OnStartStudy += HandleOnStartStudy;
        eventManager.OnStopStudy += HandleOnStopStudy;
    }

    void Update()
    {
        List<Body> bodies = new List<Body>();

        // hands
        if (handLeftTransmissionIsEnabled || handRightTransmissionIsEnabled)
        {
            List<Body> handBodies = CreateHandTrackingData();
            bodies.AddRange(handBodies);
        }

        // controller
        if (controllerTransmissionIsEnabled)
        {

            Vector3 leftPosition = HoloRoomUtil.convertFromUnityToHoloRoom(controllerLeft.transform.position);
            Quaternion leftRotation = HoloRoomUtil.convertRotation(controllerLeft.transform.rotation);
            Vector3 rightPosition = HoloRoomUtil.convertFromUnityToHoloRoom(controllerRight.transform.position);
            Quaternion rightRotation = HoloRoomUtil.convertRotation(controllerRight.transform.rotation);

            bodies.AddRange(CreateControllerData(leftPosition, leftRotation, rightPosition, rightRotation));

        }

        // interactable game objects
        GameObject upstreamObject = GameObject.FindGameObjectWithTag("Upstream");
        InteractableHrriGameObject[] hrriObjects = upstreamObject.GetComponentsInChildren<InteractableHrriGameObject>();
        foreach (InteractableHrriGameObject hrriObject in hrriObjects)
        {
            Vector3 position = HoloRoomUtil.convertFromUnityToHoloRoom(hrriObject.gameObject.transform.position);
            Quaternion rotation = HoloRoomUtil.convertRotation(hrriObject.gameObject.transform.rotation);
            String hrri = HrriUtil.CreateHrri(hrriLocation, hrriGroup, hrriObject.hrriObject, hrriObject.hrriAttribute);
            bodies.Add(CreateSingleObjectData(position, rotation, hrri));
        }

        // fake joints
        GameObject[] controllers = GameObject.FindGameObjectsWithTag("Controller");
        foreach (GameObject currController in controllers)
        {
            FakeJoint fakeJoint = currController.GetComponent<FakeJoint>();
            if (fakeJoint != null)
            {
                Vector3 position = HoloRoomUtil.convertFromUnityToHoloRoom(currController.transform.position);
                Quaternion rotation = HoloRoomUtil.convertRotation(currController.transform.rotation);
                bodies.Add(CreateFakeJointData(position, rotation, fakeJoint));
            }
        }

        Task.Run(() =>
        {
            MocapFrame mocapFrame = new MocapFrame
            {
                AdapterType = "HoloRoom",
                SourceId = "Unity",
                Bodies = {
                    bodies
                }
            };

            SendMocapFrameToVinter(mocapFrame);

        });

        if (measureRoundTripTime)
        {
            Task.Run(() =>
            {
                MocapFrame mocapFrame = new MocapFrame
                {
                    AdapterType = "HoloRoom",
                    SourceId = "Unity",
                    Bodies = {
                        CreatePing()
                    }
                };
                Debug.Log("ping sent");
                measureRoundTripTime = false;

                SendMocapFrameToVinter(mocapFrame);
            });
        }

    }

    private List<Body> CreateHandTrackingData()
    {
        List<Body> bodies = new List<Body>();
        if (GestureProvider.UpdatedInThisFrame)
        {
            GestureResult lefthand = GestureProvider.LeftHand;
            if (lefthand != null && handLeftTransmissionIsEnabled)
            {
                bodies.Add(CreateSingleHandData(lefthand, true));
            }
            GestureResult righthand = GestureProvider.RightHand;
            if (righthand != null && handRightTransmissionIsEnabled)
            {
                bodies.Add(CreateSingleHandData(righthand, false));
            }
        }
        return bodies;
    }

    private Body CreateSingleHandData(GestureResult hand, bool isLeft)
    {
        return HandConverter.convert(hand, isLeft, hrriLocation, hrriGroup);
    }

    private List<Body> CreateControllerData(Vector3 positionLeft, Quaternion rotationLeft, Vector3 positionRight, Quaternion rotationRight)
    {
        List<Body> bodies = new List<Body>();
        bodies.Add(CreateSingleControllerData(positionLeft, rotationLeft, "LEFT"));
        bodies.Add(CreateSingleControllerData(positionRight, rotationRight, "RIGHT"));
        return bodies;
    }

    private Body CreateSingleControllerData(Vector3 position, Quaternion rotation, String handedNess)
    {
        return TransformConverter.convert(position, rotation, HrriUtil.CreateHrri(hrriLocation, hrriGroup, HrriObject.CONTROLLER, handedNess));
    }

    private Body CreateSingleObjectData(Vector3 position, Quaternion rotation, String hrri)
    {
        return TransformConverter.convert(position, rotation, hrri);
    }

    public void SendFakeJointDestroyEvent(string fakeJointHrri)
    {
        MocapFrame mocapFrame = FakeJointConverter.createDestroyEvent(fakeJointHrri);
        SendMocapFrameToVinter(mocapFrame);
    }

    private Body CreateFakeJointData(Vector3 position, Quaternion rotation, FakeJoint fakeJoint)
    {
        return FakeJointConverter.convert(position, rotation, fakeJoint.GetHrri(), fakeJoint.GetConnectedHrri());
    }

    private void SendMocapFrameToVinter(MocapFrame mocapFrame)
    {
        byte[] mocapData = ((IMessage)mocapFrame).ToByteArray();
        SendBytesToVinter(mocapData);
    }

    private void SendBytesToVinter(byte[] data)
    {
        try
        {
            client.Send(data, data.Length, remoteEndPoint);
        }
        catch (Exception e)
        {
            Debug.Log("Error while sending data as UDP: " + e.ToString());
        }
    }

    private Body CreatePing()
    {
        return TransformConverter.convert(new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0), Config.pingHrri);
    }

    public void SendEvent(GameEvent gameEvent)
    {
        var eventBody = GameEvent.Serialize(gameEvent);
        eventBody.Name = HrriUtil.CreateHrri(config.hrriLocation, config.hrriGroup, HrriObject.EVENT, null);
        MocapFrame mocapFrame = new MocapFrame
        {
            AdapterType = "holoroom",
            SourceId = "holoroom",
            Bodies = { eventBody }
        };

        Task.Run(async () =>
        {
            //Debug.Log("sending event: " + mocapFrame);

            // event will be sent in quadratically growing intervals until 16 seconds after occurence
            foreach (int i in Range(0, 13))
            {
                await Task.Delay(i * i);
                SendMocapFrameToVinter(mocapFrame);
            }
        });
    }

    private void HandleOnStartBlankInteraction(GameObject blank, GameObject target)
    {
        if (blank.GetComponent<ObjectRenderer>().location != ObjectRenderer.Location.LOCAL) return;

        List<string> data = new List<string>();
        data.Add(Utils.GetHrri(blank));
        data.Add(Utils.GetHrri(target));
        SendEvent(new GameEvent(GameEvent.EventType.BlankInteraction, data));
    }

    private void HandleOnStartStudy(bool remote)
    {
        if (remote)
        {
            return;
        }

        List<string> data = new List<string>();
        SendEvent(new GameEvent(GameEvent.EventType.StartStudy, data));
    }

    private void HandleOnStopStudy(bool remote)
    {
        if (remote)
        {
            return;
        }
        List<string> data = new List<string>();
        SendEvent(new GameEvent(GameEvent.EventType.StopStudy, data));
    }
}
