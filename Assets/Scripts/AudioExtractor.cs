using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class AudioExtractor : MonoBehaviour
{
    [SerializeField] Lasp.SpectrumAnalyzer _input;
    [SerializeField] bool _logScale = true;

    [SerializeField]
    public NativeArray<float> _spectrumData;

    public int spectrumResolution = 512;

    public float[] spectrumBlocks;

    public float[] bufferedBlocks;

    public float[] bufferDecay;

    public float[] bufferedSpectrum;
    public float[] bufferDecaySpectrum;

    int bandSize = 8;

    public float spectrumArea;
    public float averageSpectrumArea;

    Queue AverageSpectrumAreaQueue;

    void OnEnable()
    {
        _input.resolution = spectrumResolution;
        _spectrumData = _logScale ? _input.logSpectrumArray : _input.spectrumArray;

        spectrumBlocks = new float[bandSize];
        bufferedBlocks = new float[bandSize];
        bufferDecay = new float[bandSize];

        bufferedSpectrum = new float[spectrumResolution];
        bufferDecaySpectrum = new float[spectrumResolution];

        cleanBlocks();
        cleanSpectrum();

        AverageSpectrumAreaQueue = new Queue();
        for (int i = 0; i < 10; i++) { AverageSpectrumAreaQueue.Enqueue(0.0f); }
        InvokeRepeating("CalculateAverageSpectrumArea", 0.1f, 0.1f);//repeat every 0.1s ~1s lag for loud changes
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    // Update is called once per frame
    void Update()
    {
        cleanBlocks();
        cleanSpectrum();

        _spectrumData = _logScale ? _input.logSpectrumArray : _input.spectrumArray;

        getSpectrumBlocks();
        bufferBlocks();

        augmentSpectrum();
        bufferSpectrum();
        calculateSpectrumArea();
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
                if (bufferedBlocks[i] < 0.0f) { bufferedBlocks[i] = 0; bufferDecay[i] = 0; }
            }
        }
    }

    void cleanBlocks()
    {
        for (int i = 0; i < bandSize; i++)
        {
            if (float.IsNaN(spectrumBlocks[i]) || float.IsInfinity(spectrumBlocks[i])) { spectrumBlocks[i] = 0; }
            if (float.IsNaN(bufferedBlocks[i]) || float.IsInfinity(bufferedBlocks[i])) { bufferedBlocks[i] = 0; }
        }

    }

    void augmentSpectrum()
    {
        for (int i = 0; i < spectrumResolution; i++)
        {
            var aug = Mathf.Pow(_spectrumData[i], 1 - _spectrumData[i]);
            if (!float.IsNaN(aug)) { _spectrumData[i] = aug; }
        }
    }

    void bufferSpectrum()
    {
        for (int i = 0; i < spectrumResolution; i++)
        {
            if (_spectrumData[i] > bufferedSpectrum[i])
            {
                bufferedSpectrum[i] = _spectrumData[i];
                bufferDecaySpectrum[i] = 0.002f;
            }
            else //new value is <= old value
            {
                bufferedSpectrum[i] -= bufferDecaySpectrum[i];
                bufferDecaySpectrum[i] *= 1.1f;
                if (bufferedSpectrum[i] < 0.0f) { bufferedSpectrum[i] = 0; bufferDecaySpectrum[i] = 0; }
            }
        }
    }

    void calculateSpectrumArea()
    {
        spectrumArea = 0.0f;
        for (int i = 0; i < spectrumResolution; i++)
        {
            spectrumArea += Mathf.Max(_spectrumData[i], 0.00f);
        }
        spectrumArea /= spectrumResolution;
    }

    void CalculateAverageSpectrumArea()
    {
        var avg = 0.0f;
        AverageSpectrumAreaQueue.Dequeue();
        AverageSpectrumAreaQueue.Enqueue(spectrumArea);
        foreach(float f in AverageSpectrumAreaQueue)
        {
            avg += f;
        }
        averageSpectrumArea = avg / AverageSpectrumAreaQueue.Count;
    }

    void cleanSpectrum()
    {
        for (int i = 0; i < spectrumResolution; i++)
        {
            if (float.IsNaN(_spectrumData[i]) || float.IsInfinity(_spectrumData[i])) { _spectrumData[i] = 0; }
            if (float.IsNaN(bufferedSpectrum[i]) || float.IsInfinity(bufferedSpectrum[i])) { bufferedSpectrum[i] = 0; }
        }

    }
}
