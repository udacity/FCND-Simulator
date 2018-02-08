using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRingBuf<T>
{
	public int Count { get { return count; } }

	public T this[int index]
	{
		get { return Get ( index ); }
	}

	protected T[] items;

	int start;
	int count;

	public SimpleRingBuf (int capacity = 500)
	{
		items = new T[capacity];
	}

	public void Add (T item)
	{
		items [ ( start + count ) % items.Length ] = item;

		if ( count == items.Length )
			start = ++start % items.Length;
		
		if ( count < items.Length )
			count++;
	}

	public T Get (int index)
	{
		if ( index >= items.Length || index < -1 )
			throw new System.IndexOutOfRangeException ();

		if (index == -1)
		{
			index = ( start + count - 1 ) % items.Length;
			return items [ index ];
		}

		index = ( start + index ) % items.Length;
		return items [ index ];
	}

	/// <summary>
	/// Get up to /Count/ values. Allocates a new array
	/// </summary>
	public T[] Values ()
	{
		T[] values = new T[count];
		for ( int i = 0; i < count; i++ )
		{
			int index = ( start + i ) % items.Length;
			if ( index < 0 || index >= items.Length )
				Debug.LogError ( "index is " + index + ": start " + start + " i " + i + " count " + count );
			try {
			values [ i ] = items [ index ];
			}
			catch (System.Exception e )
			{
				Debug.LogException ( e );
				Debug.LogError ( "index is " + index + ": start " + start + " i " + i + " count " + count );
			}
//			values [ i ] = items [ ( start + i ) % items.Length ];
		}
		return values;
	}

	/// <summary>
	/// Get up to /Count/ values, without allocation. The minimum between Count and the length of the array passed will be written
	/// </summary>
	/// <param name="values">Values.</param>
	/// <returns>The number of values written.</returns>
	public int ValuesNonAlloc (ref T[] values)
	{
		int thisCount = Mathf.Min ( count, values.Length );
		for ( int i = 0; i < thisCount; i++ )
		{
			values [ i ] = items [ ( start + i ) % items.Length ];
		}
		return thisCount;
	}

	public void Set (T value)
	{
		for ( int i = 0; i < items.Length; i++)
		{
			items [ i ] = value;
		}
	}
}