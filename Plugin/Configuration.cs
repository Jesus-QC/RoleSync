using Exiled.API.Interfaces;

namespace RoleSync.Plugin
{
    public class Configuration : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool ShowDebug { get; set; } = false;
        public string ServerToken { get; set; } = "PASTE YOUR TOKEN HERE";
    }
}