using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{

    public List<string> dialogueList;
    public List<string> speakerList;

    public GameObject dialogueTBox;
    public GameObject speakerTBox;
    public GameObject speakerBGImage;

    public int currTextIndex = 0;

    public string nextSceneName;

    public GameObject cursor;

    // Start is called before the first frame update
    void Start()
    {
        currTextIndex = 0;
        dialogueTBox.GetComponent<Text>().text = "" + dialogueList[currTextIndex];
        speakerTBox.GetComponent<Text>().text = "" + speakerList[currTextIndex];
        if (cursor != null)
        {
            cursor.active = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currTextIndex += 1;
            if (currTextIndex >= dialogueList.Count)
            {
                if (cursor != null)
                {
                    cursor.active = true;
                }
                Scene thisScene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(nextSceneName);
                //SceneManager.SetActiveScene(nextScene);
                //SceneManager.UnloadScene(thisScene.name);
                return;
            }
            // TODO: Update Text on textboxes
            dialogueTBox.GetComponent<Text>().text = "" + dialogueList[currTextIndex];
            speakerTBox.GetComponent<Text>().text = "" + speakerList[currTextIndex];
        }


        //if (speakerList[currTextIndex].Equals(""))
        //{
        //    speakerTBox.active = false;
        //   speakerBGImage.active = false;
        //}
        //else
        //{
        //    speakerTBox.active = false;
        //    speakerBGImage.active = false;
        //}
    }
}
