using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class RemoveShpereBehavior : MonoBehaviour
{
    private static readonly float INTERACTION_DISTANCE = .15f;

    private EntityManager entityManager;
    private InstructionManager instructionManager;
    private TaskManager taskManager;
    private EventManager eventManager;
    private StudyConfig studyConfig;

    public Config.HrriLocation location;

    private int frameCounter = 0;

    void Start()
    {
        studyConfig = GameObject.FindGameObjectWithTag("GameManager").GetComponent<StudyConfig>();
        Config config = GameObject.FindGameObjectWithTag("GameManager").GetComponent<Config>();
        entityManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<EntityManager>();
        taskManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<TaskManager>();
        eventManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<EventManager>();
        instructionManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<InstructionManager>();
        if (studyConfig.condition != StudyConfig.Condition.COPY
            && studyConfig.condition != StudyConfig.Condition.INSTRUCTION_BY_CONTROLLER
            && studyConfig.condition != StudyConfig.Condition.INSTRUCTIONS)
        {
            gameObject.SetActive(false);
        }
        else if (location != config.hrriLocation)
        {
            gameObject.GetComponent<Renderer>().enabled = false;
        }
    }

    void Update()
    {
        frameCounter = (frameCounter + 1) % 2;
        if (frameCounter != 0)
        {
            return;
        }

        if (studyConfig.condition == StudyConfig.Condition.COPY)
        {
            UpdateCopyCondition();
        }
        else if (studyConfig.condition == StudyConfig.Condition.INSTRUCTION_BY_CONTROLLER
            || studyConfig.condition == StudyConfig.Condition.INSTRUCTIONS)
        {
            UpdateInstructionConditions();
        }
    }

    private void UpdateCopyCondition()
    {
        var componentHrris = entityManager.GetHrrisByEntityType(ObjectRenderer.EntityType.COMPONENT);
        var localComponentHrris = componentHrris.Where(hrri => location == HrriUtil.ParseHrri(hrri).Item1).ToList();
        foreach (string componentHrri in localComponentHrris)
        {
            eventManager.hrriToGameObject.TryGetValue(componentHrri, out GameObject target);
            if (!target)
            {
                return;
            }

            var diffVector = target.transform.position - gameObject.transform.position;
            float distance = diffVector.magnitude;
            if (distance < INTERACTION_DISTANCE)
            {
                int? currentComponentId = taskManager.GetModelIdByComponentId(entityManager.GetIdentity(componentHrri).entityId);
                if (!currentComponentId.HasValue)
                {
                    return;
                }
                var hrrisWithSameComponentId = componentHrris.Where(hrri =>
                        taskManager.GetModelIdByComponentId(entityManager.GetIdentity(hrri).entityId) == currentComponentId
                    );
                if (hrrisWithSameComponentId.Count() > 1)
                {
                    target.GetComponent<ObjectRenderer>().ChangeEntityToBlank();
                }
            }
        }
    }

    private void UpdateInstructionConditions()
    {
        var instructionHrris = entityManager.GetHrrisByEntityType(ObjectRenderer.EntityType.INSTRUCTION);
        var localInstructionHrris = instructionHrris.Where(hrri => location == HrriUtil.ParseHrri(hrri).Item1).ToList();
        foreach (string instructionHrri in localInstructionHrris)
        {
            GameObject target = instructionManager.instructionRegistry.GetInstructionByHrri(instructionHrri);

            var diffVector = target.transform.position - gameObject.transform.position;
            float distance = diffVector.magnitude;
            if (distance < INTERACTION_DISTANCE)
            {
                instructionManager.CancelInstruction(target);
            }
        }
    }
}
