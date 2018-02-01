using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UdaciPlot;

public class PlottingUI : MonoBehaviour
{
	public UIPlotItem itemPrefab;
	public RawImage vizImage;
	public Transform listParent;

	List<UIPlotItem> items = new List<UIPlotItem> ();
	List<UIPlotItem> unusedItems = new List<UIPlotItem> ();

	void Awake ()
	{
		vizImage.enabled = false;
	}

	public void OnOpen ()
	{
		var fields = Plotting.ListPlottables1D ();
		if ( fields == null || fields.Length == 0 )
		{
			vizImage.enabled = false;
			return;
		}

		foreach ( string s in fields )
		{
			UIPlotItem item = GetPlotItem ();

		}
	}

	UIPlotItem GetPlotItem ()
	{
		UIPlotItem item;
		if ( unusedItems.Count > 0 )
		{
			item = unusedItems [ unusedItems.Count - 1 ];
			unusedItems.Remove ( item );
			return item;
		}

		item = Instantiate<UIPlotItem> ( itemPrefab, listParent );
		return item;
	}

	void RemovePlotItem (UIPlotItem item)
	{
		if ( item != null )
		{
			item.gameObject.SetActive ( false );
			items.Remove ( item );
			if ( !unusedItems.Contains ( item ) )
				unusedItems.Add ( item );
		}
	}
}