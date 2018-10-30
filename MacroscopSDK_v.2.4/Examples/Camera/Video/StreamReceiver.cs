using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Alarus.Frames.Sound;
using Alarus.Frames.Video;
using Alarus.IpDevices;
using Alarus.IpDevices.Events;
using Alarus.IpDevices.IO;
using Alarus.IpDevices.PTZ;
using Alarus.RealTimeFrameProviding;
using System.Threading;
using Basic.Log;
using Alarus;
using Alarus.IpDevices.Storage.Archive;

namespace SampleSource.Video
{
	public class StreamReceiver : ICameraServiceProvider
	{
		private class ReferencedBytes : IReferencedBytes
		{
			public ReferencedBytes(byte[] bytes)
			{
				Buffer = bytes;
				Offset = 0;
				Length = bytes.Length;
			}

			public void Dispose()
			{
				//
			}

			public IReferencedData CreateReferencedCopy()
			{
				return new ReferencedBytes(Buffer);
			}

			public IReferencedBytes CreateReferencedBytesCopy()
			{
				return (IReferencedBytes)CreateReferencedCopy();
			}

			public IReferencedBytes CreateSubReferencedBytes(int startIndex, int length)
			{
				return new ReferencedBytes(Buffer.Skip(startIndex).Take(length).ToArray());
			}

			public int Offset { get; private set; }
			public int Length { get; private set; }
			public byte[] Buffer { get; private set; }

			public byte this[int relativeIndex]
			{
				get { throw new NotImplementedException(); }
				set { throw new NotImplementedException(); }
			}
		}

		private ConnectionParameters conParams = null;
		private SubStreamParameters[] subParams = null;

		private Thread mainVideoThread = null;

		public StreamReceiver(ConnectionParameters conParams, SubStreamParameters[] subParams)
		{
			this.conParams = conParams;
			this.subParams = subParams;
		}

		public IAsyncResult BeginGetCapabilities(AsyncCallback callback, object state)
		{
			throw new NotImplementedException();
		}

		public DeviceCapabilities EndGetCapabilities(IAsyncResult asyncResult)
		{
			throw new NotImplementedException();
		}

		public event NewRawFrameEventHandler NewRawFrame;
		public event NewRawEventHandler NewEvent;
		public event EventHandler NewLogRecord;

		public bool IsStreamActive(ChannelStreamTypes streamType)
		{
			return mainVideoThread != null;
		}

		private void OnNoConnection()
		{
			var disEvent = new NoDataConnectionDeviceEvent();
			disEvent.StreamTypesMask = ChannelStreamTypes.MainVideo;
			NewEvent(this, new NewEventEventArgs(disEvent));
		}

		private void WorkFunc()
		{
			try
			{
				//здесь получаем кадры, организуем сетевое взаимодейтствие с утройством

				long frameStartTime = Environment.TickCount;
				long eventStartTime = Environment.TickCount;

				Random randomizer = new Random();

				int fps = 25;
 				int frameDelay = 1000 / fps;
				int disconnectDelay = 10000 + randomizer.Next(5000);

				AddLogRecord(StreamTypes.Main, ChannelStreamTypes.MainVideo, string.Format("Попытка установить подключение к камере {0}({1}) по адресу: {2}.", ModuleInfo.DevName, this.GetType().Name, conParams.Address));
				Thread.Sleep(1000); //здесь должна быть логика подключения к камере
				AddLogRecord(StreamTypes.Main, ChannelStreamTypes.MainVideo, "Подключение к камере установлено. Начинаем приём кадров.");

				//Идентификатор последовательности видео кадров
				Guid videoSeqId = Guid.NewGuid();
				//Номер очередного видео кадра в последовательности
				long videoNumInSeq = 0;

				while (true)
				{
					//имитируем разрыв с камерой
					if (Environment.TickCount - eventStartTime >= disconnectDelay)
					{
						OnNoConnection();
						
						Thread.Sleep(5000 + randomizer.Next(5000));
						disconnectDelay = 5000 + randomizer.Next(5000);
						eventStartTime = Environment.TickCount;
						continue;
					}

					//имитируем работу камеры с частотой 25 кадров в секунду
					if (Environment.TickCount - frameStartTime >= frameDelay)
					{
						
						var jpegFrame = new RawMJPEGFrame(new ReferencedBytes(Camera.Properties.Resources.testFrame));
						//jpegFrame.Id.SeqId = videoSeqId;
						//jpegFrame.Id.NumInSeq = videoNumInSeq++;
						jpegFrame.TimeStamp = DateTime.UtcNow;

						NewRawFrame(this, new NewRawFrameEventArgs(ChannelStreamTypes.MainVideo, jpegFrame));
						frameStartTime = Environment.TickCount;
					}
				}
			}
			catch (ThreadAbortException)
			{
				AddLogRecord(StreamTypes.Main, ChannelStreamTypes.MainVideo, "Ручная остановка подключения к камере.");
			}
			catch (Exception ex)
			{
				AddLogRecord(StreamTypes.Main, ChannelStreamTypes.MainVideo, "* Ошибка при получении кадров с камеры: " + ex.Message);
				SampleLogMgr.LogException(ex, string.Empty);
			}
		}

		public void StartStream(ChannelStreamTypes streamType)
		{
			try
			{
				if (streamType == ChannelStreamTypes.MainVideo)
				{
					mainVideoThread = new Thread(WorkFunc);
					mainVideoThread.IsBackground = true;
					mainVideoThread.Start();
				}
			}
			catch (Exception ex)
			{
				SampleLogMgr.LogException(ex, string.Empty);
			}
		}

		public void SendSound(RawSoundFrame frame)
		{
			return;
		}

		public IPtzController GetPtzController()
		{
			throw new NotImplementedException();
		}

		public IDigitalOutputsController GetDigitalOutputsController()
		{
			throw new NotImplementedException();
		}

		public IDeviceArchiveController GetDeviceArchiveController()
		{
			throw new NotImplementedException();
		}

		public void StopStream(ChannelStreamTypes streamType)
		{
			try
			{
				if (streamType == ChannelStreamTypes.MainVideo)
				{
					if (mainVideoThread != null)
					{
						mainVideoThread.Abort();
						mainVideoThread = null;
					}
				}
			}
			catch (Exception ex)
			{
				SampleLogMgr.LogException(ex, string.Empty);
			}
		}

		public void Release()
		{
			StopStream(ChannelStreamTypes.MainVideo);
		}

		#region Лог подключения к камере
		private int maxLinesInConnectionLog = 100;

		private bool isWritingConnectionLog;
		public bool IsWritingConnectionLog
		{
			get { return isWritingConnectionLog; }
			set
			{
				isWritingConnectionLog = value;
			}
		}

		private List<string> connectionLog = new List<string>();
		public string ConnectionLog
		{
			get
			{
				lock (connectionLog)
				{
					string logLine = string.Empty;

					foreach (string line in connectionLog)
						logLine += line + "\r\n";

					return logLine;
				}
			}
		}

		public void ClearConnectionLog()
		{
			lock (connectionLog)
			{
				connectionLog.Clear();
			}
		}

		public void AddLogRecord(StreamTypes streamType, ChannelStreamTypes channelStreamType, string info)
		{
			if (!isWritingConnectionLog)
				return;

			string typeOfData = string.Empty;

			switch (channelStreamType)
			{
				case ChannelStreamTypes.MainVideo:
				case ChannelStreamTypes.AlternativeVideo:
					typeOfData = "видео";
					break;
				case ChannelStreamTypes.MainSound:
				case ChannelStreamTypes.AlternativeSound:
					typeOfData = "звук";
					break;
				case ChannelStreamTypes.MotionDetection:
					typeOfData = "детектор движения";
					break;
			}

			string streamTypeName = string.Empty;

			if (streamType == StreamTypes.Main)
				streamTypeName = "основной поток";
			else
				streamTypeName = "альтернативный поток";

			string str = string.Format("[{0}, {1}, {2}] {3}", DateTime.Now.ToLongTimeString(), typeOfData, streamTypeName, info);

			lock (connectionLog)
			{
				connectionLog.Add(str);

				if (connectionLog.Count > maxLinesInConnectionLog)
					connectionLog.RemoveAt(0);
			}

			//уведомляем хоста о том, что содержимое лога изменилось
			NewLogRecord(this, new EventArgs());
		}

		public void AddLogRecord(StreamTypes subType, ChannelStreamTypes subChannelType, string format, params object[] args)
		{
			AddLogRecord(subType, subChannelType, string.Format(format, args));
		}

		#endregion
	}
}
