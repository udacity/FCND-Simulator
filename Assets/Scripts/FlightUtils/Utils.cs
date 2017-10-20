
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
    }
}


