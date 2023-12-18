using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class AudioExtractor : MonoBehaviour
{
    [SerializeField] Lasp.SpectrumAnalyzer _input = null;
    [SerializeField] bool _logScale = true;

    public NativeArray<float> _spectrumData;

    public int spectrumResolution = 512;

    public float[] spectrumBlocks;

    public float[] bufferedBlocks;

    public float[] bufferDecay;

    int bandSize = 8;

    public bool disableOutputSound = true;

    void OnEnable()
    {
        _input.resolution = spectrumResolution;

        spectrumBlocks = new float[bandSize];
        bufferedBlocks = new float[bandSize];
        bufferDecay = new float[bandSize];
    }

    // Update is called once per frame
    void Update()
    {
        _spectrumData = _logScale ? _input.logSpectrumArray : _input.spectrumArray;
        Debug.Log(_spectrumData.Length);
        //getAudioSpectrum();
        getSpectrumBlocks();
        bufferBlocks();
    }


    void getSpectrumBlocks()
    {

        int count = 0;
        for (int block = 0; block < spectrumBlocks.Length; block++)
        {
            float average = 0f;
            int samplesToAdd = (int)Mathf.Pow(2, block);
            for (int j =0; j < samplesToAdd; j++)
            {
                average += _spectrumData[count++];
            }
            average /= samplesToAdd;
            spectrumBlocks[block] = average;
        }
    }

    void bufferBlocks()
    {
        for (int i = 0; i < bandSize; i++)
        {
            if (spectrumBlocks[i] > bufferedBlocks[i])
            {
                bufferedBlocks[i] = spectrumBlocks[i];
                bufferDecay[i] = 0.002f;
            }
            else //new value is <= old value
            {
                bufferedBlocks[i] -= bufferDecay[i];
                bufferDecay[i] *= 1.1f;
            }
        }
    }
}
