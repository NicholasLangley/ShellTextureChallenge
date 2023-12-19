using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : MonoBehaviour
{
    public bool updateStatics;

    //Number of shells generated
    [Range(1, 256)]
    public int shellCount;

    //Total length of all shells
    [Range(0, 2)]
    public float shellLength;

    //How dense the shells will be with fur/grass -> multiples UV's by this coord then takes the floor for hashing
    [Range(10, 10000)]
    public float shellDensity;

    //How thick each strand is 
    [Range(0, 10)]
    public float thickness;

    //How much gravity/movement affects shell direction
    [Range(0, 1)]
    public float displacementStrength = 0.2f;

    [Range(0.0f, 10.0f)]
    public float curvature = 1.0f;

    public Color shellColor;

    public Mesh shellMesh;

    public Material shellMaterial;

    //How fast the ambientOcclusion takes effect
    [Range(0.0f, 5.0f)]
    public float occlusionAttenuation = 1.0f;

    //bias for ambient Occlusion to be a bit brighter
    [SerializeField]
    [Range(0, 1)]
    public float occlusionBias = 0.0f;

    //Swaps between height map textures to use for audio visualization
    [SerializeField]
    [Range(0, 1)]
    int textureSelector = 0;


    GameObject[] shells;

    private Vector3 displacementDirection = new Vector3(0, 0, 0);

    public AudioSource audioSource;
    public AudioTexturizer audioTex;

    void OnEnable()
    {
        createShellArray(shellCount);
    }

    private void OnDisable()
    {
        for (int i = 0; i<  shells.Length; i++)
        {
            Destroy(shells[i]);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            textureSelector = 0;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            textureSelector = 1;
        }

        //NEed to recreate whole array if number of shells changes
        if (shells.Length != shellCount)
        {
            for (int i = 0; i < shells.Length; i++)
            {
                Destroy(shells[i]);
            }
            shells = null;
            createShellArray(shellCount);
        }

        float velocity = 1.0f;

        Vector3 direction = new Vector3(0, 0, 0);

        // This determines the direction we are moving from wasd input. It's probably a better idea to use Unity's input system, since it handles
        // all possible input devices at once, but I did it the old fashioned way for simplicity.
        direction.x = Convert.ToInt32(Input.GetKey(KeyCode.D)) - Convert.ToInt32(Input.GetKey(KeyCode.A));
        direction.y = Convert.ToInt32(Input.GetKey(KeyCode.W)) - Convert.ToInt32(Input.GetKey(KeyCode.S));
        direction.z = Convert.ToInt32(Input.GetKey(KeyCode.Q)) - Convert.ToInt32(Input.GetKey(KeyCode.E));
        direction.Normalize();

        if (direction == Vector3.zero) { displacementDirection.y += 10.0f * Time.deltaTime; }
        else 
            {
            displacementDirection.x -= direction.x * Time.deltaTime * 10.0f;
            displacementDirection.y += direction.y * Time.deltaTime * 10.0f;
            displacementDirection.z -= direction.z * Time.deltaTime * 10.0f; 
            }

        if (displacementDirection.magnitude > 1) { displacementDirection.Normalize(); }

        Shader.SetGlobalVector("_DisplacementDirection", displacementDirection);

        updateAudioTexture();

        this.transform.localPosition += direction * velocity * Time.deltaTime;

        if (updateStatics)
        {
           

            for (int i = 0; i < shellCount; ++i)
            {
                //Set shader variables
                shells[i].GetComponent<MeshRenderer>().material.SetInt("_ShellCount", shellCount);
                shells[i].GetComponent<MeshRenderer>().material.SetInt("_ShellIndex", i);
                shells[i].GetComponent<MeshRenderer>().material.SetFloat("_ShellLength", shellLength);
                shells[i].GetComponent<MeshRenderer>().material.SetFloat("_Density", shellDensity);
                shells[i].GetComponent<MeshRenderer>().material.SetFloat("_Thickness", thickness);
                shells[i].GetComponent<MeshRenderer>().material.SetFloat("_DisplacementStrength", displacementStrength);
                shells[i].GetComponent<MeshRenderer>().material.SetFloat("_Curvature", curvature);
                shells[i].GetComponent<MeshRenderer>().material.SetColor("_ShellColor", shellColor);
                shells[i].GetComponent<MeshRenderer>().material.SetFloat("_Attenuation", occlusionAttenuation);
                shells[i].GetComponent<MeshRenderer>().material.SetFloat("_OcclusionBias", occlusionBias);
                shells[i].GetComponent<MeshRenderer>().material.SetInt("_TextureSelector", textureSelector);
            }
        }
    }


    //Creates a new array of shells
    private void createShellArray(int shellCount)
    {
        shells = new GameObject[shellCount];

        for (int i = 0; i < shellCount; i++)
        {
            //Create Shell
            shells[i] = new GameObject("Shell " + i.ToString());

            //Set Materials and meshes
            shells[i].AddComponent<MeshFilter>();
            shells[i].AddComponent<MeshRenderer>();
            shells[i].GetComponent<MeshFilter>().mesh = shellMesh;
            shells[i].GetComponent<MeshRenderer>().material = shellMaterial;

            //Rotate so it faces upwards
            //shells[i].transform.localRotation = Quaternion.Euler(90, 0, 0);
            shells[i].transform.SetParent(transform, false);
        }
    }

    private void updateAudioTexture()
    {
        for (int i = 0; i < shellCount; ++i)
        {
            //Set shader variables
            shells[i].GetComponent<MeshRenderer>().material.SetTexture("_AudioTex", audioTex.AudioTextureDiamond);
            shells[i].GetComponent<MeshRenderer>().material.SetTexture("_AudioTex1D", audioTex.AudioTexture1D);
        }
            
    }
}