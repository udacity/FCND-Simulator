using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Data;

// script to quickly capture the city's list of colliders for use in path planning
// to get list, just add a listener to OnCompleted to be notified, then iterate over .colliders
// to get a partial list of objects within a range of the quad, call GetNearbyCollideres and pass in the quad's position and desired distance

[System.Serializable]
public class ColliderVolume
{
    public Vector3 position;
    public Vector3 size;
    public Vector3 halfSize;

    public ColliderVolume() { }
    public ColliderVolume(Vector3 p, Vector3 s)
    {
        position = p;
        size = s;
        halfSize = s / 2;
    }

    public static ColliderVolume FromCollider(Collider c) { return new ColliderVolume(c.bounds.center, c.bounds.size); }
    public static ColliderVolume FromBoxCollider(BoxCollider bc) { return new ColliderVolume(bc.center, bc.size); }
    public static ColliderVolume FromSphereCollider(SphereCollider sc) { return new ColliderVolume(sc.center, new Vector3(sc.radius, sc.radius, sc.radius)); }
    public static ColliderVolume FromCapsuleCollider(CapsuleCollider cc) { return new ColliderVolume(cc.center, new Vector3(cc.radius, cc.height, cc.radius)); } // ignores direction
}

public class GenerateColliderList : MonoBehaviour
{
    public AbstractMap mapScript;
    [System.NonSerialized]
    public List<ColliderVolume> colliders;
    public bool includeInactiveColliders;
    public bool includeSurroundingTiles;

    public System.Action OnCompleted;

    ColliderVolume[] testVolumes;
    Transform testCube;
    float nextTestViz;
    [SerializeField]
    float testRange = 30;

    void Awake()
    {
		mapScript.OnInitialized += OnMapInitialized;

        // invoking on a timer because apparently the map's OnCompleted is called the same frame as it starts but the "everything" is only instantiated on the next frame
        testCube = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
        Destroy(testCube.GetComponent<Collider>());
        testCube.transform.parent = transform;
//        Invoke("OnMapInitialized", 0.2f);
    }

    void OnMapInitialized()
    {
		Debug.Log ( "Initialized!" );
		transform.position = mapScript.transform.position;
		GenerateColliders ();
    }

	public void GenerateColliders ()
	{
		GameObject mapObject = mapScript.gameObject;
		Collider[] allColliders = mapObject.GetComponentsInChildren<Collider> ( includeInactiveColliders );
		Vector3 size = Vector3.one * mapScript.UnityTileSize;
		if ( includeSurroundingTiles )
			size *= 3;

		colliders = new List<ColliderVolume> ();
		colliders.Add ( new ColliderVolume ( mapObject.transform.position, size ) );

		allColliders.ForEach ( (x) =>
		{
			// skip the tile objects
			if ( x.GetComponent<UnityTile> () != null )
				return;
			colliders.Add ( ColliderVolume.FromCollider ( x ) );
		} );

		if ( OnCompleted != null )
			OnCompleted ();
	}

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (UnityEditor.Selection.activeTransform == null || (UnityEditor.Selection.activeTransform != transform && UnityEditor.Selection.activeTransform != testCube))
            return;
        if (colliders == null || colliders.Count == 0)
            return;

        Gizmos.color = new Color(1, 0.5f, 0);
        Gizmos.DrawWireCube(colliders[0].position, colliders[0].size);

        Gizmos.color = Color.yellow;
        for (int i = colliders.Count - 1; i >= 1; i--)
            Gizmos.DrawWireCube(colliders[i].position, colliders[i].size);

        if (Time.time > nextTestViz)
        {
            testVolumes = GetNearbyColliders(testCube.position, testRange);
            nextTestViz = Time.time + 1f;
        }
        if (testVolumes != null && testVolumes.Length > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = testVolumes.Length - 1; i >= 0; i--)
                Gizmos.DrawWireCube(testVolumes[i].position, testVolumes[i].size);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(testCube.position, Vector3.one * testRange * 2);
    }
#endif

    // return a list of all volumes whose bounds are within /range/ of /position/
    public ColliderVolume[] GetNearbyColliders(Vector3 position, float range)
    {
        if (colliders == null)
            return null;
        return colliders.Where((x) =>
      {
          return !(position.x + range < x.position.x - x.halfSize.x ||
          position.x - range > x.position.x + x.halfSize.x ||
          position.y + range < x.position.y - x.halfSize.y ||
          position.y - range > x.position.y + x.halfSize.y ||
          position.z + range < x.position.z - x.halfSize.z ||
          position.z - range > x.position.z + x.halfSize.z
          );
      }).ToArray();
    }
}