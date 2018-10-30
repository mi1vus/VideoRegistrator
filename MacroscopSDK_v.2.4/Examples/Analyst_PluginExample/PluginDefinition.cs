using System;
using Alarus;

namespace Analyst_PluginExample
{
    public class PluginDefinition : IPlugin
    {
        public Guid Id
        {
            get { return new Guid("17EE3457-8FC2-4C0F-B133-EF11D0C4F38C"); }
        }

        public string Name
        {
            get { return "Модуль тестового события"; }
        }

        public string Manufacturer
        {
            get { return "Example Corp"; }
        }

        public void Initialize(IPluginHost host)
        {
            host.RegisterAnalyst(typeof(Analyst));
            host.RegisterExternalEvent(typeof(CounterEvent));
        }
    }
}
