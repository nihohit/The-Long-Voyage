using System;
using System.Collections.Generic;

namespace Assets.Scripts.Base
{
    public enum FileAccessor { Units, TerrainGeneration, General }

    /// <summary>
    /// This class handles simple key:value predefined configuration files, for known primitives.
    /// </summary>
    public static class SimpleConfigurationHandler
    {
        #region static dictionaries

        private static readonly Dictionary<string, Dictionary<string, string>> s_navigator = new Dictionary<string, Dictionary<string, string>> { };

        #endregion static dictionaries

        #region public methods

        public static void Init()
        {
            if (s_navigator.Count == 0)
            {
                ReadFiles(Enum.GetNames(typeof(FileAccessor)));
            }
        }

        public static UInt16 GetUintProperty(string str, FileAccessor access)
        {
            return Convert.ToUInt16(GetStringProperty(str, access));
        }

        public static object GetStringProperty(string str, FileAccessor access)
        {
            return s_navigator.Get(access.ToString(), "File Handler navigator").Get(str, "{0} dictionary".FormatWith(access));
        }

        public static Int32 GetIntProperty(string str, FileAccessor access)
        {
            return Convert.ToInt32(GetStringProperty(str, access));
        }

        public static float GetFloatProperty(string str, FileAccessor access)
        {
            return Convert.ToSingle(GetStringProperty(str, access));
        }

        #endregion public methods

        #region private methods

        private static void ReadFromFile(string str)
        {
            var dict = new Dictionary<string, string>();
            s_navigator.Add(str, dict);
            char[] delimiters = { '=' };
            string[] text = System.IO.File.ReadAllLines("config/{0}.ini".FormatWith(str));
            foreach (string entry in text)
            {
                string[] temp = entry.Split(delimiters);
                dict.Add(temp[0], temp[1]);
            }
        }

        private static void ReadFiles(string[] files)
        {
            foreach (var file in files)
            {
                ReadFromFile(file);
            }
        }

        #endregion private methods
    }
}