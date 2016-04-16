using CodecoreTechnologies.Elve.DriverFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using XBMCRPC;
using XBMCRPC.Global;
using XBMCRPC.Player;
using XBMCRPC.Player.Audio;

namespace NoesisLabs.Elve.Kodi.Models
{
	public class KodiPlayer
	{
		#region "fields"

		private string _album;
		private bool _areSubtitlesEnabled;
		private string _artist;
		private List<Stream> _audioStreams;
		private Client _client;
		private XBMCRPC.Player.Type _contentType;
		private Stream _currentAudioStream;
		private Subtitle _currentSubtitle;
		private string _fanArtPath;
		private string _filePath;
		private string _genre;
		private bool _isConnected;
		private bool _isLive;
		private bool _isMuted;
		private bool _isPartyMode;
		private bool _isShuffled;
		private ILogger _logger;
		private string _name;
		private double _percentage;
		private List<PlaylistItem> _playlist;
		private int _playlistId;
		private int _playlistPosition;
		private Repeat _repeat;
		private int _speed;
		private List<Subtitle> _subtitles;
		private Time _time;
		private string _title;
		private Time _totalTime;
		private TransportMode _transportMode;
		private string _version;
		private int _volume;

		#endregion "fields"

		public KodiPlayer(ILogger logger, string host, int port, string username = "", string password = "")
		{
			this._logger = logger;

			var connectionSettings = new ConnectionSettings(host, port, username, password);
			var platformServices = new PlatformServices();

			this.Name = String.Empty;

			this.SetDefaultState();

			this._client = new Client(connectionSettings, platformServices);

			this._client.Player.OnPropertyChanged += Player_OnPropertyChanged;
			this._client.Player.OnPause += Player_OnPause;
			this._client.Player.OnPlay += Player_OnPlay;
			this._client.Player.OnSeek += Player_OnSeek;
			this._client.Player.OnStop += Player_OnStop;
			this._client.Playlist.OnAdd += Playlist_OnChange;
			this._client.Playlist.OnClear += Playlist_OnChange;
			this._client.Playlist.OnRemove += Playlist_OnChange;
			this._client.Application.OnVolumeChanged += Application_OnVolumeChanged;
		}

		#region "ClientEventHandlers"

		private void Application_OnVolumeChanged(string sender = null, XBMCRPC.Application.OnVolumeChanged_data data = null)
		{
			this._logger.Info("Application_OnVolumeChanged");
			this.IsMuted = data.muted;
			this.Volume = data.volume;
			this.UpdateStatus();
		}

		private void Player_OnPause(string sender = null, XBMCRPC.Player.Notifications.Data data = null)
		{
			this._logger.Info("Player_OnPause");
			this.TransportMode = TransportMode.Pause;
			this.Speed = data.player.speed;
			this.UpdateStatus();
		}

		private void Player_OnPlay(string sender = null, XBMCRPC.Player.Notifications.Data data = null)
		{
			this._logger.Info("Player_OnPlay");
			this.TransportMode = TransportMode.Play;
			this.Speed = data.player.speed;
			this.UpdateStatus();
		}

		private void Player_OnPropertyChanged(string sender = null, OnPropertyChanged_data data = null)
		{
			this._logger.Info("Player_OnPropertyChanged");
			this.IsShuffled = data.property.shuffled;
			this.Repeat = data.property.repeat;
		}

		private void Player_OnSeek(string sender = null, OnSeek_data data = null)
		{
			this._logger.Info("Player_OnSeek");
			this.Time = data.player.time;
			this.Speed = data.player.speed;
			this.UpdateStatus();
		}

		private void Player_OnStop(string sender = null, OnStop_data data = null)
		{
			this._logger.Info("Player_OnStop");
			this.TransportMode = TransportMode.Stop;
			if (data.end) { this.OnPlaybackEnded(); }
			this.UpdateStatus();
		}

		private void Playlist_OnChange(string sender = null, object data = null)
		{
			this._logger.Info("Playlist_OnChange");
			var activePlayerId = this.GetActivePlayerId();
			if (activePlayerId.HasValue) { this.UpdatePlaylist(activePlayerId.Value); }
		}

		#endregion "ClientEventHandlers"

		#region "Events"

		public event EventHandler AlbumChanged;

		public event EventHandler AreSubtitlesEnabledChanged;

		public event EventHandler ArtistChanged;

		public event EventHandler AudioStreamsChanged;

		public event EventHandler ContentTypeChanged;

		public event EventHandler CurrentAudioStreamChanged;

		public event EventHandler CurrentSubtitleChanged;

		public event EventHandler FanArtPathChanged;

		public event EventHandler FilePathChanged;

		public event EventHandler GenreChanged;

		public event EventHandler IsConnectedChanged;

		public event EventHandler IsLiveChanged;

		public event EventHandler IsMutedChanged;

		public event EventHandler IsPartyModeChanged;

		public event EventHandler IsPlayingChanged;

		public event EventHandler IsShuffledChanged;

		public event EventHandler NameChanged;

		public event EventHandler PercentageChanged;

		public event EventHandler PlaybackEnded;

		public event EventHandler PlaylistChanged;

		public event EventHandler PlaylistIdChanged;

		public event EventHandler PlaylistPositionChanged;

		public event EventHandler RepeatChanged;

		public event EventHandler SpeedChanged;

		public event EventHandler SubtitlesChanged;

		public event EventHandler TimeChanged;

		public event EventHandler TitleChanged;

		public event EventHandler TotalTimeChanged;

		public event EventHandler TransportModeChanged;

		public event EventHandler VersionChanged;

		public event EventHandler VolumeChanged;

		#endregion "Events"

		#region "properties"

		public string Album
		{
			get { return this._album; }
			set
			{
				if (this._album != value)
				{
					this._album = value;
					this.OnAlbumChanged();
				}
			}
		}

		public bool AreSubtitlesEnabled
		{
			get { return this._areSubtitlesEnabled; }
			set
			{
				if (this._areSubtitlesEnabled != value)
				{
					this._areSubtitlesEnabled = value;
					this.OnAreSubtitlesEnabledChanged();
				}
			}
		}

		public string Artist
		{
			get { return this._artist; }
			set
			{
				if (this._artist != value)
				{
					this._artist = value;
					this.OnArtistChanged();
				}
			}
		}

		public List<Stream> AudioStreams
		{
			get { return this._audioStreams; }
			set
			{
				var comparer = new MultiSetComparer<Stream>();
				if (!comparer.Equals(this._audioStreams, value))
				{
					this._audioStreams = value;
					this.OnAudioStreamsChanged();
				}
			}
		}

		public XBMCRPC.Player.Type ContentType
		{
			get { return this._contentType; }
			set
			{
				if (this._contentType != value)
				{
					this._contentType = value;
					this.OnContentTypeChanged();
				}
			}
		}

		public Stream CurrentAudioStream
		{
			get { return this._currentAudioStream; }
			set
			{
				if (this._currentAudioStream != value)
				{
					this._currentAudioStream = value;
					this.OnCurrentAudioStreamChanged();
				}
			}
		}

		public Subtitle CurrentSubtitle
		{
			get { return this._currentSubtitle; }
			set
			{
				if (this._currentSubtitle != value)
				{
					this._currentSubtitle = value;
					this.OnCurrentSubtitleChanged();
				}
			}
		}

		public string FanArtPath
		{
			get { return this._fanArtPath; }
			set
			{
				if (this._fanArtPath != value)
				{
					this._fanArtPath = value;
					this.OnFanArtPathChanged();
				}
			}
		}

		public string FilePath
		{
			get { return this._filePath; }
			set
			{
				if (this._filePath != value)
				{
					this._filePath = value;
					this.OnFilePathChanged();
				}
			}
		}

		public string Genre
		{
			get { return this._genre; }
			set
			{
				if (this._genre != value)
				{
					this._genre = value;
					this.OnGenreChanged();
				}
			}
		}

		public bool IsConnected
		{
			get { return this._isConnected; }
			set
			{
				if (this._isConnected != value)
				{
					this._isConnected = value;
					this.OnIsConnectedChanged();
				}
			}
		}

		public bool IsLive
		{
			get { return this._isLive; }
			set
			{
				if (this._isLive != value)
				{
					this._isLive = value;
					this.OnIsLiveChanged();
				}
			}
		}

		public bool IsMuted
		{
			get { return this._isMuted; }
			set
			{
				if (this._isMuted != value)
				{
					this._isMuted = value;
					this.OnIsMutedChanged();
				}
			}
		}

		public bool IsPartyMode
		{
			get { return this._isPartyMode; }
			set
			{
				if (this._isPartyMode != value)
				{
					this._isPartyMode = value;
					this.OnIsPartyModeChanged();
				}
			}
		}

		public bool IsShuffled
		{
			get { return this._isShuffled; }
			set
			{
				if (this._isShuffled != value)
				{
					this._isShuffled = value;
					this.OnIsShuffledChanged();
				}
			}
		}

		public string Name
		{
			get { return this._name; }
			set
			{
				if (this._name != value)
				{
					this._name = value;
					this.OnNameChanged();
				}
			}
		}

		public double Percentage
		{
			get { return this._percentage; }
			set
			{
				if (this._percentage != value)
				{
					this._percentage = value;
					this.OnPercentageChanged();
				}
			}
		}

		public List<PlaylistItem> Playlist
		{
			get { return this._playlist; }
			set
			{
				var comparer = new MultiSetComparer<PlaylistItem>();

				if (!comparer.Equals(this._playlist, value))
				{
					this._playlist = value;
					this.OnPlaylistChanged();
				}
			}
		}

		public int PlaylistId
		{
			get { return this._playlistId; }
			set
			{
				if (this._playlistId != value)
				{
					this._playlistId = value;
					this.OnPlaylistIdChanged();
				}
			}
		}

		public int PlaylistPosition
		{
			get { return this._playlistPosition; }
			set
			{
				if (this._playlistPosition != value)
				{
					this._playlistPosition = value;
					this.OnPlaylistPositionChanged();
				}
			}
		}

		public Repeat Repeat
		{
			get { return this._repeat; }
			set
			{
				if (this._repeat != value)
				{
					this._repeat = value;
					this.OnRepeatChanged();
				}
			}
		}

		public int Speed
		{
			get { return this._speed; }
			set
			{
				if (this._speed != value)
				{
					this._speed = value;
					this.OnSpeedChanged();
				}
			}
		}

		public List<Subtitle> Subtitles
		{
			get { return this._subtitles; }
			set
			{
				var comparer = new MultiSetComparer<Subtitle>();
				if (!comparer.Equals(this._subtitles, value))
				{
					this._subtitles = value;
					this.OnSubtitlesChanged();
				}
			}
		}

		public Time Time
		{
			get { return this._time; }
			set
			{
				if (this._time != value)
				{
					this._time = value;
					this.OnTimeChanged();
				}
			}
		}

		public string Title
		{
			get { return this._title; }
			set
			{
				if (this._title != value)
				{
					this._title = value;
					this.OnTitleChanged();
				}
			}
		}

		public Time TotalTime
		{
			get { return this._totalTime; }
			set
			{
				if (this._totalTime != value)
				{
					this._totalTime = value;
					this.OnTotalTimeChanged();
				}
			}
		}

		public TransportMode TransportMode
		{
			get { return this._transportMode; }
			set
			{
				if (this._transportMode != value)
				{
					this._transportMode = value;
					this.OnTransportModeChanged();
				}
			}
		}

		public string Version
		{
			get { return this._version; }
			set
			{
				if (this._version != value)
				{
					this._version = value;
					this.OnVersionChanged();
				}
			}
		}

		public int Volume
		{
			get { return this._volume; }
			set
			{
				if (this._volume != value)
				{
					this._volume = value;
					this.OnVolumeChanged();
				}
			}
		}

		#endregion "properties"

		public void AddPlaylistItem(string item)
		{
			try
			{
				var activePlaylistId = this.GetActivePlaylistId();
				if (activePlaylistId.HasValue) { this._client.Playlist.Add(new XBMCRPC.Playlist.ItemFile() { file = item }, activePlaylistId.Value); }
			}
			catch (Exception ex)
			{
				this._logger.Error(String.Format("AddPlaylistItem failed for item [{0}].", item), ex);
			}
		}

		public void AddPlaylistItems(string[] items)
		{
			foreach (var item in items)
			{
				this.AddPlaylistItem(item);
			}
		}

		public void ClearPlaylist()
		{
			try
			{
				var activePlaylistId = this.GetActivePlaylistId();
				if (activePlaylistId.HasValue) { this._client.Playlist.Clear(activePlaylistId.Value); }
			}
			catch (Exception ex)
			{
				this._logger.Error("ClearPlaylist Failed.", ex);
			}
		}

		public void InsertPlaylistItem(string item)
		{
			try
			{
				var activePlaylistId = this.GetActivePlaylistId();
				if (activePlaylistId.HasValue) { this._client.Playlist.Insert(new XBMCRPC.Playlist.ItemFile() { file = item }, activePlaylistId.Value); }
			}
			catch (Exception ex)
			{
				this._logger.Error(String.Format("InsertPlaylistItem failed for item [{0}].", item), ex);
			}
		}

		public void InsertPlaylistItems(string[] items)
		{
			foreach (var item in items)
			{
				this.InsertPlaylistItem(item);
			}
		}

		public void Mute()
		{
			try
			{
				this._client.Application.SetMute(true);
			}
			catch (Exception ex)
			{
				this._logger.Error("Mute failed.", ex);
			}
		}

		public void OffsetVolume(int offset)
		{
			try
			{
				this._client.Application.SetVolume(this.Volume + offset);
			}
			catch (Exception ex)
			{
				this._logger.Error(String.Format("OffsetVolume failed for offset [{0}].", offset.ToString()), ex);
			}
		}

		public void Open(string item)
		{
			try
			{
				var activePlayerId = this.GetActivePlayerId();
				if (activePlayerId.HasValue) { this._client.Player.Open(new XBMCRPC.Playlist.ItemFile() { file = item }); }
			}
			catch (Exception ex)
			{
				this._logger.Error(String.Format("Open failed for item [{0}].", item), ex);
			}
		}

		public void Pause()
		{
			try
			{
				var activePlayerId = this.GetActivePlayerId();
				if (activePlayerId.HasValue) { this._client.Player.PlayPause(false, activePlayerId.Value); }
			}
			catch (Exception ex)
			{
				this._logger.Error("Pause failed.", ex);
			}
		}

		public void Play()
		{
			try
			{
				var activePlayerId = this.GetActivePlayerId();
				if (activePlayerId.HasValue) { this._client.Player.PlayPause(true, activePlayerId.Value); }
			}
			catch (Exception ex)
			{
				this._logger.Error("Play failed.", ex);
			}
		}

		public void PlayNext()
		{
			try
			{
				var activePlayerId = this.GetActivePlayerId();
				if (activePlayerId.HasValue) { this._client.Player.GoTo(GoTo_to1.next, activePlayerId.Value); }
			}
			catch (Exception ex)
			{
				this._logger.Error("PlayeNext failed.", ex);
			}
		}

		public void PlayPrevious()
		{
			try
			{
				var activePlayerId = this.GetActivePlayerId();
				if (!activePlayerId.HasValue) { this._client.Player.GoTo(GoTo_to1.previous, activePlayerId.Value); }
			}
			catch (Exception ex)
			{
				this._logger.Error("PlayPrevious failed.", ex);
			}
		}

		public void RemovePlaylistItem(int index)
		{
			try
			{
				var activePlaylistId = this.GetActivePlaylistId();
				if (activePlaylistId.HasValue) { this._client.Playlist.Remove(activePlaylistId.Value, index); }
			}
			catch (Exception ex)
			{
				this._logger.Error(String.Format("RemovePlaylistItem failed for index [{0}].", index.ToString()), ex);
			}
		}

		public void Seek(TimeSpan position)
		{
			try
			{
				var activePlayerId = this.GetActivePlayerId();
				if (activePlayerId.HasValue) { this._client.Player.Seek(new Seek_valueSeconds() { seconds = (int)position.TotalSeconds }, activePlayerId.Value); }
			}
			catch (Exception ex)
			{
				this._logger.Error(String.Format("Seek failed for position [{0}].", position.ToString()), ex);
			}
		}

		public void SetPlaylistPosition(int index)
		{
			try
			{
				var activePlaylistId = this.GetActivePlaylistId();
				if (activePlaylistId.HasValue) { this._client.Player.GoTo(index, activePlaylistId.Value); }
			}
			catch (Exception ex)
			{
				this._logger.Error(String.Format("SetPlaylistPosition failed for index [{0}].", index.ToString()), ex);
			}
		}

		public void SetRepeatMode(Repeat mode)
		{
			try
			{
				var activePlayerId = this.GetActivePlayerId();
				if (activePlayerId.HasValue) { this._client.Player.SetRepeat(mode, activePlayerId.Value); }
			}
			catch (Exception ex)
			{
				this._logger.Error(String.Format("SetRepeatMode failed for mode [{0}].", mode.ToString()), ex);
			}
		}

		public void SetShuffle(bool shouldShuffle)
		{
			try
			{
				var activePlayerId = this.GetActivePlayerId();
				if (activePlayerId.HasValue) { this._client.Player.SetShuffle(shouldShuffle, activePlayerId.Value); }
			}
			catch (Exception ex)
			{
				this._logger.Error(String.Format("SetShuffle failed for shouldShuffle [{0}].", shouldShuffle.ToString()), ex);
			}
		}

		public void SetVolume(int volume)
		{
			try
			{
				this._client.Application.SetVolume(volume);
			}
			catch (Exception ex)
			{
				this._logger.Error(String.Format("SetVolume failed for volue [{0}].", volume.ToString()), ex);
			}
		}

		public void StartNotificationListener()
		{
			try
			{
				bool wasConnected = this.IsConnected;
				this.IsConnected = this._client.IsNotificationListenerConnected();

				if (!wasConnected && !this.IsConnected)
				{
					this._logger.Info("Connecting to notification socket.");
					this._client.StartNotificationListener();
					return;
				}

				if (wasConnected && !this.IsConnected)
				{
					this._logger.Error("Notification socket connection lost. Attempting reconnect.");
					this._client.StartNotificationListener();
					return;
				}

				if (wasConnected && this.IsConnected)
				{
					this._logger.Info("Notification socket is connected.");
					return;
				}
			}
			catch (Exception ex)
			{
				this.IsConnected = false;
				this._logger.Error("StartNotificationListener failed.", ex);
			}
		}

		public void Stop()
		{
			try
			{
				var activePlayerId = this.GetActivePlayerId();
				if (activePlayerId.HasValue) { this._client.Player.Stop(activePlayerId.Value); }
			}
			catch (Exception ex)
			{
				this._logger.Error("Stop failed.", ex);
			}
		}

		public void SwapPlaylistItem(int fromIndex, int toIndex)
		{
			try
			{
				var activePlaylistId = this.GetActivePlaylistId();
				if (activePlaylistId.HasValue) { this._client.Playlist.Swap(activePlaylistId.Value, fromIndex, toIndex); }
			}
			catch (Exception ex)
			{
				this._logger.Error(String.Format("SwapPlaylistItem failed for fromIndex [{0}], toIndex [{1}].", fromIndex.ToString(), toIndex.ToString()), ex);
			}
		}

		public void TogglePause()
		{
			try
			{
				var activePlayerId = this.GetActivePlayerId();
				if (activePlayerId.HasValue) { this._client.Player.PlayPause(activePlayerId.Value); }
			}
			catch (Exception ex)
			{
				this._logger.Error("TogglePause failed.", ex);
			}
		}

		public void Unmute()
		{
			try
			{
				this._client.Application.SetMute(false);
			}
			catch (Exception ex)
			{
				this._logger.Error("Unmute failed.", ex);
			}
		}

		public void UpdateStatus()
		{
			int? activePlayerId = this.GetActivePlayerId();

			if (!activePlayerId.HasValue)
			{
				this.SetDefaultState();
			}
			else
			{
				this.UpdateApplicationStatus(activePlayerId.Value);
				this.UpdatePlayerProperties(activePlayerId.Value);
				this.UpdatePlayerItem(activePlayerId.Value);
				this.UpdatePlaylist(activePlayerId.Value);
			}
		}

		private int? GetActivePlayerId()
		{
			try
			{
				var activePlayers = this._client.Player.GetActivePlayers();

				var activePlayer = activePlayers.FirstOrDefault(ap => ap.type == XBMCRPC.Player.Type.audio || ap.type == XBMCRPC.Player.Type.video);

				return (activePlayer == null) ? (int?)null : activePlayer.playerid;
			}
			catch (Exception ex)
			{
				this._logger.Error("GetActivePlayerId failed.", ex);
				return (int?)null;
			}
		}

		private int? GetActivePlaylistId()
		{
			try
			{
				var activePlayerId = this.GetActivePlayerId();

				if (!activePlayerId.HasValue) { return (int?)null; }

				return this.GetActivePlaylistId(activePlayerId.Value);
			}
			catch (Exception ex)
			{
				this._logger.Error("GetActivePlaylistId failed.", ex);
				return (int?)null;
			}
		}

		private int? GetActivePlaylistId(int activePlayerId)
		{
			try
			{
				var properties = new GetProperties_properties();
				properties.Add(XBMCRPC.Player.Property.Name.playlistid);

				var val = this._client.Player.GetProperties(activePlayerId, properties);

				return val.playlistid;
			}
			catch (Exception ex)
			{
				this._logger.Error(String.Format("GetActivePlaylistId failed for activePlayerId [{0}].", activePlayerId.ToString()), ex);
				return (int?)null;
			}
		}

		private void SetDefaultApplicationStatusState()
		{
			this.IsMuted = false;
			this.Name = String.Empty;
			this.Version = String.Empty;
			this.Volume = 0;
		}

		private void SetDefaultItemState()
		{
			this.Album = String.Empty;
			this.Artist = String.Empty;
			this.FanArtPath = String.Empty;
			this.FilePath = String.Empty;
			this.Genre = String.Empty;
			this.Title = String.Empty;
		}

		private void SetDefaultPlayerPropertiesState()
		{
			this.AreSubtitlesEnabled = false;
			this.AudioStreams = new List<Stream>();
			this.ContentType = XBMCRPC.Player.Type.picture;
			this.CurrentAudioStream = null;
			this.CurrentSubtitle = null;
			this.IsLive = false;
			this.IsPartyMode = false;
			this.IsShuffled = false;
			this.Percentage = 0;
			this.PlaylistId = 0;
			this.PlaylistPosition = 0;
			this.Repeat = Repeat.off;
			this.Speed = 0;
			this.Subtitles = new List<Subtitle>();
			this.Time = new Time();
			this.TotalTime = new Time();
			this.TransportMode = TransportMode.Stop;
		}

		private void SetDefaultPlaylistState()
		{
			this.Playlist = new List<PlaylistItem>();
		}

		private void SetDefaultState()
		{
			this.SetDefaultApplicationStatusState();
			this.SetDefaultItemState();
			this.SetDefaultPlayerPropertiesState();
			this.SetDefaultPlaylistState();
		}

		private void UpdateApplicationStatus(int activePlayerId)
		{
			try
			{
				var properties = new XBMCRPC.Application.GetProperties_properties();
				properties.Add(XBMCRPC.Application.Property.Name.muted);
				properties.Add(XBMCRPC.Application.Property.Name.name);
				properties.Add(XBMCRPC.Application.Property.Name.version);
				properties.Add(XBMCRPC.Application.Property.Name.volume);

				var val = this._client.Application.GetProperties(properties);

				this.IsMuted = val.muted;
				this.Name = val.name;
				this.Version = String.Format("{0}.{1}.{2}", val.version.major, val.version.minor, val.version.revision);
				this.Volume = val.volume;
			}
			catch (Exception ex)
			{
				this._logger.Error(String.Format("UpdateApplicationStatus failed for activePlayerId [{0}].", activePlayerId.ToString()), ex);
				this.SetDefaultApplicationStatusState();
			}
		}

		private void UpdatePlayerItem(int activePlayerId)
		{
			try
			{
				var properties = new XBMCRPC.List.Fields.All();
				properties.Add(XBMCRPC.List.Fields.AllItem.album);
				properties.Add(XBMCRPC.List.Fields.AllItem.artist);
				properties.Add(XBMCRPC.List.Fields.AllItem.fanart);
				properties.Add(XBMCRPC.List.Fields.AllItem.file);
				properties.Add(XBMCRPC.List.Fields.AllItem.genre);
				properties.Add(XBMCRPC.List.Fields.AllItem.title);

				var val = this._client.Player.GetItem(activePlayerId, properties);
				var item = val.item;

				if (item == null)
				{
					this.SetDefaultItemState();
				}
				else
				{
					this.Album = item.album;

					var audioItem = item.AsAudioDetailsMedia;
					var videoItem = item.AsVideoDetailsFile;

					this.Artist = String.Join(", ", audioItem.artist.ToArray());
					this.FanArtPath = videoItem.fanart ?? audioItem.fanart;
					this.FilePath = videoItem.file;
					this.Genre = audioItem.genre.FirstOrDefault();
					this.Title = videoItem.title ?? audioItem.title;
				}
			}
			catch (Exception ex)
			{
				this._logger.Error(String.Format("UpdatePlayerItem failed for activePlayerId [{0}].", activePlayerId.ToString()), ex);
				this.SetDefaultItemState();
			}
		}

		private void UpdatePlayerProperties(int activePlayerId)
		{
			try
			{
				var properties = new GetProperties_properties();
				properties.Add(XBMCRPC.Player.Property.Name.subtitleenabled);
				properties.Add(XBMCRPC.Player.Property.Name.audiostreams);
				properties.Add(XBMCRPC.Player.Property.Name.type);
				properties.Add(XBMCRPC.Player.Property.Name.currentaudiostream);
				properties.Add(XBMCRPC.Player.Property.Name.currentsubtitle);
				properties.Add(XBMCRPC.Player.Property.Name.live);
				properties.Add(XBMCRPC.Player.Property.Name.partymode);
				properties.Add(XBMCRPC.Player.Property.Name.shuffled);
				properties.Add(XBMCRPC.Player.Property.Name.percentage);
				properties.Add(XBMCRPC.Player.Property.Name.playlistid);
				properties.Add(XBMCRPC.Player.Property.Name.position);
				properties.Add(XBMCRPC.Player.Property.Name.repeat);
				properties.Add(XBMCRPC.Player.Property.Name.speed);
				properties.Add(XBMCRPC.Player.Property.Name.subtitles);
				properties.Add(XBMCRPC.Player.Property.Name.time);
				properties.Add(XBMCRPC.Player.Property.Name.totaltime);

				var val = this._client.Player.GetProperties(activePlayerId, properties);

				this.AreSubtitlesEnabled = val.subtitleenabled;
				this.AudioStreams = val.audiostreams;
				this.ContentType = val.type;
				this.CurrentAudioStream = val.currentaudiostream;
				this.CurrentSubtitle = val.currentsubtitle;
				this.IsLive = val.live;
				this.IsPartyMode = val.partymode;
				this.IsShuffled = val.shuffled;
				this.Percentage = val.percentage;
				this.PlaylistId = val.playlistid;
				this.PlaylistPosition = val.position;
				this.Repeat = val.repeat;
				this.Speed = val.speed;
				this.Subtitles = val.subtitles;
				this.Time = val.time;
				this.TotalTime = val.totaltime;
				this.TransportMode = (val.speed == 0) ? TransportMode.Pause : TransportMode.Play;
			}
			catch (Exception ex)
			{
				this._logger.Error(String.Format("UpdatePlayerProperties failed for activePlayerId [{0}].", activePlayerId.ToString()), ex);
				this.SetDefaultPlayerPropertiesState();
			}
		}

		private void UpdatePlaylist(int activePlayerId)
		{
			try
			{
				int? activePlaylistId = this.GetActivePlaylistId(activePlayerId);

				if (!activePlaylistId.HasValue)
				{
					this.SetDefaultPlaylistState();
					return;
				}

				var properties = new XBMCRPC.List.Fields.All();
				properties.Add(XBMCRPC.List.Fields.AllItem.duration);
				properties.Add(XBMCRPC.List.Fields.AllItem.title);

				var val = this._client.Playlist.GetItems(activePlayerId, properties);
				var items = val.items;

				if (items == null)
				{
					this.SetDefaultPlaylistState();
				}
				else
				{
					this.Playlist = items.Select(item =>
					{
						var audioItem = item.AsAudioDetailsMedia;
						var videoItem = item.AsVideoDetailsFile;

						return new PlaylistItem() { Duration = TimeSpan.FromMinutes(item.duration), Id = item.id, Title = videoItem.title ?? audioItem.title };
					}).ToList();
				}
			}
			catch (Exception ex)
			{
				this._logger.Error(String.Format("UpdatePlaylist failed for activePlayerId [{0}].", activePlayerId.ToString()), ex);
				this.SetDefaultPlaylistState();
			}
		}

		#region "KodiPlayerEventHandlers"

		private void OnAlbumChanged()
		{
			if (this.AlbumChanged != null)
			{
				this.AlbumChanged(this, new EventArgs());
			}
		}

		private void OnAreSubtitlesEnabledChanged()
		{
			if (this.AreSubtitlesEnabledChanged != null)
			{
				this.AreSubtitlesEnabledChanged(this, new EventArgs());
			}
		}

		private void OnArtistChanged()
		{
			if (this.ArtistChanged != null)
			{
				this.ArtistChanged(this, new EventArgs());
			}
		}

		private void OnAudioStreamsChanged()
		{
			if (this.AudioStreamsChanged != null)
			{
				this.AudioStreamsChanged(this, new EventArgs());
			}
		}

		private void OnContentTypeChanged()
		{
			if (this.ContentTypeChanged != null)
			{
				this.ContentTypeChanged(this, new EventArgs());
			}
		}

		private void OnCurrentAudioStreamChanged()
		{
			if (this.CurrentAudioStreamChanged != null)
			{
				this.CurrentAudioStreamChanged(this, new EventArgs());
			}
		}

		private void OnCurrentSubtitleChanged()
		{
			if (this.CurrentSubtitleChanged != null)
			{
				this.CurrentSubtitleChanged(this, new EventArgs());
			}
		}

		private void OnFanArtPathChanged()
		{
			if (this.FanArtPathChanged != null)
			{
				this.FanArtPathChanged(this, new EventArgs());
			}
		}

		private void OnFilePathChanged()
		{
			if (this.FilePathChanged != null)
			{
				this.FilePathChanged(this, new EventArgs());
			}
		}

		private void OnGenreChanged()
		{
			if (this.GenreChanged != null)
			{
				this.GenreChanged(this, new EventArgs());
			}
		}

		private void OnIsConnectedChanged()
		{
			if (this.IsConnectedChanged != null)
			{
				this.IsConnectedChanged(this, new EventArgs());
			}
		}

		private void OnIsLiveChanged()
		{
			if (this.IsLiveChanged != null)
			{
				this.IsLiveChanged(this, new EventArgs());
			}
		}

		private void OnIsMutedChanged()
		{
			if (this.IsMutedChanged != null)
			{
				this.IsMutedChanged(this, new EventArgs());
			}
		}

		private void OnIsPartyModeChanged()
		{
			if (this.IsPartyModeChanged != null)
			{
				this.IsPartyModeChanged(this, new EventArgs());
			}
		}

		private void OnIsPlayingChanged()
		{
			if (this.IsPlayingChanged != null)
			{
				this.IsPlayingChanged(this, new EventArgs());
			}
		}

		private void OnIsShuffledChanged()
		{
			if (this.IsShuffledChanged != null)
			{
				this.IsShuffledChanged(this, new EventArgs());
			}
		}

		private void OnNameChanged()
		{
			if (this.NameChanged != null)
			{
				this.NameChanged(this, new EventArgs());
			}
		}

		private void OnPercentageChanged()
		{
			if (this.PercentageChanged != null)
			{
				this.PercentageChanged(this, new EventArgs());
			}
		}

		private void OnPlaybackEnded()
		{
			if (this.PlaybackEnded != null)
			{
				this.PlaybackEnded(this, new EventArgs());
			}
		}

		private void OnPlaylistChanged()
		{
			if (this.PlaylistChanged != null)
			{
				this.PlaylistChanged(this, new EventArgs());
			}
		}

		private void OnPlaylistIdChanged()
		{
			if (this.PlaylistIdChanged != null)
			{
				this.PlaylistIdChanged(this, new EventArgs());
			}
		}

		private void OnPlaylistPositionChanged()
		{
			if (this.PlaylistPositionChanged != null)
			{
				this.PlaylistPositionChanged(this, new EventArgs());
			}
		}

		private void OnRepeatChanged()
		{
			if (this.RepeatChanged != null)
			{
				this.RepeatChanged(this, new EventArgs());
			}
		}

		private void OnSpeedChanged()
		{
			if (this.SpeedChanged != null)
			{
				this.SpeedChanged(this, new EventArgs());
			}
		}

		private void OnSubtitlesChanged()
		{
			if (this.SubtitlesChanged != null)
			{
				this.SubtitlesChanged(this, new EventArgs());
			}
		}

		private void OnTimeChanged()
		{
			if (this.TimeChanged != null)
			{
				this.TimeChanged(this, new EventArgs());
			}
		}

		private void OnTitleChanged()
		{
			if (this.TitleChanged != null)
			{
				this.TitleChanged(this, new EventArgs());
			}
		}

		private void OnTotalTimeChanged()
		{
			if (this.TotalTimeChanged != null)
			{
				this.TotalTimeChanged(this, new EventArgs());
			}
		}

		private void OnTransportModeChanged()
		{
			if (this.TransportModeChanged != null)
			{
				this.TransportModeChanged(this, new EventArgs());
			}
		}

		private void OnVersionChanged()
		{
			if (this.VersionChanged != null)
			{
				this.VersionChanged(this, new EventArgs());
			}
		}

		private void OnVolumeChanged()
		{
			if (this.VolumeChanged != null)
			{
				this.VolumeChanged(this, new EventArgs());
			}
		}

		#endregion "KodiPlayerEventHandlers"
	}
}