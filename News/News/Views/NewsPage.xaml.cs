using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using News.Models;
using News.Services;

namespace News.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewsPage : ContentPage
    {
        NewsService service;

        public NewsPage()
        {
            InitializeComponent();

            service = new NewsService();
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();

            //Code here will run right before the screen appears
            //You want to set the Title or set the City

            //This is making the first load of data
            MainThread.BeginInvokeOnMainThread(async () => { await LoadNews(); });
        }

        private async Task LoadNews()
        {
            Exception exception = null;
            NewsService service = new NewsService();
            Task<NewsGroup> t1 = null;

            try
            {
                loading.IsVisible = true;
                errorMsg.IsVisible = false;
                errorMsgEx.Text = "";
                await service.GetNewsAsync((NewsCategory)Enum.Parse(typeof(NewsCategory), Title.ToLower()));
                t1 = service.GetNewsAsync((NewsCategory)Enum.Parse(typeof(NewsCategory), Title.ToLower()));
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            Task.WaitAll(t1);

            if (t1?.Status == TaskStatus.RanToCompletion)
            {
                articlesListView.ItemsSource = t1.Result.Articles;
                loading.IsVisible = false;
            }
            else
            {
                loading.IsVisible = false;
                errorMsg.IsVisible = true;
                errorMsgEx.Text = exception?.Message;
            }
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            await LoadNews();
        }

        private async void articlesListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            NewsItem newsItem = (NewsItem)e.Item;
            await Navigation.PushAsync(new ArticleView(newsItem.Url));
        }
    }
}