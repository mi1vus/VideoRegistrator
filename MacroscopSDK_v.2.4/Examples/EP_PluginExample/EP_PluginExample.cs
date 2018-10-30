using System;
using Alarus;
using Alarus.Archiving;
using Alarus.RealTimeFrameProviding;

namespace EP_PluginExample
{
    [PluginGUINameAttribute("Модуль генерации внешних событий")]
    [PluginHasSettingsAttribute]
    public class EP_PluginExample : EventProcessor, IPlugin
	{ 
	    public override void Initialize(PluginSettings settings)
	    {
		    
	    }

	    public override PluginSettings SetSettings(PluginSettings settings)
        {
            PluginSettings res;
            res.channelSpecificSettings = null;
            res.generalSettings = null;
            return res;
        }

	    public override void Dispose()
	    {
		    //Dispose
	    }

	    #region IPlugin members
        public Guid Id
        {
            get { return new Guid("17EE3457-8FC2-4C0F-B133-EF11D0C4F38C"); }
        }

        public string Name
        {
            get { return "Example Event Processor"; }
        }

        public string Manufacturer
        {
            get { return "Example Company"; }
        }

        public void Initialize(IPluginHost host)
        {
            //host.RegisterEventProcessor(this.GetType());
        }
        #endregion
    }
}
