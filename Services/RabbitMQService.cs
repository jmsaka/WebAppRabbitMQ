using System.Text.Json.Serialization;

namespace WebAppRabbitMQ.Services;

public class RabbitMQService
{
    private readonly IConfiguration _configuration;
    private IConnection _connection;
    private readonly HttpClient _httpClient;

    public RabbitMQService(IConfiguration configuration, HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        Connect();
    }

    private void Connect()
    {
        var factory = new ConnectionFactory()
        {
            HostName = _configuration["RabbitMQ:HostName"],
            UserName = _configuration["RabbitMQ:UserName"],
            Password = _configuration["RabbitMQ:Password"],
            Port = int.Parse(_configuration["RabbitMQ:Port"])
        };

        _connection = factory.CreateConnection();
    }

    public string Status()
    {
        return  _connection.IsOpen.ToString();
    }

    public async Task<List<string>> ListQueuesAsync()
    {
        var managementUri = $"http://{_configuration["RabbitMQ:HostName"]}:8080/api/queues";

        // Autenticação básica para acessar a API de gerenciamento
        var byteArray = Encoding.ASCII.GetBytes($"{_configuration["RabbitMQ:UserName"]}:{_configuration["RabbitMQ:Password"]}");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

        var response = await _httpClient.GetAsync(managementUri);
        response.EnsureSuccessStatusCode();

        var jsonString = await response.Content.ReadAsStringAsync();
        List<QueueInfo>? queues = JsonSerializer.Deserialize<List<QueueInfo>>(jsonString);

        return queues.Select(q => q.Name).ToList();
    }

    public void CreateQueue(string queueName)
    {
        using (var channel = _connection.CreateModel())
        {
            channel.QueueDeclare(queue: queueName,
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }
    }

    public void PublishMessage(string queueName, string message)
    {
        using (var channel = _connection.CreateModel())
        {
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: "",
                                 routingKey: queueName,
                                 basicProperties: null,
                                 body: body);
        }
    }

    public string ConsumeMessage(string queueName)
    {
        using (var channel = _connection.CreateModel())
        {
            var consumer = new EventingBasicConsumer(channel);
            string message = null;

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                message = Encoding.UTF8.GetString(body);
            };

            channel.BasicConsume(queue: queueName,
                                 autoAck: true,
                                 consumer: consumer);

            return message;
        }
    }

    public void DeleteQueue(string queueName)
    {
        using (var channel = _connection.CreateModel())
        {
            channel.QueueDelete(queue: queueName);
        }
    }

}

public class Arguments
{
}

public class EffectivePolicyDefinition
{
}

public class MessagesDetails
{
    [JsonPropertyName("rate")]
    public double? Rate { get; set; }
}

public class MessagesReadyDetails
{
    [JsonPropertyName("rate")]
    public double? Rate { get; set; }
}

public class MessagesUnacknowledgedDetails
{
    [JsonPropertyName("rate")]
    public double? Rate { get; set; }
}

public class ReductionsDetails
{
    [JsonPropertyName("rate")]
    public double? Rate { get; set; }
}

public class QueueInfo
{
    [JsonPropertyName("arguments")]
    public Arguments Arguments { get; set; }

    [JsonPropertyName("auto_delete")]
    public bool? AutoDelete { get; set; }

    [JsonPropertyName("consumer_capacity")]
    public int? ConsumerCapacity { get; set; }

    [JsonPropertyName("consumer_utilisation")]
    public int? ConsumerUtilisation { get; set; }

    [JsonPropertyName("consumers")]
    public int? Consumers { get; set; }

    [JsonPropertyName("durable")]
    public bool? Durable { get; set; }

    [JsonPropertyName("effective_policy_definition")]
    public EffectivePolicyDefinition EffectivePolicyDefinition { get; set; }

    [JsonPropertyName("exclusive")]
    public bool? Exclusive { get; set; }

    [JsonPropertyName("memory")]
    public int? Memory { get; set; }

    [JsonPropertyName("message_bytes")]
    public int? MessageBytes { get; set; }

    [JsonPropertyName("message_bytes_paged_out")]
    public int? MessageBytesPagedOut { get; set; }

    [JsonPropertyName("message_bytes_persistent")]
    public int? MessageBytesPersistent { get; set; }

    [JsonPropertyName("message_bytes_ram")]
    public int? MessageBytesRam { get; set; }

    [JsonPropertyName("message_bytes_ready")]
    public int? MessageBytesReady { get; set; }

    [JsonPropertyName("message_bytes_unacknowledged")]
    public int? MessageBytesUnacknowledged { get; set; }

    [JsonPropertyName("messages")]
    public int? Messages { get; set; }

    [JsonPropertyName("messages_details")]
    public MessagesDetails MessagesDetails { get; set; }

    [JsonPropertyName("messages_paged_out")]
    public int? MessagesPagedOut { get; set; }

    [JsonPropertyName("messages_persistent")]
    public int? MessagesPersistent { get; set; }

    [JsonPropertyName("messages_ram")]
    public int? MessagesRam { get; set; }

    [JsonPropertyName("messages_ready")]
    public int? MessagesReady { get; set; }

    [JsonPropertyName("messages_ready_details")]
    public MessagesReadyDetails MessagesReadyDetails { get; set; }

    [JsonPropertyName("messages_ready_ram")]
    public int? MessagesReadyRam { get; set; }

    [JsonPropertyName("messages_unacknowledged")]
    public int? MessagesUnacknowledged { get; set; }

    [JsonPropertyName("messages_unacknowledged_details")]
    public MessagesUnacknowledgedDetails MessagesUnacknowledgedDetails { get; set; }

    [JsonPropertyName("messages_unacknowledged_ram")]
    public int? MessagesUnacknowledgedRam { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("node")]
    public string Node { get; set; }

    [JsonPropertyName("reductions")]
    public int? Reductions { get; set; }

    [JsonPropertyName("reductions_details")]
    public ReductionsDetails ReductionsDetails { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("storage_version")]
    public int? StorageVersion { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("vhost")]
    public string Vhost { get; set; }
}

