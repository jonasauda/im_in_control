using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public static readonly int ESS_OFFSET = 100;
    public static readonly int INSTRUCTION_COMPONENT_ID = -1;

    public readonly int circuitWidth = 4;
    public readonly int circuitHeight = 3;
    public List<GameObject> ComponentPrefabs;

    private Dictionary<int, int> componentIdToModelId;

    private StudyConfig studyConfig;
    private Config config;
    private AssetRepository assetRepository;
    private EntityManager entityManager;
    public int totalComponents;

    void Start()
    {
        studyConfig = GetComponent<StudyConfig>();
        config = GetComponent<Config>();
        assetRepository = GetComponent<AssetRepository>();
        entityManager = GetComponent<EntityManager>();

        InitConfig();
    }

    private void InitConfig()
    {
        componentIdToModelId = new Dictionary<int, int>();
        totalComponents = circuitWidth * circuitHeight;

        // MUC
        for (int i = 1; i <= totalComponents / 2; i++)
        {
            string hrri = "MUC-ENVIRONMENT-OBJ-" + i;
            entityManager.SetIdentity(hrri, ObjectRenderer.EntityType.COMPONENT, i);
            componentIdToModelId[i] = i % ESS_OFFSET;
        }

        for (int i = totalComponents / 2 + 1; i <= 20; i++)
        {
            string hrri = "MUC-ENVIRONMENT-OBJ-" + i;
            entityManager.SetIdentity(hrri, ObjectRenderer.EntityType.BLANK, i);
        }

        // ESS
        for (int i = ESS_OFFSET + 1; i <= ESS_OFFSET + totalComponents / 2; i++)
        {
            string hrri = "ESS-ENVIRONMENT-OBJ-" + i;
            entityManager.SetIdentity(hrri, ObjectRenderer.EntityType.COMPONENT, i);
            componentIdToModelId[i] = i % ESS_OFFSET;
        }

        for (int i = ESS_OFFSET + totalComponents / 2 + 1; i <= ESS_OFFSET + 20; i++)
        {
            string hrri = "ESS-ENVIRONMENT-OBJ-" + i;
            entityManager.SetIdentity(hrri, ObjectRenderer.EntityType.BLANK, i);
        }

        //componentIdToModelId.ToList().ForEach(e => Debug.Log(e.Key + " - " + e.Value));
    }

    public int? GetModelIdByComponentId(int componentId)
    {
        if (componentId == INSTRUCTION_COMPONENT_ID)
        {
            Debug.LogError("Never ask for model of instruction directly. componentId: " + componentId);
            return null;
        }
        try
        {
            return componentIdToModelId[componentId];
        }
        catch (Exception e)
        {
            Debug.LogError("No entry in componentIdToModelId for componentId " + componentId);
        }
        return null;
    }

    public GameObject GetComponentModel(int id)
    {
        int? modelId = GetModelIdByComponentId(id);
        if (modelId == null)
        {
            return null;
        }
        return assetRepository.CircuitComponentModels[modelId.Value - 1];
    }

}
