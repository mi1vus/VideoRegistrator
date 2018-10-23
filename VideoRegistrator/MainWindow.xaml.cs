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

namespace VideoRegistrator
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
            connectButton_Click();
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

                int maxBufferSize = 50 * 1024 * 1024;
                byte[] readBuffer = new byte[maxBufferSize];

                int isJpegStartFound = -1;

                int actualData = 0;
                int bufferOffset = 0;

                //Парсер http-ответа
                var fileCount = 1;
                var frameCount = 1;
                while (true)
                {
                    int readen = streamResponse.Read(readBuffer, actualData, maxBufferSize - actualData);

                    if (readen == 0)
                        return;

                    actualData += readen;

                    if (actualData == maxBufferSize)
                    {
                        File.WriteAllBytes($@"E:\Projects\C#\VideoRegistrator\VideoRegistrator\bin\Debug\Files\Stream{fileCount}.txt", readBuffer);
                        ++fileCount;

                        actualData -= bufferOffset;

                        Array.Copy(readBuffer, bufferOffset, readBuffer, 0, actualData);
                        bufferOffset = 0;
                    }

                    if (isJpegStartFound == -1)
                    {
                        for (; bufferOffset + 2 < actualData; bufferOffset++)
                        {
                            if (readBuffer[bufferOffset] == '\r' && readBuffer[bufferOffset + 1] == '\n' && readBuffer[bufferOffset + 2] == '\r'/*0xFF*/ && readBuffer[bufferOffset + 3] == '\n'/*0xD8*/)
                            {
                                isJpegStartFound = bufferOffset;
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (; bufferOffset + 2 < actualData; bufferOffset++)
                        {
                            if (readBuffer[bufferOffset] == 0x2D /*0xFF*/ && readBuffer[bufferOffset + 1] == 0x2D /*0xD9*/&& readBuffer[bufferOffset + 2] == 0x6D && readBuffer[bufferOffset + 3] == 0x79)
                            {
                                int frameSize = bufferOffset - isJpegStartFound;

                                byte[] frame = new byte[frameSize];

                                Array.Copy(readBuffer, isJpegStartFound, frame, 0, frameSize);

                                //File.WriteAllBytes($@"E:\Projects\C#\VideoRegistrator\VideoRegistrator\bin\Debug\Files\Frame{frameCount}.jpg", frame);
                                ++frameCount;
                                AppendAllBytes($@"E:\Projects\C#\VideoRegistrator\VideoRegistrator\bin\Debug\Files\video.mp4", frame);

                                Application.Current.Dispatcher.BeginInvoke(callback, new object[] { frame });

                                isJpegStartFound = -1;
                                bufferOffset++;
                                break;
                            }
                            else if (readBuffer[bufferOffset] == '\r' && readBuffer[bufferOffset + 1] == '\n' && readBuffer[bufferOffset + 2] == '\r'/*0xFF*/ && readBuffer[bufferOffset + 3] == '\n'/*0xD8*/)
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
                image.Source = bitmapSource;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void connectButton_Click()
        {
            if (webRequest != null)
                webRequest.Abort();

            if (mainThread != null && mainThread.IsAlive)
                mainThread.Abort();

            try
            {
                //string adress = tbCameraAdress.Text;

                //if (adress.IndexOf("http://") == -1)
                //    adress = "http://" + adress;

                //adress += string.Format("/mobile?channelnum={0}&login={1}&password={2}", tbChannelName.Text, tbUserName.Text, string.IsNullOrEmpty(tbPassword.Text) ? "" : MD5Hash(tbPassword.Text));
                //adress += string.Format("/mjpeg?channelnum={0}&login={1}&password={2}", tbChannelName.Text, tbUserName.Text, string.IsNullOrEmpty(tbPassword.Text) ? "" : MD5Hash(tbPassword.Text));
                //adress += string.Format("/video?channelnum={0}&login={1}&password={2}&resolutionx=640&resolutiony=480&fps=10", tbChannelName.Text, tbUserName.Text, string.IsNullOrEmpty(tbPassword.Text) ? "" : MD5Hash(tbPassword.Text));

                var adress = string.Format(
 //"http://91.230.153.2:1235/mobile?channelnum={0}&login={1}&password={2}"
 "http://91.230.153.2:1235/video?channelnum={0}&login={1}&password={2}"
 //+ "&resolutionx=640&resolutiony=480&fps=10"
 , 0, "root", "");

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


        public static void AppendAllBytes(string path, byte[] bytes)
        {
            //argument-checking here.

            using (var stream = new FileStream(path, FileMode.Append))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }
    }
}
