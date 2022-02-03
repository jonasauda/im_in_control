using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Config;
using static EventLogger;

class InstructionManager : MonoBehaviour
{

    public InstructionRegistry instructionRegistry = new InstructionRegistry();
    public GameObject instructionArrowPrefab;
    private List<GameObject> instructionArrows = new List<GameObject>();

    public GameObject instructionRotationArrowPrefab;
    private List<GameObject> instructionRotationArrows = new List<GameObject>();

    private AssetRepository assetRepository;
    private TaskManager taskManager;
    private EventManager eventManager;
    private EventLogger eventLogger;
    private StudyConfig studyConfig;
    private Config config;
    private EntityManager entityManager;

    private delegate void OnNewInstructionDelegate(GameObject instruction, GameObject target);
    private event OnNewInstructionDelegate OnNewInstruction;

    private delegate void OnRemoveInstructionDelegate(GameObject instruction);
    private event OnRemoveInstructionDelegate OnRemoveInstruction;

    void Start()
    {
        GameObject gameManager = GameObject.FindGameObjectWithTag("GameManager");
        assetRepository = gameManager.GetComponent<AssetRepository>();
        eventLogger = gameObject.GetComponent<EventLogger>();
        config = gameManager.GetComponent<Config>();
        studyConfig = gameManager.GetComponent<StudyConfig>();
        taskManager = gameManager.GetComponent<TaskManager>();
        entityManager = GetComponent<EntityManager>();

        eventManager = gameManager.GetComponent<EventManager>();
        eventManager.OnNewRemoteObject += OnNewRemoteObject;
        eventManager.OnStartBlankInteraction += HandleOnStartBlankInteraction;
        eventManager.OnRemoteEvent += HandleOnRemoteEvent;
        eventManager.OnStartStudy += HandleOnStartStudy;

        OnNewInstruction += HandleOnNewInstruction;
        OnRemoveInstruction += HandleOnRemoveInstruction;


    }

    private void HandleOnRemoteEvent(GameEvent gameEvent)
    {
        Debug.Log("remote event: " + gameEvent.Type + ", " + gameEvent.ID);
        switch (gameEvent.Type)
        {
            case GameEvent.EventType.BlankInteraction:
                OnRemoteBlankInteraction(gameEvent);
                break;
            case GameEvent.EventType.StartStudy:
                eventManager.FireOnStartStudy(true);
                eventLogger.AddLogEvent(LogEvent.StartStudy, "");
                break;
            case GameEvent.EventType.StopStudy:
                eventManager.FireOnStopStudy(true);
                eventLogger.AddLogEvent(LogEvent.StopStudy, "");
                break;
        }
    }

    private void OnRemoteBlankInteraction(GameEvent gameEvent)
    {
        string blankHrri = gameEvent.data[0];
        string targetHrri = gameEvent.data[1];
        eventManager.hrriToGameObject.TryGetValue(blankHrri, out GameObject blank);
        if (!blank)
        {
            Debug.LogError("There is no blank object with hrri " + blankHrri + " in the scene!");
        }
        eventManager.hrriToGameObject.TryGetValue(targetHrri, out GameObject target);
        if (!target)
        {
            Debug.LogError("There is no target object with hrri " + targetHrri + " in the scene!");
        }

        eventManager.FireOnStartBlankInteraction(blank, target);
    }

    private void OnCutEntity(GameObject blank, GameObject target)
    {
        string blankHrri = Utils.GetHrri(blank);
        string targetHrri = Utils.GetHrri(target);
        eventLogger.AddLogEvent(LogEvent.CutEntity, blankHrri + " " + targetHrri);
        entityManager.SwitchEntities(blankHrri, targetHrri);
        blank.GetComponent<ObjectRenderer>().ChangeEntityToCircuit();
        target.GetComponent<ObjectRenderer>().ChangeEntityToBlank();
    }

    private void OnCopyEntity(GameObject blank, GameObject target)
    {
        string blankHrri = Utils.GetHrri(blank);
        string targetHrri = Utils.GetHrri(target);
        eventLogger.AddLogEvent(LogEvent.CopyEntity, blankHrri + " " + targetHrri);
        entityManager.CopyEntity(blankHrri, targetHrri);
        blank.GetComponent<ObjectRenderer>().ChangeEntityToCircuit();
    }

    private void HandleOnStartBlankInteraction(GameObject blank, GameObject target)
    {
        var blankObjectRenderer = blank.GetComponent<ObjectRenderer>();
        var targetObjectRenderer = target.GetComponent<ObjectRenderer>();
        if (entityManager.GetIdentity(Utils.GetHrri(target)).entityType == ObjectRenderer.EntityType.BLANK)
        {
            return;
        }
        if (studyConfig.condition == StudyConfig.Condition.INSTRUCTIONS
            && blankObjectRenderer.location != targetObjectRenderer.location)
        {
            blankObjectRenderer.ChangeEntityToInstruction(target);
            OnNewInstruction(blank, target);
        }
        else if (studyConfig.condition == StudyConfig.Condition.CUT
            && blankObjectRenderer.location != targetObjectRenderer.location)
        {
            OnCutEntity(blank, target);
        }
        else if (studyConfig.condition == StudyConfig.Condition.COPY)
        {
            OnCopyEntity(blank, target);
        }
    }

    private void HandleOnNewInstruction(GameObject instruction, GameObject target)
    {
        Debug.LogWarning("HandleOnNewInstruction");

        string instructionHrri = Utils.GetHrri(instruction);
        string targetHrri = Utils.GetHrri(target);

        // destroy old instruction with same hrri if it exists
        if (DestroyInstructionWithTargetHrri(targetHrri))
        {
            eventLogger.AddLogEvent(LogEvent.CancelInstruction, instructionHrri);
        }
        entityManager.SetIdentity(instructionHrri, ObjectRenderer.EntityType.INSTRUCTION, TaskManager.INSTRUCTION_COMPONENT_ID);
        instructionRegistry.Register(instruction, target);
        CreateInstructionArrows(target, instruction);

        eventLogger.AddLogEvent(LogEvent.NewInstruction, instructionHrri + " " + targetHrri);
    }

    public void CancelInstruction(GameObject instruction)
    {
        eventLogger.AddLogEvent(LogEvent.CancelInstruction, Utils.GetHrri(instruction));
        DestroyInstruction(instruction);
    }

    private void HandleOnRemoveInstruction(GameObject instruction)
    {
        var hrriComponent = instruction.GetComponent<HrriComponent>();
        if (!hrriComponent)
        {
            Debug.LogError("Instruction to be removed does not have an HrriComponent: " + instruction.name);
            return;
        }
        string instructionHrri = hrriComponent.hrri;
        RemoveInstructionArrows(instruction);
        instructionRegistry.Unregister(instruction);
    }

    public void OnInstructionDone(GameObject instruction)
    {
        Debug.LogWarning("OnInstructionDone");
        eventLogger.AddLogEvent(LogEvent.FinishInstruction, Utils.GetHrri(instruction));
        DestroyInstruction(instruction);
    }

    public GameObject CreateLocalInstructionObject(GameObject target)
    {
        string targetHrri = target.GetComponent<HrriComponent>().hrri;
        var (hrriLocation, hrriGroup, hrriObject, hrriAttribute) = HrriUtil.ParseHrri(targetHrri);

        // create virtual object
        //GameObject instructionObject = Instantiate(taskManager.GetComponentModel(int.Parse(hrriAttribute)));
        string instructionHrri = HrriUtil.CreateHrri(config.hrriLocation, config.hrriGroup, HrriObject.CIRCUIT_INSTRUCTION, hrriAttribute);
        entityManager.SetIdentity(instructionHrri, ObjectRenderer.EntityType.INSTRUCTION, TaskManager.INSTRUCTION_COMPONENT_ID);

        GameObject instructionObject = Instantiate(assetRepository.BaseObject);
        instructionObject.GetComponent<ObjectRenderer>().InitVirtualInstruction(target);
        HrriComponent hrriComponent = instructionObject.AddComponent<HrriComponent>();
        hrriComponent.hrri = instructionHrri;

        instructionObject.transform.parent = GameObject.FindGameObjectWithTag("Upstream").transform;
        instructionObject.transform.position = target.transform.position;
        instructionObject.transform.rotation = target.transform.rotation;


        OnNewInstruction(instructionObject, target);

        return instructionObject;
    }

    private bool DestroyInstructionWithTargetHrri(string targetHrri)
    {
        string instructionHrri = instructionRegistry.GetInstructionHrriByTargetHrri(targetHrri);
        if (instructionHrri != null)
        {
            DestroyInstructionWithHrri(instructionHrri);
            return true;
        }
        return false;
    }

    private bool DestroyInstructionWithHrri(string instructionHrri)
    {
        GameObject instruction = instructionRegistry.GetInstructionByHrri(instructionHrri);
        if (instruction != null)
        {
            DestroyInstruction(instruction);
            return true;
        }
        else
        {
            Debug.LogWarning("could not find " + instructionHrri + " in dict");
        }
        return false;
    }

    private void DestroyInstruction(GameObject instruction)
    {
        string instructionHrri = Utils.GetHrri(instruction);

        OnRemoveInstruction(instruction);
        if (instruction.GetComponent<ObjectRenderer>().physicality == ObjectRenderer.Physicality.VIRTUAL)
        {
            Debug.LogWarning("destroying virual instruction wit name " + instruction.name + " and hrri " + instructionHrri);
            entityManager.RemoveIdentity(instructionHrri);
            if (instruction.GetComponent<ObjectRenderer>().location == ObjectRenderer.Location.LOCAL)
            {
                Destroy(instruction);
            }
            //else
            //{
            //    instruction.SetActive(false);
            //}

            //eventManager.hrriToGameObject.Remove(instructionHrri);
        }
        else
        {
            Debug.LogWarning("destroying physical instruction");
            instruction.GetComponent<ObjectRenderer>().ChangeEntityToBlank();
        }
    }

    public void OnNewRemoteObject(GameObject remoteObject)
    {
        if (!IsInstructionObject(remoteObject)) return;

        string instructionHrri = Utils.GetHrri(remoteObject);
        GameObject target = GetInstructionTarget(instructionHrri);
        if (!target)
        {
            target = DetermineInstructionTarget(instructionHrri);
        }
        if (target == null)
        {
            Debug.LogWarning("There is no target related to this instruction: " + instructionHrri);
            return;
        }
        remoteObject.GetComponent<ObjectRenderer>().InitVirtualInstruction(target);

        OnNewInstruction(remoteObject, target);
    }

    private bool IsInstructionObject(GameObject go)
    {
        if (go == null) return false;

        string hrri = Utils.GetHrri(go);
        var (hrriLocation, hrriGroup, hrriObject, hrriAttribute) = HrriUtil.ParseHrri(hrri);
        return HrriObjectToInstructionObject.Values.Contains(hrriObject);
    }

    private GameObject DetermineInstructionTarget(string instructionHrri)
    {
        var (hrriLocation, hrriGroup, hrriObject, hrriAttribute) = HrriUtil.ParseHrri(instructionHrri);

        InstructionObjectToHrriObject.TryGetValue(hrriObject, out HrriObject originalObject);
        GameObject originalGameObject = null;

        foreach (HrriLocation hl in Enum.GetValues(typeof(HrriLocation)))
        {
            if (hl == HrriLocation.NONE) continue;
            string originalHrri = HrriUtil.CreateHrri(hl, HrriGroup.ENVIRONMENT, originalObject, hrriAttribute);
            eventManager.hrriToGameObject.TryGetValue(originalHrri, out originalGameObject);
            if (originalGameObject != null) break;
        }
        return originalGameObject;
    }


    public GameObject GetInstructionTarget(string instructionHrri)
    {
        GameObject originalGameObject = null;
        string originalHrri = instructionRegistry.GetTargetHrriByInstructionHrri(instructionHrri);
        if (originalHrri != null)
        {
            eventManager.hrriToGameObject.TryGetValue(originalHrri, out originalGameObject);
        }
        return originalGameObject;
    }

    public void CreateInstructionArrows(GameObject original, GameObject instruction)
    {
        Debug.LogWarning("CreateInstructionArrows");
        GameObject instructionArrow = Instantiate(instructionArrowPrefab);
        instructionArrows.Add(instructionArrow);
        ArrowRenderer arrowRenderer = instructionArrow.GetComponent<ArrowRenderer>();
        arrowRenderer.startObject = original;
        arrowRenderer.endObject = instruction;

        GameObject instructionRotationArrow = Instantiate(instructionRotationArrowPrefab);
        instructionRotationArrows.Add(instructionRotationArrow);
        instructionRotationArrow.transform.parent = instruction.transform;
        instructionRotationArrow.transform.position = instruction.transform.position;
        instructionRotationArrow.transform.rotation = instruction.transform.rotation;
        RotationArrowRenderer rotationArrowRenderer = instructionRotationArrow.GetComponent<RotationArrowRenderer>();
        rotationArrowRenderer.target = original;
        rotationArrowRenderer.instruction = instruction;
    }

    public void RemoveInstructionArrows(GameObject instructionObject)
    {
        Debug.LogWarning("RemoveInstructionArrows");
        GameObject toBeDestroyed = instructionArrows.FirstOrDefault(g => g.GetComponent<ArrowRenderer>().endObject == instructionObject);
        if (toBeDestroyed != null)
        {
            instructionArrows.Remove(toBeDestroyed);
            Destroy(toBeDestroyed);
        }
        else
        {
            Debug.LogWarning("Could not find an position arrow related to given instruction");
        }


        GameObject toBeDestroyed2 = instructionRotationArrows.FirstOrDefault(g => g != null && g.GetComponent<RotationArrowRenderer>().instruction == instructionObject);
        if (toBeDestroyed2 != null)
        {
            instructionRotationArrows.Remove(toBeDestroyed2);
            Destroy(toBeDestroyed2);
        }
        else
        {
            Debug.LogWarning("Could not find an rotation arrow related to given instruction");
        }
    }

    private void HandleOnStartStudy(bool remote)
    {
       
    }

    private void SetupInstructionByBlankConditionInstructions()
    {
        List<string> componentHrris = entityManager.GetHrrisByEntityType(ObjectRenderer.EntityType.COMPONENT);

        List<GameObject> componentGos = componentHrris.Select(
            hrri => { eventManager.hrriToGameObject.TryGetValue(hrri, out GameObject go); return go; }
        ).Where(go => go != null).ToList();
        Debug.LogWarning("componentGos.Count: " + componentGos.Count());

        IEnumerable<(GameObject, GameObject)> instructionPairs = componentGos.Select(
            go => (GetInstructionForComponent(go), go)
        );
        Debug.LogWarning("instructionPairs.Count: " + instructionPairs.Where(pair => pair.Item1 != null && pair.Item2 != null).Count());

        instructionPairs
            .Where(pair => pair.Item1 != null && pair.Item2 != null)
            .ToList()
            .ForEach(pair =>
            {
                pair.Item1.GetComponent<ObjectRenderer>().ChangeEntityToInstruction(pair.Item2);
                OnNewInstruction(pair.Item1, pair.Item2);
            });
    }

    private GameObject GetInstructionForComponent(GameObject componentGo)
    {
        int componentsPerUser = taskManager.totalComponents / 2;
        var (l, g, o, a) = HrriUtil.ParseHrri(Utils.GetHrri(componentGo));
        int componentHrriAttribute = int.Parse(a);

        int instructionHrriAttribute = componentHrriAttribute < TaskManager.ESS_OFFSET ?
            componentHrriAttribute + TaskManager.ESS_OFFSET + componentsPerUser :
            componentHrriAttribute - TaskManager.ESS_OFFSET + componentsPerUser;

        string newHrri = HrriUtil.CreateHrri(SwitchLocations(l), g, o, instructionHrriAttribute.ToString());
        eventManager.hrriToGameObject.TryGetValue(newHrri, out GameObject instructionGo);
        if (instructionGo == null) Debug.LogWarning(newHrri + "returned null");
        return instructionGo;
    }

    private HrriLocation SwitchLocations(HrriLocation l)
    {
        return l == HrriLocation.LOCATION2 ? HrriLocation.LOCATION1 : HrriLocation.LOCATION2;
    }
}