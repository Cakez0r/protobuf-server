using System.Configuration;

namespace Server
{
    public class ServerConfiguration : ConfigurationSection
    {
        [ConfigurationProperty("port", DefaultValue = "25012", IsRequired = true)]
        public int Port 
        {
            get { return (int)this["port"]; }
            set { this["port"] = value; }
        }
    }
}
