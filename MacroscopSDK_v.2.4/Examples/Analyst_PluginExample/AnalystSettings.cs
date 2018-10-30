using System;

namespace Analyst_PluginExample
{
    /// <summary>
    /// Настройки аналитика. 
    /// Может содержать любые поля.
    /// </summary>
    [Serializable]
    public class AnalystSettings
    {
        public int Delay = 5000;
    }
}
