using System;
using UnityEngine;
using UnityEngine.UI;

public class CUIColorPicker : MonoBehaviour
{
	public Color Color { get { return _color; } set { Setup( value ); } }
	public void SetOnValueChangeCallback( Action<Color> onValueChange )
	{
		_onValueChange = onValueChange;
	}
	private Color _color = Color.red;
	private Action<Color> _onValueChange;
	private Action _update = () => {};

	private static void RGBToHSV( Color color, out float h, out float s, out float v )
	{
		var cmin = Mathf.Min( color.r, color.g, color.b );
		var cmax = Mathf.Max( color.r, color.g, color.b );
		var d = cmax - cmin;
		if ( d == 0 ) {
			h = 0;
		} if ( cmax == color.r ) {
			h = Mathf.Repeat( ( color.g - color.b ) / d, 6 );
		} else if ( cmax == color.g ) {
			h = ( color.b - color.r ) / d + 2;
		} else {
			h = ( color.r - color.g ) / d + 4;
		}
		s = cmax == 0 ? 0 : d / cmax;
		v = cmax;
	}

	private static bool GetLocalMouse( Vector2 origin, Vector2 size, out Vector2 result ) 
	{
		var lm = ( Vector2 )Input.mousePosition - origin;
		var x = Mathf.Clamp( lm.x, 0, size.x );
		var y = Mathf.Clamp( lm.y, 0, size.y );
		result = new Vector2( x, y );
		return lm.x >= 0 && lm.y >= 0 && lm.x < size.x && lm.y < size.y;
	}

    private static Vector2 GetWidgetSize( GameObject go ) {
		var rt = go.transform as RectTransform;
		var corners = new Vector3[4];
		rt.GetWorldCorners( corners );
        return corners[2] - corners[0];
    }

	private GameObject GO( string name )
	{
		return transform.Find( name ).gameObject;
	}

	private void Setup( Color inputColor )
	{
		var satvalGO = GO( "SaturationValue" );
		var satvalKnob = GO( "SaturationValue/Knob" );
		var hueGO = GO( "Hue" );
		var hueKnob = GO( "Hue/Knob" );
		var result = GO( "Result" );
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
			var i0 = Mathf.Clamp( ( int )Hue, 0, 5 );
			var i1 = ( i0 + 1 ) % 6;
			var resultColor = Color.Lerp( hueColors[i0], hueColors[i1], Hue - i0 );
			satvalColors[3] = resultColor;
			resetSatValTexture();
		};
		Action applySaturationValue = () => {
			var sv = new Vector2( Saturation, Value );
			var isv = new Vector2( 1 - sv.x, 1 - sv.y );
			var c0 = isv.x * isv.y * satvalColors[0];
			var c1 = sv.x * isv.y * satvalColors[1];
			var c2 = isv.x * sv.y * satvalColors[2];
			var c3 = sv.x * sv.y * satvalColors[3];
			var resultColor = c0 + c1 + c2 + c3;
			var resImg = result.GetComponent<Image>();
			resImg.color = resultColor;
			if ( _onValueChange != null && _color != resultColor ) {
				_onValueChange( resultColor );
				_color = resultColor;
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
				if ( GetLocalMouse( hueGO.transform.position, hueSz, out mp ) ) {
					_update = dragH;
				} else if ( GetLocalMouse( satvalGO.transform.position, satvalSz, out mp ) ) {
					_update = dragSV;
				}
			}
		};
		dragH = () => {
			Vector2 mp;
			GetLocalMouse( hueGO.transform.position, hueSz, out mp );
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
			GetLocalMouse( satvalGO.transform.position, satvalSz, out mp );
			Saturation = mp.x / satvalSz.x;
			Value = mp.y / satvalSz.y;
			applySaturationValue();
			satvalKnob.transform.localPosition = mp;
			if ( Input.GetMouseButtonUp( 0 ) ) {
				_update = idle;
			}
		};
		_update = idle;
	}

	void Start()
	{
		Color = Color.red;
	}

	void Update()
	{
		_update();
	}
}
