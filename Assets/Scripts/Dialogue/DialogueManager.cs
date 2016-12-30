using UnityEngine;
using System.Collections;
using System.Xml;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

namespace DialogueSystem
{
    [System.Serializable]
    public class DialogueManager : MonoBehaviour
    {
        // Constants for Xml tags

        #region Text variables
        public TextAsset sceneDialogueScript;
        public string targetNPCTag, targetPlayerTag;
        private XmlDocument xmlDoc;
        #endregion
        #region Information variables
        public string targetInfoTag;
        private string dialogueName;
#pragma warning disable 0649
        private XmlNodeList dialogueNodesXml;
#pragma warning restore 0649
        private List<DialogueNode> dialogueNodes;
        private int currentNode = 0;
        private int currentNPCLine = 0;
        private bool endOfConversation = false;
        #endregion
        #region UI variables
        public Text[] uiEditorElements;
        public string[] uiKeynamesDictionary;
        private Dictionary<string, Text> textUIElements;
        private KeyCode nextKey = KeyCode.Space;    // Expose publicly to allow access to this from other scripts
        private bool isPlayerTurn;
        private bool typewritingText;
        public string textPlayerAnswer;
        #endregion

        private void Start () {
            InitializeEverything ();
            
            LoadConversationInfo ();
            
            currentNPCLine = 0;
            currentNode = 0;
            textUIElements["dialogueName"].text = dialogueName;
            NextTextLine ();
        }
        private void Update () {
            if (Input.GetKeyUp(nextKey) && !isPlayerTurn && !endOfConversation) {
                if (!typewritingText) {
                    NextTextLine ();
                } else {
                    StopAllCoroutines ();
                    textUIElements["textDisplayer"].text = dialogueNodes[currentNode].NPCLines[currentNPCLine];
                    currentNPCLine++;
                    typewritingText = false;
                }
            }
            if (isPlayerTurn && !endOfConversation) {
                int temp = CheckKeyValue ();
                if (temp <= 5 && temp > 0) {
                    CheckPlayerAnswer (temp);
                }
            }
        }
        
        public void NextTextLine () {
            if (currentNode >= dialogueNodes.Count) {
                StartCoroutine (AnimateText ("End of Conversation"));
                textUIElements["textInfoKeys"].text = "";
                endOfConversation = true;
                return;
            }
            // If the NPC has finished talking and it is time for next Node
            if (currentNPCLine == dialogueNodes[currentNode].numberOfNPCLines) {
                // Display player answers
                isPlayerTurn = true;
                textUIElements["textInfoKeys"].text = "Press a number to choose an option";
                SetUpAnswerText ();
                // Get next DialogueNode
                //displayedNPCLines += currentNPCLine;
                currentNPCLine = 0;
            } else {    // The NPC has not finished their lines
                // The next line is displayed
                textUIElements["textInfoKeys"].text = "Next (Space)";
                StartCoroutine (AnimateText (dialogueNodes[currentNode].NPCLines[currentNPCLine]));
            }
        }

        private void InitializeEverything () {
            xmlDoc = new XmlDocument ();
            xmlDoc.LoadXml (sceneDialogueScript.text);
            dialogueNodes = new List<DialogueNode> ();

            textUIElements = new Dictionary<string, Text> ();
            for (int i = 0; i < uiKeynamesDictionary.Length; i++) {
                textUIElements.Add (uiKeynamesDictionary[i], uiEditorElements[i]);
            }

            isPlayerTurn = false;
            typewritingText = false;

            ClearAnswerText ();
        }
        private void ClearAnswerText (int exception = 0) {
            for (int i = 1; i < 6; i++) {
                if (i != exception) { textUIElements[textPlayerAnswer + i].text = ""; }
            }
        }
        private void SetUpAnswerText () {
            for (int i = 0; i < dialogueNodes[currentNode].numberPlayerAnswers; i++) {
                textUIElements[textPlayerAnswer + (i + 1)].text = dialogueNodes[currentNode].PlayerLines[i];
            }
        }
        private void CheckPlayerAnswer (int chosenNumber) {
            // Validate input of the player
            if (chosenNumber > 0 && chosenNumber <= dialogueNodes[currentNode].numberPlayerAnswers) {
                // Store the chosen option
                dialogueNodes[currentNode].chosenAnswer = chosenNumber;
                ClearAnswerText (chosenNumber);
                isPlayerTurn = false;
                currentNode++;
                NextTextLine ();
            }
        }
        private int CheckKeyValue () {
            if      (Input.GetKeyDown (KeyCode.Alpha1)) { return 1; }
            else if (Input.GetKeyDown (KeyCode.Alpha2)) { return 2; }
            else if (Input.GetKeyDown (KeyCode.Alpha3)) { return 3; }
            else if (Input.GetKeyDown (KeyCode.Alpha4)) { return 4; }
            else if (Input.GetKeyDown (KeyCode.Alpha5)) { return 5; }
            else { return 0; }
        }
        //private void LoadXmlLines (XmlNodeList theNodeList, List<string> theTextList, string theTargetTag) {
        //    theNodeList = xmlDoc.GetElementsByTagName (theTargetTag);

        //    foreach (XmlNode node in theNodeList) {
        //        foreach (XmlNode text in node) {
        //            theTextList.Add (text.InnerText);
        //        }
        //    }
        //}
        private void LoadConversationInfo () {
            dialogueNodesXml = xmlDoc.GetElementsByTagName (targetInfoTag);

            XmlNodeList tempList = xmlDoc.GetElementsByTagName ("DialogueName");
            XmlNode tempNode = tempList.Item (0);
            dialogueName = tempNode.InnerText;
            
            foreach (XmlNode node in dialogueNodesXml) {
                DialogueNode dn = new DialogueNode ();
                dn.NPCLines.Clear ();
                dn.PlayerLines.Clear ();

                foreach (XmlNode item in node) {
                    if (item.Name == "numberNPCLines")      { dn.numberOfNPCLines = Int32.Parse (item.InnerText); }
                    if (item.Name == "numberPlayerAnswers") { dn.numberPlayerAnswers = Int32.Parse (item.InnerText); }
                    if (item.Name == "identification")      { dn.identification = Int32.Parse (item.InnerText); }
                    if (item.Name == "nodeGoesTo")          { dn.goToNode = Int32.Parse (item.InnerText); }
                    if (item.Name == "isNodeDecision") {
                        if (Int32.Parse (item.InnerText) == 0) {
                            dn.isNodeDecision = false;
                        } else if (Int32.Parse (item.InnerText) == 1) {
                            dn.isNodeDecision = true;
                        } else {
                            Debug.LogError ("isNodeDecision in Xml does not equal 0 or 1");
                        }
                    }
                    if (item.Name == "NPCLines") {
                        XmlNodeList textList = item.ChildNodes;
                        foreach (XmlNode n in textList) {
                            dn.NPCLines.Add (n.InnerText);
                        }
                    }
                    if (item.Name == "PlayerLines") {
                        XmlNodeList textList = item.ChildNodes;
                        foreach (XmlNode n in textList) {
                            dn.PlayerLines.Add (n.InnerText);
                        }
                    }
                }

                dialogueNodes.Add (dn);
            }
        }

        private IEnumerator AnimateText (string textLine) {
            typewritingText = true;
            for (int i = 0; i < textLine.Length + 1; i++) {
                textUIElements["textDisplayer"].text = textLine.Substring (0, i);
                yield return new WaitForSeconds (0.03f);
            }
            typewritingText = false;
            currentNPCLine++;
        }
    }
}