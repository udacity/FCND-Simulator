using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mapbox.Unity.Map;
using Mapbox.Utils;
using Mapbox.Unity.Utilities;
using Mapbox.Map;

// Combined version of MapAtWorldScale and MapAtSpecificLocation.
// note: _unityTileSize is ignored because of world scale

public class WorldScaleMapAtSpecificLocation : AbstractMap
{
	[SerializeField]
	bool _useRelativeScale;

	public override void Initialize(Vector2d latLon, int zoom)
	{
//		MapboxUtility.Reset ();
		_worldHeightFixed = false;
		_centerLatitudeLongitude = latLon;
		_zoom = zoom;
		_initialZoom = zoom;

		var referenceTileRect = Conversions.TileBounds(TileCover.CoordinateToTileId(_centerLatitudeLongitude, AbsoluteZoom));
		_centerMercator = referenceTileRect.Center;

		_worldRelativeScale = _useRelativeScale ? Mathf.Cos(Mathf.Deg2Rad * (float)_centerLatitudeLongitude.x) : 1f;

		// The magic line.
		// Because this offsets the map root, you must manually compensate for this offset with any future
		// conversion operations (lat/lon <--> unity world space)!!!
		_root.localPosition = -Conversions.GeoToWorldPosition(_centerLatitudeLongitude.x, _centerLatitudeLongitude.y, _centerMercator, _worldRelativeScale).ToVector3xz();

		_mapVisualizer.Initialize(this, _fileSource);
		_tileProvider.Initialize(this);

		SendInitialized();
	}
}