using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SpotifyAuthCode.Models;

namespace SpotifyAuthCode.Controllers
{
    public class SpotifyController : Controller
    {
        // Get these from your application dashboard at Spotify
        string clientId = "YOUR_CLIENT_ID";
        string clientSecret = "YOUR_CLIENT_SECRET";

        // Must match callback URLs from app settings at Spotify
        string returnURL = "YOUR_RETURN_URL";

        // Add scopes separated by a space, "streaming user-read-email ..."
        // Read more at https://developer.spotify.com/documentation/general/guides/scopes/
        string scopes = "ADD_SCOPES"; 

        [Route("")]
        public IActionResult Index()
        {
            var qb = new QueryBuilder();
            qb.Add("response_type", "code");
            qb.Add("client_id", clientId);
            qb.Add("scope", scopes);
            qb.Add("redirect_uri", returnURL);

            ViewBag.Query = qb.ToQueryString().ToString();

            return View();
        }

        [Route("/cb")]
        public IActionResult Code(string code)
        {
            string auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

            List<KeyValuePair<string, string>> args = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", returnURL),
            };

            Token token;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
                var content = new FormUrlEncodedContent(args);

                var response = client.PostAsync("https://accounts.spotify.com/api/token", content).Result;
                var responseContent = response.Content;
                var responseString = responseContent.ReadAsStringAsync().Result;

                token = JsonConvert.DeserializeObject<Token>(responseString);
            }

            // Use token with your requests to Spotify
            // Refresh access token when needed with the given RefreshToken
            return Json(token);
        }
    }
}