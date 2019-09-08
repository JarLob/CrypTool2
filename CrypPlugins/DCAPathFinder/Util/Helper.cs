using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DCAPathFinder.Logic;
using Newtonsoft.Json;

namespace DCAPathFinder.Util
{
    public static class Helper
    {
        /// <summary>
        /// Converts a bool array to string representation
        /// </summary>
        /// <param name="inputArray"></param>
        /// <returns></returns>
        public static string BoolArrayToString(bool[] inputArray)
        {
            string result = "";

            for (int i = (inputArray.Length - 1); i >= 0; i--)
            {
                if (inputArray[i])
                {
                    result += "1";
                }
                else
                {
                    result += "0";
                }
            }

            return result;
        }

        /// <summary>
        /// Deserializes a saved DifferentialAttackRoundConfiguration from disk with a given filename
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        public static DifferentialAttackRoundConfiguration LoadConfigurationFromDisk(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string res = assembly.GetManifestResourceNames().Single(str => str.EndsWith(resourceName));

            DifferentialAttackRoundConfiguration data = null;

            string configurationString;
            using (Stream stream = assembly.GetManifestResourceStream(res))
            using (StreamReader reader = new StreamReader(stream))
            {
                configurationString = reader.ReadToEnd();
            }

            data = JsonConvert.DeserializeObject<DifferentialAttackRoundConfiguration>(configurationString, new Newtonsoft.Json.JsonSerializerSettings
            {
                TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto,
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
            });

            return data;
        }
    }
}
