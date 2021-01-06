using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Gelbe_Seiten_de_Crawler_WPF.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Gelbe_Seiten_de_Crawler_WPF.Services
{
    enum Mode
    {
        Full,
        Contact,
        Small,
    }
    class ExportToCsvService
    {
        public void Export(List<Contact> contacts, Mode mode)
        {
            var utf8_Bom = new System.Text.UTF8Encoding(true);
            using var writer = new StreamWriter("file.csv", false, utf8_Bom);
            using var csv = new CsvWriter(writer, CultureInfo.CurrentCulture);
            csv.Configuration.RegisterClassMap(new ContactMap(mode));
            csv.Configuration.NewLine = NewLine.Environment;
            csv.WriteRecords(contacts);
        }
    }



    class ContactMap : ClassMap<Contact>
    {
        public ContactMap(Mode mode)
        {
            AutoMap(CultureInfo.InvariantCulture);
            switch (mode)
            {
                case Mode.Small:
                    Map(m => m.PhoneNumber).Ignore();
                    Map(m => m.FaxNumber).Ignore();
                    Map(m => m.DetailsURL).Ignore();
                    Map(m => m.ImageURL).Ignore();
                    Map(m => m.Emails).Ignore();
                    Map(m => m.Websites).Ignore();
                    Map(m => m.Coordinate.Latitude).Ignore();
                    Map(m => m.Coordinate.Longitude).Ignore();
                    break;
                case Mode.Contact:
                    Map(m => m.PhoneNumber).TypeConverter<ListStringConverter>().Name("Telefon");
                    Map(m => m.Emails).TypeConverter<ListStringConverter>().Name("eMail");
                    Map(m => m.Websites).Ignore();
                    Map(m => m.DetailsURL).Ignore();
                    Map(m => m.ImageURL).Ignore();
                    Map(m => m.Coordinate.Latitude).Ignore();
                    Map(m => m.Coordinate.Longitude).Ignore();
                    break;
                case Mode.Full:
                    Map(m => m.PhoneNumber).TypeConverter<ListStringConverter>().Name("Telefon");
                    Map(m => m.Emails).TypeConverter<ListStringConverter>().Name("eMail");
                    Map(m => m.Websites).TypeConverter<ListStringConverter>().Name("Website");
                    break;                              
                default:
                    break;
            }            
        }
    }

    class ListStringConverter : StringConverter
    {
        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            var returnValue = string.Empty;

            var list = (List<string>)value;

            if (list != null)
            {
                returnValue = string.Join(Environment.NewLine, list);
            }

            return base.ConvertToString(returnValue, row, memberMapData);
        }

        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            var list = text.Split(',').ToList();

            return list;
        }
    }
}
