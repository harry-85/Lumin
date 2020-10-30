using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using HeliosClockCommon.Messages;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HeliosClockApp.Droid
{
	[Service]
	public class AppDiscoveryService : Service
	{
		public event EventHandler<EventArgs<IPAddress>> OnIpDiscovered;
		static readonly string TAG = typeof(AppDiscoveryService).FullName;
		static readonly int NOTIFICATION_ID = 10000;

		bool isStarted;
		Handler handler;
		Action runnable;

		public AppDiscoveryService Instance { get { return this; } }

		public override void OnCreate()
		{
			base.OnCreate();

			Log.Info(TAG, "OnCreate: the discovery service is initializing.");

			runnable = async () => await StartClientDiscovery().ConfigureAwait(false);

			handler = new Handler();
		}

		public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
		{
			if (isStarted)
			{
				Log.Info(TAG, "OnStartCommand: This discovery service has already been started.");
			}
			else
			{
				Log.Info(TAG, "OnStartCommand: The discovery service is starting.");
				//DispatchNotificationThatServiceIsRunning();
				handler.Post(runnable);
				isStarted = true;
			}

			// This tells Android to restart the service if it is killed.
			return StartCommandResult.Sticky;
		}


		public override IBinder OnBind(Intent intent)
		{
			// Return null because this is a pure started service. A hybrid service would return a binder that would
			// allow access to the GetFormattedStamp() method.
			return null;
		}


		public override void OnDestroy()
		{
			// We need to shut things down.
			Log.Info(TAG, "OnDestroy: The discovery service is shutting down.");

			// Stop the handler.
			handler.RemoveCallbacks(runnable);

			// Remove the notification from the status bar.
			var notificationManager = (NotificationManager)GetSystemService(NotificationService);
			notificationManager.Cancel(NOTIFICATION_ID);

			isStarted = false;
			base.OnDestroy();
		}

		////private void DispatchNotificationThatServiceIsRunning()
		////{
		////	Notification.Builder notificationBuilder = new Notification.Builder(this)
		////		.SetSmallIcon(Resource.Drawable.icon_feed)
		////		.SetContentTitle("Discovery Service Started!")
		////		.SetContentText("The discovery service has been started!");

		////	var notificationManager = (NotificationManager)GetSystemService(NotificationService);
		////	notificationManager.Notify(NOTIFICATION_ID, notificationBuilder.Build());
		////}

		private async Task StartClientDiscovery()
		{
			while (isStarted)
			{
				var Client = new UdpClient();
				var RequestData = Encoding.ASCII.GetBytes("SomeRequestData");
				var ServerEp = new IPEndPoint(IPAddress.Any, 0);

				Client.EnableBroadcast = true;

				try
				{

					await Client.SendAsync(RequestData, RequestData.Length, new IPEndPoint(IPAddress.Broadcast, 8888)).ConfigureAwait(false);

					var ServerResponseData = Client.Receive(ref ServerEp);
					var ServerResponse = Encoding.ASCII.GetString(ServerResponseData);

					Log.Info("Recived {0} from {1}", ServerResponse, ServerEp.Address.ToString());

					OnIpDiscovered?.Invoke(this, new EventArgs<IPAddress>(ServerEp.Address));

					MessagingCenter.Send<IpDiscoveredTaskMessage, IPAddress>(new IpDiscoveredTaskMessage(), "IpDiscovered", ServerEp.Address);

					Client.Close();
				}
				catch
				{ 
				}
				await Task.Delay(200).ConfigureAwait(false);
			}
		}
	}
}