using HeliosClockCommon.Defaults;
using Microsoft.AspNetCore.Connections;
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
		private UdpClient client;

		/// <summary>Starts the discovery client.</summary>
		/// <param name="cancellationToken">The cancellation token.</param>
		public async Task StartDiscoveryClient(CancellationToken cancellationToken)
		{
			localCancellationTokenSource?.Cancel();
			localCancellationTokenSource = new CancellationTokenSource();
			localCancellationToken = localCancellationTokenSource.Token;

			try
			{
				client = new UdpClient(DefaultDiscoveryValues.DiscoveryPort, AddressFamily.InterNetwork);
			}
			catch (Exception ex) //when (ex is AddressInUseException || ex is SocketException)
			{
				//If address is in use, Discovery Server may be running in this machine. Try to listen anyway
				client = new UdpClient();
			}

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
					client.EnableBroadcast = true;
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
			client.Close();
		}

		/// <summary>Stops the discovery client.</summary>
		public void StopDiscoveryClient()
		{
			localCancellationTokenSource?.Cancel();
			client?.Dispose();
		}
	}
}