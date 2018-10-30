using System;
using System.Globalization;
using Alarus;
using Alarus.GUI;
using System.Windows.Media;
using System.Windows;
using Alarus.RealTimeFrameProviding;

namespace McRTVisualizer
{
	public class McRTVisualizer : PluginVisualiser, IPlugin
	{
		public override DrawingVisual NullableDrawingVisual => _drawingVisual;

		readonly DrawingVisual _drawingVisual = new DrawingVisual();
		protected IDrawingPanel DrawingPanel;

		public McRTVisualizer(Guid pluginId)
			: base(pluginId)
		{
		}

		public override void Initialize(Guid channelId, IPluginClientToolSet pluginToolset, IPluginVisualizerSet visualiserSet)
        {
			base.Initialize(channelId, pluginToolset, visualiserSet);

			DrawingPanel = visualiserSet.DrawingPanel;

			DrawingPanel.AddVisual(_drawingVisual);
		}

		//Пример. В случае тревоги рисует на ячейки прямоугольник залитый произвольным цветом
		//и выводит на экран произвольное целое значение как строку
		public override void ProcessEvent(Guid chId, RawEvent chEv, bool isAlarm)
		{
			if (isAlarm)
			{
				DrawingContext drawingContext = _drawingVisual.RenderOpen();

				double curWidth = DrawingPanel.PanelWidth;
				double curHeight = DrawingPanel.PanelHeight;

				var sampleRect = new Rect(0.3, 0.3, 0.4, 0.4);

				Rect normalRect = new Rect(
					sampleRect.X * curWidth,
					sampleRect.Y * curHeight,
					sampleRect.Width * curWidth,
					sampleRect.Height * curHeight
					);

				drawingContext.DrawRectangle(
					new SolidColorBrush(ConvertGuidToColor(Guid.NewGuid())),
					new Pen(Brushes.Yellow, 2.0),
					normalRect
					);

				var formattedText = new FormattedText(
					ConvertGuidToMyStringId(Guid.NewGuid()),
					CultureInfo.GetCultureInfo("ru-RU"),
					FlowDirection.LeftToRight,
					new Typeface("Verdana"),
					20,
					Brushes.Red);

				drawingContext.DrawText(formattedText, new Point(normalRect.X + normalRect.Width/2, normalRect.Y));

				drawingContext.Close();
			}
		}

		private static Color ConvertGuidToColor(Guid guid)
		{
			System.Drawing.Color cl = System.Drawing.Color.FromArgb(BitConverter.ToInt32(guid.ToByteArray(), 0));
			return Color.FromArgb(150/*cl.A*/, cl.R, cl.G, cl.B);
		}
		private static String ConvertGuidToMyStringId(Guid guid)
		{
			byte[] bArr = guid.ToByteArray();
			byte tempId = bArr[15];
			return tempId.ToString(CultureInfo.InvariantCulture);
		}
        public override void Clear()
        {
            DrawingContext dc = _drawingVisual.RenderOpen();
            dc.Close();
        }

		#region Implementation of IPlugin

		public Guid Id
		{
			get { return new Guid("3E55966B-919D-4563-B173-08EDE32C4D25"); }
		}

		public string Name
		{
			get { return "Macroscop test visualizer"; }
		}

        public string Manufacturer
        {
            get { return "MACROSCOP"; }
        }

		public void Initialize(IPluginHost host)
		{
			host.RegisterRTVisualizer(GetType(), null);
		}

		#endregion
	}
}
