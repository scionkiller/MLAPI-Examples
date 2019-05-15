using UnityEngine;

using Alpaca;


// enumeration of prefabs for convenient code reference
// MUST BE KEPT UP TO DATE WITH PREFABS SET ON CLIENT AND SERVER
public class Prefab
{
	public static readonly EntityPrefabIndex PLAYER = new EntityPrefabIndex( 0 );
}

public class WorldSettings : MonoBehaviour
{
	public CommonNodeSettings commonNodeSettings;

	public float minimumDisplayTime = 1f;
}