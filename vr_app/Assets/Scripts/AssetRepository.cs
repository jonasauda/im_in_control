using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetRepository : MonoBehaviour
{
    public Material localInstructionMaterial;
    public Material remoteInstructionMaterial;
    public Material remoteCircuitMaterial;
    public Material instructionRightMaterial;
    public Material instructionAlmostRightMaterial;
    public Material remoteBlankMaterial;
    public List<Material> playgroundMaterials;

    public GameObject BaseObject;

    public GameObject BlankModel;
    public GameObject playgroundModel;
    public List<GameObject> CircuitComponentModels;
}
