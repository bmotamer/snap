/**
 *	SnapSettings
 *	
 *	@author Bruno "BMO" Tamer <pg03bruno@vfs.com>
 *	@date 2015.03.20
 */

using UnityEngine;

/// <summary>
/// Snap tool settings
/// </summary>
public sealed class SnapSettings : MonoBehaviour
{

	// Default snap settings
	public Vector3 PositionStep = Vector3.one;
	public Vector3 RotationStep = new Vector3(15.0f, 15.0f, 15.0f);
	public Vector3 ScaleStep    = new Vector3(0.1f, 0.1f, 0.1f);

	public bool    ShowGrid     = true;
	public Vector3 GridAnchor   = new Vector3(0.5f, 0.5f, 0.5f);
	public Vector3 GridSize     = new Vector3(100.0f, 100.0f, 100.0f);
	public Color   GridColor    = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	public void Awake()
	{
		// If the project was built, destroys itself
		if (!Application.isEditor)
			Destroy(this);
	}
	
}