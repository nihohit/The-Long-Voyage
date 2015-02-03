using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Base.JsonParsing
{
    // A storage class for configurations of type Tconfiguration
    public sealed class ConfigurationStorage<TConfiguration>
        where TConfiguration : IIdentifiable<string>
    {
        #region fields

        private readonly IDictionary<string, TConfiguration> r_configurationsDictionary;
        private readonly string r_fileName;

        #endregion fields

        #region constructor

        public ConfigurationStorage(string fileName)
        {
            this.r_fileName = "Config/{0}.json".FormatWith(fileName);
            var parser = new JsonParser<TConfiguration>();
            this.r_configurationsDictionary =
                parser.GetConfigurations(this.r_fileName).ToDictionary(
                    configuration => configuration.Name,
                    configuration => configuration);
        }

        #endregion constructor

        #region public methods

        // Try to get a named configuration from the storage and fail if it can't be found.
        public TConfiguration GetConfiguration(string configurationName)
        {
            TConfiguration template;

            if (!this.r_configurationsDictionary.TryGetValue(configurationName, out template))
            {
                throw new KeyNotFoundException("Configuration {0} not found in file {1}.".FormatWith(configurationName, this.r_fileName));
            }

            return template;
        }

        // return all parsed configurations
        public IEnumerable<TConfiguration> GetAllConfigurations()
        {
            return this.r_configurationsDictionary.Values.Materialize();
        }

        #endregion public methods
    }
}