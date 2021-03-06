using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Net.Http;
using System.Net.Http.Json; //Requires nuget package System.Net.Http.Json
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;

using News.Models;
using News.Services;

namespace News.Consoles
{
    //Your can move your Console application Main here. Rename Main to myMain and make it NOT static and async
    class Program
    {
        #region used by the Console
        Views.ConsolePage theConsole;
        StringBuilder theConsoleString;
        public Program (Views.ConsolePage myConsole)
        {
            //used for the Console
            theConsole = myConsole;
            theConsoleString = new StringBuilder();
        }
        #endregion

        #region Console Demo program
        //This is the method you replace with your async method renamed and NON static Main
        public async Task myMain()
        {
            Exception exception = null;
            NewsService service = new NewsService();
            service.NewsAvailable += ReportNewsDataAvailable;

            Task<NewsGroup>[] t = new Task<NewsGroup>[Enum.GetNames(typeof(NewsCategory)).Length];

            for (int i = (int)NewsCategory.business; i < (int)NewsCategory.technology + 1; i++)
            {
                try
                {
                    await service.GetNewsAsync((NewsCategory)i);
                    t[i] = service.GetNewsAsync((NewsCategory)i);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            }

            Task.WaitAll(t);

            theConsoleString.AppendLine("--------------------");

            foreach (var item in t)
            {
                if (item.Status == TaskStatus.RanToCompletion)
                {
                    NewsGroup news = item.Result;
                    theConsoleString.AppendLine($"News in Category {item.Result.Category}");
                    foreach (var article in item.Result.Articles)
                    {
                        theConsoleString.AppendLine($" - {article.DateTime}: {article.Title} ");
                    }
                    theConsoleString.AppendLine("");
                }
                else
                {
                    theConsoleString.AppendLine($"Error: {exception.Message}");
                }
            }
            theConsole.WriteLine(theConsoleString.ToString());
            theConsoleString.Clear();
        }

        void ReportNewsDataAvailable(object sender, string message)
        {
            theConsole.WriteLine($"Event message from news service: {message}");
        }
        #endregion
    }
}
