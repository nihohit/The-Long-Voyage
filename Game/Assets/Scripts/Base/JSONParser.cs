using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Assets.Scripts.Base
{
    #region JSONParser

    public abstract class JSONParser<T>
    {
        private Dictionary<string, object> m_currentDictionary;

        #region public methods

        // Read the configuration file and load the configurations
        public IEnumerable<T> GetConfigurations(string fileName)
        {
            using (var fileReader = new StreamReader("{0}".FormatWith(fileName)))
            {
                var fileAsString = fileReader.ReadToEnd();
                var items = Json.Deserialize(fileAsString) as IEnumerable<object>;
                Assert.NotNull(items, "items");
                var itemsAsDictionaries = items.Select(item => item as Dictionary<string, object>);
                return itemsAsDictionaries.Select(item => ConvertToObject(item)).Materialize();
            }
        }

        public T ConvertToObject(Dictionary<string, object> item)
        {
            m_currentDictionary = item;
            return ConvertCurrentItemToObject();
        }

        #endregion public methods

        #region private methods

        // Check a dictionary representation of a class for a property value.
        private bool TryGetValue<TValue>(string propertyName, out TValue result)
        {
            result = default(TValue);
            object value;
            if (!m_currentDictionary.TryGetValue(propertyName, out value))
            {
                return false;
            }

            if (!(value is TValue) && (typeof(TValue).IsEnum && !(value is int)))
            {
                throw new WrongValueType(propertyName, typeof(TValue), value.GetType());
            }

            result = (TValue)value;
            return true;
        }

        // Check a dictionary representation of a class for a property value and throw an exception if it can't be found.
        protected TValue TryGetValueAndFail<TValue>(string propertyName)
        {
            TValue result;
            if (!TryGetValue(propertyName, out result))
            {
                throw new ValueNotFoundException(propertyName, typeof(T));
            }
            return result;
        }

        // Check a dictionary representation of a class for a property and return a default value if it can't be found.
        protected TValue TryGetValueOrSetDefaultValue<TValue>
            (string propertyName, TValue defaultValue)
        {
            TValue result;
            if (!TryGetValue(propertyName, out result))
            {
                result = defaultValue;
            }
            return result;
        }

        protected abstract T ConvertCurrentItemToObject();

        #endregion private methods
    }

    #endregion JSONParser

    #region ConfigurationStorage

    // A storage class for configurations of type Tconfiguration
    public abstract class ConfigurationStorage<TConfiguration, TStorageType>
        where TConfiguration : IIdentifiable<string>
        where TStorageType : ConfigurationStorage<TConfiguration, TStorageType>
    {
        #region fields

        private readonly IDictionary<string, TConfiguration> m_configurationsDictionary;
        private readonly string m_fileName;

        #endregion fields

        public static TStorageType Instance
        {
            get
            {
                return Singleton<TStorageType>.Instance;
            }
        }

        #region constructor

        protected ConfigurationStorage(string fileName)
        {
            m_fileName = "Config/{0}.json".FormatWith(fileName);
            var parser = GetParser();
            m_configurationsDictionary = parser.GetConfigurations(m_fileName).
                ToDictionary(configuration => configuration.Name,
                             configuration => configuration);
        }

        #endregion constructor

        #region public methods

        // Try to get a named configuration from the storage and fail if it can't be found.
        public TConfiguration GetConfiguration(string configurationName)
        {
            TConfiguration template;

            if (!m_configurationsDictionary.TryGetValue(configurationName, out template))
            {
                throw new KeyNotFoundException("Configuration {0} not found in file {1}.".FormatWith(configurationName, m_fileName));
            }

            return template;
        }

        // return all parsed configurations
        public IEnumerable<TConfiguration> GetAllConfigurations()
        {
            return m_configurationsDictionary.Values.Materialize();
        }

        #endregion public methods

        #region abstract methods

        protected abstract JSONParser<TConfiguration> GetParser();

        #endregion abstract methods
    }

    #endregion ConfigurationStorage
}