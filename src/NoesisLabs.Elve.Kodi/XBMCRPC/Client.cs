using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using NoesisLabs.Elve.Kodi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace XBMCRPC
{
	public partial class Client : IDisposable
	{
		private readonly ConnectionSettings _settings;
		private ISocket _clientSocket;
		private uint JsonRpcId = 0;
		private NotificationListenerSocketState socketState = new NotificationListenerSocketState();
		private Stream stream;

		public Client(ConnectionSettings settings, IPlatformServices platformServices)
		{
			PlatformServices = platformServices;
			Serializer = new JsonSerializer();
			Serializer.Converters.Add(new StringEnumConverter());
			_settings = settings;
			Addons = new Methods.Addons(this);
			Application = new Methods.Application(this);
			AudioLibrary = new Methods.AudioLibrary(this);
			Favourites = new Methods.Favourites(this);
			Files = new Methods.Files(this);
			GUI = new Methods.GUI(this);
			Input = new Methods.Input(this);
			JSONRPC = new Methods.JSONRPC(this);
			Player = new Methods.Player(this);
			Playlist = new Methods.Playlist(this);
			Profiles = new Methods.Profiles(this);
			PVR = new Methods.PVR(this);
			Settings = new Methods.Settings(this);
			System = new Methods.System(this);
			Textures = new Methods.Textures(this);
			VideoLibrary = new Methods.VideoLibrary(this);
			XBMC = new Methods.XBMC(this);
		}

		public Methods.Addons Addons { get; private set; }
		public Methods.Application Application { get; private set; }
		public Methods.AudioLibrary AudioLibrary { get; private set; }
		public Methods.Favourites Favourites { get; private set; }
		public Methods.Files Files { get; private set; }
		public Methods.GUI GUI { get; private set; }
		public Methods.Input Input { get; private set; }
		public Methods.JSONRPC JSONRPC { get; private set; }
		public Methods.Player Player { get; private set; }
		public Methods.Playlist Playlist { get; private set; }
		public Methods.Profiles Profiles { get; private set; }
		public Methods.PVR PVR { get; private set; }
		public Methods.Settings Settings { get; private set; }
		public Methods.System System { get; private set; }
		public Methods.Textures Textures { get; private set; }
		public Methods.VideoLibrary VideoLibrary { get; private set; }
		public Methods.XBMC XBMC { get; private set; }
		internal IPlatformServices PlatformServices { get; set; }
		internal JsonSerializer Serializer { get; private set; }

		public void Dispose()
		{
			var socket = _clientSocket;
			_clientSocket = null;
			if (socket != null)
			{
				socket.Dispose();
			}
		}

		public bool IsNotificationListenerConnected()
		{
			return (this._clientSocket != null && this._clientSocket.IsConnected());
		}

		public void StartNotificationListener()
		{
			_clientSocket = PlatformServices.SocketFactory.GetSocket();
			_clientSocket.Connect(_settings.Host, _settings.TcpPort, (result) =>
			{
				var stream = _clientSocket.GetInputStream();
				ListenForNotifications(stream);
			});
		}

		internal T GetData<T>(string method, object args)
		{
			var request = WebRequest.Create(_settings.JsonInterfaceAddress);
			request.Credentials = new NetworkCredential(_settings.UserName, _settings.Password);
			request.ContentType = "application/json";
			request.Method = "POST";
			var postStream = request.GetRequestStream();

			var requestId = JsonRpcId++;
			var jsonRequest = BuildJsonPost(method, args, requestId);
			byte[] postData = Encoding.UTF8.GetBytes(jsonRequest);
			postStream.Write(postData, 0, postData.Length);
			postStream.Dispose();

			var response = request.GetResponse();
			var responseStream = response.GetResponseStream();
			string responseData = null;
			if (responseStream != null)
			{
				var streamReader = new StreamReader(responseStream);
				responseData = streamReader.ReadToEnd();
				responseStream.Dispose();
				streamReader.Dispose();
			}

			//response.Dispose();

			JObject query = JObject.Parse(responseData);
			var error = query["error"];
			if (error != null)
			{
				throw new Exception(error.ToString());
			}
			var result = query["result"].ToObject<T>(Serializer);
			return result;
		}

		private static string BuildJsonPost(string method, object args, uint id)
		{
			var jsonPost = new JObject { new JProperty("jsonrpc", "2.0"), new JProperty("method", method) };
			if (args != null)
			{
				jsonPost.Add(new JProperty("params", args));
			}
			jsonPost.Add(new JProperty("id", id));

			return jsonPost.ToString();
		}

		private void ListenForNotifications(Stream stream)
		{
			this.stream = stream;
			this.stream.BeginRead(this.socketState.Buffer, 0, NotificationListenerSocketState.BufferSize, this.HandleNotifications, null);
		}

		private void HandleNotifications(IAsyncResult result)
		{
			//get number of bytes read
			var receivedDataLength = this.stream.EndRead(result);

			//get new message from socket
			var receivedDataJson = Encoding.UTF8.GetString(this.socketState.Buffer, 0, receivedDataLength);

			//Console.WriteLine("<-- start message -->" + Environment.NewLine + receivedDataJson + Environment.NewLine + "<-- end message -->");

			//append new message to any left over portion of previous message(s)
			this.socketState.Builder.Append(receivedDataJson);

			//Console.WriteLine("<-- start pre-process json -->" + Environment.NewLine + this.socketState.Builder.ToString() + Environment.NewLine + "<-- end pre-process json -->");

			int index = this.socketState.Builder.ToString().IndexOfLastTokenEnd();

			//Console.WriteLine("<-- start token end index -->" + Environment.NewLine + index.ToString() + Environment.NewLine + "<-- end token end index -->");

			if (index > 0)
			{
				//get complete json notifications
				string notificationsJson = this.socketState.Builder.ToString().Substring(0, index + 1);

				//Console.WriteLine("<-- start notification json -->" + Environment.NewLine + notificationsJson + Environment.NewLine + "<-- end notification json -->");

				var notifications = this.ParseJson(notificationsJson);

				foreach (var notification in notifications)
				{
					ParseNotification(notification);
				}

				//save any left over portion of message
				this.socketState.Builder.Remove(0, index + 1);

				//Console.WriteLine("<-- start post-process json -->" + Environment.NewLine + this.socketState.Builder.ToString() + Environment.NewLine + "<-- end post-process json -->");
			}

			this.stream.BeginRead(this.socketState.Buffer, 0, NotificationListenerSocketState.BufferSize, this.HandleNotifications, null);
		}

		private IEnumerable<JObject> ParseJson(string json)
		{
			var serializer = new JsonSerializer();

            using (var reader = new StringReader(json))
			using (var jsonReader = new JsonTextReader(reader))
			{
				jsonReader.SupportMultipleContent = true;

				while (jsonReader.Read())
				{
					yield return serializer.Deserialize<JObject>(jsonReader);
				}
			}
		}

		private void ParseNotification(JObject jObject)
		{
			if (jObject["method"] != null)
			{
				string _method;
				_method = jObject["method"].ToString();
				switch (_method)
				{
					case "Application.OnVolumeChanged":
						Application.RaiseOnVolumeChanged(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<XBMCRPC.Application.OnVolumeChanged_data>(Serializer)
			);
						break;

					case "AudioLibrary.OnCleanFinished":
						AudioLibrary.RaiseOnCleanFinished(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<object>(Serializer)
			);
						break;

					case "AudioLibrary.OnCleanStarted":
						AudioLibrary.RaiseOnCleanStarted(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<object>(Serializer)
			);
						break;

					case "AudioLibrary.OnExport":
						AudioLibrary.RaiseOnExport(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<XBMCRPC.AudioLibrary.OnExport_data>(Serializer)
			);
						break;

					case "AudioLibrary.OnRemove":
						AudioLibrary.RaiseOnRemove(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<XBMCRPC.AudioLibrary.OnRemove_data>(Serializer)
			);
						break;

					case "AudioLibrary.OnScanFinished":
						AudioLibrary.RaiseOnScanFinished(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<object>(Serializer)
			);
						break;

					case "AudioLibrary.OnScanStarted":
						AudioLibrary.RaiseOnScanStarted(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<object>(Serializer)
			);
						break;

					case "AudioLibrary.OnUpdate":
						AudioLibrary.RaiseOnUpdate(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<XBMCRPC.AudioLibrary.OnUpdate_data>(Serializer)
			);
						break;

					case "GUI.OnDPMSActivated":
						GUI.RaiseOnDPMSActivated(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<object>(Serializer)
			);
						break;

					case "GUI.OnDPMSDeactivated":
						GUI.RaiseOnDPMSDeactivated(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<object>(Serializer)
			);
						break;

					case "GUI.OnScreensaverActivated":
						GUI.RaiseOnScreensaverActivated(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<object>(Serializer)
			);
						break;

					case "GUI.OnScreensaverDeactivated":
						GUI.RaiseOnScreensaverDeactivated(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<XBMCRPC.GUI.OnScreensaverDeactivated_data>(Serializer)
			);
						break;

					case "Input.OnInputFinished":
						Input.RaiseOnInputFinished(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<object>(Serializer)
			);
						break;

					case "Input.OnInputRequested":
						Input.RaiseOnInputRequested(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<XBMCRPC.Input.OnInputRequested_data>(Serializer)
			);
						break;

					case "Player.OnPause":
						Player.RaiseOnPause(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<XBMCRPC.Player.Notifications.Data>(Serializer)
			);
						break;

					case "Player.OnPlay":
						Player.RaiseOnPlay(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<XBMCRPC.Player.Notifications.Data>(Serializer)
			);
						break;

					case "Player.OnPropertyChanged":
						Player.RaiseOnPropertyChanged(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<XBMCRPC.Player.OnPropertyChanged_data>(Serializer)
			);
						break;

					case "Player.OnSeek":
						Player.RaiseOnSeek(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<XBMCRPC.Player.OnSeek_data>(Serializer)
			);
						break;

					case "Player.OnSpeedChanged":
						Player.RaiseOnSpeedChanged(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<XBMCRPC.Player.Notifications.Data>(Serializer)
			);
						break;

					case "Player.OnStop":
						Player.RaiseOnStop(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<XBMCRPC.Player.OnStop_data>(Serializer)
			);
						break;

					case "Playlist.OnAdd":
						Playlist.RaiseOnAdd(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<XBMCRPC.Playlist.OnAdd_data>(Serializer)
			);
						break;

					case "Playlist.OnClear":
						Playlist.RaiseOnClear(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<XBMCRPC.Playlist.OnClear_data>(Serializer)
			);
						break;

					case "Playlist.OnRemove":
						Playlist.RaiseOnRemove(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<XBMCRPC.Playlist.OnRemove_data>(Serializer)
			);
						break;

					case "System.OnLowBattery":
						System.RaiseOnLowBattery(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<object>(Serializer)
			);
						break;

					case "System.OnQuit":
						System.RaiseOnQuit(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<XBMCRPC.System.OnQuit_data>(Serializer)
			);
						break;

					case "System.OnRestart":
						System.RaiseOnRestart(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<object>(Serializer)
			);
						break;

					case "System.OnSleep":
						System.RaiseOnSleep(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<object>(Serializer)
			);
						break;

					case "System.OnWake":
						System.RaiseOnWake(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<object>(Serializer)
			);
						break;

					case "VideoLibrary.OnCleanFinished":
						VideoLibrary.RaiseOnCleanFinished(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<object>(Serializer)
			);
						break;

					case "VideoLibrary.OnCleanStarted":
						VideoLibrary.RaiseOnCleanStarted(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<object>(Serializer)
			);
						break;

					case "VideoLibrary.OnExport":
						VideoLibrary.RaiseOnExport(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<XBMCRPC.VideoLibrary.OnExport_data>(Serializer)
			);
						break;

					case "VideoLibrary.OnRemove":
						VideoLibrary.RaiseOnRemove(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<XBMCRPC.VideoLibrary.OnRemove_data>(Serializer)
			);
						break;

					case "VideoLibrary.OnScanFinished":
						VideoLibrary.RaiseOnScanFinished(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<object>(Serializer)
			);
						break;

					case "VideoLibrary.OnScanStarted":
						VideoLibrary.RaiseOnScanStarted(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<object>(Serializer)
			);
						break;

					case "VideoLibrary.OnUpdate":
						VideoLibrary.RaiseOnUpdate(
			jObject["params"]["sender"].ToObject<string>(Serializer)
			, jObject["params"]["data"].ToObject<XBMCRPC.VideoLibrary.OnUpdate_data>(Serializer)
			);
						break;
				}
			}
		}
	}
}