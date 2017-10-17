
namespace FlightUtils
{
    public static class Utils
    {
        // 1 Hz = 0.5 FPS
        public static float HertzToFPS(float hz)
        {
            return 0.5f * hz;
        }

        // Converts FPS to the number of milliseconds
        // it would take to render 1 frame.
        // Ex: 10 FPS -> 100 milliseconds
        //
        // This is useful for sleeping a C# Task for
        // a certain time.
        public static int FPSToMilliSeconds(float fps)
        {
            return (int)(1000f / fps);
        }
    }
}


