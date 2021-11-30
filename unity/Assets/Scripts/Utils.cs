using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class Utils
{
    public static long GetTime()
    {
        return (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds;
    }

    public static bool IsRemoteObject(GameObject go)
    {
        MocapRigidBodyTransformer transformer = go.GetComponent<MocapRigidBodyTransformer>();
        return transformer != null;
    }

    public static string GetHrri(GameObject go)
    {
        HrriComponent hrriComponent = go.GetComponent<HrriComponent>();
        return hrriComponent ? hrriComponent.hrri : null;
    }
    public static void ChangeObjectMaterial(GameObject go, Material material)
    {
        go.GetComponentsInChildren<Renderer>().ToList().ForEach(r =>
        {
            if (r.gameObject.name != "Line" && r.gameObject.name != "Head")
            {
                r.material = material;
            }
        });
    }
    public static void ChangeAlpha(GameObject go, float alpha)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();

        foreach (var rend in renderers.ToList())
            if (rend)
            {
                // Change the material of all hit colliders
                // to use a transparent shader.
                Shader s = Shader.Find("Transparent/Diffuse");
                rend.material.shader = s;
                Color tempColor = rend.material.color;
                tempColor.a = alpha;
                rend.material.color = tempColor;
            }
    }


    public static void ChangeObjectLayer(GameObject go, int layer)
    {
        go.GetComponentsInChildren<Transform>().ToList().ForEach(c =>
        {
            //Debug.Log("component.gameObject: " + c.gameObject.name);
            c.gameObject.layer = layer;
        });
    }
}
