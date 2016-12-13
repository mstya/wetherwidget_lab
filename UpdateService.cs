using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Newtonsoft.Json;
using APIXULib;
using Android.Graphics;
using System.Timers;

namespace WeatherWidget
{
	[Service]
	public class UpdateService : Service
	{
		private LocationManager _locationManager;
		private string _locationProvider;
		private RemoteViews updateViews;
		private Address address;
		private List<int> imageIds = new List<int>
		{	
			Resource.Id.imageView1,
			Resource.Id.imageView2,
			Resource.Id.imageView3,
			Resource.Id.imageView4,
			Resource.Id.imageView5
		};
		private List<int> textIds = new List<int>
		{
			Resource.Id.textView1,
			Resource.Id.textView2,
			Resource.Id.textView3,
			Resource.Id.textView4,
			Resource.Id.textView5
		};
		private List<int> days = new List<int>
		{
			Resource.Id.textView6,
			Resource.Id.textView7,
			Resource.Id.textView8,
			Resource.Id.textView9,
			Resource.Id.textView10
		};

		public override async void OnStart(Intent intent, int startId)
		{
			this.updateViews = new RemoteViews(this.PackageName, Resource.Layout.widget_word);

			await BuildUpdateAsync(this);

			ComponentName thisWidget = new ComponentName(this, Java.Lang.Class.FromType(typeof(WordWidget)).Name);
			AppWidgetManager manager = AppWidgetManager.GetInstance(this);

			manager.UpdateAppWidget(thisWidget, updateViews);

			System.Timers.Timer timer = new System.Timers.Timer();
			timer.Interval = 3600000;
			timer.Elapsed += OnTimedEvent;
			timer.Enabled = true;
		}

		async void OnTimedEvent(object sender, ElapsedEventArgs e)
		{
			this.updateViews = new RemoteViews(this.PackageName, Resource.Layout.widget_word);

			await BuildUpdateAsync(this);

			ComponentName thisWidget = new ComponentName(this, Java.Lang.Class.FromType(typeof(WordWidget)).Name);
			AppWidgetManager manager = AppWidgetManager.GetInstance(this);

			manager.UpdateAppWidget(thisWidget, updateViews);
		}

		private Bitmap GetImageBitmapFromUrl(string url)
		{
			Bitmap imageBitmap = null;

			using (var webClient = new WebClient())
			{
				var imageBytes = webClient.DownloadData(url);
				if (imageBytes != null && imageBytes.Length > 0)
				{
					imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
				}
			}

			return imageBitmap;
		}

		public override IBinder OnBind(Intent intent)
		{
			// We don't need to bind to this service
			return null;
		}

		public async Task BuildUpdateAsync(Context context)
		{
			string url = "http://api.apixu.com/v1/forecast.json?key=4bc73fd87035405d9eb171054161112&q=Kharkiv&days=5";
			WeatherModel data = FetchWeatherAsync(url);

			var iconToday = data.forecast.forecastday[0].day.condition.icon;
			var tempCToday = data.forecast.forecastday[0].hour.First(x => DateTime.Parse(x.time).Hour == DateTime.Now.Hour).temp_c;

			this.updateViews.SetImageViewBitmap(this.imageIds[0], GetImageBitmapFromUrl("http:" + iconToday));
			this.updateViews.SetTextViewText(this.textIds[0], new Java.Lang.String(tempCToday.ToString()));

			var titles = GetDayTitles();
			this.updateViews.SetTextViewText(this.days[0], titles[0]);

			for (int i = 1; i < data.forecast.forecastday.Count; i++)
			{
				var item = data.forecast.forecastday[i];
				var icon = item.day.condition.icon;
				var t = item.hour.First(x => DateTime.Parse(x.time).Hour == DateTime.Now.Hour).temp_c;

				this.updateViews.SetImageViewBitmap(this.imageIds[i], GetImageBitmapFromUrl("http:" + icon));
				this.updateViews.SetTextViewText(this.textIds[i], new Java.Lang.String(item.day.avgtemp_c.ToString()));

				this.updateViews.SetTextViewText(this.days[i], titles[i]);
			}
		}

		private List<string> GetDayTitles()
		{
			List<string> titles = new List<string>();
			var dayOfWeek = DateTime.Today.DayOfWeek;
			
			Dictionary<int, string> dayTitle = new Dictionary<int, string>()
			{
				[0] = "Вс",
				[1] = "Пн",
				[2] = "Вт",
				[3] = "Ср",
				[4] = "Чт",
				[5] = "Пт",
				[6] = "Сб"
			};

			int count = 0;
			for (int i = (int)dayOfWeek; i < dayTitle.Count; i++)
			{
				titles.Add(dayTitle[i]);
				count++;
				if (count == 5)
				{
					return titles;
				}
			}
			if (count != 5)
			{
				for (int i = 0; i < dayTitle.Count; i++)
				{
					titles.Add(dayTitle[i]);

					count++;
					if (count == 5)
					{
						return titles;
					}
				}
			}
			return titles;
		}

		private WeatherModel FetchWeatherAsync(string url)
		{
			WebClient client = new WebClient();
			string json = client.DownloadString(new Uri(url));

			WeatherModel obj = JsonConvert.DeserializeObject<WeatherModel>(json);
			return obj;
		}
	}
}
