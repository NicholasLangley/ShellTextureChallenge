using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioTexturizer : MonoBehaviour
{
    public Texture2D audioTexture;

    public AudioExtractor _audioExtractor;

    // Start is called before the first frame update
    void OnEnable()
    {
        createTexture();
    }

    // Update is called once per frame
    void Update()
    {
        drawBars();
    }

    void createTexture()
    {
        int texSize = 32;
        audioTexture = new Texture2D(texSize, texSize);
        Color[] colors = audioTexture.GetPixels();
        for (int i = 0; i < audioTexture.width * audioTexture.height; i++)
        {
            colors[i] = Color.black;
        }
        audioTexture.SetPixels(colors);
        audioTexture.Apply();
    }

    void drawBars()
    {
        Color[] colors = audioTexture.GetPixels();
        for (int x = 0; x < audioTexture.width; x++)
        {
            for (int y = 0; y < audioTexture.height; y++)
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
                colors[x * audioTexture.width + y] = c;
            }
        }
        audioTexture.SetPixels(colors);
        audioTexture.Apply();
    }

    int convertToSpectrumIndex(int x, int y)
    {
        int[] shape = { 8, 6, 4, 2, 0, 1, 3, 5, 7 };

        int spec = 0;
        float middle = (audioTexture.width / 2f) - 0.5f;
        spec += Mathf.FloorToInt(Mathf.Abs(x - middle));
        spec += Mathf.FloorToInt(Mathf.Abs(y - middle));
        spec = Mathf.FloorToInt(spec / 2);
        //spec -= 1;
        //if (spec < 0) {return 999; }
        return shape[spec % (_audioExtractor.bufferedBlocks.Length)];
    }
}
