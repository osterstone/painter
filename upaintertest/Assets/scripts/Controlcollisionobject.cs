using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controlcollisionobject : MonoBehaviour
{
    public Vector3 startposition = new Vector3(0.0f, 0.0f, 12.0f );
    public Vector3 collisionPosition = new Vector3(0.0f, 0.0f, 6.5f);

    // Start is called before the first frame update
    void Start()
    {
        transform.position = startposition;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) //set rock collision active
        {
            transform.position = collisionPosition;

        }
        else if (Input.GetKeyDown(KeyCode.S)) //set rockcollision off. move rock under floor
        {
            transform.position = startposition;

        }

        else if (Input.GetKeyDown("escape"))
        {
            Application.Quit();
        }
    }
}
