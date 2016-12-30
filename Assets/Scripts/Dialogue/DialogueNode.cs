using UnityEngine;
using System.Collections;
using System.Xml;
using System.Collections.Generic;

namespace DialogueSystem
{
    public class DialogueNode
    {
        public bool isNodeDecision;
        public int numberOfNPCLines;
        public int numberPlayerAnswers;
        public int chosenAnswer;    // Change this to 1, 2, 3, or whatever number is associated with the answer
        public int identification;
        public int goToNode = 0;

        // Add a lines variable
        public List<string> NPCLines = new List<string> ();
        public List<string> PlayerLines = new List<string> ();
    }
}