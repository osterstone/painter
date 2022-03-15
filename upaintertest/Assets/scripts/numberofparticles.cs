using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class numberofparticles : MonoBehaviour
{
    ParticleSystem myPs;
    
    public float penPressure;
    public float maxNoOfParticles = 60;
    public float minNoOfParticles = 20;
    // Start is called before the first frame update
    void Start()
    {
       
        myPs = GetComponent<ParticleSystem>();
        var myPsemission = myPs.emission;
        myPsemission.rateOverTime = 30;
    }

    // Update is called once per frame
    void Update()
    {
        Pen pen = Pen.current;
        penPressure = pen.pressure.ReadValue();
        var myPsemission = myPs.emission;
        myPsemission.rateOverTime = minNoOfParticles+maxNoOfParticles*penPressure;
    }
}
