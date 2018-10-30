using System;
using System.Collections.Generic;
using Basic.Log;
using Alarus;
using Alarus.Config;
using Alarus.IpDevices;
using Alarus.IpDevices.Registration;
using Alarus.IpDevices.Util;
using Alarus.RealTimeFrameProviding;
using SampleSource.Video;

namespace SampleSource
{
    public partial class ModuleInfo : IPlugin
    {
        public const string deviceId = "5C48C51D-B17B-40b0-9486-6DC71DD76D90";

        /// <summary>
        /// Имя производителя
        /// </summary>
        public const string DevBrandName = "SampleSource";

		/// <summary>
		/// Название модели
		/// </summary>
		public const string DevName = "Sample Camera";

        public Guid Id
        {
			get { return new Guid("D7E40264-D81A-4FAF-8B87-E48964398DF2"); }
        }

        public string Name
        {
            get { return DevBrandName; }
        }

        public string Manufacturer
        {
            get { return "MACROSCOP SDK"; }
        }

        public void Initialize(IPluginHost host)
        {
            SampleLogMgr.LogMgr = host.GetLogManager();

			DevType_RegInfo sampleDevice = new DevType_RegInfo();
			sampleDevice.DeviceTypeGuid = new Guid(deviceId);
			sampleDevice.DevTypeModelName = DevName;
			sampleDevice.DevTypeBrandName = DevBrandName;
			sampleDevice.AvailableResolutions = new List<Resolution> { new Resolution(640, 480) };
			sampleDevice.Capabilities = DevType_Capabilities.SupportsCameras;
			sampleDevice.GetCameraService = (conParams, subParams) =>
			{
				return new StreamReceiver(conParams, subParams);
			};
			sampleDevice.SetDeviceParameters = (conParams, subParams) =>
			{
				return true;
			};
			host.RegisterDevType(sampleDevice);

			MediaStream_RegInfo sampleDevice_Mjpeg = new MediaStream_RegInfo();
			sampleDevice_Mjpeg.DeviceTypeGuid = new Guid(deviceId);
			sampleDevice_Mjpeg.StreamFormat = VideoStreamFormats.MJPEG;
			sampleDevice_Mjpeg.ConnectionType = NetworkConnectionTypes.HTTP;
			sampleDevice_Mjpeg.Capabilities.SupportedStreamTypes = ChannelStreamTypes.MainVideo;
			sampleDevice_Mjpeg.Capabilities.SupportedExtraParameters = new DeviceParameters[] { DeviceParameters.Null, DeviceParameters.Null };
			sampleDevice_Mjpeg.Capabilities.SupportedDeviceParameters = new DeviceParameters[] { DeviceParameters.Null, DeviceParameters.Null };
			host.RegisterMediaStreamInfo(sampleDevice_Mjpeg);
        }
    }
}