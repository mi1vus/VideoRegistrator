using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Net;
using System.IO;

namespace HttpVideo
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Thread mainThread = null;
		private HttpWebRequest webRequest = null;

		private delegate void OnFrameReceivedCallback(byte[] framebuffer);


		public MainWindow()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Получается MD5-хэш из строки
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		private static string MD5Hash(string str)
		{
			System.Text.StringBuilder s = new System.Text.StringBuilder();
			foreach (byte b in new System.Security.Cryptography.MD5CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(str)))
			{
				s.Append(b.ToString("X2"));
			}
			return s.ToString();
		}

		private void WorkerThreadEntry()
		{
			try
			{
				OnFrameReceivedCallback callback = new OnFrameReceivedCallback(OnFrameReceived);

				HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
				Stream streamResponse = webResponse.GetResponseStream();

				int maxBufferSize = 32 * 1024 * 1024;
				byte[] readBuffer = new byte[maxBufferSize];

				int isJpegStartFound = -1;

				int actualData = 0;
				int bufferOffset = 0;

				//Парсер http-ответа
				while (true)
				{
					int readen = streamResponse.Read(readBuffer, actualData, maxBufferSize - actualData);

					if (readen == 0)
						return;

					actualData += readen;

					if (actualData == maxBufferSize)
					{
						actualData -= bufferOffset;

						Array.Copy(readBuffer, bufferOffset, readBuffer, 0, actualData);
						bufferOffset = 0;
					}

					if (isJpegStartFound == -1)
					{
						for (; bufferOffset + 2 < actualData; bufferOffset++)
						{
							if (readBuffer[bufferOffset] == '\r' &&readBuffer[bufferOffset + 1] == '\n' && readBuffer[bufferOffset + 2] == 0xFF && readBuffer[bufferOffset + 3] == 0xD8)
							{
								isJpegStartFound = bufferOffset;
								break;
							}
						}
					}
					else
					{
						for (; bufferOffset + 1 < actualData; bufferOffset++)
						{
							if (readBuffer[bufferOffset] == 0xFF && readBuffer[bufferOffset + 1] == 0xD9)
							{
								int frameSize = bufferOffset - isJpegStartFound + 2;

								byte[] frame = new byte[frameSize];
								
								Array.Copy(readBuffer, isJpegStartFound, frame, 0, frameSize);

								Application.Current.Dispatcher.BeginInvoke(callback, new object[] { frame });

								isJpegStartFound = -1;
								bufferOffset++;
								break;
							}
							else if (readBuffer[bufferOffset] == 0xFF && readBuffer[bufferOffset + 1] == 0xD8)
							{
								isJpegStartFound = bufferOffset;
							}
						}
					}
				}
			}
			catch (ThreadAbortException)
			{
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		/// <summary>
		/// В момент получения кадра основным потоком
		/// </summary>
		/// <param name="framebuffer"></param>
		private void OnFrameReceived(byte[] framebuffer)
		{
			try
			{
				JpegBitmapDecoder decoder = new JpegBitmapDecoder(new MemoryStream(framebuffer), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
				BitmapSource bitmapSource = decoder.Frames[0];
				frameRender.Source = bitmapSource;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private void connectButton_Click(object sender, RoutedEventArgs e)
		{
            //FileInfo file = new FileInfo(@"C:\Users\Jman\VideoMaker.avi");

            //var currentDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            //// Default installation path of VideoLAN.LibVLC.Windows
            //var libDirectory =
            //    new DirectoryInfo(Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));

            //using (var mediaPlayer = new Vlc.DotNet.Core.VlcMediaPlayer(libDirectory))
            //{

            //    var mediaOptions = new[]
            //    {
            //    ":sout=#rtp{sdp=rtsp://192.168.1.162:8008/test}",
            //    ":sout-keep"
            //};

            //    //mediaPlayer.SetMedia(new Uri("http://hls1.addictradio.net/addictrock_aac_hls/playlist.m3u8"),
            //    //    mediaOptions);

            //    mediaPlayer.SetMedia(file, mediaOptions);

            //    mediaPlayer.Play();

            //    Console.WriteLine("Streaming on rtsp://192.168.1.162:8008/test");
            //    Console.WriteLine("Press any key to exit");
            //    Console.ReadKey();
            //}
            //            try
            //            {
            //                Vlc.DotNet.Core.

            //                LibVlc vlc = new LibVlc();
            //                vlc.Initialize();
            //                vlc.VideoOutput = pictureBox1;
            //                vlc.PlaylistClear();
            //                string[] Options = new string[] { ":sout=#duplicate{dst=display,dst=std {access=udp,mux=ts,dst=224.100.0.1:1234}}" };
            //                vlc.AddTarget("c:\\1.flv", Options);
            //                vlc.Play();
            //            }
            //            catch (Exception e1)
            //            {
            //                MessageBox.Show(e1.ToString());
            //            }

            //            using System;
            //            using System.Threading;
            //            using LibVLC.NET;

            //class Program
            //        {
            //            static void Main()
            //            {
            //LibVLCLibrary library = LibVLCLibrary.Load(null);
            //IntPtr inst, mp, m;

            //inst = library.libvlc_new();                                      // Load the VLC engine 
            //m = library.libvlc_media_new_location(inst, "path/to/your/file"); // Create a new item 
            //mp = library.libvlc_media_player_new_from_media(m);               // Create a media player playing environement 
            //library.libvlc_media_release(m);                                  // No need to keep the media now 
            //library.libvlc_media_player_play(mp);                             // play the media_player 
            //Thread.Sleep(10000);                                              // Let it play a bit 
            //library.libvlc_media_player_stop(mp);                             // Stop playing 
            //library.libvlc_media_player_release(mp);                          // Free the media_player 
            //library.libvlc_release(inst);

            //LibVLCLibrary.Free(library);
            ////            }
            ////        }

            //LibVLCSharp.Shared.LibVLC l;
            //l.













            if (webRequest != null)
				webRequest.Abort();

			if (mainThread != null && mainThread.IsAlive)
				mainThread.Abort();

			try
			{
				string adress = tbCameraAdress.Text;

				if (adress.IndexOf("http://") == -1)
					adress = "http://" + adress;

                //adress += string.Format("/mobile?channelnum={0}&login={1}&password={2}", tbChannelName.Text, tbUserName.Text, string.IsNullOrEmpty(tbPassword.Text) ? "" : MD5Hash(tbPassword.Text));
                //adress += string.Format("/mjpeg?channelnum={0}&login={1}&password={2}", tbChannelName.Text, tbUserName.Text, string.IsNullOrEmpty(tbPassword.Text) ? "" : MD5Hash(tbPassword.Text));
                //adress += string.Format("/video?channelnum={0}&login={1}&password={2}&resolutionx=640&resolutiony=480&fps=10", 0, "root", "");
                adress += string.Format("/mobile?channelnum={0}&login={1}&password={2}&resolutionx=640&resolutiony=480&fps=10", 0, "root", "");

                webRequest = (HttpWebRequest)WebRequest.Create(adress);
				webRequest.Method = "GET";
				webRequest.Timeout = 10000;
				webRequest.ReadWriteTimeout = 10000;

				mainThread = new Thread(WorkerThreadEntry);
				mainThread.IsBackground = true;
				mainThread.Start();
			}
			catch (Exception we)
			{
				MessageBox.Show(we.Message);
				return;
			}
		}
	}
}
