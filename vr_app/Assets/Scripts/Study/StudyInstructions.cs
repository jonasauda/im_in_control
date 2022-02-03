using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Serialization;

public class StudyInstructions : MonoBehaviour
{
    public enum Instruction
    {
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        I,
        J,
        K,
        L,
        Undefined
    }

    public GameObject blank;

    public enum MyParts
    {
        MicroProcessors,
        Circuits,
        Undefined
    }

    public Instruction instruction = Instruction.Undefined;
    public MyParts myParts = MyParts.Undefined;


  


    // Start is called before the first frame update
    void Start()
    {
        if (GameObject.FindGameObjectWithTag("GameManager").GetComponent<StudyConfig>().playground)
        {
            return;
        }
        
        ShuffleInstructions();
        
        
        GameObject instructionA = GameObject.Find("StudyInstructionsA");
        GameObject instructionB = GameObject.Find("StudyInstructionsB");
        GameObject instructionC = GameObject.Find("StudyInstructionsC");
        GameObject instructionD = GameObject.Find("StudyInstructionsD");
        GameObject instructionE = GameObject.Find("StudyInstructionsE");
        GameObject instructionF = GameObject.Find("StudyInstructionsF");
        GameObject instructionG = GameObject.Find("StudyInstructionsG");
        GameObject instructionH = GameObject.Find("StudyInstructionsH");
        GameObject instructionI = GameObject.Find("StudyInstructionsI");
        GameObject instructionJ = GameObject.Find("StudyInstructionsJ");
        GameObject instructionK = GameObject.Find("StudyInstructionsK");
        GameObject instructionL = GameObject.Find("StudyInstructionsL");
        
        List<GameObject> instructions = new List<GameObject>();
        instructions.Add(instructionA);
        instructions.Add(instructionB);
        instructions.Add(instructionC);
        instructions.Add(instructionD);
        instructions.Add(instructionE);
        instructions.Add(instructionF);
        instructions.Add(instructionG);
        instructions.Add(instructionH);
        instructions.Add(instructionI);
        instructions.Add(instructionJ);
        instructions.Add(instructionK);
        instructions.Add(instructionL);
        
        foreach (var inst in instructions)
        {
            print(inst.name);
            print(instruction.ToString());
            if (!inst.name.EndsWith(instruction.ToString()))
            {
                inst.SetActive(false);
            }
            else
            {
                Debug.Log("BLANK THE PART!");
                BlankPartsForCollaborator(inst);
            }
        }

    
    }

    private void ShuffleInstructions()
    {
                GameObject instructionsA = GameObject.Find("StudyInstructionsA");
                GameObject instructionsB = GameObject.Find("StudyInstructionsB");
                GameObject instructionsC = GameObject.Find("StudyInstructionsC");
                GameObject instructionsD = GameObject.Find("StudyInstructionsD");
                GameObject instructionsE = GameObject.Find("StudyInstructionsE");
                GameObject instructionsF = GameObject.Find("StudyInstructionsF");
                GameObject instructionsG = GameObject.Find("StudyInstructionsG");
                GameObject instructionsH = GameObject.Find("StudyInstructionsH");
                GameObject instructionsI = GameObject.Find("StudyInstructionsI");
                GameObject instructionsJ = GameObject.Find("StudyInstructionsJ");
                GameObject instructionsK = GameObject.Find("StudyInstructionsK");
                GameObject instructionsL = GameObject.Find("StudyInstructionsL");
        
        
                int[] permutationA = {8, 9, 5, 3, 1, 7, 4, 2, 11, 6, 10, 0};
                int[] permutationB = {8, 4, 0, 5, 11, 6, 3, 9, 1, 2, 10, 7};
                int[] permutationC = {10, 2, 4, 0, 8, 9, 11, 7, 1, 5, 6, 3};
                int[] permutationD = {7, 2, 11, 1, 0, 3, 5, 4, 10, 6, 9, 8};
                int[] permutationE = {4, 3, 9, 10, 1, 8, 7, 2, 5, 11, 0, 6};
                int[] permutationF = {0, 10, 2, 7, 8, 5, 6, 1, 9, 3, 4, 11};
                int[] permutationG = {3, 5, 4, 7, 10, 1, 0, 6, 9, 8, 2, 11};
                int[] permutationH = {8, 5, 1, 7, 11, 6, 3, 2, 0, 4, 9, 10};
                int[] permutationI = {10, 4, 11, 5, 3, 0, 8, 9, 6, 7, 1, 2};
                int[] permutationJ = {0, 6, 9, 3, 7, 5, 10, 4, 1, 2, 8, 11};
                int[] permutationK = {2, 11, 1, 0, 9, 4, 3, 7, 8, 5, 10, 6};
                int[] permutationL = {0, 4, 7, 5, 9, 11, 3, 1, 10, 6, 2, 8};
        
        
                Shuffle(instructionsA, permutationA);
                Shuffle(instructionsB, permutationB);
                Shuffle(instructionsC, permutationC);
                Shuffle(instructionsD, permutationD);
                Shuffle(instructionsE, permutationE);
                Shuffle(instructionsF, permutationF);
                Shuffle(instructionsG, permutationG);
                Shuffle(instructionsH, permutationH);
                Shuffle(instructionsI, permutationI);
                Shuffle(instructionsJ, permutationJ);
                Shuffle(instructionsK, permutationK);
                Shuffle(instructionsL, permutationL);
    }

    private void BlankPartsForCollaborator(GameObject instructionX)
    {
        for (var i = 0; i < instructionX.transform.childCount; i++)
        {
            var part = instructionX.transform.GetChild(i).gameObject;
            switch (myParts)
            {
                case MyParts.Circuits:
                {
                    if (!part.name.Contains("Circuit"))
                    {
                        Instantiate(blank, part.transform.position, part.transform.rotation);
                        part.SetActive(false);
                        Debug.Log("BLANK I: "+ i);
                    }

                    break;
                }
                case MyParts.MicroProcessors:
                {
                    if (!part.name.Contains("Microprocessor"))
                    {
                        Instantiate(blank, part.transform.position, part.transform.rotation);
                        part.SetActive(false);
                        Debug.Log("BLANK I: "+ i);
                    }

                    break;
                }
                case MyParts.Undefined:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
    private void Shuffle(GameObject instruction, int[] permutation) 
    {
        try
        {
            int childCount = instruction.transform.childCount;

            Vector3[] prevPositions = new Vector3[childCount];

            for (int i = 0; i < childCount; i++)
            {
                prevPositions[i] = instruction.transform.GetChild(i).position;
            }

            for (int i = 0; i < childCount; i++)
            {
                instruction.transform.GetChild(i).position = prevPositions[permutation[i]];
            }
        }
        catch (NullReferenceException e)
        {
            
        }
    }
}