using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeliosClockCommon.Discorvery
{
	public class DiscoveryClient
	{
		public event EventHandler<EventArgs<IPAddress>> OnIpDiscovered;
		private CancellationTokenSource localCancellationTokenSource;
		private CancellationToken localCancellationToken;

		public async Task StartDiscoveryCient(CancellationToken cancellationToken)
		{
			localCancellationTokenSource?.Cancel();
			localCancellationTokenSource = new CancellationTokenSource();
			localCancellationToken = localCancellationTokenSource.Token;


			var Client = new UdpClient();
			var RequestData = Encoding.ASCII.GetBytes("HeliosClockIpBroadcast");

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			Task.Run(async () =>
			{
				while (!cancellationToken.IsCancellationRequested && !localCancellationToken.IsCancellationRequested)
				{
					await Client.SendAsync(RequestData, RequestData.Length, new IPEndPoint(IPAddress.Broadcast, 8888)).ConfigureAwait(false);
					await Task.Delay(500).ConfigureAwait(false);
				}
			});
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

			while (!cancellationToken.IsCancellationRequested && !localCancellationToken.IsCancellationRequested)
			{
				Client.EnableBroadcast = true;

				try
				{
					var ServerResponseData = await Client.ReceiveAsync().ConfigureAwait(false);
					var ServerResponse = Encoding.ASCII.GetString(ServerResponseData.Buffer);

					OnIpDiscovered?.Invoke(this, new EventArgs<IPAddress>(ServerResponseData.RemoteEndPoint.Address));
				}
				catch
				{
				}
			}
			Client.Close();
		}


		public void StopDiscoveryClien()
		{
			localCancellationTokenSource?.Cancel();
		}
	}
}