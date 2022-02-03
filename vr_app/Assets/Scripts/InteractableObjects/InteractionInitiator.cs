using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using System.Linq;

public class InteractionInitiator : MonoBehaviour
{
    public SteamVR_Action_Boolean grabAction;
    private SteamVR_Behaviour_Pose pose;
    private List<GameObject> collidedInteractables = new List<GameObject>();
 
    void Awake()
    {
        pose = GetComponent<SteamVR_Behaviour_Pose>();
    }

    void Start() {}

    void Update()
    {
        if (grabAction != null) {
            if (grabAction.GetStateDown(pose.inputSource))
            {
                collidedInteractables.ForEach(go => {
                    Interactable interactable = go.GetComponent<Interactable>();
                    if (interactable != null)
                    {
                        interactable.StartInteraction();
                    }
                });
            }
            if (grabAction.GetStateUp(pose.inputSource))
            {
                collidedInteractables.ForEach(go => {
                    Interactable interactable = go.GetComponent<Interactable>();
                    if (interactable != null)
                    {
                        interactable.StopInteraction();
                    }
                });
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Interactable")) {
            collidedInteractables.Add(other.gameObject);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        collidedInteractables.Remove(other.gameObject);
    }
}
