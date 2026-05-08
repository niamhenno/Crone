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
        // Check tutorial is done before allowing interaction
        if (SuspicionManager.Instance == null) return;

        if (!SuspicionManager.Instance.CanTalkToNPC())
        {
            Debug.Log("Tutorial not complete yet — can't talk to " + gameObject.name);
            return;
        }

        // Run the Fungus block
        if (flowchart != null)
            flowchart.ExecuteBlock(blockName);
    }
}