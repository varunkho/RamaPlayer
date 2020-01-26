using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WMPLib;

namespace RamaPlayer
{
	public partial class PlayerForm : Form
	{
		private static HashSet<string> mediaExtensions = new HashSet<string>("mp4;mp3;avi;mpeg;wav;wmv".Split(';'), StringComparer.OrdinalIgnoreCase);
		private readonly string cmdFilePath;
		private string currentFolder;
		private List<string> allFiles;
		private bool imClosing;
		private Task statusUpdateWorker;
		private bool isRepeating;

		private LibVLC _libVLC;
		private MediaPlayer _mp;

		private string _cfile;
		private string CurrentFile
		{
			get
			{
				return _cfile;
			}
			set
			{
				_cfile = value;
				if (!string.IsNullOrEmpty(_cfile))
				{
					StartPlaying();
				}
			}
		}

		public PlayerForm(string filePath)
		{
			this.cmdFilePath = filePath;
			Core.Initialize();
			this.WindowState = FormWindowState.Maximized;
			InitializeComponent();

			_libVLC = new LibVLC();
			_mp = new MediaPlayer(_libVLC);
			videoView1.MediaPlayer = _mp;
			videoView1.PreviewKeyDown += WM_PreviewKeyDown;
			//_mp.uiMode = "none";
			//_mp.stretchToFit = true;
			videoView1.KeyDown += WM_KeyDownEvent;
			_mp.TimeChanged += Media_TimeChange;

			this.FormClosing += PlayerForm_FormClosing;
			this.StatusLabel.TextChanged += (s, e) => videoView1.AccessibleDescription = StatusLabel.Text;

			//statusUpdateWorker = Task.Factory.StartNew(() =>
			//	{
			//		while (!imClosing)
			//		{
			//			Thread.Sleep(200);
			//			try
			//			{
			//				Media_TimeChange(null, null);
			//			}
			//			catch (Exception Exception) { }
			//		}
			//	});
		}

		void PlayerForm_KeyDown(object sender, KeyEventArgs e)
		{
			//ProcessKeyInput(new KeyInputEventArgs(e));
			//WM_PreviewKeyDown(null, new PreviewKeyDownEventArgs(e.KeyData));
			//if (e.KeyCode == Keys.G)
			//{
			//    MessageBox.Show(e.KeyData.ToString());
			//}
		}

		void PlayerForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			this.imClosing = true;
			if (_mp.Fullscreen)
			{
				_mp.Fullscreen = false;
			}

			//statusUpdateWorker.Wait(500);
			Application.DoEvents();
		}

		void Media_TimeChange(object sender, MediaPlayerTimeChangedEventArgs e)
		{
			string status = "";
			if (_mp.Media != null && _mp.Length > 0)
			{
				status = string.Format("{0:H:m:s} ({1}% of {2:H:mm:ss}) {3}",
					new DateTime(TimeSpan.FromMilliseconds(_mp.Time).Ticks),
					_mp.Time == 0 ? 0 : (int)(_mp.Time / (_mp.Length / 100)),
					new DateTime(TimeSpan.FromMilliseconds(_mp.Length).Ticks),
					isRepeating ? "(Repeating)" : string.Empty);
			}
			else
			{
				status = "Not Playing";
			}

			StatusLabel.BeginInvoke(new Action(() => StatusLabel.Text = status));
		}

		void Media_StateChanged(object sender, LibVLCSharp.Shared.MediaStateChangedEventArgs e)
		{
			BeginInvoke(new Action(() =>
			{
				switch (e.State)
				{
					case VLCState.Ended:
						if (isRepeating)
						{
							StartPlaying();
						}
						else
						{
							Next();
							//Task.Factory.StartNew(() =>
							//{
							//	Thread.Sleep(500);
							//	videoView1.Invoke(new Action(() => _mp.Play()));
							//});
						}
						break;
				}
			}));
		}

		void WM_KeyDownEvent(object sender, KeyEventArgs e)
		{
			// Though this event should always follow the previewkeydown event, yet we make it so as to handle cases in which fullscreen is active and focus is on that.
			// Following variable will be true if previewkeydown did process this keystroke.
			if (!keyProcessed)
			{
				ProcessKeyInput(new KeyInputEventArgs(e));
			}

			keyProcessed = false;
		}

		bool keyProcessed = false;
		void WM_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			keyProcessed = true;
			ProcessKeyInput(new KeyInputEventArgs(e));
		}

		void BrowsMedia()
		{
			if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				InitializeNewLocation(openFileDialog1.FileName);
			}
		}

		void InitializeNewLocation(string filePath)
		{
			this.currentFolder = Path.GetDirectoryName(filePath);
			this.CurrentFile = Path.GetFileName(filePath);

			var ext = Path.GetExtension(this.CurrentFile);
			if (!mediaExtensions.Contains(ext))
				mediaExtensions.Add(ext);

			this.allFiles = Directory.GetFiles(this.currentFolder)
				.Where(f =>
				{
					var x = Path.GetExtension(f);
					return x.Length > 2 && mediaExtensions.Contains(x);
				})
				.Select(f => Path.GetFileName(f))
				.ToList();

		}

		void Next()
		{
			if (this.allFiles != null)
			{
				var current = this.allFiles.IndexOf(this.CurrentFile);
				var next = current < 0 || current + 1 == this.allFiles.Count ? 0 : current + 1;
				this.CurrentFile = this.allFiles[next];
			}
		}

		void Previous()
		{
			if (this.allFiles != null)
			{
				var current = this.allFiles.IndexOf(this.CurrentFile);
				var prior = current <= 0 ? this.allFiles.Count - 1 : current - 1;
				this.CurrentFile = this.allFiles[prior];
			}
		}

		void StartPlaying()
		{
			lock (this)
			{
				this.Text = this.CurrentFile + " Rama Player";
				if (_mp.Media != null)
				{
					_mp.Media.Dispose();
				}

				var media = new Media(_libVLC, $@"{this.currentFolder}\{this.CurrentFile}", FromType.FromPath);
				media.StateChanged += Media_StateChanged;
				_mp.Media = media;
				_mp.Play();
				_mp.Mute = false;
			}
		}

		void ProcessKeyInput(KeyInputEventArgs e)
		{
			if (_mp.Media != null && _mp.Length > 0)
			{
				int offset = (e.Shift && e.Control ? 60 : e.Control ? 40 : e.Alt ? 15 : 4) * 1000;
				var Offset20 = _mp.Length / 100 * 20;
				switch (e.KeyCode)
				{
					case Keys.Left:
						_mp.Time = (_mp.Time - offset) < 0 ? 0 : _mp.Time - offset;
						break;
					case Keys.Right:
						_mp.Time = (_mp.Time + offset) > _mp.Length ? _mp.Length : _mp.Time + offset;
						break;

					case Keys.PageUp:
						_mp.Time = _mp.Time < Offset20 ? 0 : _mp.Time - Offset20;
						break;
					case Keys.PageDown:
						_mp.Time = (_mp.Time + Offset20) > _mp.Length ? _mp.Length : _mp.Time + Offset20;
						break;

					case Keys.Up:
						if (e.Shift)
							_mp.SetRate(_mp.Rate + 1);
						else
							_mp.Volume += 1;
						break;
					case Keys.Down:
						if (e.Shift)
							_mp.SetRate(_mp.Rate - 1);
						else
							_mp.Volume -= 1;
						break;

					case Keys.M:
						_mp.Mute = !_mp.Mute;
						break;

					case Keys.P:
					case Keys.Space:
						_mp.SetPause(_mp.State == VLCState.Playing);
						break;

					case Keys.W:
						Previous();
						break;
					case Keys.E:
						Next();
						break;
					case Keys.Home:
						_mp.Time = 0;
						break;
					case Keys.End:
						if (e.Shift)
							_mp.Time = _mp.Length - 2500;
						else
							_mp.Time = _mp.Length;
						break;

					case Keys.F:
						_mp.ToggleFullscreen();
						// Focussing was primarily intended as fullscreen is turned off; however, it did the trick for arrow keys not functioning in fullscreen mode! Preview keydown rocks!
						videoView1.Focus();
						break;
					case Keys.Escape:
						if (_mp.Fullscreen)
							_mp.Fullscreen = false;

						videoView1.Focus();
						break;

				}
			}

			switch (e.KeyCode)
			{
				case Keys.O:
					BrowsMedia();
					break;
				case Keys.Q:
					Application.Exit();
					break;
				case Keys.R:
					isRepeating = !isRepeating;
					break;

				case Keys.G:
					var input = InputForm.Ask("Go to Time", ParseTime);
					if (input.result == DialogResult.OK)
					{
						_mp.Time = (int)input.value.TotalSeconds * 1000;
					}
					break;
			}
		}

		private TimeSpan ParseTime(string value)
		{
			TimeSpan time;
			if (int.TryParse(value, out var minutes))
				time = TimeSpan.FromMinutes(minutes);
			else
			{
				var formats = new[]
				{
				"h:m:s",
				"h:m",
				"m-s"
			};

				time = DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result)
					? result.TimeOfDay : throw new InvalidOperationException("Invalid time format. Either h:m or h:m:s");
			}

			if ((time.TotalSeconds * 1000) > _mp.Length)
				throw new InvalidOperationException($"Out of duration");

			return time;
		}

		private void PlayerForm_Load(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(cmdFilePath) && File.Exists(cmdFilePath))
				InitializeNewLocation(cmdFilePath);
		}
	}

	class KeyInputEventArgs
	{
		public bool Alt { get; private set; }
		public bool Control { get; private set; }
		public bool Shift { get; private set; }
		public Keys KeyCode { get; private set; }

		//public KeyInputEventArgs(AxWMPLib._WMPOCXEvents_KeyDownEvent e)
		//{
		//	this.KeyCode = (Keys)e.nKeyCode;
		//	this.Shift = (e.nShiftState & 1) == 1;
		//	this.Control = (e.nShiftState & 2) == 2;
		//	this.Alt = (e.nShiftState & 4) == 4;
		//}

		public KeyInputEventArgs(PreviewKeyDownEventArgs e)
		{
			this.KeyCode = e.KeyCode;
			this.Shift = e.Shift;
			this.Control = e.Control;
			this.Alt = e.Alt;
		}

		public KeyInputEventArgs(KeyEventArgs e)
		{
			this.KeyCode = e.KeyCode;
			this.Shift = e.Shift;
			this.Control = e.Control;
			this.Alt = e.Alt;
		}
	}
}
