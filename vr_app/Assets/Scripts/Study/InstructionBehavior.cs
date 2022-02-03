using UnityEngine;
using System;
using System.Collections.Generic;

public class InstructionBehavior : MonoBehaviour
{
    private static readonly float SUCCESS_ROTATION_THRESHOLD = 15f;
    private static readonly float SUCCESS_DISTANCE_THRESHOLD = .03f;
    private static readonly float SUCCESS_TIME_THRESHOLD = 1500f;

    private InstructionManager instructionManager;
    private string instructionHrri;

    private List<GameObject> collidingOtherCircuitComponents = new List<GameObject>();
    private State state = State.Default;
    private long? beginSuccessTimestamp = null;

    public Material initialMaterial;
    private Material rightMaterial;
    private Material almostRightMaterial;

    public GameObject instruction;
    public GameObject target;

    private enum State { Default, Colliding, Correct, Done }

    private StudyConfig studyConfig;

    private int frameCounter = 0;

    private void Start()
    {
        GameObject gameManager = GameObject.FindGameObjectWithTag("GameManager");
        instructionManager = gameManager.GetComponent<InstructionManager>();
        AssetRepository ar = gameManager.GetComponent<AssetRepository>();
        studyConfig = gameManager.GetComponent<StudyConfig>();
        rightMaterial = ar.instructionRightMaterial;
        almostRightMaterial = ar.instructionAlmostRightMaterial;

        string targetHrri = instructionManager.instructionRegistry.GetTargetHrriByInstructionHrri(Utils.GetHrri(instruction));
        target = instructionManager.instructionRegistry.GetTargetByHrri(targetHrri);

        instructionHrri = instruction.GetComponent<HrriComponent>().hrri;
    }

    private void Update()
    {
        frameCounter = (frameCounter + 1) % 2;
        if (frameCounter != 0)
        {
            return;
        }

        if (state == State.Done)
        {
            return;
        }

        var diffVector = target.transform.position - instruction.transform.position;
        float distance = diffVector.magnitude;
        if (distance < SUCCESS_DISTANCE_THRESHOLD)
        {
            float instructionRot = instruction.transform.rotation.eulerAngles.y;
            if (instructionRot > 180) instructionRot -= 360f;
            float targetRot = target.transform.rotation.eulerAngles.y;
            if (targetRot > 180) targetRot -= 360f;
            float rotationDiff = Mathf.Abs(targetRot - instructionRot);
            if (state == State.Correct && Utils.GetTime() - beginSuccessTimestamp > SUCCESS_TIME_THRESHOLD)
            {
                state = State.Done;
                Debug.LogWarning("finish instruction with name " + gameObject.name + " and hrri " + instructionHrri);
                instructionManager.OnInstructionDone(instruction);
            }
            else if (state == State.Colliding && rotationDiff < SUCCESS_ROTATION_THRESHOLD)
            {
                state = State.Correct;
                beginSuccessTimestamp = Utils.GetTime();
                Utils.ChangeObjectMaterial(instruction, rightMaterial);
            }
            else if (state != State.Colliding)
            {
                if (rotationDiff > SUCCESS_ROTATION_THRESHOLD)
                {
                    state = State.Colliding;
                    beginSuccessTimestamp = null;
                    Utils.ChangeObjectMaterial(instruction, almostRightMaterial);
                }
                else if (state != State.Correct)
                {
                    state = State.Correct;
                    beginSuccessTimestamp = Utils.GetTime();
                    Utils.ChangeObjectMaterial(instruction, rightMaterial);
                }
            }
        }
        else
        {
            if (state != State.Default)
            {
                state = State.Default;
                beginSuccessTimestamp = null;
                Utils.ChangeObjectMaterial(instruction, initialMaterial);
            }
        }
    }
}