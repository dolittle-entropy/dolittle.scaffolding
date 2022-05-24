public record KafkaConfiguration(Kafka Kafka);
public record Kafka(string GroupId, string BrokerUrl, string InputTopic, string CommandTopic, string ReceiptsTopic, string ChangeEventsTopic, Ssl Ssl);
public record Ssl(string Authority, string Certificate, string Key);