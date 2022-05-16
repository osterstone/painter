using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class numberofparticles : MonoBehaviour
{
    ParticleSystem myPs;
    
    public float penPressure;
    public float maxNoOfParticles;
    public float minNoOfParticles;

    public float limitblub;
    public float limitblubnomove;
    public float limitlines;

    public OSC myosc;

    IEnumerator FadeDown()
    {
        for (float max=80.0f;max>=0; max -= 0.1f) 
        {
            maxNoOfParticles = max;
            yield return null;
        }
    }
    IEnumerator FadeUp()
    {
        for (float max = 0.0f; max < 80.0f; max += 0.1f)
        {
            maxNoOfParticles = max;
            yield return null;
        }
    }
    void Start()
    {
        maxNoOfParticles = 80;
        minNoOfParticles = 0;
        myPs = GetComponent<ParticleSystem>();
        var myPsemission = myPs.emission;
        myPsemission.rateOverTime = 30;
        Debug.Log(gameObject.name);

        limitblub = 500.00f;
        limitblubnomove = 150.0F;
        limitlines = 150.0f;

        myosc.SetAddressHandler("/zeichnen/maxbub", SetNewMaxForBub);
        myosc.SetAddressHandler("/zeichnen/maxbubnomove", SetNewMaxForBubnomove);
        myosc.SetAddressHandler("/zeichnen/lines", SetNewMaxForline);
    }
    public void SetNewMaxForBub(OscMessage message)
    {
        if (gameObject.name == "Particle System")
        {
            float factor = message.GetFloat(0);
            var main = myPs.main;
            main.maxParticles = Mathf.RoundToInt(limitblub * factor);
        }
    }
    public void SetNewMaxForBubnomove(OscMessage message)
    {
        if (gameObject.name == "ParticlesNoMovement")
        {
            float factor = message.GetFloat(0);
            var main = myPs.main;
            main.maxParticles = Mathf.RoundToInt(limitblubnomove * factor);
        }
    }
    public void SetNewMaxForline(OscMessage message)
    {
         if (gameObject.name == "ParticlesLines")
         {
              float factor = message.GetFloat(0);
              var main = myPs.main;
              main.maxParticles = Mathf.RoundToInt(limitlines * factor);
         }
     }
    
    
    // Update is called once per frame
    void Update()
    {
        Pen pen = Pen.current;
        penPressure = pen.pressure.ReadValue();
        var myPsemission = myPs.emission;
        myPsemission.rateOverTime = minNoOfParticles+maxNoOfParticles*penPressure;

        if ((gameObject.name=="ParticlesNoMovement") & (Input.GetKeyDown("d")))
        { 
            StartCoroutine(FadeDown());
        }

        if ((gameObject.name == "ParticlesNoMovement") & (Input.GetKeyDown("u")))
        {
            StartCoroutine(FadeUp());
        }
    }
}
