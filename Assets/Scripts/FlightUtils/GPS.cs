namespace FlightUtils
{
    struct GPS
    {
        float latitude { get; }
        float longitude { get; }

        GPS(float lat, float lon)
        {
            this.latitude = lat;
            this.longitude = lon;
        }
    }
}