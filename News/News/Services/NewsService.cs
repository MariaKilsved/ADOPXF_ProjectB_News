//#define UseNewsApiSample  // Remove or undefine to use your own code to read live data

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

        //HttpClient httpClient = new HttpClient();
        WebClient client = new WebClient();
        readonly string apiKey = "db0929c407f3440ca443b379f6781dba";
        //db0929c407f3440ca443b379f6781dba
        //6684a8228f5f462ab6f1df57634ccc4a

        // change time to account for disk saves, in minutes.
        private const int CacheTime = 60;
        ConcurrentDictionary<NewsCategory, (DateTime, NewsGroup)> data = new ConcurrentDictionary<NewsCategory, (DateTime, NewsGroup)>();
        public event EventHandler<string> NewsAvailable;

        private void PopulateData(NewsCategory category)
        {
            // Try to get the cached files for every category, throwing an exception in case the file doesn't exist
            NewsCacheKey cachedKey = new NewsCacheKey(category, DateTime.Now);

            if (cachedKey.CacheExist)
            {
                NewsGroup cachedData = NewsGroup.Deserialize(cachedKey.FileName);
                data.TryAdd(category, (DateTime.Now, cachedData));
            }
            else
            {
                throw new Exception($"Cached news for {category.ToString()} is not available.");
            }
        }

        private void CacheData(NewsGroup cacheData)
        {
            NewsCacheKey cacheKey = new NewsCacheKey(cacheData.Category, DateTime.Now);
            NewsGroup.Serialize(cacheData, cacheKey.FileName);
            data.Clear();
        }
        public async Task<NewsGroup> GetNewsAsync(NewsCategory category)
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
            var webclient = new WebClient();
            var response = await webclient.DownloadStringTaskAsync(uri);
            NewsApiData nd = Newtonsoft.Json.JsonConvert.DeserializeObject<NewsApiData>(response);

#endif
            var news = new NewsGroup();
            news.Articles = new List<NewsItem>();
            news.Category = category;
            nd.Articles.ForEach(x => news.Articles.Add(new NewsItem { DateTime = x.PublishedAt, Title = x.Title, Description = x.Description, Url = x.Url, UrlToImage = x.UrlToImage }));
            NewsAvailable?.Invoke(this, $"News in category is available: {category}");

            // Create a cacheData object containing all relevant data for the cach (category name, datetime and news)
            CacheData(news);

            return news;
        }
    }
}
