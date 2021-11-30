using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public virtual void StartInteraction()
    {
        Debug.Log("Interaction started. Please override this method to implemented your own interactable action.");
    }
    public virtual void StopInteraction()
    {
        Debug.Log("Interaction stopped. Please override this method to implemented your own interactable stop action if needed.");
    }
}
