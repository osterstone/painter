using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class followpen : MonoBehaviour
{
    public float zposition = 88.0f;
    private Vector2 currentInputVector;
    private Vector2 smoothInputVelocity;
    [SerializeField]
    private float smoothInputSpeed = .6f;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;

        

        //set damping for each scene separately
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name == "mypaint")
        {
            smoothInputSpeed = 0.0f;
        }
        else if (scene.name == "myra")
        {
            smoothInputSpeed = .6f;
        }

    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        Pen pen = Pen.current;
        Vector3 position = pen.position.ReadValue();
        Vector2 pos;
        pos.x = position.x;
        pos.y = position.y;

        currentInputVector = Vector2.SmoothDamp(currentInputVector,pos, ref smoothInputVelocity,smoothInputSpeed);
        position.x = currentInputVector.x;
        position.y = currentInputVector.y;
        position.z = zposition;

        Vector3 worldposition = Camera.main.ScreenToWorldPoint(position);
        transform.position =  worldposition;
    }
}
