using Gelbe_Seiten_de_Crawler_WPF.Models;
using Gelbe_Seiten_de_Crawler_WPF.ViewModels;
using Gelbe_Seiten_de_Crawler_WPF.Views;
using HtmlAgilityPack;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Threading;

namespace Gelbe_Seiten_de_Crawler_WPF.Services
{
    class CrawlerService
    {



        public void SendRequest(CancellationTokenSource cancellationTokenSource, Dispatcher dispatcher, ObservableCollection<Contact> contacts, String umkreis, String was, String wo, String position, String anzahl)
        {
            try
            {

                using var wb = new WebClient();
                var data = new NameValueCollection
                {
                    ["umkreis"] = umkreis,
                    ["WAS"] = was,
                    ["WO"] = wo,
                    ["position"] = position,
                    ["anzahl"] = anzahl
                };

                // TOTAL HITS
                int resultTotalNum = GetResultTotalNum(data);

                Debug.WriteLine(resultTotalNum);
                // REQUEST SMALLER THEN TOTAL HITS? LIMIT!
                if (resultTotalNum > int.Parse(anzahl))
                {
                    resultTotalNum = int.Parse(anzahl);
                }
                Debug.WriteLine(resultTotalNum);

                // anzahl immer 10
                data["anzahl"] = "10";

                CancellationToken cancellationToken = cancellationTokenSource.Token;
                List<Task> tasks = new List<Task>();

                for (int i = 0; i < resultTotalNum; i += 10)
                {
                    // position 0, 10, 20 , 30 usw 
                    data["position"] = i.ToString();


                    if (i == 0)
                    {
                        i += 40;
                    }

                    var response = wb.UploadValues("https://www.gelbeseiten.de/AjaxSuche", "POST", data);
                    string responseInString = Encoding.UTF8.GetString(response).Replace("\\t", "").Replace("\\n", "");
                    responseInString = responseInString.Substring(responseInString.IndexOf("\"html\":\"") + 8);

                    if (responseInString == "\"}")
                        return;

                    /////////////////////
                    ///


                    tasks.Add(Task.Factory.StartNew(() =>
                    {

                        var htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(responseInString);

                        HtmlNodeCollection htmlBody = htmlDoc.DocumentNode.SelectNodes("//article");


                        foreach (HtmlNode item in htmlBody)
                        {
                            var tmpContact = new Contact
                            {
                                PhoneNumber = new List<String>(),
                                Emails = new List<String>(),
                                Websites = new List<String>()
                            };

                            // Name
                            tmpContact.Name = HttpUtility.HtmlDecode(item.SelectSingleNode(".//h2").InnerText);
                            // Strasse
                            tmpContact.Street = HttpUtility.HtmlDecode(item
                                .SelectSingleNode(".//p[contains(@data-wipe-name, 'Adresse')]/text()")?
                                .InnerText
                                .Trim()
                                .Replace(",", ""));
                            // PLZ + ORT
                            string raw = item
                                .SelectSingleNode(".//span[contains(@class, 'nobr')]")?
                                .InnerText;
                            tmpContact.Postcode = raw?.Substring(0, 5);
                            tmpContact.City = raw?[5..];
                            // PHONE NR
                            string tmpPhone = item
                                .SelectSingleNode(".//p[contains(@class, 'mod-AdresseKompakt__phoneNumber')]")?
                                .InnerText
                                .Trim()
                                .Replace(",", "");
                            tmpContact.PhoneNumber.Add(tmpPhone == null ? null : HttpUtility.HtmlDecode(tmpPhone));
                            // URL DETIALS
                            tmpContact.DetailsURL = item
                                .SelectSingleNode(".//a")?
                                .GetAttributeValue("href", "none")?
                                .Replace("\"", "")
                                .Replace("\\", "");

                            if (tmpContact.DetailsURL != null)
                            {
                                try
                                {
                                    tmpContact = RequestDetails(tmpContact);
                                }
                                catch (Exception)
                                {
                                    //
                                }

                            }
                            // ADD CONTACT
                            dispatcher.Invoke(() => contacts.Add(tmpContact));
                        }



                    }, cancellationToken));



                    MainViewModel.searchIsActive = false;

                }





            }
            catch (ThreadInterruptedException a)
            {
                Debug.WriteLine("Thread Interrupted : " + a.Message);
            }
            catch (WebException webException)
            {
                Debug.WriteLine("WebException : " + webException.Message);
            }
            catch (HttpRequestException e)
            {
                Debug.WriteLine("Connection Lost: " + e.Message);
            }
            catch (SocketException s)
            {
                Debug.WriteLine("Connection Lost: " + s.Message);
            }
            finally
            {

                //MainViewModel.searchIsActive = false;
            }
        }

        private int GetResultTotalNum(NameValueCollection data)
        {
            using var wb = new WebClient();

            data["anzahl"] = "1";

            var response = wb.UploadValues("https://www.gelbeseiten.de/AjaxSuche", "POST", data);

            JObject jObj = JObject.Parse(Encoding.UTF8.GetString(response));

            return Int32.Parse(jObj["gesamtanzahlTreffer"].ToString());
        }



        private Contact RequestDetails(Contact tmpContact)
        {
            using (var wbInner = new WebClient())
            {
                var responseInner = wbInner.DownloadString(tmpContact.DetailsURL?.Replace("\"", "")?.Replace("\\", ""));
                string responseInnerString = responseInner;

                var htmlDocInner = new HtmlDocument();
                htmlDocInner.LoadHtml(responseInnerString);
                HtmlNodeCollection htmlBodyInner = htmlDocInner.DocumentNode.SelectNodes("//li[contains(@class, 'mod-Kontaktdaten__list-item')]");

                foreach (HtmlNode itemInner in htmlBodyInner)
                {
                    // INNER PHONE NR
                    var phoneNr = itemInner.SelectSingleNode("//a[contains(@class, 'nolink-black')]")?.InnerText.Trim();
                    if (!tmpContact.PhoneNumber.Contains(phoneNr))
                        tmpContact.PhoneNumber.Add(phoneNr);

                    tmpContact.FaxNumber = itemInner.SelectSingleNode("//span[contains(@property, 'faxnumber')]")?.InnerText;

                    // INNER EMAIl
                    var email = itemInner.SelectSingleNode(".//a[contains(@href, 'mailto')]")?.InnerText.Trim();
                    if (email != null)
                        tmpContact.Emails.Add(email);


                    // INNE WEBSITES 
                    var websites = itemInner.SelectSingleNode(".//a[contains(@href, 'http://')]")?.GetAttributeValue("href", "none");
                    if (websites != null)
                        tmpContact.Websites.Add(websites);
                }
                // IMAGE
                HtmlNodeCollection htmlBodyInnerIMG = htmlDocInner.DocumentNode.SelectNodes("//img[contains(@class, 'mod-TeilnehmerKopf__logo-image')]");
                tmpContact.ImageURL = htmlBodyInnerIMG?[0]
                    .GetAttributeValue("src", "none");

                // COORDINATES
                HtmlNodeCollection htmlBodyInnerCoordinates = htmlDocInner.DocumentNode.SelectNodes("//meta[contains(@typeof, 'number')]");
                tmpContact.Coordinate = new Coordinate(htmlBodyInnerCoordinates?[1]
                                                        .GetAttributeValue("content", "none"),
                                                        htmlBodyInnerCoordinates?[0]
                                                        .GetAttributeValue("content", "none"));
            }
            return tmpContact;
        }


    }
}
