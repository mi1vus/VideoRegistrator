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
using System.Threading.Tasks;
using System.Net;
using System.IO;
using NReco.VideoConverter;



namespace VideoRegistrator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Thread mainThread = null;

        private HttpWebRequest webRequest = null;
        string log = "";
        string progr = "";
        int maxBufferReadSize = 1 * 124 * 1024;
        int maxBufferSaveSize = 1 * 1024 * 1024;
        public static Timer timer;
        
        int recvCount = 1;
        bool isIFrame = false;
        int decCount = 1;

        private static FFMPegStream ffinStream;
        private static MemoryStream outStream = new MemoryStream();

        private delegate void OnFrameReceivedCallback(byte[] framebuffer, bool iFrame);
        private delegate void OnFrameDecodeCallback(byte[] framebuffer);

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                //File.Delete($@"Files\video.mpeg4");
                //File.Delete($@"Files\video2.mpeg");
                Directory.Delete(@"Files", true); //true - если директория не пуста удаляем все ее содержимое
                Directory.CreateDirectory(@"Files");
                Directory.CreateDirectory(@"Files\Streams");
                Directory.CreateDirectory(@"Files\Frames");


                ConvertStream(/*inStream, */outStream);

                connectButton_Click();
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
                //adress += string.Format("/mobile?channelnum={0}&login={1}&password={2}", tbChannelName.Text, tbUserName.Text, string.IsNullOrEmpty(tbPassword.Text) ? "" : MD5Hash(tbPassword.Text));
                //adress += string.Format("/mjpeg?channelnum={0}&login={1}&password={2}", tbChannelName.Text, tbUserName.Text, string.IsNullOrEmpty(tbPassword.Text) ? "" : MD5Hash(tbPassword.Text));
                //adress += string.Format("/video?channelnum={0}&login={1}&password={2}&resolutionx=640&resolutiony=480&fps=10", tbChannelName.Text, tbUserName.Text, string.IsNullOrEmpty(tbPassword.Text) ? "" : MD5Hash(tbPassword.Text));

                var adress = string.Format(
 //"http://91.230.153.2:1235/mobile?channelnum={0}&login={1}&password={2}"
 "http://91.230.153.2:1235/video?channelnum={0}&login={1}&password={2}"
 //+ "&resolutionx=640&resolutiony=480&fps=2"
 //+ "&streamtype=alternative"
 , 0, "root", "");

                webRequest = (HttpWebRequest)WebRequest.Create(adress);
                webRequest.Method = "GET";
                webRequest.Timeout = 10000;
                webRequest.ReadWriteTimeout = 10000;

                mainThread = new Thread(ReceiveThreadEntry);
                mainThread.IsBackground = true;
                mainThread.Start();

                Task saveTask = new Task(SaveThreadEntry);
                saveTask.Start();


                //saveThread = new Thread(SaveThreadEntry);
                //saveThread.IsBackground = false;
                //saveThread.Start();
                //while (true)
                //{
                //    Task progrTask = new Task(() =>
                //    {
                //        label1.Content = progr;
                //        label2.Content = log;
                //        Thread.Sleep(250);
                //    });
                //    progrTask.Start();
                //}
            }
            catch (Exception we)
            {
                MessageBox.Show(we.Message);
                return;
            }
        }
        
        private void ReceiveThreadEntry()
        {
            try
            {
                OnFrameReceivedCallback callback = new OnFrameReceivedCallback(OnFrameReceived);

                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                Stream streamResponse = webResponse.GetResponseStream();

                byte[] readBuffer = new byte[maxBufferReadSize];

                int isJpegStartFound = -1;

                int actualData = 0;
                int bufferOffset = 0;

                //Парсер http-ответа
                var fileCount = 1;
                while (true)
                {
                    int readen = streamResponse.Read(readBuffer, actualData, maxBufferReadSize - actualData);

                    if (readen == 0)
                        return;

                    actualData += readen;

                    if (actualData == maxBufferReadSize)
                    {
                        actualData -= bufferOffset;

                        byte[] readed = new byte[bufferOffset];

                        Array.Copy(readBuffer, 0, readed, 0, bufferOffset);

                        File.WriteAllBytes($@"Files\Streams\1Stream{fileCount}.txt", readed);

                        //Application.Current.Dispatcher.BeginInvoke(callback, new object[] { readed, true });

                        ++fileCount;
                        Array.Copy(readBuffer, bufferOffset, readBuffer, 0, actualData);
                        bufferOffset = 0;
                    }


                    if (isJpegStartFound == -1)
                    {
                        for (; bufferOffset + 2 < actualData; bufferOffset++)
                        {
                            if (
                                readBuffer[bufferOffset] == '\r' && 
                                readBuffer[bufferOffset + 1] == '\n' && 
                                readBuffer[bufferOffset + 2] == '\r'/*0xFF*/ && 
                                readBuffer[bufferOffset + 3] == '\n'/*0xD8*/)
                            {
                                isJpegStartFound = bufferOffset;
                                break;
                            }
                            else if (readBuffer[bufferOffset] == 0x49 &&    /*I*/
                                readBuffer[bufferOffset + 1] == 0x2D &&     /*-*/
                                readBuffer[bufferOffset + 2] == 0x66 &&     /*f*/
                                readBuffer[bufferOffset + 3] == 0x72 &&     /*r*/
                                readBuffer[bufferOffset + 4] == 0x61 &&     /*a*/
                                readBuffer[bufferOffset + 5] == 0x6D &&     /*m*/
                                readBuffer[bufferOffset + 6] == 0x65)       /*e*/
                            {
                                isIFrame = true;
                            }
                        }
                    }
                    else
                    {
                        for (; bufferOffset + 2 < actualData; bufferOffset++)
                        {
                            if (
                                readBuffer[bufferOffset] == 0x2D &&     /*-*/
                                readBuffer[bufferOffset + 1] == 0x2D && /*-*/
                                readBuffer[bufferOffset + 2] == 0x6D && /*m*/
                                readBuffer[bufferOffset + 3] == 0x79)   /*y*/
                            {
                                int frameSize = bufferOffset - isJpegStartFound;

                                if (frameSize > 0)
                                {
                                    byte[] frame = new byte[frameSize];

                                    Array.Copy(readBuffer, isJpegStartFound, frame, 0, frameSize);

                                    Application.Current.Dispatcher.BeginInvoke(callback, new object[] { frame, isIFrame ? true : false });
                                    isIFrame = false;
                                }

                                isJpegStartFound = -1;
                                bufferOffset++;
                                break;
                            }
                            else if (
                                readBuffer[bufferOffset] == '\r' && 
                                readBuffer[bufferOffset + 1] == '\n' && 
                                readBuffer[bufferOffset + 2] == '\r'/*0xFF*/ && 
                                readBuffer[bufferOffset + 3] == '\n'/*0xD8*/)
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

        private void SaveThreadEntry()
        {
            try
            {
                OnFrameDecodeCallback callback = new OnFrameDecodeCallback(OnFrameDecode);

                byte[] saveBuffer = new byte[maxBufferSaveSize];

                int isJpegStartFound = -1;

                int actualData = 0;
                int actualStreamPosition = 0;
                int bufferOffset = 0;

                //Парсер http-ответа
                var fileCount = 1;
                while (true)
                {
                    if (outStream.Length == 0)
                        continue;
                    int readen = 0;
                    lock (outStream)
                    {
                        //outStream.Position = actualStreamPosition;
                        outStream.Seek(actualStreamPosition, SeekOrigin.Begin);
                        readen = outStream.Read(saveBuffer, actualData, maxBufferSaveSize - actualData);
                        if (outStream.Length <= readen + actualStreamPosition)
                        {
                            outStream.SetLength(0);
                            actualStreamPosition = 0;
                        }
                        else
                        {
                            outStream.Seek(outStream.Length, SeekOrigin.Begin);
                            actualStreamPosition = readen + actualStreamPosition;
                        }
                    }
                    if (readen == 0)
                        return;

                    actualData += readen;

                    if (actualData == maxBufferSaveSize)
                    {
                        //File.WriteAllBytes($@"Files\Streams\2Stream{fileCount}.txt", saveBuffer);
                        ++fileCount;

                        actualData -= bufferOffset;

                        Array.Copy(saveBuffer, bufferOffset, saveBuffer, 0, actualData);
                        bufferOffset = 0;
                    }


                    if (isJpegStartFound == -1)
                    {
                        for (; bufferOffset + 3 <= actualData; bufferOffset++)
                        {
                            if (saveBuffer[bufferOffset] == 0xFF && saveBuffer[bufferOffset + 1] == 0xD8 && saveBuffer[bufferOffset + 2] == 0xFF && saveBuffer[bufferOffset + 3] == 0xE0)
                            {
                                isJpegStartFound = bufferOffset;
                                bufferOffset++;
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (; bufferOffset + 2 < actualData; bufferOffset++)
                        {
                            if (saveBuffer[bufferOffset] == 0xFF && saveBuffer[bufferOffset + 1] == 0xD8 && saveBuffer[bufferOffset + 2] == 0xFF && saveBuffer[bufferOffset + 3] == 0xE0)
                            {
                                int frameSize = bufferOffset - isJpegStartFound;


                                if (frameSize > 0)
                                {
                                    byte[] frame = new byte[frameSize];

                                    Array.Copy(saveBuffer, isJpegStartFound, frame, 0, frameSize);

                                    Application.Current.Dispatcher.BeginInvoke(callback, frame);

                                    isJpegStartFound = -1;
                                }
                                //bufferOffset++;
                                break;
                            }
                        }
                    }
                }
                //ffinStream.Close();
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void OnFrameReceived(byte[] framebuffer, bool iFrame)
        {
            try
            {
                if (framebuffer.Length > 0 /*&& (iFrame || recvCount < 150 || recvCount % 1 == 0)*/)
                {
                    //File.WriteAllBytes($@"Files\Frames\Frame{recvCount}.mpeg4", framebuffer);
                    AppendAllBytes($@"Files\video.mpeg4", framebuffer);
                    ffinStream.Write(framebuffer, 0, framebuffer.Length);
                    //File.AppendAllText($@"Files\countlog.txt", $@"recv {recvCount} - " + (iFrame ? "I" : "-") + $@"     {DateTime.Now}" + Environment.NewLine);
                    ++recvCount;
                    //isIFrame = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void OnFrameDecode(byte[] framebuffer)
        {
            try
            {
                //File.WriteAllBytes($@"Files\Frames\Frame{frameCount}.mpeg4", framebuffer);
                //AppendAllBytes($@"Files\video2.mpeg", framebuffer);

                if (framebuffer.Length > 0)
                {
                    //AppendAllBytes($@"Files\video2.mpeg", framebuffer);
                    File.AppendAllText($@"Files\countlog.txt", $@"dec {decCount}         {DateTime.Now}" + Environment.NewLine);
                    ++decCount;
                    JpegBitmapDecoder decoder = new JpegBitmapDecoder(new MemoryStream(framebuffer), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                    BitmapSource bitmapSource = decoder.Frames[0];
                    image.Source = bitmapSource;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private Task ConvertStream(/*Stream input, */Stream outputStream)
        {
            Task convertTask = new Task(() => {
                try
                {
                    var convertSettings = new ConvertSettings
                    {
                        CustomOutputArgs = "-map 0",
                        CustomInputArgs = "-vcodec h264"
                    };
                    convertSettings.SetVideoFrameSize(360, 360);
                    var ffMpeg = new FFMpegConverter();
                    ffMpeg.ConvertProgress += FfMpeg_ConvertProgress;
                    ffMpeg.LogReceived += FfMpeg_LogReceived;

                    //task = ffMpeg.ConvertLiveMedia(Format.h264, outStream, Format.mjpeg, convertSettings);
                    //task = ffMpeg.ConvertLiveMedia(inStream, Format.h264, outStream, Format.mjpeg, convertSettings);
                    //            task = ffMpeg.ConvertLiveMedia(inStream, Format.h264, $@"Files\video2.mpeg", Format.mjpeg, convertSettings);
                    //            task.Start();
                    //ffMpeg.ConvertMedia($@"Files\video.mpeg4", Format.h264, $@"Files\video3.mpeg", Format.mjpeg, convertSettings);
                    //var task = ffMpeg.ConvertLiveMedia(Format.h264, "C:\\Work\\Test\\converted.avi", Format.avi, convertSettings);
                    //ffMpeg.ConvertMedia($@"Files\video.mpeg4", $@"Files\video2.flv", Format.flv);
                    //var setting = new NReco.VideoConverter.ConvertSettings();
                    //setting.SetVideoFrameSize(360, 360);
                    //setting.VideoCodec = "h264";
                    //ffMpeg.ConvertMedia($@"Files\video.mpeg4", Format.h264, $@"Files\video2.mjpeg", Format.mjpeg, setting);
                    //task.Stop();
                    var task = ffMpeg.ConvertLiveMedia(Format.h264, outputStream, Format.mjpeg, convertSettings);

                    task.Start();

                    ffinStream = new FFMPegStream(task);
                    //var copyTask = input.CopyToAsync(ffmpegStream);
                    //copyTask.Wait();
                    //ffmpegStream.Close();

                    task.Wait();
                    //ffMpeg.ConvertMedia(@"C:\Work\Test\MyHomeSecureNode\devices\test\video.h264", "C:\\Work\\Test\\converted.avi", Format.avi);

                    outputStream.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            });

            convertTask.Start();
            return convertTask;
        }

        private void FfMpeg_ConvertProgress(object sender, ConvertProgressEventArgs e)
        {
            try
            { 
                progr = e.Processed.ToString();
                File.AppendAllText($@"Files\progress.txt", string.Format("ffmpeg progress: {0}" + Environment.NewLine, e.Processed.ToString()));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void FfMpeg_LogReceived(object sender, FFMpegLogEventArgs e)
        {
            try
            { 
                log = e.Data.ToString();
                File.AppendAllText($@"Files\log.txt", string.Format("ffmpeg log: {0}" + Environment.NewLine + Environment.NewLine + Environment.NewLine, e.Data.ToString()));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public static void AppendAllBytes(string path, byte[] bytes)
        {
            //argument-checking here.
            try
            {
                using (var stream = new FileStream(path, FileMode.Append))
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private class FFMPegStream : Stream
        {
            private ConvertLiveMediaTask mMediaTask;
            public FFMPegStream(ConvertLiveMediaTask mediaTask)
            {
                try
                {
                    mMediaTask = mediaTask;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }


            public override void Write(byte[] buffer, int offset, int count)
            {
                try
                {
                    lock (outStream)
                    {
                        mMediaTask.Write(buffer, offset, count);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

            public override void Close()
            {
                try
                {
                    mMediaTask.Stop();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

            public override bool CanRead
            {
                get
                {
                    return false;
                }
            }

            public override bool CanSeek
            {
                get
                {
                    return false;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    return true;
                }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotImplementedException();
            }

            public override long Length
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override long Position
            {
                get
                {
                    throw new NotImplementedException();
                }

                set
                {
                    throw new NotImplementedException();
                }
            }

            public override void Flush()
            {
                throw new NotImplementedException();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotImplementedException();
            }

            public override void SetLength(long value)
            {
                throw new NotImplementedException();
            }
        }
    }
}
