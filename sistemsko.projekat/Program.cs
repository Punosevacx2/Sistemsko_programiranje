using System;
using System.Configuration;
using System.Net;
using System.Text;
using System.Threading;
using ProjekatSP;
using System.Net.Http;
using Newtonsoft.Json.Linq;

class Program
{
    private static readonly string ApiKey = ConfigurationManager.AppSettings["ApiKey"];
    private static readonly HttpClient http = new HttpClient();
    private static readonly string ApiBaseUrl = "http://api.airvisual.com/v2/city";

    static void Main()
    {
        var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:5050/");
        listener.Start();

        Console.WriteLine("Osluskujem 5050");

        while (true)
        {
            ThreadPool.QueueUserWorkItem(Request, listener.GetContext());
        }
    }

    static void Request(object state)
    {
        var context = (HttpListenerContext)state;
        var request = context.Request;
        var response = context.Response;
        string url = request.Url.ToString();
        Console.WriteLine($"Request: {url}");

        IQAir responseData;

        if (Kes.Contains(url))
        {
            responseData = Kes.ReadFromCache(url);
        }
        else
        {
            responseData = GetData(url);

            if (responseData == null) return;
            Kes.WriteToCache(url, responseData);
        }

        byte[] buffer = Encoding.UTF8.GetBytes(responseData.data.ToString());
        response.ContentLength64 = buffer.Length;
        response.ContentType = "text/html";
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }

    static IQAir GetData(string url)
    {
        if (url.Contains("favicon")) return null;

        int questionMark = url.IndexOf('?');
        if (questionMark < 0 || questionMark == url.Length - 1)
            return new IQAir("Error");

        var query = WebUtility.UrlDecode(url.Substring(questionMark + 1));
        var parameters = query.Split('&');
        var city = "";
        foreach (var parameter in parameters)
        {
            var parts = parameter.Split('=');
            if (parts.Length == 2 && parts[0] == "city")
            {
                city = parts[1];
                break;
            }
        }

        if (string.IsNullOrEmpty(city))
            return new IQAir("Error");

        try
        {
            HttpResponseMessage odgovor;
            try
            {
                string apiUrl = $"{ApiBaseUrl}?city={city}&state=Central Serbia&country=Serbia&key={ApiKey}";
                odgovor = http.GetAsync(apiUrl).Result;

                if (!odgovor.IsSuccessStatusCode)
                    throw new Exception(odgovor.StatusCode.ToString());
            }
            catch (Exception)
            {
                string apiUrl = $"{ApiBaseUrl}?city={city}&state=Autonomna Pokrajina Vojvodina&country=Serbia&key={ApiKey}";
                odgovor = http.GetAsync(apiUrl).Result;

                if (!odgovor.IsSuccessStatusCode)
                    throw new Exception(odgovor.StatusCode.ToString());
            }

            string responseContent = odgovor.Content.ReadAsStringAsync().Result;
            Console.WriteLine(responseContent);

            JObject nov = JObject.Parse(responseContent);
            IQAir name = new IQAir("Uspesno");
            name.data = (int)nov["data"]["current"]["pollution"]["aqius"];

            return name;
        }
        catch (Exception e)
        {
            return new IQAir("Greska: " + e);
        }
    }
}