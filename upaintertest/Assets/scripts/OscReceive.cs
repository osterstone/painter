using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OscReceive : MonoBehaviour
{
    public OSC myosc;

    // Start is called before the first frame update
    void Start()
    {
        myosc.SetAddressHandler("/zeichnen/pen", OnReceiveZeichnen);
        myosc.SetAllMessageHandler(OnReceiveZeichnen);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnReceiveZeichnen(OscMessage message)
    {
        Debug.Log(message.address);
        Debug.Log(message.GetInt(0));
    }
}
