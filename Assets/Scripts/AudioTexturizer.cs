using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTexturizer : MonoBehaviour
{
    public Texture2D AudioTextureDiamond;
    public Texture2D AudioTexture1D;

    public AudioExtractor _audioExtractor;

    int reduce1Dby;

    // Start is called before the first frame update
    void OnEnable()
    {
        reduce1Dby = 10;
        createTextureDiamond();
        createTexture1D();
    }

    // Update is called once per frame
    void Update()
    {
        drawBars();
        draw1D();
    }

    void createTextureDiamond()
    {
        int texSize = 32;
        AudioTextureDiamond = new Texture2D(texSize, texSize);
        Color[] colors = AudioTextureDiamond.GetPixels();
        for (int i = 0; i < AudioTextureDiamond.width * AudioTextureDiamond.height; i++)
        {
            colors[i] = Color.black;
        }
        AudioTextureDiamond.SetPixels(colors);
        AudioTextureDiamond.Apply();
    }

    void createTexture1D()
    {
        int texSize = Mathf.FloorToInt(_audioExtractor.spectrumResolution / reduce1Dby);
        AudioTexture1D = new Texture2D(texSize, 1);
        Color[] colors = AudioTexture1D.GetPixels();
        for (int i = 0; i < AudioTexture1D.width * AudioTexture1D.height; i++)
        {
            colors[i] = Color.black;
        }
        AudioTexture1D.SetPixels(colors);
        AudioTexture1D.Apply();
    }

    void drawBars()
    {
        Color[] colors = AudioTextureDiamond.GetPixels();
        for (int x = 0; x < AudioTextureDiamond.width; x++)
        {
            for (int y = 0; y < AudioTextureDiamond.height; y++)
            {
                Color c;
                int spec = convertToSpectrumIndex(x, y);
                if (spec >= _audioExtractor.bufferedBlocks.Length)
                {
                    c = Color.black;
                }
                else
                {
                    c = new Color(_audioExtractor.bufferedBlocks[spec], _audioExtractor.bufferedBlocks[spec]*100, 0, 1);
                }
                colors[x * AudioTextureDiamond.width + y] = c;
            }
        }
        AudioTextureDiamond.SetPixels(colors);
        AudioTextureDiamond.Apply();
    }

    int convertToSpectrumIndex(int x, int y)
    {
        int[] shape = { 8, 6, 4, 2, 0, 1, 3, 5, 7 };

        int spec = 0;
        float middle = (AudioTextureDiamond.width / 2f) - 0.5f;
        spec += Mathf.FloorToInt(Mathf.Abs(x - middle));
        spec += Mathf.FloorToInt(Mathf.Abs(y - middle));
        spec = Mathf.FloorToInt(spec / 2);
        //spec -= 1;
        //if (spec < 0) {return 999; }
        return shape[spec % (_audioExtractor.bufferedBlocks.Length)];
    }

    void draw1D()
    {
        int count = 0;
        Color[] colors = AudioTexture1D.GetPixels();
        for (int i = 0; i < colors.Length; i++)
        {
            float average = 0f;
            int samplesToAdd = reduce1Dby;
            for (int j = 0; j < samplesToAdd; j++)
            {
                average += _audioExtractor.bufferedSpectrum[count++];
            }
            average /= samplesToAdd;

            colors[i] = new Color(average, 0, 0, 1);
        }
        AudioTexture1D.SetPixels(colors);
        AudioTexture1D.Apply();
    }
}
