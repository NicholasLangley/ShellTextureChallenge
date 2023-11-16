using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioExtractor : MonoBehaviour
{
    public AudioSource _audio;

    float[] _spectrumData;

    public float[] spectrumBlocks;

    public float[] bufferedBlocks;

    public float[] bufferDecay;

    int bandSize = 8;

    void OnEnable()
    {
        _spectrumData = new float[512];
        spectrumBlocks = new float[bandSize];
        bufferedBlocks = new float[bandSize];
        bufferDecay = new float[bandSize];
    }

    // Update is called once per frame
    void Update()
    {
        getAudioSpectrum();
        getSpectrumBlocks();
        bufferBlocks();
    }

    void getAudioSpectrum()
    {
        _audio.GetSpectrumData(_spectrumData, 0, FFTWindow.BlackmanHarris); 
    }

    void getSpectrumBlocks()
    {
        
        //spectrumBlocks[0] = (_spectrumData[0] + _spectrumData[1]) / 2.0f;
        //spectrumBlocks[1] = (_spectrumData[2] + _spectrumData[3]) / 2.0f;
        //spectrumBlocks[2] = (_spectrumData[4] + _spectrumData[5]) / 2.0f;
        //spectrumBlocks[3] = (_spectrumData[6] + _spectrumData[7])/ 2.0f;

        int count = 0;
        for (int block = 0; block < spectrumBlocks.Length; block++)
        {
            float average = 0f;
            int samplesToAdd = (int)Mathf.Pow(2, block);
            for (int j =0; j < samplesToAdd; j++)
            {
                average += _spectrumData[count++] * (count);
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

    public void setAudioSource(AudioSource a)
    {
        _audio = a;
    }
}
