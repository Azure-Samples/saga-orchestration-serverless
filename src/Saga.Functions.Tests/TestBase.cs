using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Saga.Functions.Tests
{
    public class TestBase
    {
        public TestBase()
        {
            SetEnvironmentVariables();
        }
        private void SetEnvironmentVariables()
        {
            using (var settingsFile = File.OpenText("launchSettings.json"))
            {
                JsonTextReader jsonReader = new JsonTextReader(settingsFile);
                JObject jsonObject = JObject.Load(jsonReader);

                List<JProperty> envVariables = jsonObject
                  .GetValue("Values")
                  .OfType<JProperty>()
                  .ToList();

                foreach (JProperty property in envVariables)
                {
                    Environment.SetEnvironmentVariable(property.Name, property.Value.ToString());
                }
            }
        }
    }
}
