using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArrowRenderer : MonoBehaviour
{

    private const float lineWidthFactor = .05f;
    private const float arrowWidthFactor = lineWidthFactor * 3;
    private const float arrowLengthFactor = lineWidthFactor * 3;
    private const float arrowCurvature = .5f;
    private const float resolution = .01f;
    private const float minimalDistance = .1f;

    private List<Vector3> curvePath = new List<Vector3>();
    private LineRenderer lineRenderer;
    private LineRenderer arrowHeadRenderer;
    private Material material;

    public AssetRepository assetRepository;
    public GameObject startObject;
    public GameObject endObject;

    void Start()
    {
        assetRepository = GameObject.FindGameObjectWithTag("GameManager").GetComponent<AssetRepository>();
        material = endObject.GetComponent<ObjectRenderer>().location == ObjectRenderer.Location.LOCAL ?
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
        if (startObject == null || endObject == null) return;
        UpdateCurve();
    }

    private void UpdateCurve()
    {
        Vector3 startPosition = startObject.transform.position;
        Vector3 endPosition = endObject.transform.position;
        Vector3 direction = endPosition - startPosition;
        float distance = direction.magnitude;

        if (distance < minimalDistance)
        {
            ClearCurveAndHead();
            return;
        }

        direction.Normalize();
        Vector3 pathStep = (distance / 3) * direction;
        Vector3 midPointHeightOffset = arrowCurvature * distance * Vector3.up;

        List<Vector3> controlPoints = new List<Vector3>();
        controlPoints.Add(startPosition);
        controlPoints.Add(startPosition + pathStep + midPointHeightOffset);
        controlPoints.Add(startPosition + 2 * pathStep + midPointHeightOffset);
        controlPoints.Add(endObject.transform.position);

        BezierCurve.GetBezierCurve(controlPoints, curvePath, resolution);
        DrawCurve(distance);
        DrawArrowHead(distance);
    }

    private void ClearCurveAndHead()
    {
        lineRenderer.startWidth = 0;
        lineRenderer.endWidth = 0;
        arrowHeadRenderer.startWidth = 0;
        arrowHeadRenderer.endWidth = 0;
    }

    private void DrawCurve(float distance)
    {
        Vector3[] positions = new Vector3[curvePath.Count];

        for (int i = 0; i < curvePath.Count; i++)
        {
            positions[i] = curvePath[i];
        }
        lineRenderer.positionCount = positions.Length;
        lineRenderer.startWidth = lineWidthFactor * distance;
        lineRenderer.endWidth = lineWidthFactor * distance;
        lineRenderer.SetPositions(positions);
    }

    private void DrawArrowHead(float distance)
    {
        Vector3 lastPoint = curvePath[curvePath.Count - 1];
        Vector3 arrowHeadDirection = lastPoint - curvePath[curvePath.Count - 10];
        arrowHeadDirection.Normalize();

        Vector3[] positions = new Vector3[2];
        positions[0] = lastPoint;
        positions[1] = lastPoint + arrowHeadDirection * arrowLengthFactor * distance;

        arrowHeadRenderer.positionCount = 2;
        arrowHeadRenderer.startWidth = arrowWidthFactor * distance;
        arrowHeadRenderer.endWidth = 0f;
        arrowHeadRenderer.SetPositions(positions);
    }


    private static class BezierCurve
    {
        public static void GetBezierCurve(List<Vector3> controlPoints, List<Vector3> allRopeSections, float resolution)
        {
            Vector3 A = controlPoints[0];
            Vector3 B = controlPoints[1];
            Vector3 C = controlPoints[2];
            Vector3 D = controlPoints[3];

            allRopeSections.Clear();

            float t = .05f;

            while (t <= .9f)
            {
                Vector3 newPos = DeCasteljausAlgorithm(A, B, C, D, t);
                allRopeSections.Add(newPos);
                t += resolution;
            }
        }

        static Vector3 DeCasteljausAlgorithm(Vector3 A, Vector3 B, Vector3 C, Vector3 D, float t)
        {
            float oneMinusT = 1f - t;

            Vector3 Q = oneMinusT * A + t * B;
            Vector3 R = oneMinusT * B + t * C;
            Vector3 S = oneMinusT * C + t * D;

            Vector3 P = oneMinusT * Q + t * R;
            Vector3 T = oneMinusT * R + t * S;

            Vector3 U = oneMinusT * P + t * T;

            return U;
        }
    }
}