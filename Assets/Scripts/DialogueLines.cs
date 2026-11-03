using UnityEngine;
using TMPro;

public class DialogueLines : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] TMP_Text dialogueText;

    [SerializeField] string[] timelineTextLines;

    int currentLine = 0;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NextDialogueLine()
    {
        currentLine ++;
        dialogueText.text = timelineTextLines[currentLine];
    }

}
