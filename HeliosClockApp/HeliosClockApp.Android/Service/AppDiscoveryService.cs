using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using HeliosClockCommon.Messages;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace HeliosClockApp.Droid
{
	[Service]
	public class AppDiscoveryService : Service
	{
		private CancellationTokenSource cancellationTokenSource;
		private CancellationToken cancellationToken;
		public event EventHandler<EventArgs<IPAddress>> OnIpDiscovered;
		static readonly string TAG = typeof(AppDiscoveryService).FullName;
		static readonly int NOTIFICATION_ID = 10000;

		Handler handler;
		Action runnable;

		public AppDiscoveryService Instance { get { return this; } }

		public override void OnCreate()
		{
			base.OnCreate();

			Log.Info(TAG, "OnCreate: the discovery service is initializing.");

			handler = new Handler(Looper.MainLooper);
		}

		/// <summary>
		/// Called by the system every time a client explicitly starts the service by calling
		/// <see cref="M:Android.Content.Context.StartService(Android.Content.Intent)" />, providing the arguments it supplied and a
		/// unique integer token representing the start request.
		/// </summary>
		/// <param name="intent">The Intent supplied to <see cref="M:Android.Content.Context.StartService(Android.Content.Intent)" />,
		/// as given.  This may be null if the service is being restarted after
		/// its process has gone away, and it had previously returned anything
		/// except <format type="text/html"><a href="https://docs.microsoft.com/en-us/search/index?search='Android App Service START_STICKY_COMPATIBILITY';scope=Xamarin" title="Android.App.Service.START_STICKY_COMPATIBILITY">Android.App.Service.START_STICKY_COMPATIBILITY</a></format>.</param>
		/// <param name="flags">Additional data about this start request.  Currently either
		/// 0, <format type="text/html"><a href="https://docs.microsoft.com/en-us/search/index?search='Android App Service START_FLAG_REDELIVERY';scope=Xamarin" title="Android.App.Service.START_FLAG_REDELIVERY">Android.App.Service.START_FLAG_REDELIVERY</a></format>, or <format type="text/html"><a href="https://docs.microsoft.com/en-us/search/index?search='Android App Service START_FLAG_RETRY';scope=Xamarin" title="Android.App.Service.START_FLAG_RETRY">Android.App.Service.START_FLAG_RETRY</a></format>.</param>
		/// <param name="startId">A unique integer representing this specific request to
		/// start.  Use with <see cref="M:Android.App.Service.StopSelfResult(System.Int32)" />.</param>
		/// <returns>To be added.</returns>
		/// <remarks>
		/// Portions of this page are modifications based on work created and shared by the <format type="text/html"><a href="https://developers.google.com/terms/site-policies" title="Android Open Source Project">Android Open Source Project</a></format> and used according to terms described in the <format type="text/html"><a href="https://creativecommons.org/licenses/by/2.5/" title="Creative Commons 2.5 Attribution License">Creative Commons 2.5 Attribution License.</a></format>
		/// </remarks>
		/// <since version="Added in API level 5" />
		/// <altmember cref="M:Android.App.Service.StopSelfResult(System.Int32)" />
		public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
		{
			Log.Info(TAG, "OnStartCommand: Stop running discovery service!");
			cancellationTokenSource?.Cancel();


			if (runnable != null)
				handler.RemoveCallbacks(runnable);

			runnable = null;

			cancellationTokenSource = new CancellationTokenSource();
			cancellationToken = cancellationTokenSource.Token;

			runnable = async () => await StartClientDiscovery().ConfigureAwait(false);
			

			Log.Info(TAG, "OnStartCommand: Discovery Service Starting.");

			//DispatchNotificationThatServiceIsRunning();
			handler.Post(runnable);

			// This tells Android to restart the service if it is killed.
			return StartCommandResult.Sticky;
		}


		/// <summary>Return the communication channel to the service.</summary>
		/// <param name="intent">The Intent that was used to bind to this service,
		/// as given to <format type="text/html"><a href="https://docs.microsoft.com/en-us/search/index?search='M:Android Content Context BindService(Android Content Intent,Android Content IServiceConnection,Android Content IServiceConnection)';scope=Xamarin" title="M:Android.Content.Context.BindService(Android.Content.Intent,Android.Content.IServiceConnection,Android.Content.IServiceConnection)">M:Android.Content.Context.BindService(Android.Content.Intent,Android.Content.IServiceConnection,Android.Content.IServiceConnection)</a></format>.  Note that any extras that were included with
		/// the Intent at that point will <i>not</i> be seen here.</param>
		/// <returns>To be added.</returns>
		/// <remarks>
		/// Portions of this page are modifications based on work created and shared by the <format type="text/html"><a href="https://developers.google.com/terms/site-policies" title="Android Open Source Project">Android Open Source Project</a></format> and used according to terms described in the <format type="text/html"><a href="https://creativecommons.org/licenses/by/2.5/" title="Creative Commons 2.5 Attribution License">Creative Commons 2.5 Attribution License.</a></format>
		/// </remarks>
		/// <since version="Added in API level 1" />
		public override IBinder OnBind(Intent intent)
		{
			// Return null because this is a pure started service. A hybrid service would return a binder that would
			// allow access to the GetFormattedStamp() method.
			return null;
		}


		/// <summary>Called by the system to notify a Service that it is no longer used and is being removed.</summary>
		/// <remarks>
		/// Portions of this page are modifications based on work created and shared by the <format type="text/html"><a href="https://developers.google.com/terms/site-policies" title="Android Open Source Project">Android Open Source Project</a></format> and used according to terms described in the <format type="text/html"><a href="https://creativecommons.org/licenses/by/2.5/" title="Creative Commons 2.5 Attribution License">Creative Commons 2.5 Attribution License.</a></format>
		/// </remarks>
		/// <since version="Added in API level 1" />
		public override void OnDestroy()
		{
			// We need to shut things down.
			Log.Info(TAG, "OnDestroy: The discovery service is shutting down.");

			//Stop the service
			cancellationTokenSource?.Cancel();

			// Stop the handler.
			handler.RemoveCallbacks(runnable);

			// Remove the notification from the status bar.
			var notificationManager = (NotificationManager)GetSystemService(NotificationService);
			notificationManager.Cancel(NOTIFICATION_ID);

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

		/// <summary>Starts the client discovery.</summary>
		private async Task StartClientDiscovery()
		{
			var Client = new UdpClient();
			var RequestData = Encoding.ASCII.GetBytes("HeliosClockIpBroadcast");

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			Task.Run(async () =>
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					await Client.SendAsync(RequestData, RequestData.Length, new IPEndPoint(IPAddress.Broadcast, 8888)).ConfigureAwait(false);
					await Task.Delay(500).ConfigureAwait(false);
				}
			});
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

			while (!cancellationToken.IsCancellationRequested)
			{
				Client.EnableBroadcast = true;

				try
				{
					var ServerResponseData = await Client.ReceiveAsync().ConfigureAwait(false);
					var ServerResponse = Encoding.ASCII.GetString(ServerResponseData.Buffer);

					
					Log.Debug("Revived {0} from {1}", ServerResponse, ServerResponseData.RemoteEndPoint.Address);

					OnIpDiscovered?.Invoke(this, new EventArgs<IPAddress>(ServerResponseData.RemoteEndPoint.Address));

					MessagingCenter.Send<IpDiscoveredTaskMessage, IPAddress>(new IpDiscoveredTaskMessage(), "IpDiscovered", ServerResponseData.RemoteEndPoint.Address);
				}
				catch
				{
				}
			}
			Client.Close();
		}
	}
}