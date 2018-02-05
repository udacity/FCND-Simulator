using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UdaciPlot
{
	public delegate int Comparer<T> (T t1, T t2);

	public struct TimedSample<T>
	{
		public T value;
		public double timestamp;
		
		public TimedSample (T val, double stamp)
		{
			value = val;
			timestamp = stamp;
		}
	}
	
	public class Plottable<T>
	{
		public string label;
		SimpleRingBuf<TimedSample<T>> buffer;
		public T max;
		public T min;
		Comparer<T> comparer;
		
		
		public Plottable (string _label, int sampleCount = 3000, Comparer<T> _comparer = null)
		{
			label = _label;
			buffer = new SimpleRingBuf<TimedSample<T>> ( 3000 );
			comparer = _comparer;
		}
		
		public void AddSample (T sample, double stamp)
		{
			buffer.Add ( new TimedSample<T> ( sample, stamp ) );
			if ( comparer != null )
			{
				if ( comparer ( sample, max ) == 1 )
					max = sample;
				else
				if ( comparer ( sample, min ) == -1 )
					min = sample;
			}
		}
		
		public TimedSample<T> GetSample (int index)
		{
			return buffer.Get ( index );
		}
		
		public TimedSample<T>[] GetSamples ()
		{
			return buffer.Values ();
		}

		/// <summary>
		/// Calculates the min and max samples. If no comparer is assigned, returns false.
		/// </summary>
		public bool CalcMinMax ()
		{
			if ( comparer == null )
				return false;

			max = min = default(T);
			int count = buffer.Count;
			for ( int i = 0; i < count; i++ )
			{
				T cur = buffer [ i ].value;
				if ( comparer ( max, cur ) == 1 )
					max = cur;
				else
				if ( comparer ( min, cur ) == -1 )
					min = cur;
			}
			return true;
		}
	}
}