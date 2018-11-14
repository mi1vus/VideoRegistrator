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
        VLC VLСControl;
        //AxVLCPlugin2 VLCPlug;

        private Thread mainThread = null;

        private HttpWebRequest webRequest = null;
        string log = "";
        string progr = "";
        int maxBufferReadSize =     100 * 1024 * 1024;
        int maxBufferlogSize =            100 * 1024;
        int maxBufferSaveSize =    100 * 1024 * 1024;
        public static Timer timer;
        
        int recvCount = 1;
        bool isIFrame = false;
        int decCount = 1;

        private static FFMPegStream ffinStream;
        private static MemoryStream outStream = new MemoryStream();

        Stream streamResponse;

        private delegate void OnFrameReceivedCallback(byte[] framebuffer, bool iFrame);
        private delegate void OnFrameDecodeCallback(byte[] framebuffer);

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                try
                {
                    Directory.Delete(@"Files", true); //true - если директория не пуста удаляем все ее содержимое
                }
                catch  { }

                Directory.CreateDirectory(@"Files");
                Directory.CreateDirectory(@"Files\Streams");
                Directory.CreateDirectory(@"Files\Frames");


                //ConvertStream(/*inStream, */outStream);

                //connectButton_Click();

                VLСControl = new VLC();
                //VLCPlug = new AxVLCPlugin2();
                WinFormsHost.Child = VLСControl;
                //VLСControl.axVLCPlugin21.playlist.add("rtsp://91.230.153.2:126/rtsp?channelId=0c09cc2a-b077-4171-bcec-772bc81133e0&login=root&password=", null, null);
                //VLСControl.axVLCPlugin21.playlist.add("rtsp://91.230.153.2:126/rtsp?channelId=0c09cc2a-b077-4171-bcec-772bc81133e0&login=root&password=&streamtype=alternative", null, null);
                //VLСControl.axVLCPlugin21.playlist.add("rtsp://91.230.153.2:126/rtsp?channelId=0c09cc2a-b077-4171-bcec-772bc81133e0&login=root&password=&mode=archive&starttime=08.11.2018+03:05:01.125&speed=1", null, null);
                VLСControl.axVLCPlugin21.playlist.add("rtsp://91.230.153.2:126/rtsp?channelId=0c09cc2a-b077-4171-bcec-772bc81133e0&login=root&password=&streamtype=alternative&mode=archive&starttime=08.11.2018+03:05:01.125&speed=1", null, null);
                //VLСControl.axVLCPlugin21.playlist.add("rtsp://91.230.153.2:1237/rtsp?login=admin&password=3108020718camera", null, null);
                //VLСControl.axVLCPlugin21.playlist.add("rtsp://91.230.153.2:1237/rtsp", null, null);
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
//"http://91.230.153.2:1235/mobile?"
"http://91.230.153.2:1235/video?"
//+ "channelId=0c09cc2a-b077-4171-bcec-772bc81133e0"
+ "channelnum={0}"
+ "&login={1}&password={2}"
//+ "&resolutionx=640&resolutiony=480&fps=2"
+ "&streamtype=alternative"
//+ "&mode=archive&startTime=07.11.2018+05:05:01"
//+ "&sound=on&speed=1"
, 0, "root", "");

                webRequest = (HttpWebRequest)WebRequest.Create(adress);
                webRequest.Method = "GET";
                webRequest.Timeout = 10000;
                webRequest.ReadWriteTimeout = 10000;

                //mainThread = new Thread(ReceiveThreadEntry);
                //mainThread.IsBackground = true;
                //mainThread.Start();

                Task saveTask1 = new Task(ReceiveThreadEntry);
                saveTask1.Start();

                Task saveTask2 = new Task(SaveThreadEntry);
                saveTask2.Start();


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

                byte[] readBuffer = new byte[maxBufferReadSize];

                int isJpegStartFound = -1;

                int actualData = 0;
                int bufferOffset = 0;
                int savedOffset = 0;

                //Парсер http-ответа
                var fileCount = 1;
                while (true)
                {
                    int readen = ReadFromInet(readBuffer, actualData, maxBufferReadSize);

                    if (readen > 0)
                    {

                        File.AppendAllText($@"Files\read.txt", $@"try read {maxBufferReadSize - actualData} from {actualData} of max {maxBufferReadSize} {Environment.NewLine}");
                        actualData += readen;
                        File.AppendAllText($@"Files\read.txt", $@"read {readen} - actualData - {actualData} bufferOffset - {bufferOffset} {Environment.NewLine}");

                        if (actualData - savedOffset > maxBufferlogSize)
                        {
                            byte[] readed = new byte[maxBufferlogSize];

                            Array.Copy(readBuffer, savedOffset, readed, 0, maxBufferlogSize);

                            File.WriteAllBytes($@"Files\Streams\1Stream{fileCount}.txt", readed);
                            ++fileCount;
                            savedOffset += maxBufferlogSize;
                        }


                        if (actualData >= maxBufferReadSize)
                        {
                            actualData -= bufferOffset;
                            File.AppendAllText($@"Files\read.txt", $@"ovf actualData - {actualData} bufferOffset - {bufferOffset} {Environment.NewLine}");


                            //Application.Current.Dispatcher.BeginInvoke(callback, new object[] { readed, true });

                            Array.Copy(readBuffer, bufferOffset, readBuffer, 0, actualData);
                            bufferOffset = 0;
                            File.AppendAllText($@"Files\read.txt", $@"ovf new actualData - {actualData} bufferOffset - {bufferOffset} {Environment.NewLine}");
                        }
                    }

                    if (isJpegStartFound == -1)
                    {
                        for (; bufferOffset + 6 <= actualData; bufferOffset++)
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
                        for (; bufferOffset + 3 <= actualData; bufferOffset++)
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
            catch (ThreadAbortException ex)
            {
                MessageBox.Show(ex.ToString());
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
                    int readen = 0;
                    if (outStream.Length > 0)
                    {
                        lock (outStream)
                        {
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
                    }

                    //readen = ReadFromInet(saveBuffer, actualData, maxBufferSaveSize);

                    if (readen > 0)
                    {
                        actualData += readen;

                        if (actualData == maxBufferSaveSize)
                        {
                            //File.WriteAllBytes($@"Files\Streams\2Stream{fileCount}.txt", saveBuffer);
                            ++fileCount;

                            actualData -= bufferOffset;

                            Array.Copy(saveBuffer, bufferOffset, saveBuffer, 0, actualData);
                            bufferOffset = 0;
                        }
                    }

                    if (isJpegStartFound == -1)
                    {
                        for (; bufferOffset + 3 <= actualData; bufferOffset++)
                        {
                            if (saveBuffer[bufferOffset] == 0xFF && 
                                saveBuffer[bufferOffset + 1] == 0xD8 && 
                                saveBuffer[bufferOffset + 2] == 0xFF && 
                                saveBuffer[bufferOffset + 3] == 0xE0)
                            {
                                isJpegStartFound = bufferOffset;
                                bufferOffset++;
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (; bufferOffset + 3 <= actualData; bufferOffset++)
                        {
                            if (saveBuffer[bufferOffset] == 0xFF && 
                                saveBuffer[bufferOffset + 1] == 0xD8 && 
                                saveBuffer[bufferOffset + 2] == 0xFF && 
                                saveBuffer[bufferOffset + 3] == 0xE0)
                            {
                                int frameSize = bufferOffset - isJpegStartFound;


                                if (frameSize > 0)
                                {
                                    byte[] frame = new byte[frameSize];

                                    Array.Copy(saveBuffer, isJpegStartFound, frame, 0, frameSize);

                                    Application.Current.Dispatcher.BeginInvoke(callback, new object[] { frame });

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
                if (framebuffer.Length > 0 /*&& (iFrame || recvCount < 150 || recvCount % 3 == 0)*/)
                {
                    //File.WriteAllBytes($@"Files\Frames\Frame{recvCount}.mpeg4", framebuffer);
                    //AppendAllBytes($@"Files\video.mpeg4", framebuffer);
                    ffinStream.Write(framebuffer, 0, framebuffer.Length);
                    File.AppendAllText($@"Files\countlog.txt", $@"recv {recvCount} - " + (iFrame ? "I" : "-") + $@"     {DateTime.Now}" + Environment.NewLine);
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
                //MessageBox.Show(ex.ToString());
            }
        }

        private int ReadFromInet(byte[] readBuffer, int actualData, int max)
        {
            if (streamResponse == null)
            {
                HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();
                streamResponse = webResponse.GetResponseStream();
            }
            return streamResponse.Read(readBuffer, actualData, max - actualData);
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
                    //convertSettings.SetVideoFrameSize(360, 360);
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

        private void button_Click(object sender, RoutedEventArgs e)
        {
            VLСControl.axVLCPlugin21.playlist.play();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            VLСControl.axVLCPlugin21.playlist.stop();
        }
    }
}
