using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RotationArrowRenderer : MonoBehaviour
{

    private const float lineWidth = .015f;
    private const float arrowWidthFactor = 3;
    private const float arrowLengthFactor = 3;
    private const float radius = .10f;
    private const float heightOffset = .03f;

    private List<Vector3> curvePath = new List<Vector3>();
    private LineRenderer lineRenderer;
    private LineRenderer arrowHeadRenderer;
    private Material material;

    public AssetRepository assetRepository;
    public GameObject target;
    public GameObject instruction;


    void Start()
    {
        assetRepository = GameObject.FindGameObjectWithTag("GameManager").GetComponent<AssetRepository>();
        material = instruction.GetComponent<ObjectRenderer>().location == ObjectRenderer.Location.LOCAL ?
            assetRepository.localInstructionMaterial :
            assetRepository.remoteInstructionMaterial;

        var arrowLine = new GameObject("Line");
        lineRenderer = arrowLine.AddComponent<LineRenderer>();
        arrowLine.transform.parent = transform;
        arrowLine.transform.position = transform.position;
        arrowLine.transform.rotation = transform.rotation;
        lineRenderer.material = material;
        lineRenderer.useWorldSpace = false;

        var arrowHead = new GameObject("Head");
        arrowHeadRenderer = arrowHead.AddComponent<LineRenderer>();
        arrowHead.transform.parent = transform;
        arrowHead.transform.position = transform.position;
        arrowHead.transform.rotation = transform.rotation;
        arrowHeadRenderer.material = material;
        arrowHeadRenderer.useWorldSpace = false;
    }

    void Update()
    {
        UpdateCurve();
    }

    private void UpdateCurve()
    {
        float diffAngle = instruction.transform.rotation.eulerAngles.y - target.transform.rotation.eulerAngles.y;
        if (diffAngle > 180f) diffAngle = diffAngle - 360;
        if (diffAngle < -180f) diffAngle = diffAngle + 360;

        if (diffAngle > 30f || diffAngle < -30f)
        {
            CircleCurve.GetPath(instruction.transform.position, curvePath, radius, diffAngle, instruction.transform.rotation * Vector3.up);
            DrawCurve();
            DrawArrowHead();
        }
        else
        {
            ClearCurveAndHead();
        }
    }

    private void ClearCurveAndHead()
    {
        lineRenderer.startWidth = 0;
        lineRenderer.endWidth = 0;
        arrowHeadRenderer.startWidth = 0;
        arrowHeadRenderer.endWidth = 0;
    }

    private void DrawCurve()
    {
        Vector3[] positions = new Vector3[curvePath.Count];

        for (int i = 0; i < curvePath.Count; i++)
        {
            positions[i] = curvePath[i];
        }
        lineRenderer.positionCount = positions.Length;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.SetPositions(positions);
    }

    private void DrawArrowHead()
    {
        Vector3 lastPoint = curvePath[0];
        Vector3 arrowHeadDirection = lastPoint - curvePath[3];
        arrowHeadDirection.Normalize();

        Vector3[] positions = new Vector3[2];
        positions[0] = lastPoint;
        positions[1] = lastPoint + arrowHeadDirection * arrowLengthFactor * lineWidth;

        arrowHeadRenderer.positionCount = 2;
        arrowHeadRenderer.startWidth = arrowWidthFactor * lineWidth;
        arrowHeadRenderer.endWidth = 0f;
        arrowHeadRenderer.SetPositions(positions);
    }


    private static class CircleCurve
    {
        public static void GetPath(Vector3 center, List<Vector3> pathData, float radius, float angle, Vector3 normal)
        {
            pathData.Clear();

            bool counterClockwise = angle < 0;

            Vector3 axis1 = Vector3.Cross(normal, Vector3.forward);
            axis1.Normalize();
            if (axis1.magnitude < .1f)
            {
                axis1 = Vector3.Cross(normal, Vector3.up);
                axis1.Normalize();
            }
            Vector3 axis2 = Vector3.Cross(normal, axis1);
            axis2.Normalize();

            int segments = 100;
            int drawnSegments = (int)(segments * Mathf.Abs(angle) / 360f);

            for (int i = 0; i <= drawnSegments; i++)
            {
                var rad = Mathf.Deg2Rad * (i * 360f / segments);
                if (counterClockwise) rad = 2 * Mathf.PI - rad;
                Vector3 comp1 = axis1 * Mathf.Sin(rad) * radius;
                Vector3 comp2 = axis2 * Mathf.Cos(rad) * radius;
                Vector3 comp3 = normal * heightOffset;
                Vector3 result = comp1 + comp2 + comp3;
                pathData.Add(result);
            }
        }
    }
}