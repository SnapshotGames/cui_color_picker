using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CUIColorPicker : MonoBehaviour
{
    [SerializeField]
    private GameObject saturationValue;
    [SerializeField]
    private GameObject saturationValueKnob;
    [SerializeField]
    private GameObject hue;
    [SerializeField]
    private GameObject hueKnob;
    [SerializeField]
    private GameObject result;
    [SerializeField]
    private Color color = Color.white;

    [Space]
    public UnityEvent<Color> onValueChanged;

    private Action update;
    private Color[] hueColors;
    private Color[] saturationValueColors;
    private Texture2D hueTexture;  
    private Texture2D saturationValueTexture;
    private Vector2 hueSize;
    private Vector2 saturationValueSize;

    public Color Color
    {
        get
        {
            return color;
        }

        set
        {
            color = value;
        }
    }

    private void Awake()
    {
        if (saturationValue == null)
            throw new Exception("The saturationValue can't be null");

        if (saturationValueKnob == null)
            throw new Exception("The saturationValueKnob can't be null");

        if (hue == null)
            throw new Exception("The hue can't be null");

        if (hueKnob == null)
            throw new Exception("The hueKnob can't be null");

        if (result == null)
            throw new Exception("The result can't be null");

        hueColors = new Color[] {
            Color.red,
            Color.yellow,
            Color.green,
            Color.cyan,
            Color.blue,
            Color.magenta,
        };

        saturationValueColors = new Color[] {
            new Color( 0, 0, 0 ),
            new Color( 0, 0, 0 ),
            new Color( 1, 1, 1 ),
            hueColors[0],
        };

        hueTexture = new Texture2D(1, 7);
        for (int i = 0; i < 7; i++)
            hueTexture.SetPixel(0, i, hueColors[i % 6]);
        hueTexture.Apply();

        saturationValueTexture = new Texture2D(2, 2);

        hue.GetComponent<Image>().sprite = Sprite.Create(hueTexture, new Rect(0, 0.5f, 1, 6), new Vector2(0.5f, 0.5f));
        hueSize = ((RectTransform)hue.transform).rect.size;

        saturationValue.GetComponent<Image>().sprite = Sprite.Create(saturationValueTexture, new Rect(0.5f, 0.5f, 1, 1), new Vector2(0.5f, 0.5f));
        saturationValueSize = ((RectTransform)saturationValue.transform).rect.size;
    }

    private void Update()
    {
        if (gameObject.activeSelf)
            update();
    }

    private void Start()
    {
        Color.RGBToHSV(color, out float colorHue, out float colorSaturation, out float colorValue);

        ApplyHue(colorHue);
        ApplySaturationValue(colorSaturation, colorValue);

        saturationValueKnob.transform.localPosition = new Vector2(colorSaturation * saturationValueSize.x, colorValue * saturationValueSize.y);
        hueKnob.transform.localPosition = new Vector2(hueKnob.transform.localPosition.x, colorHue / 6 * saturationValueSize.y);

        Action dragH = null;
        Action dragSV = null;

        Action idle = () =>
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (GetLocalMouse(hue, out Vector2 mp))
                    update = dragH;
                else if (GetLocalMouse(saturationValue, out mp))
                    update = dragSV;
            }
        };

        dragH = () =>
        {
            GetLocalMouse(hue, out Vector2 mp);
            colorHue = mp.y / hueSize.y * 6;
            ApplyHue(colorHue);
            ApplySaturationValue(colorSaturation, colorValue);
            hueKnob.transform.localPosition = new Vector2(hueKnob.transform.localPosition.x, mp.y);
            if (Input.GetMouseButtonUp(0))
            {
                update = idle;
            }
        };

        dragSV = () =>
        {
            GetLocalMouse(saturationValue, out Vector2 mp);
            colorSaturation = mp.x / saturationValueSize.x;
            colorValue = mp.y / saturationValueSize.y;
            ApplySaturationValue(colorSaturation, colorValue);
            saturationValueKnob.transform.localPosition = mp;
            if (Input.GetMouseButtonUp(0))
            {
                update = idle;
            }
        };

        update = idle;
    }

    private void ResetSaturationValueTexture()
    {
        for (int j = 0; j < 2; j++)
            for (int i = 0; i < 2; i++)
                saturationValueTexture.SetPixel(i, j, saturationValueColors[i + j * 2]);
        saturationValueTexture.Apply();
    }

    private void ApplyHue(float colorHue)
    {
        int i0 = Mathf.Clamp((int)colorHue, 0, 5);
        int i1 = (i0 + 1) % 6;
        Color resultColor = Color.Lerp(hueColors[i0], hueColors[i1], colorHue - i0);
        saturationValueColors[3] = resultColor;
        ResetSaturationValueTexture();
    }

    private void ApplySaturationValue(float colorSaturation, float colorValue)
    {
        Vector2 sv = new Vector2(colorSaturation, colorValue);
        Vector2 isv = new Vector2(1 - sv.x, 1 - sv.y);
        Color c0 = isv.x * isv.y * saturationValueColors[0];
        Color c1 = sv.x * isv.y * saturationValueColors[1];
        Color c2 = isv.x * sv.y * saturationValueColors[2];
        Color c3 = sv.x * sv.y * saturationValueColors[3];
        Color resultColor = c0 + c1 + c2 + c3;
        Image resultImage = result.GetComponent<Image>();
        resultImage.color = resultColor;
        if (color != resultColor)
        {
            onValueChanged?.Invoke(resultColor);
            color = resultColor;
        }
    }

    private static bool GetLocalMouse(GameObject go, out Vector2 result)
    {
        var rt = (RectTransform)go.transform;
        var mp = rt.InverseTransformPoint(Input.mousePosition);
        result.x = Mathf.Clamp(mp.x, rt.rect.min.x, rt.rect.max.x);
        result.y = Mathf.Clamp(mp.y, rt.rect.min.y, rt.rect.max.y);
        return rt.rect.Contains(mp);
    }
}
