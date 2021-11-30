using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandReachesController : MonoBehaviour
{
    public bool isLeft = false;
    private int collideCounter = 0;
    private HandRenderer handRenderer;
    private VinterUpstream vinterUpstream;

    // Start is called before the first frame update
    void Start()
    {
        GameObject gameManager = GameObject.FindGameObjectWithTag("GameManager");
        vinterUpstream = gameManager.GetComponent<VinterUpstream>();
        handRenderer = gameObject.GetComponent<HandRenderer>();
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag.Contains("Controller")) {
            collideCounter += 1;
            handRenderer.HideHand();
            if (isLeft) {
                vinterUpstream.handLeftTransmissionIsEnabled = false;
            } else {
                vinterUpstream.handRightTransmissionIsEnabled = false;
            }
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.gameObject.tag.Contains("Controller")) {
            collideCounter -= 1;
            if (collideCounter <= 0) {
                handRenderer.ShowHand();
                if (isLeft) {
                    vinterUpstream.handLeftTransmissionIsEnabled = true;
                } else {
                    vinterUpstream.handRightTransmissionIsEnabled = true;
                }
            }
        }
    }

}
