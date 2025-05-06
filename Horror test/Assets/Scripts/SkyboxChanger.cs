using UnityEngine;

public class SkyboxCycler : MonoBehaviour
{
    public Material[] skyboxes;
    private static int currentSkyboxIndex = -1; // static to persist between triggers

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && skyboxes.Length > 0)
        {
            // Advance and loop through skyboxes
            currentSkyboxIndex = (currentSkyboxIndex + 1) % skyboxes.Length;
            RenderSettings.skybox = skyboxes[currentSkyboxIndex];
            DynamicGI.UpdateEnvironment();

            Debug.Log($"Skybox changed to: {skyboxes[currentSkyboxIndex].name}");
        }
    }
}
