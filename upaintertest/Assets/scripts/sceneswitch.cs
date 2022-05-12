using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;


public class sceneswitch : MonoBehaviour
{
    public OSC myosc;
    private void Start()
    {
        myosc.SetAddressHandler("/zeichnen/myra", SwitchToMyra);
        myosc.SetAddressHandler("/zeichnen/mypaint", SwitchToMypaint);
    }
    

    // Update is called once per frame
    void Update()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (Input.GetKeyDown(KeyCode.Return))
        {
           if (scene.name=="mypaint")
            {
                SceneManager.LoadScene("myra", LoadSceneMode.Single);
                SceneManager.SetActiveScene(SceneManager.GetSceneByName("myra"));
            }
           else if (scene.name == "myra")
            {
                SceneManager.LoadScene("mypaint", LoadSceneMode.Single);
                SceneManager.SetActiveScene(SceneManager.GetSceneByName("mypaint"));
            }

        }
    }


    public void SwitchToMypaint(OscMessage message)
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name != "mypaint")
        {
            SceneManager.LoadScene("mypaint", LoadSceneMode.Single);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("mypaint"));
        }
    }
    public void SwitchToMyra(OscMessage message)
    {
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name != "myra")
        {
            SceneManager.LoadScene("myra", LoadSceneMode.Single);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName("myra"));
        }
    }

}
