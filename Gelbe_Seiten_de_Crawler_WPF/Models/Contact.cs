using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;

namespace Gelbe_Seiten_de_Crawler_WPF.Models
{
    public class Contact
    {
        [Name("Name")]
        public String Name { get; set; }
        [Name("Strasse")]
        public String Street { get; set; }
        [Name("PLZ")]
        public String Postcode { get; set; }
        [Name("Stadt")]
        public String City { get; set; }
        public List<String> PhoneNumber { get; set; }
        [Name("Fax")]
        public String FaxNumber { get; set; }
        [Name("Details Url")]
        public String DetailsURL { get; set; }
        [Name("Bild Url")]
        public String ImageURL { get; set; }
        public List<String> Emails { get; set; }
        public List<String> Websites { get; set; }

        public Coordinate Coordinate { get; set; }
    }
}
