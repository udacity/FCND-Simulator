using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UdaciPlot
{
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
	
	public class Plottable<T>// : IPlottable
	{
		
		public string label;
		SimpleRingBuf<TimedSample<T>> buffer;
		
		
		public Plottable (string _label, int sampleCount = 3000)
		{
			label = _label;
			buffer = new SimpleRingBuf<TimedSample<T>> ( 3000 );
		}
		
		public void AddSample (T sample, double stamp)
		{
			buffer.Add ( new TimedSample<T> ( sample, stamp ) );
		}
		
		public TimedSample<T> GetSample (int index)
		{
			return buffer.Get ( index );
		}
		
		public TimedSample<T>[] GetSamples ()
		{
			return buffer.Values ();
		}
	}
}