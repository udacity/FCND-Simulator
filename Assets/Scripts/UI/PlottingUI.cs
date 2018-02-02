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

	public Color[] presetColors;

	List<UIPlotItem> items = new List<UIPlotItem> ();
	List<UIPlotItem> unusedItems = new List<UIPlotItem> ();
	PlotViz plotViz;

	void Awake ()
	{
		vizImage.enabled = false;
	}

	void Start ()
	{
		plotViz = PlotViz.Instance;
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
			if ( items.Find ( x => x.Label == s ) == null )
			{
				UIPlotItem item = GetPlotItem ();
				item.Init ( s, OnItemClicked );
				items.Add ( item );
			}
		}
		for ( int i = 0; i < items.Count; i++ )
		{
			Color c = Color.clear;
			if ( i < presetColors.Length )
				c = presetColors [ i ];
			else
				c = Random.ColorHSV ();
			items [ i ].SetColor ( c );
			if ( items [ i ].IsOn )
				plotViz.SetColor ( items [ i ].Label, c );
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

	void OnItemClicked (UIPlotItem item, bool on)
	{
		if ( on )
		{
			plotViz.AddPlottable ( Plotting.GetPlottable1D ( item.Label ) );
			plotViz.SetColor ( item.Label, item.Color );

		} else
		{
			plotViz.RemovePlottable ( Plotting.GetPlottable1D ( item.Label ) );
		}
	}
}