using System;
using Alarus;
using Alarus.Analysis;
using Alarus.Archiving;
using Analyst_PluginExample.GUI;

namespace Analyst_PluginExample
{
    [PluginGUINameAttribute("Модуль тестового события")]
    [PluginHasSettingsAttribute]
    public class Analyst : VideoAnalyst
    {
        private long lastTimeEventGen;
        private int delay;

		private int currentCount = 0;

	    public override void Initialize(Guid id, IPluginAnalystToolSet analystToolSet, PluginEnvironment pluginEnv)
	    {
			lastTimeEventGen = Environment.TickCount;

			//читаем общие настройки
			if (pluginEnv.pluginSettings.generalSettings != null)
				delay = (pluginEnv.pluginSettings.generalSettings as AnalystSettings).Delay;
		}

	    public override void Process(ImageData image, MotionMap motionMap, BackgroundImage background)
        {
            // здесь анализируется кадр. в качестве результата должно генерироваться событие.

			// добавляем к счетчику количество движущихся объектов (как бы считаем что любой движущийся объект = человек).
			if (motionMap != null)
				currentCount += motionMap.MotionObjectsCount;

            //генерирует событие не чаще раз в delay мс
            if (Environment.TickCount - lastTimeEventGen > delay)
            {
				CounterEvent ev = new CounterEvent(currentCount);

				ev.EventTime = image.Timestamp;

                GenerateEvent(ev, false);
                
                lastTimeEventGen = Environment.TickCount;
				// обнавляем счетчик людей.
				currentCount = 0;
            }
        }

        public override void Dispose()
        {

        }

        public override object ProcessCommand(object cmdObj)
        {
            return null;
        }

        public override PluginSettings SetSettings(Guid channel, ISettingsHost settingsHost, PluginSettings settings)
        {
            AnalystSettings analystSettings;

            if (settings.generalSettings == null)
            {
                analystSettings = new AnalystSettings();
                settings.generalSettings = analystSettings;
            }
            else
                analystSettings = settings.generalSettings as AnalystSettings;

            var window = new SettingsWindow();
			settingsHost.SetFramesHandler(window.OnNewFrameCame);

            window.tbDelay.Text = analystSettings.Delay.ToString();

            window.Closed += (snd, args) =>
                {
                    int.TryParse(window.tbDelay.Text, out analystSettings.Delay);
                };

            window.ShowDialog();

            return settings;
        }
	}
}
