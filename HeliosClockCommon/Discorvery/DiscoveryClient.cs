using HeliosClockCommon.Defaults;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeliosClockCommon.Discorvery
{
	public class DiscoveryClient : IDisposable
	{
		public event EventHandler<EventArgs<IPAddress>> OnIpDiscovered;
		private CancellationTokenSource localCancellationTokenSource;
		private CancellationToken localCancellationToken;
		private readonly UdpClient client;

        /// <summary>Initializes a new instance of the <see cref="DiscoveryClient"/> class.</summary>
        public DiscoveryClient(DiscoverFactory factory)
		{
			client = factory.UdpClient;
		}

		/// <summary>Starts the discovery client.</summary>
		/// <param name="cancellationToken">The cancellation token.</param>
		public async Task StartDiscoveryClient(CancellationToken cancellationToken)
		{
			localCancellationTokenSource?.Cancel();
			localCancellationTokenSource = new CancellationTokenSource();
			localCancellationToken = localCancellationTokenSource.Token;

			var RequestData = Encoding.ASCII.GetBytes(DefaultDiscoveryValues.DefaultDiscoveryRequest);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			Task.Run(async () =>
			{
				while (!cancellationToken.IsCancellationRequested && !localCancellationToken.IsCancellationRequested)
				{
					await client.SendAsync(RequestData, RequestData.Length, new IPEndPoint(IPAddress.Broadcast, DefaultDiscoveryValues.DiscoveryPort)).ConfigureAwait(false);
					await Task.Delay(500).ConfigureAwait(false);
				}
			}, cancellationToken);

			var receiveTask = Task.Run(async () =>
			{
				while (!cancellationToken.IsCancellationRequested && !localCancellationToken.IsCancellationRequested)
				{
					try
					{
						var serverResponseData = await client.ReceiveAsync().ConfigureAwait(false);
						var serverResponse = Encoding.ASCII.GetString(serverResponseData.Buffer);

						if (serverResponse != DefaultDiscoveryValues.DefaultDiscoveryResponse)
							continue;

						OnIpDiscovered?.Invoke(this, new EventArgs<IPAddress>(serverResponseData.RemoteEndPoint.Address));
					}
					catch
					{
					}
				}
			}, cancellationToken);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

			try
			{
				await Task.Run(() => Task.WaitAll(new Task[] { receiveTask }, localCancellationToken)).ConfigureAwait(false);
			}
			catch
			{}

			localCancellationTokenSource.Cancel();
		}

		/// <summary>Stops the discovery client.</summary>
		public void StopDiscoveryClient()
		{
			localCancellationTokenSource?.Cancel();
		}

		public void Dispose()
		{
			client?.Close();
			client?.Dispose();
		}
	}
}