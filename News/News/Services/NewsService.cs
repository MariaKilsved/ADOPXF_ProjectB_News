#define UseNewsApiSample  // Remove or undefine to use your own code to read live data

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http.Json;

using News.Models;
using News.ModelsSampleData;

namespace News.Services
{
    public class NewsService
    {

        //Here is where you lift in your Service code from Part A
        /*
                public async Task<NewsGroup> GetNewsAsync(NewsCategory category)
                {

        #if UseNewsApiSample      
                    NewsApiData nd = await NewsApiSampleData.GetNewsApiSampleAsync(category);

        #else
                    //https://newsapi.org/docs/endpoints/top-headlines
                    var uri = $"https://newsapi.org/v2/top-headlines?country=se&category={category}&apiKey={apiKey}";


                    //Recommend to use Newtonsoft Json Deserializer as it works best with Android
                    var webclient = new WebClient();
                    var json = await webclient.DownloadStringTaskAsync(uri);
                    NewsApiData nd = Newtonsoft.Json.JsonConvert.DeserializeObject<NewsApiData>(json);

        #endif
        */

        HttpClient httpClient = new HttpClient();
        WebClient client = new WebClient();
        readonly string apiKey = "db0929c407f3440ca443b379f6781dba";
        //db0929c407f3440ca443b379f6781dba
        //6684a8228f5f462ab6f1df57634ccc4a

        // change time to account for disk saves, in minutes.
        private const int CacheTime = 60;
        ConcurrentDictionary<NewsCategory, (DateTime, Models.News)> data = new ConcurrentDictionary<NewsCategory, (DateTime, Models.News)>();
        public event EventHandler<string> NewsAvailable;

        private void PopulateData(NewsCategory category)
        {
            // Try to get the cached files for every category, throwing an exception in case the file doesn't exist
            try
            {
                CacheData cachedData = News.Models.CacheData.Deserialize(category.ToString() + ".xml");
                data.TryAdd(cachedData.Category, (cachedData.Time, cachedData.News));
            }
            catch (Exception)
            {
                throw new Exception($"Cached news for {category.ToString()} is not available.");
            }
        }

        private void CacheData(CacheData cacheData)
        {
            data.Clear();
            News.Models.CacheData.Serialize(cacheData.Category.ToString() + ".xml", cacheData);
        }
        public async Task<Models.News> GetNewsAsync(NewsCategory category)
        {


            //part of cache code here
            // Load deserialized data from file to the ConcurrentDictionary as it is more flexible to work with
            try
            {
                PopulateData(category);

                if (data.TryGetValue(category, out var _cacheNews))
                {
                    if ((int)(DateTime.Now - _cacheNews.Item1).TotalMinutes <= CacheTime)
                    {
                        NewsAvailable?.Invoke(this, $"Cached news for {category.ToString()} available.");
                        return _cacheNews.Item2;
                    }
                    else
                    {
                        NewsAvailable?.Invoke(this, $"Cached news for {category.ToString()} is not available.");
                    }
                }
            }
            catch (Exception ex)
            {
                NewsAvailable?.Invoke(this, ex.Message);
            }
#if UseNewsApiSample
            NewsApiData nd = await NewsApiSampleData.GetNewsApiSampleAsync(category);

#else
            //https://newsapi.org/docs/endpoints/top-headlines
            var uri = $"https://newsapi.org/v2/top-headlines?country=se&category={category}&apiKey={apiKey}";

            // Your code here to get live data
            HttpResponseMessage response = await httpClient.GetAsync(uri);
            var response2 = client.DownloadStringTaskAsync(uri);
            response.EnsureSuccessStatusCode();
            NewsApiData nd = await response.Content.ReadFromJsonAsync<NewsApiData>();
            //NewsApiData nd = await JsonConvert.DeserializeObject<NewsApiData>(response.Result);

#endif
            var news = new Models.News();
            news.Articles = new List<NewsItem>();
            news.Category = category;
            nd.Articles.ForEach(x => news.Articles.Add(new NewsItem { DateTime = x.PublishedAt, Title = x.Title, Description = x.Description, Url = x.Url, UrlToImage = x.UrlToImage }));
            NewsAvailable?.Invoke(this, $"News in category is available: {category}");

            // Create a cacheData object containing all relevant data for the cach (category name, datetime and news)
            CacheData cacheData = new CacheData { Category = category, Time = DateTime.Now, News = news };
            CacheData(cacheData);

            return news;
        }
    }
}
