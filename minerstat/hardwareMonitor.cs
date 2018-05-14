using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace minerstat
{
    class hardwareMonitor
    {
        public static async void jsonserver()
        {
            try
            {
                // Create a computer instance with all sensors enabled.
                var computer = new Computer()
                {
                    MainboardEnabled = false,
                    CPUEnabled = false,
                    RAMEnabled = false,
                    GPUEnabled = true,
                    FanControllerEnabled = false,
                    HDDEnabled = false,
                };

                // Initialize the sensors.
                computer.Open();
                // Start the HTTP server on a separate thread.
                minerstat.modules.StartServer(computer);

                await Task.Delay(2000);
                Program.NewMessage("MONITOR => Hardware Monitor has been started..", "INFO");
                Program.NewMessage("MONITOR => PORT: " + Program.monitorport, "INFO");

            }
            catch (Exception)
            {

            }
        }

        // This is a copy-paste of the Visitor from OpenHardwareMonitor's GUI project.
        public class Visitor : IVisitor
        {
            public void VisitComputer(IComputer computer)
            {
                computer.Traverse(this);
            }

            public void VisitHardware(IHardware hardware)
            {
                hardware.Update();

                foreach (IHardware subHardware in hardware.SubHardware)
                {
                    subHardware.Accept(this);
                }
            }

            public void VisitParameter(IParameter parameter) { }
            public void VisitSensor(ISensor sensor) { }
        }

        // Json.NET custom converter to output ToString() for the given property.
        public class ToStringJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return true;
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString());
            }
        }

        // Json.NET custom contract resolver. Needed in order to use the custom converter, since the custom converter is
        // intended to be used with annotations, and we can't annotate properties in a compiled library.
        public class CustomContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                JsonProperty property = base.CreateProperty(member, memberSerialization);
                // Don't serialize the Values or Parameters properties. The Values property is an array of all the sensor
                // readings in the past 24 hours, making the JSON size very, very large if enough time has passed. The
                // Parameters property only holds strings on how it calculated the Value property, and can be ignored since
                // it won't ever change.
                if (property.PropertyName == "Values" || property.PropertyName == "Parameters")
                {
                    property.ShouldSerialize = (x) => false;
                }

                return property;
            }

            protected override JsonObjectContract CreateObjectContract(Type objectType)
            {
                JsonObjectContract contract = base.CreateObjectContract(objectType);
                // Use the ToString JSON converter if the type is OpenHardwareMonitor.Hardware.Identifier. Otherwise, the
                // default serialization will serialize it as an object, and since it has no public properties, it will
                // serialize to "Identifier: {}".
                if (objectType == typeof(Identifier))
                {
                    contract.Converter = new ToStringJsonConverter();
                }

                return contract;
            }
        }

    }
}
