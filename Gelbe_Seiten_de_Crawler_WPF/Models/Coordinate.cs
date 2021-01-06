using System;

namespace Gelbe_Seiten_de_Crawler_WPF.Models
{
    public struct Coordinate
    {
        public String Longitude { get; }
        public String Latitude { get; }

        public Coordinate(String longitude, String latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }

        public override string ToString()
        {
            if (Longitude == null)
                return "";


            return $"Long.: {Longitude}, Lat.: {Latitude}";
        }
    }
}
