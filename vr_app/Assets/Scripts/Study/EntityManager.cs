using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EntityManager : MonoBehaviour
{
    private Dictionary<string, Identity> hrriToIdentity = new Dictionary<string, Identity>();
    public List<string> debugEntities = new List<string>();

    public class Identity
    {
        public ObjectRenderer.EntityType entityType;
        public int entityId;
    }

    public void SetIdentity(string hrri, ObjectRenderer.EntityType entityType, int entityId)
    {
        Debug.LogWarning("setting identity of hrri " + hrri + " to " + entityType + " and componentId " + entityId);
        Identity identity = new Identity();
        identity.entityType = entityType;
        identity.entityId = entityId;
        hrriToIdentity[hrri] = identity;
    }

    public Identity GetIdentity(string hrri)
    {
        try
        {
            return hrriToIdentity[hrri];
        }
        catch (KeyNotFoundException e)
        {
            Debug.LogError("Could not get identity of hrri " + hrri);
            UpdateDebugEntities();
            Debug.Log("Current instruction entity mapping:");
            debugEntities.ToList().Where(h => h.Contains("INSTRUCTION")).ToList().ForEach(Debug.Log);
            return null;
        }
    }

    public void SwitchEntities(string hrri1, string hrri2)
    {
        Debug.Log("Switching identities: " + hrri1 + " and " + hrri2);
        Identity oldIdentity1 = hrriToIdentity[hrri1];
        Identity oldIdentity2 = hrriToIdentity[hrri2];
        hrriToIdentity[hrri1] = oldIdentity2;
        hrriToIdentity[hrri2] = oldIdentity1;
    }

    public void CopyEntity(string hrri1, string hrri2)
    {
        Debug.Log("Copying identities: " + hrri2 + " to " + hrri1);
        hrriToIdentity[hrri1] = hrriToIdentity[hrri2];
    }

    public void RemoveIdentity(string hrri)
    {
        Debug.Log("Removing identity of " + hrri);
        hrriToIdentity.Remove(hrri);
    }

    public List<string> GetHrrisByEntityType(ObjectRenderer.EntityType entityType)
    {
        return hrriToIdentity.ToList().Where(e => e.Value.entityType == entityType).Select(e => e.Key).ToList();
    }

    private void Update()
    {
        UpdateDebugEntities();
    }

    private void UpdateDebugEntities()
    {
        debugEntities = hrriToIdentity.ToList().Select(e => e.Key + " - " + e.Value.entityType + " - " + e.Value.entityId).ToList();
    }
}
