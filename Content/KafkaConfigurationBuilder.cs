using Confluent.Kafka;
using System.Text;

namespace $(solutionName).Messaging
{
    /// <summary>
    /// Needs a reference to package <see cref="Confluent.Kafka"/> from Nuget.org
    /// </summary>
    public static class KafkaConfigurationBuilder
    {
        public static ConsumerConfig BuildSubscriberConfiguration(IConfigurationSection kafka, ILogger log)
        {
            var groupId = EnsureProperGroupId(kafka["GroupId"]!);
            DisplayConfigurationValuesForKafka(kafka, groupId, "Subscriber", log);

            return new ConsumerConfig
            {
                GroupId = groupId,
                EnableAutoOffsetStore = false,
                EnableAutoCommit = true,
                BootstrapServers = kafka["BrokerUrl"],
                AutoOffsetReset = AutoOffsetReset.Earliest,

                SecurityProtocol = SecurityProtocol.Ssl,
                SslCaLocation = kafka.GetSection("Ssl")["Authority"],
                SslCertificateLocation = kafka.GetSection("Ssl")["Certificate"],
                SslKeyLocation = kafka.GetSection("Ssl")["Key"],
            };
        }

        public static ProducerConfig BuildPublisherConfiguration(IConfigurationSection kafka, ILogger log)
        {
            var groupId = EnsureProperGroupId(kafka["GroupId"]!);
            DisplayConfigurationValuesForKafka(kafka, groupId, "Publisher", log);

            return new ProducerConfig
            {
                ClientId = groupId,
                BootstrapServers = kafka["BrokerUrl"],

                SecurityProtocol = SecurityProtocol.Ssl,
                SslCaLocation = kafka.GetSection("Ssl")["Authority"],
                SslCertificateLocation = kafka.GetSection("Ssl")["Certificate"],
                SslKeyLocation = kafka.GetSection("Ssl")["Key"],
                EnableIdempotence = true,

                // Adapt the value to your needs
                QueueBufferingMaxKbytes = 1_000
            };
        }

        static void DisplayConfigurationValuesForKafka(IConfigurationSection kafka, string groupId, string variant, ILogger log)
        {
            var stringBuilder = new StringBuilder(2048);
            stringBuilder.AppendLine($"Kafka {variant} Environment values:");
            stringBuilder.AppendLine("------------------------------------");
            stringBuilder.AppendLine($"Kafka__GroupId: {groupId}");
            stringBuilder.AppendLine($"Kafka__BrokerUrl: {kafka["BrokerUrl"]}");
            stringBuilder.AppendLine($"Kafka__Ssl__Authority: {kafka.GetSection("Ssl")["Authority"]}");
            stringBuilder.AppendLine($"Kafka__Ssl__Certificate: {kafka.GetSection("Ssl")["Certificate"]}");
            stringBuilder.AppendLine($"Kafka__Ssl__Key: {kafka.GetSection("Ssl")["Key"]}");
            stringBuilder.AppendLine($"Kafka__Ssl__Key: {kafka.GetSection("Ssl")["Key"]}");

            stringBuilder.AppendLine($"Kafka__InputTopic   : {kafka["InputTopic"]}");
            stringBuilder.AppendLine($"Kafka__CommandTopic : {kafka["CommandTopic"]}");
            stringBuilder.AppendLine($"Kafka__ReceiptsTopic: {kafka["ReceiptsTopic"]}");

            if (File.Exists(kafka.GetSection("Ssl")["Authority"]))
            {
                stringBuilder.AppendLine($"PEM file found: YES");
            }
            else
            {
                stringBuilder.AppendLine($"PEM file found: NO");
            }

            if (File.Exists(kafka.GetSection("Ssl")["Certificate"]))
            {
                stringBuilder.AppendLine($"Certificate file found: YES");
            }
            else
            {
                stringBuilder.AppendLine($"Certificate file found: NO");
            }

            if (File.Exists(kafka.GetSection("Ssl")["Key"]))
            {
                stringBuilder.AppendLine($"Key file found: YES");
            }
            else
            {
                stringBuilder.AppendLine($"Key file found: NO");
            }
            stringBuilder.AppendLine("------------------------------------");

            log.LogInformation(stringBuilder.ToString());
        }

        /// <summary>
        /// It is recommended that each microservice identifies itself by name by applying an 
        /// ENVIRONMENT variable to your launchsettings file named "KAFKA__GROUP" 
        /// This will expand your group Id to look something like: 
        ///     solutionName-dev-jonnydangerous-orders         
        /// </summary>
        /// <param name="groupId">Typically, the current microservice name</param>
        /// <returns>groupid with microservice name appended, or just the groupId if the ENV variable was not set</returns>
        static string EnsureProperGroupId(string groupId)
        {
            if (Environment.GetEnvironmentVariable("KAFKA__GROUP") is { } groupIdAdd)
            {
                groupId += $"-{groupIdAdd}";
            }
            return groupId;
        }
    }
}
