using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrinterInteractable : Interactable
{
    public GameObject printedObject;
    private static int printedObjectCount = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void StartInteraction()
    {
        GameObject printedInstance = Instantiate(printedObject);
        printedInstance.transform.parent = GameObject.FindGameObjectWithTag("Upstream").transform;
        Vector3 printerPosition = gameObject.transform.position;
        printerPosition.y += printedObject.transform.localScale.y + 0.1f;
        printedInstance.transform.position = printerPosition;
        printedObjectCount += 1;
        printedInstance.GetComponent<InteractableHrriGameObject>().hrriAttribute = printedObjectCount + "";
    }

}
