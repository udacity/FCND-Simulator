namespace FlightUtils
{
    struct GPS
    {
        public float latitude { get; }
        public float longitude { get; }

        GPS(float lat, float lon)
        {
            this.latitude = lat;
            this.longitude = lon;
        }
    }
}