using UnityEngine;

public class ParallaxEffectManager : MonoBehaviour
{
    private Transform cameraTransform; // Reference to the main camera's transform
    private Vector3 initialCameraPosition; // Stores the starting position of the camera
    private float cameraMovementDistance; // Tracks how far the camera has moved horizontally from initial position

    private GameObject[] parallaxLayers; // Stores background layers
    private Material[] layerMaterials; // Stores materials of the layers
    private float[] parallaxSpeeds; // Stores calculated speeds for each layer
    private float furthestLayerDepth = 0.0f; // Holds the depth value of the farthest background layer

    [Header("Parallax Settings")]
    [Range(0.01f, 0.05f)]
    public float parallaxEffectMultiplier = 0.02f; // Controls parallax effect speed

    [Range(0f, 0.5f)]
    public float singleLayerScrollSpeed = 0.2f; // Used when only one layer exists

    private void Start()
    {
        InitializeParallax();
    }

    private void InitializeParallax()
    {
        // Get the main camera's transform and store its initial position
        cameraTransform = Camera.main.transform;
        initialCameraPosition = cameraTransform.position;

        // Get the number of child layers (backgrounds)
        int layerCount = transform.childCount;
        parallaxLayers = new GameObject[layerCount];
        layerMaterials = new Material[layerCount];
        parallaxSpeeds = new float[layerCount];

        // Loop through child objects to store references and materials
        for (int i = 0; i < layerCount; i++)
        {
            parallaxLayers[i] = transform.GetChild(i).gameObject;
            layerMaterials[i] = parallaxLayers[i].GetComponent<Renderer>().material;
        }

        CalculateParallaxSpeeds(layerCount);
    }

    private void CalculateParallaxSpeeds(int layerCount)
    {
        for (int i = 0; i < layerCount; i++)
        {
            // Calculate the Z-axis distance from the camera to the layer
            float distanceFromCamera = parallaxLayers[i].transform.position.z - cameraTransform.position.z;

            // Check if this is the farthest layer
            if (distanceFromCamera > furthestLayerDepth)
                furthestLayerDepth = distanceFromCamera;

            // Calculate and assign the parallax speed based on current depth
            parallaxSpeeds[i] = 1 - (distanceFromCamera / furthestLayerDepth);
        }
    }


    private void LateUpdate()
    {
        ApplyParallaxEffect();
    }

    private void ApplyParallaxEffect()
    {
        // Calculate horizontal movement of the camera
        cameraMovementDistance = cameraTransform.position.x - initialCameraPosition.x;

        // Move the parent object to follow the camera position (to prevent floating point issues)
        transform.position = new Vector3(cameraTransform.position.x, transform.position.y, 0);

        // Apply parallax effect to each background layer
        for (int i = 0; i < parallaxLayers.Length; i++)
        {
            float speedFactor = parallaxSpeeds[i] * parallaxEffectMultiplier;
            layerMaterials[i].SetTextureOffset("_MainTex", new Vector2(cameraMovementDistance, 0) * speedFactor);
        }

        // Continuous scrolling for single-layer backgrounds
        if (parallaxLayers.Length == 1)
        {
            cameraMovementDistance += singleLayerScrollSpeed * Time.deltaTime;
            layerMaterials[0].SetTextureOffset("_MainTex", Vector2.right * cameraMovementDistance);
        }
    }
}
