#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PathGridGenerator : MonoBehaviour
{
	public int width = 10;
	public int height = 4;
	public int depth = 10;

	public float cellSize = 1;
	public bool addRenderer;
	public bool rebuildMesh;

	public Color boxBGColor;
	public bool alwaysDrawGizmos;

	bool[,,] cells;
	int lastWidth;
	int lastHeight;
	int lastDepth;
	Mesh boxMesh;
	Transform testSphere;
	Vector3 lastSpherePos;

	void OnEnable ()
	{
		if ( cells == null )
		{
			lastWidth = width;
			lastHeight = height;
			lastDepth = depth;
			BuildGrid ();
		}

		if ( testSphere == null )
		{
			testSphere = GameObject.CreatePrimitive ( PrimitiveType.Sphere ).transform;
			testSphere.localScale *= 0.2f;
		}	
		testSphere.position = transform.position;

		if ( boxMesh == null || rebuildMesh )
		{
			rebuildMesh = false;
			boxMesh = new Mesh ();
			boxMesh.Clear ();
			boxMesh.vertices = new Vector3[] {
				// front
				new Vector3 ( 0, 0, 0 ),
				new Vector3 ( 0, 1, 0 ),
				new Vector3 ( 1, 1, 0 ),
				new Vector3 ( 1, 0, 0 ),
				// left
				new Vector3 ( 0, 0, 1 ),
				new Vector3 ( 0, 1, 1 ),
				new Vector3 ( 0, 1, 0 ),
				new Vector3 ( 0, 0, 0 ),
				// right
				new Vector3 ( 1, 0, 0 ),
				new Vector3 ( 1, 1, 0 ),
				new Vector3 ( 1, 1, 1 ),
				new Vector3 ( 1, 0, 1 ),
				// back
				new Vector3 ( 1, 0, 1 ),
				new Vector3 ( 1, 1, 1 ),
				new Vector3 ( 0, 1, 1 ),
				new Vector3 ( 0, 0, 1 ),
				// top
				new Vector3 ( 0, 1, 0 ),
				new Vector3 ( 0, 1, 1 ),
				new Vector3 ( 1, 1, 1 ),
				new Vector3 ( 1, 1, 0 ),
				// bottom 
				new Vector3 ( 0, 0, 0 ),
				new Vector3 ( 1, 0, 0 ),
				new Vector3 ( 1, 0, 1 ),
				new Vector3 ( 0, 0, 1 )
				
			};
			boxMesh.triangles = new int[] {
				// front
				0, 2, 1,
				0, 3, 2,
				// left
				4, 6, 5,
				4, 7, 6,
				// right
				8, 10, 9,
				8, 11, 10,
				// back
				12, 14, 13,
				12, 15, 14,
				// top
				16, 18, 17,
				16, 19, 18,
				// bottom
				20, 22, 21,
				20, 23, 22
			};
			boxMesh.uv = new Vector2[] {
				
				new Vector2 ( 0, 0 ), new Vector2 ( 0, 1 ), new Vector2 ( 1, 1 ), new Vector2 ( 1, 0 ),
				new Vector2 ( 0, 0 ), new Vector2 ( 0, 1 ), new Vector2 ( 1, 1 ), new Vector2 ( 1, 0 ),
				new Vector2 ( 0, 0 ), new Vector2 ( 0, 1 ), new Vector2 ( 1, 1 ), new Vector2 ( 1, 0 ),
				new Vector2 ( 0, 0 ), new Vector2 ( 0, 1 ), new Vector2 ( 1, 1 ), new Vector2 ( 1, 0 ),
				new Vector2 ( 0, 0 ), new Vector2 ( 0, 1 ), new Vector2 ( 1, 1 ), new Vector2 ( 1, 0 ),
				new Vector2 ( 0, 0 ), new Vector2 ( 0, 1 ), new Vector2 ( 1, 1 ), new Vector2 ( 1, 0 ),

			};
			boxMesh.RecalculateBounds ();
			boxMesh.RecalculateNormals ();
			boxMesh.RecalculateTangents ();
//			Vector3[] normals = boxMesh.normals;
//			for ( int i = 0; i < normals.Length; i++ )
//				normals [ i ] *= -1;
//			boxMesh.normals = normals;
		}
		if ( addRenderer )
		{
			MeshFilter filter = GetComponent<MeshFilter> ();
			if ( filter == null )
			{
				filter = gameObject.AddComponent<MeshFilter> ();
				MeshRenderer rend = gameObject.AddComponent<MeshRenderer> ();
				rend.material = new Material ( Shader.Find ( "Standard" ) );
			}
			filter.mesh = boxMesh;
		}
	}

	void Update ()
	{
		if ( lastWidth != width || lastHeight != height || lastDepth != depth )
		{
			lastWidth = width;
			lastHeight = height;
			lastDepth = depth;
			BuildGrid ();
		}
		if ( lastSpherePos != testSphere.position )
		{
			var index = CellPositionToIndex ( lastSpherePos );
			if ( IsValidIndex ( index ) )
				cells [ index [ 0 ], index [ 1 ], index [ 2 ] ] = false;
			lastSpherePos = testSphere.position;
			index = CellPositionToIndex ( lastSpherePos );
			if ( IsValidIndex ( index ) )
				cells [ index [ 0 ], index [ 1 ], index [ 2 ] ] = true;
		}
	}

	void OnDrawGizmos ()
	{
		if ( alwaysDrawGizmos && UnityEditor.Selection.activeGameObject != gameObject )
			OnDrawGizmosSelected ();
	}

	void OnDrawGizmosSelected ()
	{
		if ( cells == null )
			return;

		// draw available cells empty with gray border, unavailable ones shaded gray
		Color empty = Color.white;
		Color occupied = Color.red;
//		Gizmos.color = new Color ( 0.25f, 0.25f, 0.25f, 1f );
		Vector3 position;
		Vector3 halfCell = Vector3.one * cellSize / 2.05f;
		Vector3 cell = Vector3.one * cellSize;// * 0.95f;

		Gizmos.color = boxBGColor;
//		Gizmos.DrawMesh ( boxMesh, transform.position );
		Gizmos.DrawMesh ( boxMesh, Min (), Quaternion.identity, new Vector3 ( width, height, depth ) );

		for ( int w = 0; w < width; w++ )
		{
			for ( int h = 0; h < height; h++ )
			{
				for ( int d = 0; d < depth; d++ )
				{
					position = CellIndexToPosition ( w, h, d );
					if ( cells [ w, h, d ] )
					{
						Gizmos.color = occupied;
						Gizmos.DrawWireCube ( position, cell );
					}
//					else
//						Gizmos.color = empty;

//					Gizmos.DrawWireCube ( position, cell );
//					if ( cells [ w, h, d ] )
//						Gizmos.DrawWireCube ( position, cell );
//					else
//						Gizmos.DrawCube ( position, cell );
				}
			}
		}

		// draw a bounding box in green
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube ( transform.position, new Vector3 ( width, height, depth ) );
//		Gizmos.DrawWireCube ( transform.position, new Vector3 ( width / 2, height / 2, depth / 2 ) );
	}

	Vector3 Min ()
	{
		return transform.position - new Vector3 ( width / 2, height / 2, depth / 2 );
	}

	Vector3 Max ()
	{
		return transform.position + new Vector3 ( width / 2, height / 2, depth / 2 );
	}

	Vector3 Center ()
	{
		return transform.position;
	}

	bool CompareDimensions (int desiredWidth, int desiredHeight, int desiredDepth)
	{
		if ( cells == null )
			return false;
		return cells.GetLength ( 0 ) == desiredWidth && cells.GetLength ( 1 ) == desiredHeight && cells.GetLength ( 2 ) == desiredDepth;
	}

	bool IsValidIndex (int x, int y, int z)
	{
		return x >= 0 && x < cells.GetLength ( 0 ) && y >= 0 && y < cells.GetLength ( 1 ) && z >= 0 && z < cells.GetLength ( 2 );
	}

	bool IsValidIndex (int[] index)
	{
		return index [ 0 ] >= 0 && index [ 0 ] < cells.GetLength ( 0 ) && index [ 1 ] >= 0 && index [ 1 ] < cells.GetLength ( 1 ) && index [ 2 ] >= 0 && index [ 2 ] < cells.GetLength ( 2 );
	}

	Vector3 CellIndexToPosition (int x, int y, int z)
	{
		if ( cells != null )
		{
			Vector3 min = Min ();
			Vector3 halfCell = Vector3.one * cellSize / 2;
			if ( x < 0 || y < 0 || z < 0 )
				throw new System.IndexOutOfRangeException ();
			if ( x >= cells.GetLength ( 0 ) || y >= cells.GetLength ( 1 ) || z >= cells.GetLength ( 2 ) )
				throw new System.IndexOutOfRangeException ();

			return min + new Vector3 ( cellSize * x, cellSize * y, cellSize * z ) + halfCell;

		} else
			return Vector3.zero;
	}

	int[] CellPositionToIndex (Vector3 position)
	{
		float halfSize = cellSize / 2;
		Vector3 pos = transform.position;
		position -= new Vector3 ( halfSize, halfSize, halfSize );
		position /= cellSize;
		position -= Min ();
		return new int[3] {
			Mathf.RoundToInt ( position.x ),
			Mathf.RoundToInt ( position.y ),
			Mathf.RoundToInt ( position.z )
		};
//		int x = Mathf.RoundToInt ( ( position.x - halfSize ) / cellSize - pos.x );
//		int y = Mathf.RoundToInt ( ( position.y - halfSize ) / cellSize - pos.y );
//		int z = Mathf.RoundToInt ( ( position.z - halfSize ) / cellSize - pos.z );
//		return new int[3] { x, y, z };
	}

	void BuildGrid ()
	{
		// if the grid is the same size, no action necessary
		if ( CompareDimensions ( width, height, depth ) )
			return;
		
		cells = new bool[width, height, depth];

		Vector3 pos;
		Collider[] colliders;
//		if ( useOnlyChildren )
//			colliders = transform.GetComponentsInChildren<Collider> ();
//		else
			colliders = UnityEditor.SceneView.FindObjectsOfType<Collider> ();

//		var index = CellPositionToIndex ( transform.position );
//		cells [ index [ 0 ], index [ 1 ], index [ 2 ] ] = true;
//		Debug.Log ( "index is " + index [ 0 ] + "," + index [ 1 ] + ", " + index [ 2 ] );

		for ( int w = 0; w < width; w++ )
		{
			for ( int h = 0; h < height; h++ )
			{
				for ( int d = 0; d < depth; d++ )
				{
					pos = CellIndexToPosition ( w, h, d );

				}
			}
		}
	}
}
#endif