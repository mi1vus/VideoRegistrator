using System;
using System.Collections.Generic;
using Alarus;
using Alarus.Analysis;

namespace MD_Example
{
	public class MD_PluginExample : MotionDetector//, IPlugin
	{
		public MD_PluginExample()
		{
		}

		public override MotionMap Detect(int width, int height, int offset, int stride, byte[] bgr24bytes, DateTime timestamp)
		{
			short[,] map = new short[1,1];
			map[0,0] = 1;
			short[] pixX = new short[] {0};
			short[] pixY = new short[] {0};

			List<MotionObject> mos = new List<MotionObject>();
			mos.Add(new MotionObject(pixX, pixY, (short)(width), (short)(height)));
			MotionMap mm = new MotionMap(map, mos);

			return mm;
		}

		#region IPlugin members
		public Guid Id
		{
			get { return new Guid("48E581EB-8621-4660-8C95-D09751358359"); }
		}

		public string Name
		{
			get { return "Example Motion Detector"; }
		}

        public string Manufacturer
        {
            get { return "Example Company"; }
        }

		public void Initialize(IPluginHost host)
		{
			host.RegisterMotionDetector(this.GetType());
		}
		#endregion
	}
}
