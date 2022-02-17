using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class particleseek : MonoBehaviour
{
    public Transform target;
    public float force = 10.0f;
    ParticleSystem ps;

    // Start is called before the first frame update
    void Start()
    {
        ps = GetComponent<ParticleSystem>(); 
    }

    // Update is called once per frame
    void LateUpdate()
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];
        ps.GetParticles(particles);

        for (int i = 0; i < particles.Length; i++)
        {
            ParticleSystem.Particle p = particles[i];
            Vector3 directionToTarget = (target.position - p.position).normalized;
            Vector3 seekForce = directionToTarget * force * Time.deltaTime;
            p.velocity += seekForce;

            particles[i] = p;


        }

        ps.SetParticles(particles, particles.Length);
    }
}
