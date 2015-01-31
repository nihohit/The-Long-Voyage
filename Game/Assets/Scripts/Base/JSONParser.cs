using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Assets.Scripts.Base
{
    #region JSONParser

    public sealed class JsonParser<T>
    {
        private Dictionary<string, object> m_currentDictionary;

        #region public methods

        // Read the configuration file and load the configurations
        public IEnumerable<T> GetConfigurations(string fileName)
        {
            using (var fileReader = new StreamReader("{0}".FormatWith(fileName)))
            {
                var fileAsString = fileReader.ReadToEnd();
                var items = Json.Deserialize(fileAsString).SafeCast<IEnumerable<object>>("items");
                var itemsAsDictionaries = items.Select(item => item as Dictionary<string, object>);
                return itemsAsDictionaries.Select(item => this.ConvertToObject(item)).Materialize();
            }
        }

        public T ConvertToObject(Dictionary<string, object> item)
        {
            m_currentDictionary = item;
            return ConvertCurrentItemToObject();
        }

        #endregion public methods

        #region private methods

        private T ConvertCurrentItemToObject()
        {
            return ObjectConstructor.ParseObject<T>(m_currentDictionary);
        }

        #endregion private methods
    }

    #endregion JSONParser

    #region ConfigurationStorage

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

    #endregion ConfigurationStorage
}