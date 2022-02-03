using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class InstructionRegistry
{
    private Dictionary<string, GameObject> instructionHrriToInstruction = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> targetHrriToTarget = new Dictionary<string, GameObject>();
    private Dictionary<string, string> instructionHrriToTargetHrri = new Dictionary<string, string>();
    private Dictionary<string, string> targetHrriToInstructionHrri = new Dictionary<string, string>();


    public void Register(GameObject instruction, GameObject target)
    {
        string instructionHrri = Utils.GetHrri(instruction);
        string targetHrri = Utils.GetHrri(target);
        instructionHrriToInstruction[instructionHrri] = instruction;
        targetHrriToTarget[targetHrri] = target;
        instructionHrriToTargetHrri[instructionHrri] = targetHrri;
        targetHrriToInstructionHrri[targetHrri] = instructionHrri;
    }

    public void Unregister(GameObject instruction)
    {
        string instructionHrri = Utils.GetHrri(instruction);
        string targetHrri = instructionHrriToTargetHrri[instructionHrri];
        instructionHrriToInstruction.Remove(instructionHrri);
        targetHrriToTarget.Remove(targetHrri);
        instructionHrriToTargetHrri.Remove(instructionHrri);
        targetHrriToInstructionHrri.Remove(targetHrri);
    }

    public GameObject GetInstructionByHrri(string instructionHrri)
    {
        instructionHrriToInstruction.TryGetValue(instructionHrri, out GameObject instructionGo);
        return instructionGo;
    }

    public GameObject GetTargetByHrri(string hrri)
    {
        targetHrriToTarget.TryGetValue(hrri, out GameObject targetGo);
        return targetGo;
    }

    public string GetTargetHrriByInstructionHrri(string instructionHrri)
    {
        instructionHrriToTargetHrri.TryGetValue(instructionHrri, out string targetHrri);
        return targetHrri;
    }

    public string GetInstructionHrriByTargetHrri(string targetHrri)
    {
        targetHrriToInstructionHrri.TryGetValue(targetHrri, out string instructionHrri);
        return instructionHrri;
    }
}
