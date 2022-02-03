using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectRenderer : MonoBehaviour
{
    private readonly int LAYER_DEFAULT = 0;
    private readonly int LAYER_PHYSICAL = 10;
    private readonly int LAYER_VIRTUAL = 11;

    private AssetRepository assetRepository;

    public enum EntityType { BLANK, COMPONENT, INSTRUCTION }
    public enum Location { LOCAL, REMOTE }
    public enum Physicality { PHYSICAL, VIRTUAL }

    public Location location;
    public Physicality physicality = Physicality.VIRTUAL;
    private int? targetComponentId;

    private StudyConfig studyConfig;
    private TaskManager taskManager;
    private Config config;
    private EntityManager entityManager;

    private bool initialized = false;

    private void Start()
    {
        GameObject gameManager = GameObject.FindGameObjectWithTag("GameManager");
        assetRepository = gameManager.GetComponent<AssetRepository>();
        config = gameManager.GetComponent<Config>();
        studyConfig = gameManager.GetComponent<StudyConfig>();
        taskManager = gameManager.GetComponent<TaskManager>();
        entityManager = gameManager.GetComponent<EntityManager>();

        Initialize();
    }

    private void Initialize()
    {
        string hrri = Utils.GetHrri(gameObject);

        if (initialized)
        {
            return;
        }
        initialized = true;

        var (hrriLocation, _, hrriObject, hrriAttribute) = HrriUtil.ParseHrri(hrri);
        location = config.hrriLocation == hrriLocation ? Location.LOCAL : Location.REMOTE;

        UpdateEntity();

        if (gameObject.layer != LAYER_PHYSICAL)
        {
            SetLayer(LAYER_VIRTUAL);
        }
    }

    public void InitVirtualInstruction(GameObject target)
    {
        this.targetComponentId = int.Parse(HrriUtil.ParseHrri(Utils.GetHrri(target)).Item4);
    }

    public void ChangeEntityToInstruction(GameObject target)
    {
        this.targetComponentId = int.Parse(HrriUtil.ParseHrri(Utils.GetHrri(target)).Item4);
        ChangeEntity(EntityType.INSTRUCTION);
    }

    public void ChangeEntityToBlank()
    {
        this.targetComponentId = null;
        ChangeEntity(EntityType.BLANK);
    }

    public void ChangeEntityToCircuit()
    {
        this.targetComponentId = null;
        ChangeEntity(EntityType.COMPONENT);
    }

    private void ChangeEntity(EntityType type, GameObject target = null)
    {
        Reset();
        string hrri = Utils.GetHrri(gameObject);
        var oldIdentity = entityManager.GetIdentity(hrri);
        entityManager.SetIdentity(hrri, type, type == EntityType.INSTRUCTION ? TaskManager.INSTRUCTION_COMPONENT_ID : oldIdentity.entityId);
        UpdateEntity();
    }

    public void SetPhysical()
    {
        physicality = Physicality.PHYSICAL;
        SetLayer(LAYER_PHYSICAL);
    }

    private void SetLayer(int layer)
    {
        Utils.ChangeObjectLayer(gameObject, layer);
        gameObject.layer = LAYER_DEFAULT;
    }

    private void UpdateEntity()
    {
        string hrri = Utils.GetHrri(gameObject);
        Debug.LogWarning("UpdateEntity with name " + gameObject.name + " and hrri " + hrri);
        int ownComponentId = entityManager.GetIdentity(hrri).entityId;

        GameObject componentModel = null;
        EntityType entityType = entityManager.GetIdentity(hrri).entityType;
        switch (entityType)
        {
            case EntityType.BLANK:
                componentModel = GetBlankObject(location);
                AddBlankObjectBehavior();
                break;
            case EntityType.COMPONENT:
                componentModel = GetCircuitObject(location, ownComponentId);
                if (location == Location.LOCAL)
                {
                    AddLocalCircuitBehavior();
                }
                else
                {
                    AddRemoteCircuitBehavior();
                }
                break;
            case EntityType.INSTRUCTION:
                componentModel = GetCircuitInstructionObject(location, targetComponentId.Value);
                if (location == Location.LOCAL)
                {
                    AddLocalInstructionBehavior();
                }
                else
                {
                    AddRemoteInstructionBehavior();
                }
                break;
        }
        gameObject.AddComponent<ActionLogBehavior>();
        SetModel(componentModel);
    }

    private void SetModel(GameObject componentModel)
    {
        if (componentModel == null) return;

        componentModel.transform.parent = gameObject.transform;
        componentModel.transform.localPosition = new Vector3();
        componentModel.transform.localRotation = new Quaternion();
    }

    private void AddLocalCircuitBehavior()
    {
        if (config.enableSelfInstructions)
        {
            gameObject.AddComponent<InstructibleForeignRigidBody>();
        }
        gameObject.GetComponent<Rigidbody>().useGravity = false;
    }

    private void AddRemoteCircuitBehavior()
    {
        gameObject.AddComponent<InstructibleForeignRigidBody>();
        gameObject.GetComponent<Rigidbody>().useGravity = false;
    }

    private void AddLocalInstructionBehavior()
    {
        gameObject.GetComponent<Rigidbody>().useGravity = true;
        InstructionBehavior ib = gameObject.AddComponent<InstructionBehavior>();
        ib.instruction = gameObject;
        ib.initialMaterial = assetRepository.localInstructionMaterial;

        // if virtual object
        if (!gameObject.GetComponent<MocapRigidBodyTransformer>())
        {
            gameObject.tag = "Tangible";

            InteractableHrriGameObject insctructibleInteractable = gameObject.AddComponent<InteractableHrriGameObject>();
            insctructibleInteractable.hrriObject = Config.HrriObject.CIRCUIT_INSTRUCTION;
            insctructibleInteractable.hrriAttribute = HrriUtil.ParseHrri(Utils.GetHrri(gameObject)).Item4;
        }
    }

    private void AddRemoteInstructionBehavior()
    {
        InstructionBehavior ib = gameObject.AddComponent<InstructionBehavior>();
        ib.instruction = gameObject;
        ib.initialMaterial = assetRepository.remoteInstructionMaterial;
        gameObject.GetComponent<Rigidbody>().useGravity = false;
    }

    private void AddBlankObjectBehavior()
    {
        if (location == Location.LOCAL)
        {
            gameObject.AddComponent<BlankBehavior>();
        }
        else
        {
            Destroy(gameObject.GetComponent<BlankBehavior>());
        }
    }

    private GameObject GetCircuitObject(Location location, int componentId)
    {
        GameObject go;
        if (studyConfig.playground)
        {
            int? modelId = taskManager.GetModelIdByComponentId(componentId);
            go = Instantiate(assetRepository.playgroundModel);
            int materialId = modelId.Value % assetRepository.playgroundMaterials.Count();
            Utils.ChangeObjectMaterial(go, assetRepository.playgroundMaterials[materialId]);
        }
        else
        {
            GameObject componentModel = taskManager.GetComponentModel(componentId);
            if (componentModel == null)
            {
                Debug.LogError("component model is null. componentId: " + componentId + ", gameObject.name: " + gameObject.name + ", ");
            }
            go = Instantiate(componentModel);
        }


        if (location == Location.REMOTE)
        {
            Utils.ChangeAlpha(go, .4f);
        }

        return go;
    }

    private GameObject GetCircuitInstructionObject(Location location, int componentId)
    {
        GameObject go = GetCircuitObject(location, componentId);
        Material material = location == Location.LOCAL ? assetRepository.localInstructionMaterial : assetRepository.remoteInstructionMaterial;
        Utils.ChangeObjectMaterial(go, material);
        return go;
    }

    private GameObject GetBlankObject(Location location)
    {
        GameObject model = Instantiate(assetRepository.BlankModel);

        if (location == Location.REMOTE)
        {
            Utils.ChangeObjectMaterial(model, assetRepository.remoteBlankMaterial);
        }

        return model;
    }

    public void Reset()
    {
        // remove components
        var specificComponents = GetComponents(typeof(Component)).ToList().Where(IsSpecificComponent);
        specificComponents.ToList().ForEach(Destroy);

        // reset other
        if (gameObject.tag != "MocapRigidBody")
        {
            gameObject.tag = "Untagged";
        }
        gameObject.GetComponent<Rigidbody>().useGravity = true;

        // destroy children
        RemoveModel();
    }

    private void RemoveModel()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }

    private bool IsSpecificComponent(Component c)
    {
        return c.GetType() != typeof(ObjectRenderer)
            && c.GetType() != typeof(MocapRigidBodyTransformer)
            && c.GetType() != typeof(HrriComponent)
            && c.GetType() != typeof(BoxCollider)
            && c.GetType() != typeof(Rigidbody)
            && c.GetType() != typeof(Transform);
    }
}
