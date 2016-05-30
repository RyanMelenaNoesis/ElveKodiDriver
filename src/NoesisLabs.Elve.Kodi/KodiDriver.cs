using CodecoreTechnologies.Elve.DriverFramework;
using CodecoreTechnologies.Elve.DriverFramework.DriverInterfaces;
using CodecoreTechnologies.Elve.DriverFramework.Scripting;
using NoesisLabs.Elve.Kodi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace NoesisLabs.Elve.Kodi
{
	[Driver("Kodi Driver", "A driver for monitoring and controlling Kodi media player.", "Ryan Melena", "Media Player", "", "Kodi", DriverCommunicationPort.Network, DriverMultipleInstances.MultiplePerDriverService, 0, 5, DriverReleaseStages.Production, "Kodi", "https://kodi.tv/", null)]
	public class KodiDriver : Driver, IMediaPlayerDriver
	{
		private const int BASE_INDEX = 1;

		#region Fields

		public KodiPlayer player;
		private Timer refreshTimer;

		#endregion Fields

		#region DriverSettings

		[DriverSetting("Player Hostname/Ip", "Hostname/IP.", null, true)]
		public string HostnameSetting { get; set; }

		[DriverSetting("Password", "Password.", null, false)]
		public string PasswordSetting { get; set; }

		[DriverSetting("Port", "Port.", 1D, double.MaxValue, "80", true)]
		public int PortSetting { get; set; }

		[DriverSetting("Refresh Interval", "Interval in seconds between status update requests.  Some values update asynchronously when changed via Kodi and all when changed via Elve.", 1D, double.MaxValue, "60", true)]
		public int RefreshIntervalSetting { get; set; }

		[DriverSetting("Username", "Username.", null, false)]
		public string UsernameSetting { get; set; }

		#endregion DriverSettings

		[ScriptObjectProperty("Connected", "Gets a value indicating if a connection is established with the Kodi player.", "a value indicating if the {NAME} is connected")]
		public ScriptBoolean Connected
		{
			get { return new ScriptBoolean(this.player.IsConnected); }
		}

		public string[] SupportedPlaylistFileTypes
		{
			get { return new string[0]; }
		}

		public bool SupportsPlaylistIDFromCompanionMediaLibrary
		{
			get { return false; }
		}

		[ScriptObjectProperty("Zone Count", "Gets the number of zones supported by this media player driver.", "the total number of {NAME} zones")]
		public ScriptNumber ZoneCount
		{
			get { return new ScriptNumber(1); }
		}

		[SupportsDriverPropertyBinding("Current Item Album Changed", "Occurs when the current item's album changes.")]
		[ScriptObjectProperty("Current Item Album", "Gets the current item's album property.", "the current {NAME} item's album")]
		public IScriptArray ZoneCurrentTrackAlbums
		{
			get { return new ScriptArrayMarshalByValue(new string[1] { this.player.Album }, BASE_INDEX); }
		}

		[SupportsDriverPropertyBinding("Current Item Artist Changed", "Occurs when the current item's artist changes.")]
		[ScriptObjectProperty("Current Item Artist", "Gets the current item's artist property.", "the current {NAME} item's artist")]
		public IScriptArray ZoneCurrentTrackArtists
		{
			get { return new ScriptArrayMarshalByValue(new string[1] { this.player.Artist }, BASE_INDEX); }
		}

		[SupportsDriverPropertyBinding("Current Item Cover Art Changed", "Occurs when the current item's cover art changes.")]
		[ScriptObjectProperty("Current Item Cover Art", "Gets the current item's cover art.", "the current {NAME} item's cover art")]
		public IScriptArray ZoneCurrentTrackCoverArts
		{
			get { return new ScriptArrayMarshalByValue(new string[1] { this.player.FanArtPath }, BASE_INDEX); }
		}

		[SupportsDriverPropertyBinding("Current Item Duration", "Occurs when the current item's duration changes.")]
		[ScriptObjectProperty("Current Item Duration", "Gets the current item's duration.", "the current {NAME} item's duration")]
		public IScriptArray ZoneCurrentTrackDurations
		{
			get
			{
				var totalTime = this.player.TotalTime;
				return new ScriptArrayMarshalByValue(new TimeSpan[1] { new TimeSpan(totalTime.hours, totalTime.minutes, totalTime.seconds) }, BASE_INDEX);
			}
		}

		[SupportsDriverPropertyBinding("Current Item Genre Changed", "Occurs when the current item's genre changes.")]
		[ScriptObjectProperty("Current Item Genre", "Gets the current item's genre property.", "the current {NAME} item's genre")]
		public IScriptArray ZoneCurrentTrackGenres
		{
			get { return new ScriptArrayMarshalByValue(new string[1] { this.player.Genre }, BASE_INDEX); }
		}

		[SupportsDriverPropertyBinding("Current Item Path Changed", "Occurs when the current item's path changes.")]
		[ScriptObjectProperty("Current Item Path", "Gets the current item's file path.", "the current {NAME} item's file path")]
		public IScriptArray ZoneCurrentTrackPaths
		{
			get { return new ScriptArrayMarshalByValue(new string[1] { this.player.FilePath }, BASE_INDEX); }
		}

		[SupportsDriverPropertyBinding("Current Item Position Changed", "Occurs when the current item's position changes.")]
		[ScriptObjectProperty("Current Item Position", "Gets or sets the current item's playback position.", "the playback position of the currently playing item on {NAME} player", "Set the playback position of the currently playing item on {NAME} player to #{VALUE|00:00:00}.")]
		public IScriptArray ZoneCurrentTrackPositions
		{
			get
			{
				var time = this.player.Time;
				return new ScriptArrayMarshalByValue(new TimeSpan[1] { new TimeSpan(time.hours, time.minutes, time.seconds) }, BASE_INDEX);
			}

			set { this.SetZoneCurrentTrackPosition(new ScriptNumber(0), (ScriptTimeSpan)value[0]); }
		}

		[SupportsDriverPropertyBinding("Current Item Title Changed", "Occurs when the current item's title changes.")]
		[ScriptObjectProperty("Current Item Title", "Gets the current item's title property .", "the current {NAME} item's title")]
		public IScriptArray ZoneCurrentTrackTitles
		{
			get { return new ScriptArrayMarshalByValue(new string[1] { this.player.Title }, BASE_INDEX); }
		}

		[SupportsDriverPropertyBinding("Name Changed", "Occurs when a player name changes.")]
		[ScriptObjectProperty("Name", "Gets the name of the player.", "The {NAME} player name.")]
		public IScriptArray ZoneNames
		{
			get { return new ScriptArrayMarshalByValue(new string[1] { this.player.Name }, BASE_INDEX); }
		}

		[SupportsDriverPropertyBinding("Playlist Length Changed", "Occurs when the total number of items in a playlist changes.")]
		[ScriptObjectProperty("Playlist Length", "Gets the total number of items in the playlist.", "the total number of items in the {NAME} player playlist")]
		public IScriptArray ZonePlaylistLengths
		{
			get { return new ScriptArrayMarshalByValue(new int[1] { (this.player.Playlist == null) ? 0 : this.player.Playlist.Count }, BASE_INDEX); }
		}

		[SupportsDriverPropertyBinding("Playlist Position Changed", "Occurs when the index of the currently playing item changes on a zone.")]
		[ScriptObjectProperty("Playlist Position", "Gets the playlist item position that is currently playing by index.", "the index in the {NAME} player playlist of the current item", "Play item #{VALUE|0} in the {NAME} player playlist.")]
		public IScriptArray ZonePlaylistPositions
		{
			get { return new ScriptArrayMarshalByValue(new int[1] { this.player.PlaylistPosition }, BASE_INDEX); }
		}

		[SupportsDriverPropertyBinding("Repeat State Changed", "Occurs when the repeat state of the player changes.")]
		[ScriptObjectProperty("Repeat State", "Gets or sets the repeat state which indicates if the player will stop playing at the end of the playlist, repeat the current item indefinitely, or repeat the playlist indefinitely. A value of 'Off' indicates that the player will stop at the end of the playlist, 'One' indicates that the player will repeat the current item indefinitely and a value of 'All' indicates that the player will repeat the entire playlist indefinitely.", new string[] { "off", "all", "one" }, "the {NAME} repeat state", "Set the {NAME} repeat state to {VALUE|Off}.")]
		public IScriptArray ZoneRepeatModes
		{
			get { return new ScriptArrayMarshalByValue(new string[1] { Enum.GetName(typeof(XBMCRPC.Player.Repeat), this.player.Repeat).ToLower() }, BASE_INDEX); }
			set { this.SetZoneRepeatMode(new ScriptNumber(0), (ScriptNumber)value[0]); }
		}

		[ScriptObjectProperty("Shuffle State", "Gets or set the shuffle state of the playlist.", "the shuffle state of the {NAME} playlist", "Set the {NAME} playlist shuffle state to {VALUE|true|on|off}.", typeof(ScriptBoolean), 1, 1, null)]
		[SupportsDriverPropertyBinding("Shuffle State Changed", "Occurs when the shuffle state of the player changes.")]
		public IScriptArray ZoneShuffleStates
		{
			get { return new ScriptArrayMarshalByValue(new bool[1] { this.player.IsShuffled }, BASE_INDEX); }
			set { this.SetZoneShuffleState(new ScriptNumber(0), (ScriptBoolean)value[0]); }
		}

		[ScriptObjectProperty("Transport Mode", "Gets or sets the transport mode as 'play', 'stop' or 'pause'.", new string[] { "play", "stop", "pause" }, "the {NAME} transport mode", "Set the {NAME} transport mode to {VALUE|play}.")]
		[SupportsDriverPropertyBinding("Mode Changed", "Occurs when the player's mode changes.")]
		public IScriptArray ZoneTransportModes
		{
			get { return new ScriptArrayMarshalByValue(new string[1] { this.player.TransportMode.ToString().ToLower() }, BASE_INDEX); }
			set { this.SetZoneTransportMode(new ScriptNumber(0), (ScriptString)value[0]); }
		}

		[ScriptObjectProperty("Volume", "Gets or sets the current volume setting for the player. The scale is 0 to 100.", 0.0, 100.0, "the {NAME} volume", "Set {NAME}  volume to {value|50}.")]
		[SupportsDriverPropertyBinding("Volume Changed", "Occurs when the current volume setting for the player changes.")]
		public IScriptArray ZoneVolumes
		{
			get { return new ScriptArrayMarshalByValue(new int[1] { this.player.Volume }, BASE_INDEX); }
			set { this.SetZoneVolume(new ScriptNumber(0), (ScriptNumber)value[0]); }
		}

		[ScriptObjectMethodParameter("ZoneNumber", "The zone number. (Always 1)", 1.0, 1.0)]
		[ScriptObjectMethodParameter("Item", "An item file path.")]
		[ScriptObjectMethod("Append Playlist", "Adds the specified item file path to the end of the playlist.", "Add {PARAM|1|item file path} to the end of the {NAME} player playlist. Zone {PARAM|0|1}.")]
		public void AppendZonePlaylistItem(ScriptNumber zoneNumber, ScriptString item)
		{
			this.player.AddPlaylistItem(item.ToPrimitiveString());
		}

		public void AppendZonePlaylistItems(int zoneNumber, string[] locations)
		{
			this.player.AddPlaylistItems(locations);
		}

		[ScriptObjectMethodParameter("ZoneNumber", "The zone number. (Always 1)", 1.0, 1.0)]
		[ScriptObjectMethod("Clear Playlist", "Clears the playlist and stops the player.", "Clears the {NAME} player playlist and stop the player. Zone {PARAM|0|1}.")]
		public void ClearZonePlaylist(ScriptNumber zoneNumber)
		{
			this.player.ClearPlaylist();
		}

		public byte[] GetZoneCurrentTrackCoverArt(int zoneNumber)
		{
			throw new NotImplementedException();
		}

		[ScriptObjectMethodParameter("ZoneNumber", "The zone number. (Always 1)", 1.0, 1.0)]
		[ScriptObjectMethod("Get Current Item Duration", "Gets the duration of the currently playing item.", "Get the duration of the current item playing on the {NAME} player. Zone {PARAM|0|1}.")]
		public TimeSpan GetZoneCurrentTrackDuration(ScriptNumber zoneNumber)
		{
			return this.GetZoneCurrentTrackDuration(zoneNumber.ToPrimitiveInt32());
		}

		public TimeSpan GetZoneCurrentTrackDuration(int zoneNumber)
		{
			return (this.player.TotalTime == null) ? TimeSpan.MinValue : new TimeSpan(this.player.TotalTime.hours, this.player.TotalTime.minutes, this.player.TotalTime.seconds);
		}

		[ScriptObjectMethodParameter("ZoneNumber", "The zone number. (Always 1)", 1.0, 1.0)]
		[ScriptObjectMethod("Get Current Item Position", "Gets the playback position of the currently playing item.", "Get the playback position of the current item playing on the {NAME} player. Zone {PARAM|0|1}.")]
		public TimeSpan GetZoneCurrentTrackPosition(ScriptNumber zoneNumber)
		{
			return this.GetZoneCurrentTrackPosition(zoneNumber.ToPrimitiveInt32());
		}

		public TimeSpan GetZoneCurrentTrackPosition(int zoneNumber)
		{
			return (this.player.Time == null) ? TimeSpan.MinValue : new TimeSpan(this.player.Time.hours, this.player.Time.minutes, this.player.Time.seconds);
		}

		[ScriptObjectMethodParameter("ZoneNumber", "The zone number. (Always 1)", 1.0, 1.0)]
		[ScriptObjectMethodParameter("Page Index", "The page index.", 1.0, 1.0)]
		[ScriptObjectMethodParameter("Page Size", "The page size.", 1.0, 1.0)]
		[ScriptObjectMethod("Get Playlist", "Gets the playlist.", "Get the playlist on the {NAME} player with page size {PARAM|2|100} and page index {PARAM|1|0}. Zone {PARAM|0|1}.")]
		public MediaPlayerPlaylistItem[] GetZonePlaylist(ScriptNumber zoneNumber, ScriptNumber pageIndex, ScriptNumber pageSize)
		{
			return this.GetZonePlaylist(zoneNumber.ToPrimitiveInt32(), pageIndex.ToPrimitiveInt32(), pageSize.ToPrimitiveInt32());
		}

		public MediaPlayerPlaylistItem[] GetZonePlaylist(int zoneNumber, int pageIndex, int pageSize)
		{
			return this.player.Playlist.Select((pi, index) => new MediaPlayerPlaylistItem(index, pi.Title, String.Empty, pi.Duration)).Skip(pageIndex * pageSize).Take(pageSize).ToArray();
		}

		[ScriptObjectMethodParameter("ZoneNumber", "The zone number. (Always 1)", 1.0, 1.0)]
		[ScriptObjectMethodParameter("Item", "An item file path.")]
		[ScriptObjectMethod("Insert Playlist Item", "Inserts the specified item file path to be played immediately after the current item in the playlist.", "Insert {PARAM|1|item file path} to the the {NAME} player playlist after the current item. Zone {PARAM|0|1}.")]
		public void InsertZonePlaylistItem(ScriptNumber zoneNumber, ScriptString item)
		{
			this.player.InsertPlaylistItem(item.ToPrimitiveString());
		}

		public void InsertZonePlaylistItems(int zoneNumber, string[] locations)
		{
			this.player.InsertPlaylistItems(locations);
		}

		[ScriptObjectMethodParameter("ZoneNumber", "The zone number. (Always 1)", 1.0, 1.0)]
		[ScriptObjectMethodParameter("ToIndex", "The destination index.", 1.0, 2147483647.0)]
		[ScriptObjectMethodParameter("FromIndex", "The index of the item to move.", 1.0, 2147483647.0)]
		[ScriptObjectMethod("Move Playlist Item", "Moves the item at the specified index to a new index in the playlist.", "Move the item at index {PARAM|1|0} to index {PARAM|2|1} in the current {NAME} player playlist. Zone {PARAM|0|1}.")]
		public void MoveZonePlaylistItem(ScriptNumber zoneNumber, ScriptNumber fromIndex, ScriptNumber toIndex)
		{
			this.player.SwapPlaylistItem(fromIndex.ToPrimitiveInt32(), toIndex.ToPrimitiveInt32());
		}

		[ScriptObjectMethodParameter("ZoneNumber", "The zone number. (Always 1)", 1.0, 1.0)]
		[ScriptObjectMethod("Mute", "Turn the mute on.", "Mute {NAME} player. Zone {PARAM|0|1}.")]
		public void MuteZone(ScriptNumber zoneNumber)
		{
			this.player.Mute();
		}

		[ScriptObjectMethodParameter("ZoneNumber", "The zone number. (Always 1)", 1.0, 1.0)]
		[ScriptObjectMethodParameter("Offset", "The amount to offset the volume.", -100.0, 100.0)]
		[ScriptObjectMethod("Offset Volume", "Offsets the current volume setting for the player. Positive numbers increase the volume by the specified number and negative numbers decrease the volume. The full volume scale is 0 to 100.", "Offset {NAME} player volume by {PARAM|1|10}. Zone {PARAM|0|1}.")]
		public void OffsetZoneVolume(ScriptNumber zoneNumber, ScriptNumber offset)
		{
			this.player.OffsetVolume(offset.ToPrimitiveInt32());
		}

		[ScriptObjectMethodParameter("ZoneNumber", "The zone number. (Always 1)", 1.0, 1.0)]
		[ScriptObjectMethod("Pause", "Pauses the playing of the playlist.", "Pause {NAME} player. Zone {PARAM|0|1}.")]
		public void PauseZone(ScriptNumber zoneNumber)
		{
			this.player.Pause();
		}

		[ScriptObjectMethodParameter("ZoneNumber", "The zone number. (Always 1)", 1.0, 1.0)]
		[ScriptObjectMethod("Play Next Item", "Plays the next item in the playlist.", "Play the next item on the {NAME} player. Zone {PARAM|0|1}.")]
		public void PlayNextZoneTrack(ScriptNumber zoneNumber)
		{
			this.player.PlayNext();
		}

		[ScriptObjectMethodParameter("ZoneNumber", "The zone number. (Always 1)", 1.0, 1.0)]
		[ScriptObjectMethod("Play Previous Item", "Plays the previous item in the playlist.", "Play the previous item on the {NAME} player. Zone {PARAM|0|1}.")]
		public void PlayPreviousZoneTrack(ScriptNumber zoneNumber)
		{
			this.player.PlayPrevious();
		}

		[ScriptObjectMethodParameter("ZoneNumber", "The zone number. (Always 1)", 1.0, 1.0)]
		[ScriptObjectMethod("Play", "Starts playing the playlist.", "Play the {NAME} player. Zone {PARAM|0|1}.")]
		public void PlayZone(ScriptNumber zoneNumber)
		{
			this.player.Play();
		}

		[ScriptObjectMethodParameter("ZoneNumber", "The zone number. (Always 1)", 1.0, 1.0)]
		[ScriptObjectMethodParameter("Item", "An item or playlist file path.")]
		[ScriptObjectMethod("Play Item", "Adds the specified item or playlist file path into the playlist and plays starting at the first item. Any items previously in the playlist are discarded.", "Play {PARAM|1|item or playlist file path} on {NAME} player. Zone {PARAM|0|1}.")]
		public void PlayZoneItem(ScriptNumber zoneNumber, ScriptString item)
		{
			this.player.Open(item.ToPrimitiveString());
		}

		public void PlayZoneItems(int zoneNumber, string[] locations)
		{
			throw new NotImplementedException();
		}

		[ScriptObjectMethodParameter("ZoneNumber", "The zone number. (Always 1)", 1.0, 1.0)]
		[ScriptObjectMethodParameter("Locations", "An IScriptArray of objects representing item file paths.")]
		[ScriptObjectMethod("Play Items", "Adds the specified item file paths into the playlist and plays starting at the first item. Any items previously in the playlist are discarded.", "Play {PARAM|1|item file paths} on {NAME} player. Zone {PARAM|0|1}.")]
		public void PlayZoneItems(ScriptNumber zoneNumber, IScriptArray locations)
		{
			throw new NotImplementedException();
		}

		[ScriptObjectMethodParameter("ZoneNumber", "The zone number. (Always 1)", 1.0, 1.0)]
		[ScriptObjectMethodParameter("Index", "The index of the item to remove. The first item is at index 1.", 1.0, 2147483647.0)]
		[ScriptObjectMethod("Remove Playlist Item", "Removes the item at the specified index from the playlist.", "Remove the item at the index {PARAM|1|index} from the {NAME} player playlist. Zone {PARAM|0|1}.")]
		public void RemoveZonePlaylistItem(ScriptNumber zoneNumber, ScriptNumber index)
		{
			this.player.RemovePlaylistItem(index.ToPrimitiveInt32());
		}

		public void SetZoneCurrentTrackPosition(int zoneNumber, TimeSpan position)
		{
			this.player.Seek(position);
		}

		[ScriptObjectMethodParameter("ZoneNumber", "The zone number. (Always 1)", 1.0, 1.0)]
		[ScriptObjectMethodParameter("Position", "The item playback position.")]
		[ScriptObjectMethod("Set Track Position", "Sets the item playback position.", "Set the {NAME} player item playback position to {PARAM|1|00:00:00}. Zone {PARAM|0|1}.")]
		public void SetZoneCurrentTrackPosition(ScriptNumber zoneNumber, ScriptTimeSpan position)
		{
			this.player.Seek(new TimeSpan(position.Hours.ToPrimitiveInt32(), position.Minutes.ToPrimitiveInt32(), position.Seconds.ToPrimitiveInt32()));
		}

		[ScriptObjectMethodParameter("ZoneID", "The zone number. (Always 1)", 1.0, 1.0)]
		[ScriptObjectMethodParameter("Index", "The index of the playlist item to set as the current item.", 1.0, 99999)]
		[ScriptObjectMethod("Set Playlist Position", "Sets the currently playing item.", "Set the currently playing playlist item to index {PARAM|1|1} in {NAME} player. Zone {PARAM|0|1}.")]
		public void SetZonePlaylistPosition(ScriptNumber zoneID, ScriptNumber index)
		{
			this.player.SetPlaylistPosition(index.ToPrimitiveInt32());
		}

		[ScriptObjectMethodParameter("ZoneNumber", "The zone number. (Always 1)", 1.0, 1.0)]
		[ScriptObjectMethodParameter("RepeatMode", "Indicates the repeat mode.", new double[] { 0.0, 1.0, 2.0 }, new string[] { "off", "all", "one" })]
		[ScriptObjectMethod("Set Repeat Mode", "Set the repeat mode for the playlist.", "Set the {NAME} repeat mode for player to {PARAM|1|0}. Zone {PARAM|0|1}.")]
		public void SetZoneRepeatMode(ScriptNumber zoneNumber, ScriptNumber repeatMode)
		{
			XBMCRPC.Player.Repeat mode;

			switch (repeatMode.ToPrimitiveInt32())
			{
				case 0:
					mode = XBMCRPC.Player.Repeat.off;
					break;

				case 1:
					mode = XBMCRPC.Player.Repeat.all;
					break;

				case 2:
					mode = XBMCRPC.Player.Repeat.one;
					break;

				default:
					this.Logger.ErrorFormat("Unrecognized RepeatMode [{0}].", repeatMode.ToPrimitiveString());
					return;
			}

			this.player.SetRepeatMode(mode);
		}

		[ScriptObjectMethodParameter("ZoneNumber", "The zone number. (Always 1)", 1.0, 1.0)]
		[ScriptObjectMethodParameter("ShuffleState", "Indicates if shuffle is on or off.")]
		[ScriptObjectMethod("Set Shuffle State", "Set the shuffle state for the playlist.", "Set the {NAME} player shuffle state to {PARAM|1|true|on|off}. Zone {PARAM|0|1}.")]
		public void SetZoneShuffleState(ScriptNumber zoneNumber, ScriptBoolean shuffleState)
		{
			this.player.SetShuffle(shuffleState.ToPrimitiveBoolean());
		}

		[ScriptObjectMethodParameter("ZoneNumber", "The zone number. (Always 1)", 1.0, 1.0)]
		[ScriptObjectMethodParameter("Mode", "The mode, one of 'play', 'stop', or 'pause'.", new string[] { "play", "stop", "pause" })]
		[ScriptObjectMethod("Set Mode", "Sets the transport mode as 'play', 'stop', or 'pause'.", "Set the {NAME} player mode to {PARAM|1|play}. Zone {PARAM|0|1}.")]
		public void SetZoneTransportMode(ScriptNumber zoneNumber, ScriptString mode)
		{
			switch (mode.ToPrimitiveString())
			{
				case "pause":
					this.player.Pause();
					break;

				case "play":
					this.player.Play();
					break;

				case "stop":
					this.player.Stop();
					break;

				default:
					this.Logger.ErrorFormat("Unrecognized Transport Mode [{0}].", mode.ToPrimitiveString());
					return;
			}
		}

		[ScriptObjectMethodParameter("ZoneNumber", "The zone number. (Always 1)", 1.0, 1.0)]
		[ScriptObjectMethodParameter("Volume", "The volume level.", -100.0, 100.0)]
		[ScriptObjectMethod("Set Volume", "Sets the volume for the player. The full volume scale is 0 to 100.", "Set {NAME} player volume to {PARAM|1|10}. Zone {PARAM|0|1}.")]
		public void SetZoneVolume(ScriptNumber zoneNumber, ScriptNumber volume)
		{
			this.player.SetVolume(volume.ToPrimitiveInt32());
		}

		public override bool StartDriver(Dictionary<string, byte[]> configFileData)
		{
			Logger.Debug("Starting Kodi Driver.");

			try
			{
				this.player = new KodiPlayer(this.Logger, this.HostnameSetting, this.PortSetting, this.UsernameSetting, this.PasswordSetting);

				this.player.AlbumChanged += new EventHandler(this.AlbumChanged);
				//this.player.AreSubtitlesEnabledChanged += new EventHandler(this.AreSubtitlesEnabledChanged);
				this.player.ArtistChanged += new EventHandler(this.ArtistChanged);
				//this.player.AudioStreamsChanged += new EventHandler(this.AudioStreamsChanged);
				//this.player.ContentTypeChanged += new EventHandler(this.ContentTypeChanged);
				//this.player.CurrentAudioStreamChanged += new EventHandler(this.CurrentAudioStreamChanged);
				//this.player.CurrentSubtitleChanged += new EventHandler(this.CurrentSubtitleChanged);
				this.player.FanArtPathChanged += new EventHandler(this.FanArtPathChanged);
				this.player.FilePathChanged += new EventHandler(this.FilePathChanged);
				this.player.GenreChanged += new EventHandler(this.GenreChanged);
				this.player.IsConnectedChanged += new EventHandler(this.IsConnectedChanged);
				//this.player.IsLiveChanged += new EventHandler(this.IsLiveChanged);
				//this.player.IsPartyModeChanged += new EventHandler(this.IsPartyModeChanged);
				this.player.IsPlayingChanged += new EventHandler(this.IsPlayingChanged);
				this.player.IsShuffledChanged += new EventHandler(this.IsShuffledChanged);
				//this.player.PercentageChanged += new EventHandler(this.PercentageChanged);
				//this.player.PlaylistIdChanged += new EventHandler(this.PlaylistIdChanged);
				this.player.PlaylistChanged += new EventHandler(this.PlaylistChanged);
				this.player.PlaylistPositionChanged += new EventHandler(this.PlaylistPositionChanged);
				this.player.NameChanged += new EventHandler(this.NameChanged);
				//this.player.PositionChanged += new EventHandler(this.PositionChanged);
				this.player.RepeatChanged += new EventHandler(this.RepeatChanged);
				//this.player.SpeedChanged += new EventHandler(this.SpeedChanged);
				//this.player.SubtitlesChanged += new EventHandler(this.SubtitlesChanged);
				this.player.TimeChanged += new EventHandler(this.TimeChanged);
				this.player.TitleChanged += new EventHandler(this.TitleChanged);
				this.player.TotalTimeChanged += new EventHandler(this.TotalTimeChanged);
				this.player.TransportModeChanged += new EventHandler(this.TransportModeChanged);
				//this.player.VersionChanged += new EventHandler(this.VersionChanged);
				this.player.VolumeChanged += new EventHandler(this.VolumeChanged);

				this.refreshTimer = new System.Timers.Timer();
				this.refreshTimer.Interval = this.RefreshIntervalSetting * 1000;
				this.refreshTimer.AutoReset = false;
				this.refreshTimer.Elapsed += new ElapsedEventHandler(this.TimedRefresh);
				this.TimedRefresh(this, null);

				return true;
			}
			catch (Exception ex)
			{
				Logger.Error("Kodi Driver initialization failed.", ex);
				return false;
			}
		}

		[ScriptObjectMethodParameter("ZoneNumber", "The zone number. (Always 1)", 1.0, 1.0)]
		[ScriptObjectMethod("Stop", "Stops playing the playlist.", "Stop the {NAME} player. Zone {PARAM|0|1}.")]
		public void StopZone(ScriptNumber zoneNumber)
		{
			this.player.Stop();
		}

		[ScriptObjectMethodParameter("ZoneNumber", "The zone number. (Always 1)", 1.0, 1.0)]
		[ScriptObjectMethod("Toggle Pause", "Toggles the pause state of the playlist.", "Toggle the {NAME} player pause state. Zone {PARAM|0|1}.")]
		public void ToggleZonePause(ScriptNumber zoneNumber)
		{
			this.player.TogglePause();
		}

		[ScriptObjectMethodParameter("ZoneNumber", "The zone number. (Always 1)", 1.0, 1.0)]
		[ScriptObjectMethod("Unmute", "Turn the mute off.", "Unmute {NAME} player. Zone {PARAM|0|1}.")]
		public void UnmuteZone(ScriptNumber zoneNumber)
		{
			this.player.Unmute();
		}

		private void TimedRefresh(object sender, ElapsedEventArgs e)
		{
			try
			{
				this.refreshTimer.Stop();

				this.Logger.Debug("Refreshing Kodi Values.");

				this.player.StartNotificationListener();
				this.player.UpdateStatus();
			}
			catch (Exception ex)
			{
				this.Logger.Error("Error refreshing Kodi values.", ex);
			}
			finally { this.refreshTimer.Start(); }
		}

		private void TitleChanged(object sender, EventArgs e)
		{
			this.DevicePropertyChangeNotification("ZoneCurrentTrackTitles", BASE_INDEX, this.ZoneCurrentTrackTitles[BASE_INDEX]);
		}

		#region "PlayerEventHandlers"

		protected void TimeChanged(object sender, EventArgs e)
		{
			this.DevicePropertyChangeNotification("ZoneCurrentTrackPositions", BASE_INDEX, this.ZoneCurrentTrackPositions[BASE_INDEX]);
		}

		protected void TotalTimeChanged(object sender, EventArgs e)
		{
			this.DevicePropertyChangeNotification("ZoneCurrentTrackDurations", BASE_INDEX, this.ZoneCurrentTrackDurations[BASE_INDEX]);
		}

		protected void VolumeChanged(object sender, EventArgs e)
		{
			this.DevicePropertyChangeNotification("ZoneVolumes", BASE_INDEX, this.ZoneVolumes[BASE_INDEX]);
		}

		private void AlbumChanged(object sender, EventArgs e)
		{
			this.DevicePropertyChangeNotification("ZoneCurrentTrackAlbums", BASE_INDEX, this.ZoneCurrentTrackAlbums[BASE_INDEX]);
		}

		private void ArtistChanged(object sender, EventArgs e)
		{
			this.DevicePropertyChangeNotification("ZoneCurrentTrackArtists", BASE_INDEX, this.ZoneCurrentTrackArtists[BASE_INDEX]);
		}

		private void FanArtPathChanged(object sender, EventArgs e)
		{
			this.DevicePropertyChangeNotification("ZoneCurrentTrackCoverArts", BASE_INDEX, this.ZoneCurrentTrackCoverArts[BASE_INDEX]);
		}

		private void FilePathChanged(object sender, EventArgs e)
		{
			this.DevicePropertyChangeNotification("ZoneCurrentTrackPaths", BASE_INDEX, this.ZoneCurrentTrackPaths[BASE_INDEX]);
		}

		private void GenreChanged(object sender, EventArgs e)
		{
			this.DevicePropertyChangeNotification("ZoneCurrentTrackGenres", BASE_INDEX, this.ZoneCurrentTrackGenres[BASE_INDEX]);
		}

		private void IsConnectedChanged(object sender, EventArgs e)
		{
			this.DevicePropertyChangeNotification("Connected", BASE_INDEX, this.Connected);
		}

		private void IsPlayingChanged(object sender, EventArgs e)
		{
			this.DevicePropertyChangeNotification("ZoneTransportModes", BASE_INDEX, this.ZoneTransportModes[BASE_INDEX]);
		}

		private void IsShuffledChanged(object sender, EventArgs e)
		{
			this.DevicePropertyChangeNotification("ZoneShuffleStates", BASE_INDEX, this.ZoneShuffleStates[BASE_INDEX]);
		}

		private void NameChanged(object sender, EventArgs e)
		{
			this.DevicePropertyChangeNotification("ZoneNames", BASE_INDEX, this.ZoneNames[BASE_INDEX]);
		}

		private void PlaylistChanged(object sender, EventArgs e)
		{
			this.DevicePropertyChangeNotification("ZonePlaylistLengths", BASE_INDEX, this.ZonePlaylistLengths[BASE_INDEX]);
			this.DevicePropertyChangeNotification("ZonePlaylistPositions", BASE_INDEX, this.ZonePlaylistPositions[BASE_INDEX]);
		}

		private void PlaylistPositionChanged(object sender, EventArgs e)
		{
			this.DevicePropertyChangeNotification("ZonePlaylistPositions", BASE_INDEX, this.ZonePlaylistPositions[BASE_INDEX]);
		}

		private void RepeatChanged(object sender, EventArgs e)
		{
			this.DevicePropertyChangeNotification("ZoneRepeatModes", BASE_INDEX, this.ZoneRepeatModes[BASE_INDEX]);
		}

		private void TransportModeChanged(object sender, EventArgs e)
		{
			this.DevicePropertyChangeNotification("ZoneTransportModes", BASE_INDEX, this.ZoneTransportModes[BASE_INDEX]);
		}

		#endregion "PlayerEventHandlers"
	}
}