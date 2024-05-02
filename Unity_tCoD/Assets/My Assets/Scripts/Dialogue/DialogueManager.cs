using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour
{
    #region Variables

    [Header("Params")]
    [SerializeField] private float typingSpeed = 0.04f;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI displayNameText;
    [SerializeField] private Animator npcPortraitAnimator;
    [SerializeField] private Animator playerPortraitAnimator;
    [Header("Start and End Animations")]
    [SerializeField] private Animator startAnimation;

    [Header("Choices UI")]
    [SerializeField] private GameObject[] choices;
    private TextMeshProUGUI[] choicesText;

    private static DialogueManager instance;
    private Story currentStory;
    public bool dialogueIsPlaying;
    private Coroutine displayLineCoroutine;
    private bool canContinueToNextLine = false;
    private float startTimer = 0f;

    private const string SPEAKER_TAG = "speaker";
    private const string NPC_PORTRAIT_TAG = "npcportrait";
    private const string PLAYER_PORTRAIT_TAG = "playerportrait";
    private const string LAYOUT_TAG = "layout";

    #endregion Variables

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Found more then one Dialogue Manager in the scene");
        }
        instance = this;
    }

    public static DialogueManager GetInstance()
    {
        return instance;
    }

    private void Start()
    {
        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);
        //StartDialogueAnimation();
        // get all the choices text

        choicesText = new TextMeshProUGUI[choices.Length];

        int index = 0;
        foreach (GameObject choice in choices)
        {
            choicesText[index] = choice.GetComponentInChildren<TextMeshProUGUI>();
            index++;
        }
    }

    private void Update()
    {
        if (!dialogueIsPlaying)
        {
            return;
        }

        if (Input.GetKeyDown("space"))
            if (canContinueToNextLine
                && currentStory.currentChoices.Count == 0
                && Input.GetKeyDown("space"))
            {
                ContinueStory();
            }
    }

    public void EnterDialogueMode(TextAsset inkJSON)
    {
        
        currentStory = new Story(inkJSON.text);
        dialogueIsPlaying = true;
        dialoguePanel.SetActive(true);
        //startAnimation.Play("StartDialogue");

      
        ContinueStory();
           
    }

    private IEnumerator ExitDialogueMode()
    {
        yield return new WaitForSeconds(0.2f);

        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
    }

    private void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            if (displayLineCoroutine != null)
            {
                StopCoroutine(displayLineCoroutine);
            }
            displayLineCoroutine = StartCoroutine(DisplayLine(currentStory.Continue()));
            //Set text for current Dialogue line


            HandleTags(currentStory.currentTags);
        }
        else
        {
            StartCoroutine(ExitDialogueMode());
        }



    }

    private void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;

        // Defencive check if UI can support the number of choices coming in
        if (currentChoices.Count > choices.Length)
        {
            Debug.LogError("More choices were given then UI can support. Number of choices given:" + currentChoices.Count);
        }

        int index = 0;
        // enable ant initialize the choices up to the amoune of choices for this line of dialogue

        foreach (Choice choice in currentChoices)
        {
            choices[index].gameObject.SetActive(true);
            choicesText[index].text = choice.text;
            index++;
        }

        // go through the remaining choices the UI supports and make sure they're hidden
        for (int i = index; i < choices.Length; i++)
        {
            choices[i].gameObject.SetActive(false);
        }

        StartCoroutine(SelectFirstChoice());

    }

    private IEnumerator SelectFirstChoice()
    {

        // Event System reaquires we clear it first, then wait
        // for at least one frame before we set current selected object.


        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();
        EventSystem.current.SetSelectedGameObject(choices[0].gameObject);
    }

    public void MakeChoice(int choiceIndex)
    {
        if (canContinueToNextLine)
        {
            currentStory.ChooseChoiceIndex(choiceIndex);
        }
    }

    private void HandleTags(List<string> currentTags)
    {
        foreach (string tag in currentTags)
        {
            string[] splitTag = tag.Split(':');
            if (splitTag.Length < 2)
            {
                Debug.LogError("Wrong tag: " + tag);
            }

            string tagKey = splitTag[0].Trim();
            string tagValue = splitTag[1].Trim();

            switch (tagKey)
            {
                case SPEAKER_TAG:

                    displayNameText.text = tagValue;
                    break;

                case NPC_PORTRAIT_TAG:
                    npcPortraitAnimator.Play(tagValue);
                    break;

                case PLAYER_PORTRAIT_TAG:
                    playerPortraitAnimator.Play(tagValue);
                    break;

                case LAYOUT_TAG:
                    startAnimation.Play(tagValue);
                    break;

                default:
                    Debug.LogWarning("Tag came in but not being handled: " + tag);
                    break;

            }

        }



    }

    private IEnumerator DisplayLine(string line)
    {
        dialogueText.text = "";
        canContinueToNextLine = false;

        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;

            yield return new WaitForSeconds(typingSpeed);
        }


        DisplayChoices();

        canContinueToNextLine = true;

    }

}