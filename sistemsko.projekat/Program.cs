using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Text;
using ProjekatSP;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

class Program
{
    private static readonly string ApiKey = "0c335201-4b5e-4834-b2df-e93be0f8a6b5";
    private static readonly HttpClient http = new HttpClient();
    private static readonly Kes Cache = new Kes();
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

        lock (Cache)
        {
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
        var query = WebUtility.UrlDecode(url.Substring(url.IndexOf('?') + 1));
        var parameters = query.Split('&');
        var city = "";
        foreach (var parameter in parameters)
        {
            var parts = parameter.Split('=');

            if (parts.Length == 2)
            {
                city = parts[1];
                break;
            }
        }

        if (string.IsNullOrEmpty(city))
        {
            return new IQAir("Error");
        }

        try
        {
            HttpResponseMessage odgovor;
            try
            {
                query = $"city={city}&state=Central Serbia&country=Serbia";
                string apiUrl = $"{ApiBaseUrl}?{query}&key={ApiKey}";
                odgovor = http.GetAsync(apiUrl).Result;

                if (!odgovor.IsSuccessStatusCode)
                {
                    throw new Exception(odgovor.StatusCode.ToString());
                }
            }
            catch (Exception e)
            {
                query = $"city={city}&state=Autonomna Pokrajina Vojvodina&country=Serbia";
                string apiUrl = $"{ApiBaseUrl}?{query}&key={ApiKey}";
                odgovor = http.GetAsync(apiUrl).Result;

                if (!odgovor.IsSuccessStatusCode)
                {
                    throw new Exception(odgovor.StatusCode.ToString());
                }
            }
            using (var streamReader = new StreamReader(odgovor.Content.ReadAsStreamAsync().Result))
            {
                var responseText = streamReader.ReadToEnd();

                string responseContent =  odgovor.Content.ReadAsStringAsync().Result;
                Console.WriteLine(responseContent);

                JObject nov = JObject.Parse(responseContent);
                IQAir name = new IQAir("Uspesno");
                name.data = (int)nov["data"]["current"]["pollution"]["aqius"]; //Izvlacimo podatak koji nam je potreban


                return name;
            }
        }
        catch (Exception e)
        {
            return new IQAir("Greska: " + e);
        }
    }
}