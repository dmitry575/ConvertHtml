using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace convert
{
    public class JsonData
    {
        public Dictionary<int, string> Tags { get; set; }
        public Dictionary<int, string> Groups { get; set; }
    }

    public class BaseHtmlConvert
    {
        protected const string PrefixTag = "11";

        protected const string GroupPrefixTag = "12";

        /// <summary>
        /// Check and create path
        /// </summary>
        /// <param name="path">Path for creating</param>
        protected void CreatePath(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        /// <summary>
        /// Write data to file
        /// </summary>
        /// <param name="newFile">new file name</param>
        /// <param name="data">Data for saving</param>
        protected void WriteToFile(string newFile, JsonData data)
        {
            try
            {
                WriteToFile(newFile, JsonConvert.SerializeObject(data));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Write data to file
        /// </summary>
        /// <param name="newFile">new file name</param>
        /// <param name="data">Data for saving</param>
        protected void WriteToFile(string newFile, string data)
        {
            try
            {
                File.WriteAllText(newFile, data);
                Console.WriteLine($"json file saved: {newFile}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

}
