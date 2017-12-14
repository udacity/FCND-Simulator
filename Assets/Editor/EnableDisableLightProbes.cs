using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EnableDisableLightProbes : MonoBehaviour
{
	[MenuItem ("Utility/Set Light Probes (by Lightmap Static flag)", false, 40)]
	static void DisableLightProbes ()
	{
		Renderer[] renderers = SceneView.FindObjectsOfType<MeshRenderer> ();
		foreach ( Renderer r in renderers )
		{
			if ( r.name.ToLower ().Contains ( "reference" ) )
				continue;
			
			if ( GameObjectUtility.AreStaticEditorFlagsSet ( r.gameObject, StaticEditorFlags.LightmapStatic ) )
			{
				r.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
				var proxy = r.GetComponent<LightProbeProxyVolume> ();
				if ( proxy != null )
					DestroyImmediate ( proxy );
			}
			else
			{
				if ( r.GetComponent<LightProbeProxyVolume> () == null )
					r.gameObject.AddComponent<LightProbeProxyVolume> ();
				r.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.UseProxyVolume;
			}
		}
	}
}