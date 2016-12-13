using System;
using Android.App;
using Android.Appwidget;
using Android.Content;

namespace WeatherWidget
{
	[BroadcastReceiver (Label = "@string/widget_name")]
	[IntentFilter (new string [] { "android.appwidget.action.APPWIDGET_UPDATE" })]
	[MetaData ("android.appwidget.provider", Resource = "@xml/widget_word")]
	public class WordWidget : AppWidgetProvider
	{
		public override void OnUpdate (Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
		{
			// To prevent any ANR timeouts, we perform the update in a service
				context.StartService (new Intent (context, typeof (UpdateService)));
		}

		public override void OnReceive(Context context, Intent intent)
		{
			base.OnReceive(context, intent);
		}
	}
}
