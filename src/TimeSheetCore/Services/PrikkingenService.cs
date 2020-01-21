using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using TimeSheetCore.Helpers;
using TimeSheetCore.Model;

namespace TimeSheetCore.Services
{
    public class PrikkingenService : IPrikkingenService
    {
        private readonly HttpClient _httpClient;
        
        public PrikkingenService(HttpClient httpClient)
        {            
            _httpClient = httpClient;
        }

        private DayOverview GetDayOverview(int personeelsNr, string datum)
        {
            var htmlResult = InternalGetByDate(personeelsNr.ToString(), datum);
            var overview = ParseHtml(datum, htmlResult);

            return overview;
        }

        private UserOverview GetUserOverview(int personeelsNr, string datum)
        {
            var htmlResult = InternalGetByDate(personeelsNr.ToString(), datum);
            var user = ParseHtml2(htmlResult);

            return user;
        }

        public DayOverview GetDayOverview(int personeelsNr, DateTime workday)
        {
            return GetDayOverview(personeelsNr,workday.Date.ToString("dd/MM/yyyy"));
        }

        public UserOverview GetUserOverview(int personeelsNr, DateTime workday)
        {
            var user = GetUserOverview(personeelsNr,workday.Date.ToString("dd/MM/yyyy"));

            if (user.Name == string.Empty)
            {
                throw new ApplicationException("UserNotFoundException");
            }

            return user;
        }
        
        private DayOverview ParseHtml(string datum, string htmlResult)
        {
            var document = new HtmlDocument();
            document.LoadHtml(htmlResult);
            
            HtmlNode mainTable = document.DocumentNode.SelectSingleNode("//*[@id=\"ctl00_ContentPlaceHolder1_DetailPrikking1_gvwPrikkingen\"]");
            IEnumerable<string[]> tableData = null;
            
            try
            {
                tableData = mainTable.Descendants("tr").Select(n => n.Elements("td").Select(e => e.InnerText).ToArray());
            }
            catch (Exception)
            {
                //throw;
            }

            var day = new DayOverview
            {
                Prikkingen = new List<Prikking>(),
                Date = DateTime.Parse(datum)
            };

            if (tableData != null)
            {
                foreach (var tableRow in tableData)
                {
                    if (!tableRow.Any())
                    {
                        continue;
                    }

                    var prikking = new Prikking
                    {
                        Datum = DateTime.Parse(datum + RemoveHtmlTags(tableRow[3])),
                        Zone = RemoveHtmlTags(tableRow[1]),
                        Prikklok = RemoveHtmlTags(tableRow[2])
                    };

                    day.Prikkingen.Add(prikking);
                }

                // make sure timestamps are ordered in the correct way
                day.Prikkingen = day.Prikkingen.OrderBy(x => x.Datum).ToList();

                day.CalculateTotal();

                if (day.Total >= TimeSpan.FromHours(8))
                {
                    day.Completed = true;
                }
            }
            else if (day.Date < DateTime.Now)
            {
                day.Absence = true;
                day.CalculateTotal();
            }

            return day;
        }

        private UserOverview ParseHtml2(string htmlResult)
        {
            var document = new HtmlDocument();
            document.LoadHtml(htmlResult);
            var user = new UserOverview();

            HtmlNode mainTable = document.DocumentNode.SelectSingleNode("//*[@valign=\"top\"]");

            try
            {
                var tableData = mainTable.Descendants("tr").Select(n => n.Elements("td").Select(e => e.InnerHtml).ToArray());
                var userInfoData = tableData as string[][] ?? tableData.ToArray();
                
                var badgenummer = userInfoData[0][3];
                var stamnummer = userInfoData[1][1];
                var name = userInfoData[2][1];
                var firma = userInfoData[3][1];

                user.Firma = GetValueAttribute(firma);
                user.Name = GetValueAttribute(name);
                user.Badge = GetValueAttribute(badgenummer);
                user.Id = int.Parse(GetValueAttribute(stamnummer));
            }
            catch (Exception)
            {
                //throw;
            }

            return user;
        }

        private string GetValueAttribute(string name)
        {
            Regex regex = new Regex(@"(?<=\bvalue="")[^""]*");
            Match match = regex.Match(name);
            return match.Value.TrimEnd();
        }

        private string InternalGetByDate(string personeelsnr, string datum)
        {
            var uri = $"{_httpClient.BaseAddress}/Prikking/Detail.aspx?Personeelsnr={personeelsnr}&datum={datum}";

            var responseString = AsyncHelper.RunSync(() => _httpClient.GetStringAsync(uri));
            
            return responseString;
        }

        public static string RemoveHtmlTags(string strHtml)
        {
            string strText = Regex.Replace(strHtml, "<(.|\n)*?>", String.Empty);
            strText = HttpUtility.HtmlDecode(strText);
            strText = Regex.Replace(strText, @"\s+", " ");
            return strText.TrimEnd();
        }
    }
}
