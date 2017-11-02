using UnityEngine;
using System.IO;


namespace FlightUtils
{
    public static class Utils
    {
        // Converts Hertz to the number of milliseconds
        // it would take to render 1 frame.
        // Ex: 10 Hertz -> 10 FPS -> 100 milliseconds
        //
        // This is useful for sleeping a C# Task for
        // a certain time.
        public static int HertzToMilliSeconds(float hz)
        {
            return (int)(1000f / hz);
        }

        public static void CollidersToCSV(string filename)
        {
            var go = GameObject.Find("ColliderGatherer");
            if (go == null)
            {
                Debug.Log("ColliderGatherer GameObject not found in scene ...");
                return;
            }
            var collidersGenerator = go.GetComponent<GenerateColliderList>();
            var colliders = collidersGenerator.colliders;

            Debug.Log(string.Format("Writing colliders to {0} ...", filename));

            // SimpleFileBrowser.ShowSaveDialog

            // Write headers
            File.AppendAllText(Path.Combine(filename), "posX,posY,posZ,halfSizeX,halfSizeY,halfSizeZ\n");
            foreach (var c in colliders)
            {
                var pos = c.position;
                var hsize = c.halfSize;
                var row = string.Format("{0},{1},{2},{3},{4},{5}\n", pos.x, pos.y, pos.z, hsize.x, hsize.y, hsize.z);
                File.AppendAllText(Path.Combine(filename), row);
            }
        }

        private static void CreateFile(string filename)
        {
            Debug.Log("ok " + filename);
            // File.
        }

    }
}


