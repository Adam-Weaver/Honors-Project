using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueController : MonoBehaviour
{

    public List<string> dialogueList;
    public List<string> speakerList;

    public GameObject dialogueTBox;
    public GameObject speakerTBox;
    public GameObject speakerBGImage;

    public int currTextIndex = 0;

    public string nextSceneName;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currTextIndex += 1;
            if (currTextIndex >= dialogueList.Count)
            {

                Scene thisScene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(nextSceneName);
                //SceneManager.SetActiveScene(nextScene);
                //SceneManager.UnloadScene(thisScene.name);
                return;
            }
            // TODO: Update Text on textboxes
        }
    }
}
