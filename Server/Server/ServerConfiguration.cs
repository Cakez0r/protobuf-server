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

        [ConfigurationProperty("relevanceDistanceSquared", DefaultValue = "1600", IsRequired = true)]
        public float RelevanceDistanceSquared
        {
            get { return (float)this["relevanceDistanceSquared"]; }
            set { this["relevanceDistanceSquared"] = value; }
        }
    }
}
