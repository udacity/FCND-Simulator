using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Diagnostics;
#if UNITY_EDITOR
using UnityEditor.Callbacks;
#endif

[Serializable]
public class TunableParameter
{
	public Tunable data;
	public FieldInfo field;

	public TunableParameter (Tunable t, FieldInfo f)
	{
		data = t;
		field = f;
	}
}

[System.Serializable]
public class RuntimeTunableParameter
{
	public Tunable data;
	public FieldInfo field;
	public object fieldInstance;
}

public class TunableManager
{
	static TunableManager Instance
	{
		get
		{
			if ( instance == null )
				instance = new TunableManager ();
			return instance;
		}
	}
	static TunableManager instance;

	public static List<TunableParameter> Parameters { get { return Instance.parameters; } }
	List<TunableParameter> parameters;
	public static List<RuntimeTunableParameter> RuntimeParameters { get { return Instance.runtimeParameters; } }
	List<RuntimeTunableParameter> runtimeParameters;

	void Init ()
	{
		Stopwatch sw = new Stopwatch ();
		sw.Start ();
		if ( parameters == null )
			parameters = new List<TunableParameter> ();
		Assembly asm = Assembly.GetExecutingAssembly ();
		var types = asm.GetExportedTypes ();
		foreach ( Type t in types )
		{
			FieldInfo[] fields = t.GetFields ();
			foreach ( var field in fields )
			{
				var tunableAttribute = field.GetCustomAttribute<Tunable> ( false );
				if ( tunableAttribute != null )
				{
					parameters.Add ( new TunableParameter ( tunableAttribute, field ) );
//					UnityEngine.Debug.Log ( "Found one! default: " + tunableAttribute.defaultValue + " min: " + tunableAttribute.minValue + " max: " + tunableAttribute.maxValue );
//					var tt = Activator.CreateInstance ( t );
//					field.SetValue ( tt, tunableAttribute.minValue );
//					UnityEngine.Debug.Log ( "Field value is now " + field.GetValue ( tt ) );
				}
			}
		}
		sw.Stop ();
		UnityEngine.Debug.Log ( "That took " + sw.ElapsedMilliseconds + "ms" );
	}

	public static void Init (object o)
	{
		Instance.InitRuntime ( o );
	}
	void InitRuntime (object o)
	{
		if ( runtimeParameters == null )
			runtimeParameters = new List<RuntimeTunableParameter> ();

		// iterate over the objects and check if they have a Tunable attribute.
		// if so, add it to the list. then push children onto queue to iterate over them
		// but also track objects already iterated
		HashSet<int> hashes = new HashSet<int> ();
		hashes.Add ( o.GetHashCode () );
		Queue<object> objects = new Queue<object> ();
		objects.Enqueue ( o );
		Type t;
		object obj;
		while ( objects.Count > 0 )
		{
			obj = objects.Dequeue ();
			t = obj.GetType ();

			var fields = t.GetFields ();
//			var fields = t.GetFields ( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
			if ( fields == null || fields.Length == 0 )
				continue;

			foreach ( var field in fields )
			{
				var tunableAttribute = field.GetCustomAttribute<Tunable> ( false );
				if ( tunableAttribute != null )
				{
					RuntimeTunableParameter p = new RuntimeTunableParameter ();
					p.data = tunableAttribute;
					p.field = field;
					p.fieldInstance = obj;
					runtimeParameters.Add ( p );
				}
				// assembly check will prevent primitive types, unity types, etc from being added to this.
				if ( field.FieldType.Assembly != o.GetType ().Assembly || field.FieldType.IsEnum )
					continue;
				object obj2 = field.GetValue ( obj );
				if ( obj2 != null && !hashes.Contains ( obj2.GetHashCode () ) )
				{
					hashes.Add ( obj2.GetHashCode () );
					objects.Enqueue ( obj2 );
				}
			}
		}
	}

	#if UNITY_EDITOR
	[DidReloadScripts]
	static void OnScriptReload ()
	{
		Instance.Init ();
	}
	#endif
}