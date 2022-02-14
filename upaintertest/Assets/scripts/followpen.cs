using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class followpen : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        Pen pen = Pen.current;
        Vector3 position = pen.position.ReadValue();
        position.z = 88.0f;

        Vector3 worldposition = Camera.main.ScreenToWorldPoint(position);
        transform.position =  worldposition;
    }
}
