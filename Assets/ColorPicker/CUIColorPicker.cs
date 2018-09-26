/*   Created by zloedi. Forked by JulianDM1995 - Julián David Medina. 26/09/18


	If you want to move or scale the ColorPicker DONT MOVE "CUIColorPicker" Gameobject!!!
	Move or resize the "content" GO, or the "Content" childs instead.
	
	The "CUIColorPicker" GameObject should be the same size as its GO Parent, just like the "Exit" button.
	This means that it needs to be "Streched/Streched" and "0 left, 0 right, 0 top, 0 botton ".
	This is already configured by default. Dont touch it if you want to keep working fine :)
	*/

using System;
using UnityEngine;
using UnityEngine.UI;

public class CUIColorPicker : MonoBehaviour
{

	private GameObject panelColor;
	private GameObject buttonExit;
	public Color initialColor;
	private bool state = true;


	//HERE COMES YOUR OWN FUNCTIONS WITH NEW COLORS!!!!!!!!!

	public void yourFunctionWhenFinishSelectingColor(){
		Debug.Log("Function that runs when you finish selecting the color");
		var result = GO( "Content/FrameResult/Result" );
		Color c = result.GetComponent<Image> ().color;
	}

	public void yourFuncionOnChangeColor(){
		Debug.Log("Function that runs each time you change the selected color");
		var result = GO( "Content/FrameResult/Result" );
		Color c = result.GetComponent<Image> ().color;
	}


	public void Exit(){
		panelColor.GetComponent<RectTransform> ().localScale = new Vector3 (0, 0, 0);
		buttonExit.GetComponent<RectTransform> ().localScale = new Vector3 (0, 0, 0);
		panelColor.SetActive (false);
		buttonExit.SetActive (false);
		state = false;
		yourFunctionWhenFinishSelectingColor ();
	}

	public void Open(){
		
		panelColor.GetComponent<RectTransform> ().localScale = new Vector3 (1, 1, 1);
		buttonExit.GetComponent<RectTransform> ().localScale = new Vector3 (1, 1, 1);
		panelColor.SetActive (true);
		buttonExit.SetActive (true);
		state = true;
	}

    public Color Color { get { return _color; } set { Setup( value ); } }
    public void SetOnValueChangeCallback( Action<Color> onValueChange )
    {

        _onValueChange = onValueChange;
    }
    private Color _color = Color.red;
    private Action<Color> _onValueChange;
    private Action _update;

    private static void RGBToHSV( Color color, out float h, out float s, out float v )
    {
        var cmin = Mathf.Min( color.r, color.g, color.b );
        var cmax = Mathf.Max( color.r, color.g, color.b );
        var d = cmax - cmin;
        if ( d == 0 ) {
            h = 0;
        } else if ( cmax == color.r ) {
            h = Mathf.Repeat( ( color.g - color.b ) / d, 6 );
        } else if ( cmax == color.g ) {
            h = ( color.b - color.r ) / d + 2;
        } else {
            h = ( color.r - color.g ) / d + 4;
        }
        s = cmax == 0 ? 0 : d / cmax;
        v = cmax;
    }

    private static bool GetLocalMouse( GameObject go, out Vector2 result )
	{

		var rt = (RectTransform)go.transform;
		var mp = rt.InverseTransformPoint (Input.mousePosition);
		result.x = Mathf.Clamp (mp.x, rt.rect.min.x, rt.rect.max.x);
		result.y = Mathf.Clamp (mp.y, rt.rect.min.y, rt.rect.max.y);
		return rt.rect.Contains (mp);

	}

    private static Vector2 GetWidgetSize( GameObject go ) 
    {
        var rt = ( RectTransform )go.transform;
        return rt.rect.size;
    }

    private GameObject GO( string name )
    {
        return transform.Find( name ).gameObject;
    }

    private void Setup( Color inputColor )
    {
		panelColor = GO( "Content/ColorPanel" );
		buttonExit = GO( "ExitButton" );
		var satvalGO = GO( "Content/ColorPanel/SaturationValue" );
		var satvalKnob = GO( "Content/ColorPanel/SaturationValue/Knob" );
		var hueGO = GO( "Content/ColorPanel/Hue" );
		var hueKnob = GO( "Content/ColorPanel/Hue/Knob" );
		var result = GO( "Content/FrameResult/Result" );
        var hueColors = new Color [] {
            Color.red,
            Color.yellow,
            Color.green,
            Color.cyan,
            Color.blue,
            Color.magenta,
        };
        var satvalColors = new Color [] {
            new Color( 0, 0, 0 ),
            new Color( 0, 0, 0 ),
            new Color( 1, 1, 1 ),
            hueColors[0],
        };
        var hueTex = new Texture2D( 1, 7 );
        for ( int i = 0; i < 7; i++ ) {
            hueTex.SetPixel( 0, i, hueColors[i % 6] );
        }
        hueTex.Apply();
        hueGO.GetComponent<Image>().sprite = Sprite.Create( hueTex, new Rect( 0, 0.5f, 1, 6 ), new Vector2( 0.5f, 0.5f ) );
        var hueSz = GetWidgetSize( hueGO );
        var satvalTex = new Texture2D(2,2);
        satvalGO.GetComponent<Image>().sprite = Sprite.Create( satvalTex, new Rect( 0.5f, 0.5f, 1, 1 ), new Vector2( 0.5f, 0.5f ) );
        Action resetSatValTexture = () => {
            for ( int j = 0; j < 2; j++ ) {
                for ( int i = 0; i < 2; i++ ) {
                    satvalTex.SetPixel( i, j, satvalColors[i + j * 2] );
                }
            }
            satvalTex.Apply();
        };
        var satvalSz = GetWidgetSize( satvalGO );
        float Hue, Saturation, Value;
        RGBToHSV( inputColor, out Hue, out Saturation, out Value );
		Action applyHue = () => {
			if (state) {
				var i0 = Mathf.Clamp ((int)Hue, 0, 5);
				var i1 = (i0 + 1) % 6;
				var resultColor = Color.Lerp (hueColors [i0], hueColors [i1], Hue - i0);
				satvalColors [3] = resultColor;
				resetSatValTexture ();
			}
		};
		Action applySaturationValue = () => {
			if (state) {
				var sv = new Vector2 (Saturation, Value);
				var isv = new Vector2 (1 - sv.x, 1 - sv.y);
				var c0 = isv.x * isv.y * satvalColors [0];
				var c1 = sv.x * isv.y * satvalColors [1];
				var c2 = isv.x * sv.y * satvalColors [2];
				var c3 = sv.x * sv.y * satvalColors [3];
				var resultColor = c0 + c1 + c2 + c3;
				var resImg = result.GetComponent<Image> ();
				resImg.color = resultColor;
				if (_color != resultColor) {
					if (_onValueChange != null) {
						_onValueChange (resultColor);
					}
					yourFuncionOnChangeColor();
					_color = resultColor;
				}
			}
		};
        applyHue();
        applySaturationValue();
        satvalKnob.transform.localPosition = new Vector2( Saturation * satvalSz.x, Value * satvalSz.y );
        hueKnob.transform.localPosition = new Vector2( hueKnob.transform.localPosition.x, Hue / 6 * satvalSz.y );
        Action dragH = null;
        Action dragSV = null;
        Action idle = () => {
            if ( Input.GetMouseButtonDown( 0 ) ) {
                Vector2 mp;
				if(state){
	                if ( GetLocalMouse( hueGO, out mp ) ) {
	                    _update = dragH;
	                } else if ( GetLocalMouse( satvalGO, out mp ) ) {
	                    _update = dragSV;
	                }
				}
			}
		};
        dragH = () => {
            Vector2 mp;
            GetLocalMouse( hueGO, out mp );
            Hue = mp.y / hueSz.y * 6;
            applyHue();
            applySaturationValue();
            hueKnob.transform.localPosition = new Vector2( hueKnob.transform.localPosition.x, mp.y );
            if ( Input.GetMouseButtonUp( 0 ) ) {
                _update = idle;
            }
        };
        dragSV = () => {
            Vector2 mp;
            GetLocalMouse( satvalGO, out mp );
            Saturation = mp.x / satvalSz.x;
            Value = mp.y / satvalSz.y;
            applySaturationValue();
            satvalKnob.transform.localPosition = mp;
            if ( Input.GetMouseButtonUp( 0 ) ) {
                _update = idle;
            }
        };
        _update = idle;
		Exit();
    }

    public void SetRandomColor()
    {
		state = true;
		Debug.Log ("random");
		var rng = new System.Random();
        var r = ( rng.Next() % 1000 ) / 1000.0f;
        var g = ( rng.Next() % 1000 ) / 1000.0f;
        var b = ( rng.Next() % 1000 ) / 1000.0f;
        Color = new Color( r, g, b );
		state = false;
    }

    void Awake()
    {
		Color = initialColor;
    }

    void Update()
    {
        _update();
    }


}
