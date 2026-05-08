using UnityEngine;
using Fungus;

public class NPCInteract : MonoBehaviour
{
    [Tooltip("Drag your Flowchart here")]
    public Flowchart flowchart;

    [Tooltip("Exact name of the Fungus block to run")]
    public string blockName;

    void OnMouseDown()
    {
        if (!SuspicionManager.Instance.CanTalkToNPC())
        {
            Debug.Log("Tutorial not complete yet.");
            return;
        }

        if (flowchart != null)
            flowchart.ExecuteBlock(blockName);
    }
}
    
