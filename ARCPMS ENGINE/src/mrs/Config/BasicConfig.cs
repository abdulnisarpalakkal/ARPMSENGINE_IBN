using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Xml;
using System.IO; 
using System.Configuration;

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace ARCPMS_ENGINE.src.mrs.Config
{
    class BasicConfig
    {

        /// <summary>
        /// It will copy all the contents of a object to a new object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T Clone<T>(T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }
        /// <summary>
        /// read all from app.config
        /// </summary>
        public static void ReadAllApSettings()
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;

                if (appSettings.Count == 0)
                {
                    Console.WriteLine("AppSettings is empty.");
                }
                else
                {
                    foreach (var key in appSettings.AllKeys)
                    {
                        Console.WriteLine("Key: {0} Value: {1}", key, appSettings[key]);
                    }
                }
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error reading app settings");
            }
        }

        /// <summary>
        /// read value from app.config using key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string ReadApSetting(string key)
        {
            string result = null;
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                result = appSettings[key] ?? "Not Found";
                
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error reading app settings");
            }
            return result;
        }

        /// <summary>
        /// update value using key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void AddUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
        }

        /// <summary>
        /// get directory path of application
        /// </summary>
        /// <returns></returns>
        public static string GetApplicationLocation()
        {
            return AppDomain.CurrentDomain.BaseDirectory;

        }

        /// <summary>
        /// read content from xml using tag
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetXmlTextOfTag(string tagName, string filePath = "ibm_config\\config.xml")
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlNodeList xmlnode;
           
            string tagText = null;
            string appLocation = GetApplicationLocation();
            using (FileStream fs = new FileStream(appLocation + "\\" + filePath, FileMode.Open, FileAccess.Read))
            {
                xmldoc.Load(fs);
                xmlnode = xmldoc.GetElementsByTagName(tagName);
                tagText = xmlnode.Item(0).InnerText.Trim();
            }
            return tagText;
        }
        /// <summary>
        /// write content to xml using tag
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static void SetXmlTextOfTag(string tagName, string tagText, string filePath = "ibm_config\\config.xml" )
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlNodeList xmlnode;

            
            string appLocation = GetApplicationLocation();
            using (FileStream fs = new FileStream(appLocation + "\\" + filePath, FileMode.Open, FileAccess.ReadWrite))
            {
                xmldoc.Load(fs);
                xmlnode = xmldoc.GetElementsByTagName(tagName);
                xmlnode.Item(0).InnerText = tagText;
                //xmldoc.Save(appLocation + "\\" + filePath);
            }
            xmldoc.Save(appLocation + "\\" + filePath);
        }

        /// <summary>
        /// read attribute value from xml using other attribute parameters
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="tagName"></param>
        /// <param name="refAttributeName"></param>
        /// <param name="refAttributeVal"></param>
        /// <param name="retAttributeName"></param>
        /// <returns></returns>
        public static string GetXmlAttributeValueOfTag(string filePath, string tagName, string refAttributeName, string refAttributeVal,string retAttributeName)
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlNodeList xmlnode;

            string retAttributeVal = null;
            string appLocation = GetApplicationLocation();
            using (FileStream fs = new FileStream(appLocation + "\\" + filePath, FileMode.Open, FileAccess.Read))
            {
                xmldoc.Load(fs);
                xmlnode = xmldoc.GetElementsByTagName(tagName);
                //tagText = xmlnode.Item(0).Attributes.GetNamedItem(,);

                for (int i = 0; i < xmlnode.Count; i++)
                {
                    if (xmlnode[i].Attributes[refAttributeName].Value.Equals(refAttributeVal))
                    {
                        retAttributeVal = xmlnode[i].Attributes[retAttributeName].Value;
                        break;
                    }
                } 
            }
            return retAttributeVal;
        }

        /// <summary>
        /// read all attribute vlues from xml using tag
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="tagName"></param>
        /// <param name="refAttributeName"></param>
        /// <returns></returns>
        public static List<string> GetAllXmlAttributeValuesOfTag(string filePath, string tagName, string refAttributeName)
        {
            XmlDocument xmldoc = new XmlDocument();
            XmlNodeList xmlnode;
            List<string> eesCameras = null;
            eesCameras = new List<string>();

            string retAttributeVal = null;
            string appLocation = GetApplicationLocation();
            using (FileStream fs = new FileStream(appLocation + "\\" + filePath, FileMode.Open, FileAccess.Read))
            {
                xmldoc.Load(fs);
                xmlnode = xmldoc.GetElementsByTagName(tagName);
                //tagText = xmlnode.Item(0).Attributes.GetNamedItem(,);

                for (int i = 0; i < xmlnode.Count; i++)
                {
                    eesCameras.Add(xmlnode[i].Attributes[refAttributeName].Value);
                    
                }
            }
            return eesCameras;
        }
        
    }
}
