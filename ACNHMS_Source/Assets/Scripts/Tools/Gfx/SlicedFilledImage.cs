using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_2017_4 || UNITY_2018_2_OR_NEWER
using UnityEngine.U2D;
#endif
using Sprites = UnityEngine.Sprites;

#if UNITY_EDITOR
using UnityEditor;

// Custom Editor to order the variables in the Inspector similar to Image component
[CustomEditor( typeof( SlicedFilledImage ) )]
public class SlicedFilledImageEditor : Editor
{
	private SerializedProperty spriteProp, colorProp;
	private GUIContent spriteLabel;

	private void OnEnable()
	{
		spriteProp = serializedObject.FindProperty( "m_Sprite" );
		colorProp = serializedObject.FindProperty( "m_Color" );
		spriteLabel = new GUIContent( "Source Image" );
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.PropertyField( spriteProp, spriteLabel );
		EditorGUILayout.PropertyField( colorProp );
		DrawPropertiesExcluding( serializedObject, "m_Script", "m_Sprite", "m_Color", "m_OnCullStateChanged" );

		serializedObject.ApplyModifiedProperties();
	}
}
#endif

// Credit: https://bitbucket.org/Unity-Technologies/ui/src/2018.4/UnityEngine.UI/UI/Core/Image.cs
[AddComponentMenu( "UI/Sliced Filled Image", 11 )]
public class SlicedFilledImage : MaskableGraphic, ISerializationCallbackReceiver, ILayoutElement, ICanvasRaycastFilter
{
	private static class SetPropertyUtility
	{
		public static bool SetStruct<T>( ref T currentValue, T newValue ) where T : struct
		{
			if( EqualityComparer<T>.Default.Equals( currentValue, newValue ) )
				return false;

			currentValue = newValue;
			return true;
		}

		public static bool SetClass<T>( ref T currentValue, T newValue ) where T : class
		{
			if( ( currentValue == null && newValue == null ) || ( currentValue != null && currentValue.Equals( newValue ) ) )
				return false;

			currentValue = newValue;
			return true;
		}
	}

	public enum FillDirection { Right = 0, Left = 1, Up = 2, Down = 3 }

	private static readonly Vector3[] s_Vertices = new Vector3[4];
	private static readonly Vector2[] s_UVs = new Vector2[4];
	private static readonly Vector2[] s_SlicedVertices = new Vector2[4];
	private static readonly Vector2[] s_SlicedUVs = new Vector2[4];

#pragma warning disable 1692
#pragma warning disable IDE1006 // Suppress 'Naming rule violation' warnings
#pragma warning disable 0649
	[SerializeField]
	private Sprite m_Sprite;
	public Sprite sprite
	{
		get { return m_Sprite; }
		set
		{
			if( SetPropertyUtility.SetClass( ref m_Sprite, value ) )
			{
				SetAllDirty();
				TrackImage();
			}
		}
	}

	[SerializeField]
	private FillDirection m_FillDirection;
	public FillDirection fillDirection
	{
		get { return m_FillDirection; }
		set
		{
			if( SetPropertyUtility.SetStruct( ref m_FillDirection, value ) )
				SetVerticesDirty();
		}
	}

	[Range( 0, 1 )]
	[SerializeField]
	private float m_FillAmount = 1f;
	public float fillAmount
	{
		get { return m_FillAmount; }
		set
		{
			if( SetPropertyUtility.SetStruct( ref m_FillAmount, Mathf.Clamp01( value ) ) )
				SetVerticesDirty();
		}
	}

	[SerializeField]
	private bool m_FillCenter = true;
	public bool fillCenter
	{
		get { return m_FillCenter; }
		set
		{
			if( SetPropertyUtility.SetStruct( ref m_FillCenter, value ) )
				SetVerticesDirty();
		}
	}

	[SerializeField]
	private float m_PixelsPerUnitMultiplier = 1f;
	public float pixelsPerUnitMultiplier
	{
		get { return m_PixelsPerUnitMultiplier; }
		set { m_PixelsPerUnitMultiplier = Mathf.Max( 0.01f, value ); }
	}

	public float pixelsPerUnit
	{
		get
		{
			float spritePixelsPerUnit = 100;
			if( activeSprite )
				spritePixelsPerUnit = activeSprite.pixelsPerUnit;

			float referencePixelsPerUnit = 100;
			if( canvas )
				referencePixelsPerUnit = canvas.referencePixelsPerUnit;

			return m_PixelsPerUnitMultiplier * spritePixelsPerUnit / referencePixelsPerUnit;
		}
	}
#pragma warning restore 0649

	[NonSerialized]
	private Sprite m_OverrideSprite;
	public Sprite overrideSprite
	{
		get { return activeSprite; }
		set
		{
			if( SetPropertyUtility.SetClass( ref m_OverrideSprite, value ) )
			{
				SetAllDirty();
				TrackImage();
			}
		}
	}

	private Sprite activeSprite { get { return m_OverrideSprite != null ? m_OverrideSprite : m_Sprite; } }

	public override Texture mainTexture
	{
		get
		{
			if( activeSprite != null )
				return activeSprite.texture;

			return material != null && material.mainTexture != null ? material.mainTexture : s_WhiteTexture;
		}
	}

	public bool hasBorder
	{
		get
		{
			if( activeSprite != null )
			{
				Vector4 v = activeSprite.border;
				return v.sqrMagnitude > 0f;
			}

			return false;
		}
	}

	public override Material material
	{
		get
		{
			if( m_Material != null )
				return m_Material;

			if( activeSprite && activeSprite.associatedAlphaSplitTexture != null )
			{
#if UNITY_EDITOR
				if( Application.isPlaying )
#endif
					return Image.defaultETC1GraphicMaterial;
			}

			return defaultMaterial;
		}
		set { base.material = value; }
	}

	public float alphaHitTestMinimumThreshold { get; set; }
#pragma warning restore IDE1006
#pragma warning restore 1692

	protected SlicedFilledImage()
	{
		useLegacyMeshGeneration = false;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		TrackImage();
	}

	protected override void OnDisable()
	{
		base.OnDisable();

		if( m_Tracked )
			UnTrackImage();
	}

#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		m_PixelsPerUnitMultiplier = Mathf.Max( 0.01f, m_PixelsPerUnitMultiplier );
	}
#endif

	protected override void OnPopulateMesh( VertexHelper vh )
	{
		if( activeSprite == null )
		{
			base.OnPopulateMesh( vh );
			return;
		}

		GenerateSlicedFilledSprite( vh );
	}

	/// <summary>
	/// Update the renderer's material.
	/// </summary>
	protected override void UpdateMaterial()
	{
		base.UpdateMaterial();

		// Check if this sprite has an associated alpha texture (generated when splitting RGBA = RGB + A as two textures without alpha)
		if( activeSprite == null )
		{
			canvasRenderer.SetAlphaTexture( null );
			return;
		}

		Texture2D alphaTex = activeSprite.associatedAlphaSplitTexture;
		if( alphaTex != null )
			canvasRenderer.SetAlphaTexture( alphaTex );
	}

	private void GenerateSlicedFilledSprite( VertexHelper vh )
	{
		vh.Clear();

		if( m_FillAmount < 0.001f )
			return;

		Rect rect = GetPixelAdjustedRect();
		Vector4 outer = Sprites.DataUtility.GetOuterUV( activeSprite );
		Vector4 padding = Sprites.DataUtility.GetPadding( activeSprite );

		if( !hasBorder )
		{
			Vector2 size = activeSprite.rect.size;

			int spriteW = Mathf.RoundToInt( size.x );
			int spriteH = Mathf.RoundToInt( size.y );

			// Image's dimensions used for drawing. X = left, Y = bottom, Z = right, W = top.
			Vector4 vertices = new Vector4(
				rect.x + rect.width * ( padding.x / spriteW ),
				rect.y + rect.height * ( padding.y / spriteH ),
				rect.x + rect.width * ( ( spriteW - padding.z ) / spriteW ),
				rect.y + rect.height * ( ( spriteH - padding.w ) / spriteH ) );

			GenerateFilledSprite( vh, vertices, outer, m_FillAmount );
			return;
		}

		Vector4 inner = Sprites.DataUtility.GetInnerUV( activeSprite );
		Vector4 border = GetAdjustedBorders( activeSprite.border / pixelsPerUnit, rect );

		padding = padding / pixelsPerUnit;

		s_SlicedVertices[0] = new Vector2( padding.x, padding.y );
		s_SlicedVertices[3] = new Vector2( rect.width - padding.z, rect.height - padding.w );

		s_SlicedVertices[1].x = border.x;
		s_SlicedVertices[1].y = border.y;

		s_SlicedVertices[2].x = rect.width - border.z;
		s_SlicedVertices[2].y = rect.height - border.w;

		for( int i = 0; i < 4; ++i )
		{
			s_SlicedVertices[i].x += rect.x;
			s_SlicedVertices[i].y += rect.y;
		}

		s_SlicedUVs[0] = new Vector2( outer.x, outer.y );
		s_SlicedUVs[1] = new Vector2( inner.x, inner.y );
		s_SlicedUVs[2] = new Vector2( inner.z, inner.w );
		s_SlicedUVs[3] = new Vector2( outer.z, outer.w );

		float rectStartPos;
		float _1OverTotalSize;
		if( m_FillDirection == FillDirection.Left || m_FillDirection == FillDirection.Right )
		{
			rectStartPos = s_SlicedVertices[0].x;

			float totalSize = ( s_SlicedVertices[3].x - s_SlicedVertices[0].x );
			_1OverTotalSize = totalSize > 0f ? 1f / totalSize : 1f;
		}
		else
		{
			rectStartPos = s_SlicedVertices[0].y;

			float totalSize = ( s_SlicedVertices[3].y - s_SlicedVertices[0].y );
			_1OverTotalSize = totalSize > 0f ? 1f / totalSize : 1f;
		}

		for( int x = 0; x < 3; x++ )
		{
			int x2 = x + 1;

			for( int y = 0; y < 3; y++ )
			{
				if( !m_FillCenter && x == 1 && y == 1 )
					continue;

				int y2 = y + 1;

				float sliceStart, sliceEnd;
				switch( m_FillDirection )
				{
					case FillDirection.Right:
						sliceStart = ( s_SlicedVertices[x].x - rectStartPos ) * _1OverTotalSize;
						sliceEnd = ( s_SlicedVertices[x2].x - rectStartPos ) * _1OverTotalSize;
						break;
					case FillDirection.Up:
						sliceStart = ( s_SlicedVertices[y].y - rectStartPos ) * _1OverTotalSize;
						sliceEnd = ( s_SlicedVertices[y2].y - rectStartPos ) * _1OverTotalSize;
						break;
					case FillDirection.Left:
						sliceStart = 1f - ( s_SlicedVertices[x2].x - rectStartPos ) * _1OverTotalSize;
						sliceEnd = 1f - ( s_SlicedVertices[x].x - rectStartPos ) * _1OverTotalSize;
						break;
					case FillDirection.Down:
						sliceStart = 1f - ( s_SlicedVertices[y2].y - rectStartPos ) * _1OverTotalSize;
						sliceEnd = 1f - ( s_SlicedVertices[y].y - rectStartPos ) * _1OverTotalSize;
						break;
					default: // Just there to get rid of the "Use of unassigned local variable" compiler error
						sliceStart = sliceEnd = 0f;
						break;
				}

				if( sliceStart >= m_FillAmount )
					continue;

				Vector4 vertices = new Vector4( s_SlicedVertices[x].x, s_SlicedVertices[y].y, s_SlicedVertices[x2].x, s_SlicedVertices[y2].y );
				Vector4 uvs = new Vector4( s_SlicedUVs[x].x, s_SlicedUVs[y].y, s_SlicedUVs[x2].x, s_SlicedUVs[y2].y );
				float fillAmount = ( m_FillAmount - sliceStart ) / ( sliceEnd - sliceStart );

				GenerateFilledSprite( vh, vertices, uvs, fillAmount );
			}
		}
	}

	private Vector4 GetAdjustedBorders( Vector4 border, Rect adjustedRect )
	{
		Rect originalRect = rectTransform.rect;

		for( int axis = 0; axis <= 1; axis++ )
		{
			float borderScaleRatio;

			// The adjusted rect (adjusted for pixel correctness) may be slightly larger than the original rect.
			// Adjust the border to match the adjustedRect to avoid small gaps between borders (case 833201).
			if( originalRect.size[axis] != 0 )
			{
				borderScaleRatio = adjustedRect.size[axis] / originalRect.size[axis];
				border[axis] *= borderScaleRatio;
				border[axis + 2] *= borderScaleRatio;
			}

			// If the rect is smaller than the combined borders, then there's not room for the borders at their normal size.
			// In order to avoid artefacts with overlapping borders, we scale the borders down to fit.
			float combinedBorders = border[axis] + border[axis + 2];
			if( adjustedRect.size[axis] < combinedBorders && combinedBorders != 0 )
			{
				borderScaleRatio = adjustedRect.size[axis] / combinedBorders;
				border[axis] *= borderScaleRatio;
				border[axis + 2] *= borderScaleRatio;
			}
		}

		return border;
	}

	private void GenerateFilledSprite( VertexHelper vh, Vector4 vertices, Vector4 uvs, float fillAmount )
	{
		if( m_FillAmount < 0.001f )
			return;

		float uvLeft = uvs.x;
		float uvBottom = uvs.y;
		float uvRight = uvs.z;
		float uvTop = uvs.w;

		if( fillAmount < 1f )
		{
			if( m_FillDirection == FillDirection.Left || m_FillDirection == FillDirection.Right )
			{
				if( m_FillDirection == FillDirection.Left )
				{
					vertices.x = vertices.z - ( vertices.z - vertices.x ) * fillAmount;
					uvLeft = uvRight - ( uvRight - uvLeft ) * fillAmount;
				}
				else
				{
					vertices.z = vertices.x + ( vertices.z - vertices.x ) * fillAmount;
					uvRight = uvLeft + ( uvRight - uvLeft ) * fillAmount;
				}
			}
			else
			{
				if( m_FillDirection == FillDirection.Down )
				{
					vertices.y = vertices.w - ( vertices.w - vertices.y ) * fillAmount;
					uvBottom = uvTop - ( uvTop - uvBottom ) * fillAmount;
				}
				else
				{
					vertices.w = vertices.y + ( vertices.w - vertices.y ) * fillAmount;
					uvTop = uvBottom + ( uvTop - uvBottom ) * fillAmount;
				}
			}
		}

		s_Vertices[0] = new Vector3( vertices.x, vertices.y );
		s_Vertices[1] = new Vector3( vertices.x, vertices.w );
		s_Vertices[2] = new Vector3( vertices.z, vertices.w );
		s_Vertices[3] = new Vector3( vertices.z, vertices.y );

		s_UVs[0] = new Vector2( uvLeft, uvBottom );
		s_UVs[1] = new Vector2( uvLeft, uvTop );
		s_UVs[2] = new Vector2( uvRight, uvTop );
		s_UVs[3] = new Vector2( uvRight, uvBottom );

		int startIndex = vh.currentVertCount;

		for( int i = 0; i < 4; i++ )
			vh.AddVert( s_Vertices[i], color, s_UVs[i] );

		vh.AddTriangle( startIndex, startIndex + 1, startIndex + 2 );
		vh.AddTriangle( startIndex + 2, startIndex + 3, startIndex );
	}

	int ILayoutElement.layoutPriority { get { return 0; } }
	float ILayoutElement.minWidth { get { return 0; } }
	float ILayoutElement.minHeight { get { return 0; } }
	float ILayoutElement.flexibleWidth { get { return -1; } }
	float ILayoutElement.flexibleHeight { get { return -1; } }

	float ILayoutElement.preferredWidth
	{
		get
		{
			if( activeSprite == null )
				return 0;

			return Sprites.DataUtility.GetMinSize( activeSprite ).x / pixelsPerUnit;
		}
	}

	float ILayoutElement.preferredHeight
	{
		get
		{
			if( activeSprite == null )
				return 0;

			return Sprites.DataUtility.GetMinSize( activeSprite ).y / pixelsPerUnit;
		}
	}

	void ILayoutElement.CalculateLayoutInputHorizontal() { }
	void ILayoutElement.CalculateLayoutInputVertical() { }

	bool ICanvasRaycastFilter.IsRaycastLocationValid( Vector2 screenPoint, Camera eventCamera )
	{
		if( alphaHitTestMinimumThreshold <= 0 )
			return true;

		if( alphaHitTestMinimumThreshold > 1 )
			return false;

		if( activeSprite == null )
			return true;

		Vector2 local;
		if( !RectTransformUtility.ScreenPointToLocalPointInRectangle( rectTransform, screenPoint, eventCamera, out local ) )
			return false;

		Rect rect = GetPixelAdjustedRect();

		// Convert to have lower left corner as reference point.
		local.x += rectTransform.pivot.x * rect.width;
		local.y += rectTransform.pivot.y * rect.height;

		Rect spriteRect = activeSprite.rect;
		Vector4 border = activeSprite.border;
		Vector4 adjustedBorder = GetAdjustedBorders( border / pixelsPerUnit, rect );

		for( int i = 0; i < 2; i++ )
		{
			if( local[i] <= adjustedBorder[i] )
				continue;

			if( rect.size[i] - local[i] <= adjustedBorder[i + 2] )
			{
				local[i] -= ( rect.size[i] - spriteRect.size[i] );
				continue;
			}

			float lerp = Mathf.InverseLerp( adjustedBorder[i], rect.size[i] - adjustedBorder[i + 2], local[i] );
			local[i] = Mathf.Lerp( border[i], spriteRect.size[i] - border[i + 2], lerp );
		}

		// Normalize local coordinates.
		Rect textureRect = activeSprite.textureRect;
		Vector2 normalized = new Vector2( local.x / textureRect.width, local.y / textureRect.height );

		// Convert to texture space.
		float x = Mathf.Lerp( textureRect.x, textureRect.xMax, normalized.x ) / activeSprite.texture.width;
		float y = Mathf.Lerp( textureRect.y, textureRect.yMax, normalized.y ) / activeSprite.texture.height;

		switch( m_FillDirection )
		{
			case FillDirection.Right:
				if( x > m_FillAmount )
					return false;
				break;
			case FillDirection.Left:
				if( 1f - x > m_FillAmount )
					return false;
				break;
			case FillDirection.Up:
				if( y > m_FillAmount )
					return false;
				break;
			case FillDirection.Down:
				if( 1f - y > m_FillAmount )
					return false;
				break;
		}

		try
		{
			return activeSprite.texture.GetPixelBilinear( x, y ).a >= alphaHitTestMinimumThreshold;
		}
		catch( UnityException e )
		{
			Debug.LogError( "Using alphaHitTestMinimumThreshold greater than 0 on Image whose sprite texture cannot be read. " + e.Message + " Also make sure to disable sprite packing for this sprite.", this );
			return true;
		}
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize() { }
	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		m_FillAmount = Mathf.Clamp01( m_FillAmount );
	}

	// Whether this is being tracked for Atlas Binding
	private bool m_Tracked = false;

#if UNITY_2017_4 || UNITY_2018_2_OR_NEWER
	private static List<SlicedFilledImage> m_TrackedTexturelessImages = new List<SlicedFilledImage>();
	private static bool s_Initialized;
#endif

	private void TrackImage()
	{
		if( activeSprite != null && activeSprite.texture == null )
		{
#if UNITY_2017_4 || UNITY_2018_2_OR_NEWER
			if( !s_Initialized )
			{
				SpriteAtlasManager.atlasRegistered += RebuildImage;
				s_Initialized = true;
			}

			m_TrackedTexturelessImages.Add( this );
#endif
			m_Tracked = true;
		}
	}

	private void UnTrackImage()
	{
#if UNITY_2017_4 || UNITY_2018_2_OR_NEWER
		m_TrackedTexturelessImages.Remove( this );
#endif
		m_Tracked = false;
	}

#if UNITY_2017_4 || UNITY_2018_2_OR_NEWER
	private static void RebuildImage( SpriteAtlas spriteAtlas )
	{
		for( int i = m_TrackedTexturelessImages.Count - 1; i >= 0; i-- )
		{
			SlicedFilledImage image = m_TrackedTexturelessImages[i];
			if( spriteAtlas.CanBindTo( image.activeSprite ) )
			{
				image.SetAllDirty();
				m_TrackedTexturelessImages.RemoveAt( i );
			}
		}
	}
#endif
}