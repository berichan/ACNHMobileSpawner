using UnityEngine;

// idk why I commented this so much I was high on tea or something idk
public class UVMove : MonoBehaviour {

	// the material index on the shader
	public int materialIndex = 0;

	// name of texture in shader
	public string textureName = "_MainTex";

	// the direction and speed of movement (doesn't get normalised)
	public Vector2 uvAnimationRate = new Vector2( 1.0f, 0.0f );

	// should we also move uvs of the bump map?
	public bool includeBump = false;

	// what's the bump map's name in the shader?
	public string bumpName = "_BumpMap";

	/* private vars */

	// the amount to be offsetted by this frame
	private Vector2 uvOffset = Vector2.zero;

	// lateupdate so it happens after all graphics stuff
	void FixedUpdate() 
	{
		// calculate UV movement for this frame 
		uvOffset += ( uvAnimationRate * Time.deltaTime );
		if( GetComponent<Renderer>().enabled )
		{
			// offset in material to move UVs
			GetComponent<Renderer>().materials[ materialIndex ].SetTextureOffset( textureName, uvOffset );
			
			if (includeBump)
				GetComponent<Renderer>().materials[ materialIndex ].SetTextureOffset( bumpName, uvOffset );
		}
	}
}
