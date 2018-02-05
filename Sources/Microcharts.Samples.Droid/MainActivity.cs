using Android.App;
using Android.Widget;
using Android.OS;
using Microcharts.Droid;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System;

namespace Microcharts.Samples.Droid
{
	[Activity(Label = "Microcharts", MainLauncher = true, Icon = "@mipmap/icon")]
	public class MainActivity : Activity
	{
		Random random = new Random();
		ObservableCollection<Entry> data = new ObservableCollection<Entry>();
		protected override async void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);

			var charts = Data.CreateQuickstart();

			foreach (var entry in charts[0].Entries)
				data.Add(entry);
			charts[0].Entries = data;
			FindViewById<ChartView>(Resource.Id.chartView1).Chart = charts[0];
			FindViewById<ChartView>(Resource.Id.chartView2).Chart = charts[1];
			FindViewById<ChartView>(Resource.Id.chartView3).Chart = charts[2];
			FindViewById<ChartView>(Resource.Id.chartView4).Chart = charts[3];

			await Task.Run(async () =>
			{
				while (true)
				{
					await Task.Delay(2000);
					data.Add(new Entry(random.Next()));
				}
			});

		}
	}
}

