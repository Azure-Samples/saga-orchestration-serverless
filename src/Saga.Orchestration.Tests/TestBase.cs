using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Saga.Common.Messaging;
using Saga.Common.Commands;
using Saga.Common.Processors;

namespace Saga.Orchestration.Tests
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

        protected ICommandContainer DeserializeEventData(EventData eventData)
        {
            var jsonBody = Encoding.UTF8.GetString(eventData.Body);
            var jObject = JsonConvert.DeserializeObject<JObject>(jsonBody);
            return new CommandContainer(jObject);
        }

        protected MessageHeader GetCommandHeader(ICommandContainer commandContainer)
        {
            var defaultCommand = commandContainer.ParseCommand<DefaultCommand>();
            return defaultCommand.Header;
        }
    }
}
