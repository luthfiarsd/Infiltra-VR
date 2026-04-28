using UnityEngine;

public class WateringSystem : MonoBehaviour
{
    // Pastikan nama class di atas SAMA PERSIS dengan nama file .cs kamu
    public ParticleSystem waterParticles;
    public float tiltThreshold = 40f;

    void Start()
    {
        if (waterParticles != null) waterParticles.Stop();
    }

    void Update()
    {
        float currentTilt = transform.eulerAngles.x;

        if (currentTilt > 180) currentTilt -= 360;

        if (currentTilt > tiltThreshold)
        {
            if (!waterParticles.isEmitting) waterParticles.Play();
        }
        else
        {
            if (waterParticles.isEmitting) waterParticles.Stop();
        }
    }
}