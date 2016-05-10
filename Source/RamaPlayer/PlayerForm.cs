using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
		private string currentFolder;
		private List<string> allFiles;
		private bool imClosing;
		private Task statusUpdateWorker;
		private bool isRepeating;

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
			this.WindowState = FormWindowState.Maximized;
			InitializeComponent();

			axWindowsMediaPlayer1.PreviewKeyDown += WM_PreviewKeyDown;
			axWindowsMediaPlayer1.uiMode = "none";
			axWindowsMediaPlayer1.stretchToFit = true;
			axWindowsMediaPlayer1.KeyDownEvent += WM_KeyDownEvent;
			axWindowsMediaPlayer1.PlayStateChange += WM_PlayStateChange;
			axWindowsMediaPlayer1.PositionChange += WM_PositionChange;
			if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
				InitializeNewLocation(filePath);

			this.FormClosing += PlayerForm_FormClosing;
			statusUpdateWorker = Task.Factory.StartNew(() =>
				{
					while (!imClosing)
					{
						Thread.Sleep(200);
						try
						{
							WM_PositionChange(null, null);
						}
						catch (Exception Exception) { }
					}
				});
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
			if (axWindowsMediaPlayer1.fullScreen)
			{
				axWindowsMediaPlayer1.fullScreen = false;
			}

			statusUpdateWorker.Wait();
			Application.DoEvents();
		}

		void WM_PositionChange(object sender, AxWMPLib._WMPOCXEvents_PositionChangeEvent e)
		{
			if (axWindowsMediaPlayer1.Ctlcontrols != null && axWindowsMediaPlayer1.Ctlcontrols.currentItem != null)
			{
				StatusLabel.Text = string.Format("{0}% of {1:H:mm:ss}{2}",
					axWindowsMediaPlayer1.Ctlcontrols.currentPosition == 0 ? 0 : (int)(axWindowsMediaPlayer1.Ctlcontrols.currentPosition / (axWindowsMediaPlayer1.Ctlcontrols.currentItem.duration / 100)),
					new DateTime(TimeSpan.FromSeconds(axWindowsMediaPlayer1.Ctlcontrols.currentItem.duration).Ticks),
					isRepeating ? "(Repeating)" : string.Empty);
			}
			else
			{
				StatusLabel.Text = "Not Playing";
			}
		}

		bool moveToNext = false;
		void WM_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
		{
			switch ((WMPPlayState)e.newState)
			{
				case WMPPlayState.wmppsMediaEnded:
					if (isRepeating)
					{
						Task.Factory.StartNew(() =>
						{
							Thread.Sleep(500);
							axWindowsMediaPlayer1.Invoke(new Action(() => axWindowsMediaPlayer1.Ctlcontrols.play()));
						});
					}
					else
					{
						moveToNext = true;
					}
					break;
				case WMPPlayState.wmppsStopped:
					if (moveToNext)
					{
						moveToNext = false;
						Next();
						Task.Factory.StartNew(() =>
						{
							Thread.Sleep(500);
							axWindowsMediaPlayer1.Invoke(new Action(() => axWindowsMediaPlayer1.Ctlcontrols.play()));
						});
					}
					break;
			}
		}

		void WM_KeyDownEvent(object sender, AxWMPLib._WMPOCXEvents_KeyDownEvent e)
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
				axWindowsMediaPlayer1.currentMedia = axWindowsMediaPlayer1.newMedia(this.currentFolder + @"\" + this.CurrentFile);
				axWindowsMediaPlayer1.settings.mute = false;
			}
		}

		void ProcessKeyInput(KeyInputEventArgs e)
		{
			if (axWindowsMediaPlayer1.Ctlcontrols.currentItem != null)
			{
				int offset = e.Shift && e.Control ? 60 : e.Control ? 40 : e.Alt ? 15 : 4;
				var Offset20 = axWindowsMediaPlayer1.Ctlcontrols.currentItem.duration / 100 * 20;
				switch (e.KeyCode)
				{
					case Keys.Left:
						axWindowsMediaPlayer1.Ctlcontrols.currentPosition -= offset;
						break;
					case Keys.Right:
						axWindowsMediaPlayer1.Ctlcontrols.currentPosition += offset;
						break;

					case Keys.PageUp:
						axWindowsMediaPlayer1.Ctlcontrols.currentPosition = axWindowsMediaPlayer1.Ctlcontrols.currentPosition < Offset20 ? 0 : axWindowsMediaPlayer1.Ctlcontrols.currentPosition - Offset20;
						break;
					case Keys.PageDown:
						axWindowsMediaPlayer1.Ctlcontrols.currentPosition = axWindowsMediaPlayer1.Ctlcontrols.currentPosition + Offset20 > axWindowsMediaPlayer1.Ctlcontrols.currentItem.duration ? axWindowsMediaPlayer1.Ctlcontrols.currentItem.duration : axWindowsMediaPlayer1.Ctlcontrols.currentPosition + Offset20;
						break;

					case Keys.Up:
						axWindowsMediaPlayer1.settings.volume += 1;
						break;
					case Keys.Down:
						axWindowsMediaPlayer1.settings.volume -= 1;
						break;

					case Keys.M:
						axWindowsMediaPlayer1.settings.mute = !axWindowsMediaPlayer1.settings.mute;
						break;

					case Keys.P:
					case Keys.Space:
						if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPlaying)
							axWindowsMediaPlayer1.Ctlcontrols.pause();
						else
							axWindowsMediaPlayer1.Ctlcontrols.play();
						break;

					case Keys.W:
						Previous();
						break;
					case Keys.E:
						Next();
						break;
					case Keys.Home:
						axWindowsMediaPlayer1.Ctlcontrols.currentPosition = 0;
						break;
					case Keys.End:
						axWindowsMediaPlayer1.Ctlcontrols.currentPosition = axWindowsMediaPlayer1.Ctlcontrols.currentItem.duration;
						break;

					case Keys.F:
						if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPlaying || axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPaused)
						{
							axWindowsMediaPlayer1.fullScreen = !axWindowsMediaPlayer1.fullScreen;
							// Focussing was primarily intended as fullscreen is turned off; however, it did the trick for arrow keys not functioning in fullscreen mode! Preview keydown rocks!
							axWindowsMediaPlayer1.Focus();
						}
						break;
					case Keys.Escape:
						if (axWindowsMediaPlayer1.fullScreen)
							axWindowsMediaPlayer1.fullScreen = false;

						axWindowsMediaPlayer1.Focus();
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
			}
		}
	}

	class KeyInputEventArgs
	{
		public bool Alt { get; private set; }
		public bool Control { get; private set; }
		public bool Shift { get; private set; }
		public Keys KeyCode { get; private set; }

		public KeyInputEventArgs(AxWMPLib._WMPOCXEvents_KeyDownEvent e)
		{
			this.KeyCode = (Keys)e.nKeyCode;
			this.Shift = (e.nShiftState & 1) == 1;
			this.Control = (e.nShiftState & 2) == 2;
			this.Alt = (e.nShiftState & 4) == 4;
		}

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
