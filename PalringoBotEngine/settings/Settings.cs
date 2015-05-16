using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PalringoBotEngine.settings
{
    class Settings
    {
        private string JsonString;
        public string JsonEmail { get; set; }
        public string JsonPassword { get; set; }
        public Settings()
        {
            try
            {
                using (StreamReader sr = new StreamReader("../../settings/settings.json"))
                {
                    JsonString = sr.ReadToEnd();
                    String test = JsonString;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }

            dynamic settings = JsonConvert.DeserializeObject(JsonString);

            JsonEmail = settings.settings.email;
            JsonPassword = settings.settings.pass;
        }


    }
}
