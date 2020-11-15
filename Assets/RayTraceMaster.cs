using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RayTraceMaster : MonoBehaviour
{
    public ComputeShader rayTracingShader;
    public Texture skyboxTexture;
    public Light directionalLight;
    public int maxSamples;
    public int sampleStep;
    public float marchDistance;
    public float maxMarchDistance;

    public TerrainManager terrainManager;
    //public ComputeShader denoisingShader;
    //public float blurIters;
    //public float depthBufferThreshold;

    public int randomSeed = 0;
    public Vector2 sphereRadius = new Vector2(3.0f, 8.0f);
    public uint spheresMax = 100;
    public float spherePlacementRadius = 100.0f;

    public bool reloadWorldTexture = false;
    
    private ComputeBuffer sphereBuffer;
    private RenderTexture target;
    private RenderTexture converged;
    private new Camera camera;

    private uint currentSample = 0;
    private float timer;
    private Material addMaterial;

    private Texture3D worldTexture;

    private void Awake()
    {
        camera = GetComponent<Camera>();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (transform.hasChanged)
        {
            currentSample = 0;
            timer = 0f;
            transform.hasChanged = false;
        }
    }

    private void SetShaderParameters()
    {
        rayTracingShader.SetMatrix("_CameraToWorld", camera.cameraToWorldMatrix);
        rayTracingShader.SetMatrix("_CameraInverseProjection", camera.projectionMatrix.inverse);
        rayTracingShader.SetTexture(0, "_SkyboxTexture", skyboxTexture);
        rayTracingShader.SetFloat("marchDistance", marchDistance);
        rayTracingShader.SetFloat("maxMarchDistance", maxMarchDistance);

        rayTracingShader.SetBuffer(0, "sphereBuffer", sphereBuffer);
        if (worldTexture == null) worldTexture = CreateWorldTexture();
        rayTracingShader.SetTexture(0, "worldTexture", worldTexture);

        Vector3 light = directionalLight.transform.forward;
        rayTracingShader.SetVector("directionalLight", new Vector4(light.x, light.y, light.z, directionalLight.intensity));
    }

    private void SetRandomParameters()
    {
        rayTracingShader.SetVector("pixelOffset", new Vector2(Random.value, Random.value));
        rayTracingShader.SetFloat("seed", Random.value);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        SetShaderParameters();
        // Make sure we have a current render target
        InitRenderTexture();

        if (currentSample < maxSamples)
        {
            for (int i = 0; i < sampleStep; i++)
            {
                SetRandomParameters();
                Render();
            }
        }
        Graphics.Blit(converged, destination);
    }

    private void Render()
    {
        if (currentSample < maxSamples)
        {
            // Set the target and dispatch the compute shader
            rayTracingShader.SetTexture(0, "Result", target);
            int threadGroupsX = Mathf.CeilToInt(Screen.width / 16.0f);
            int threadGroupsY = Mathf.CeilToInt(Screen.height / 16.0f);

            // set denoising data
            RenderTexture tempDepthBuffer = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
            tempDepthBuffer.enableRandomWrite = true;
            tempDepthBuffer.Create();
            RenderTexture tempSmoothBuffer = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            tempSmoothBuffer.enableRandomWrite = true;
            tempSmoothBuffer.Create();
            rayTracingShader.SetTexture(0, "depthBuffer", tempDepthBuffer);
            rayTracingShader.SetTexture(0, "smoothBuffer", tempSmoothBuffer);

            // dispatch the compute shader
            rayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

            // blit the result texture to converged texture
            if (addMaterial == null) addMaterial = new Material(Shader.Find("Hidden/AddShader"));
            addMaterial.SetFloat("sample", currentSample);
            Graphics.Blit(target, converged, addMaterial, 0);
            currentSample++;

            // release spare textures
            tempDepthBuffer.Release();
            tempSmoothBuffer.Release();
        }
    }

    private void InitRenderTexture()
    {
        if (target == null || converged == null || target.width != Screen.width || target.height != Screen.height)
        {
            // Release render texture if we already have one
            if (target != null) target.Release();
            if (converged != null) converged.Release();

            // Get a render target for Ray Tracing
            target = new RenderTexture(Screen.width, Screen.height, 0,RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            target.enableRandomWrite = true;
            target.Create();

            converged = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            converged.enableRandomWrite = true;
            converged.Create();

            // reset currentSample
            currentSample = 0;
            timer = 0f;
        }
    }

    private void OnEnable()
    {
        currentSample = 0;
        timer = 0f;
        SetUpScene();
    }

    private void OnDisable()
    {
        if (sphereBuffer != null) sphereBuffer.Release();
    }

    private void SetUpScene()
    {
        Random.InitState(randomSeed);
        List<Sphere> spheres = new List<Sphere>();
        // Add a number of random spheres
        for (int i = 0; i < spheresMax; i++)
        {
            Sphere sphere = new Sphere();
            // Radius and radius
            sphere.radius = sphereRadius.x + Random.value * (sphereRadius.y - sphereRadius.x);
            Vector2 randomPos = Random.insideUnitCircle * spherePlacementRadius;
            sphere.position = new Vector3(randomPos.x, 0f, randomPos.y);
            // Reject spheres that are intersecting others
            foreach (Sphere other in spheres)
            {
                float minDist = sphere.radius + other.radius;
                if (Vector3.SqrMagnitude(sphere.position - other.position) < minDist * minDist)
                    goto SkipSphere;
            }
            // Albedo and specular color
            Color color = Random.ColorHSV();
            bool metal = Random.value < 0.5f;
            bool emissive = Random.value < 0.4f && sphere.radius < sphereRadius.y * 0.75f;
            sphere.albedo = metal ? Vector3.zero : new Vector3(color.r, color.g, color.b);
            sphere.specular = metal ? new Vector3(color.r, color.g, color.b) : Vector3.one * 0.04f;
            sphere.smoothness = Random.value;
            sphere.emission = emissive ? new Vector3(color.r, color.g, color.b) * (Random.value * 10f + 1f) : Vector3.zero;
            // Add the sphere to the list
            spheres.Add(sphere);
            SkipSphere:
            continue;
        }
        // Assign to compute buffer
        sphereBuffer = new ComputeBuffer(spheres.Count, Sphere.GetSize());
        sphereBuffer.SetData(spheres);
    }

    struct Sphere
    {
        public Vector3 albedo;
        public Vector3 specular;
        public float smoothness;
        public Vector3 emission;
        public float radius;
        public Vector3 position;

        public static int GetSize()
        {
            return sizeof(float) * 14;
        }
    }

    public Texture3D CreateWorldTexture()
    {
        // Configure the texture
        int sizeX = (int)(terrainManager.maxChunks.x * terrainManager.terrainGenerator.chunkSize.x);
        int sizeY = (int)(terrainManager.maxChunks.y * terrainManager.terrainGenerator.chunkSize.y);
        int sizeZ = (int)(terrainManager.maxChunks.z * terrainManager.terrainGenerator.chunkSize.z);
        TextureFormat format = TextureFormat.R16;
        TextureWrapMode wrapMode = TextureWrapMode.Clamp;

        // Create the texture and apply the configuration
        Texture3D texture = new Texture3D(sizeX, sizeY, sizeZ, format, false);
        texture.wrapMode = wrapMode;
        texture.filterMode = FilterMode.Point;

        // get data
        ushort[] values = terrainManager.GenerateChunkData();
        
        // set data and return
        texture.SetPixelData(values, 0, 0);
        texture.Apply();
        return texture;
    }
}