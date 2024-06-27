using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace SmartLockDoor
{
    public class MQTTService
    {
        private readonly IMqttClient _mqttClient;
        private readonly IConfiguration _configuration;

        public MQTTService(IConfiguration configuration)
        {
            _configuration = configuration;

            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            var broker = _configuration.GetSection("MQTT:Broker").Value;
            var port = int.Parse(_configuration.GetSection("MQTT:Port").Value);
            var username = _configuration.GetSection("MQTT:Username").Value;
            var password = _configuration.GetSection("MQTT:Password").Value;
            var clientId = _configuration.GetSection("MQTT:ClientId").Value;

            var basePath = AppContext.BaseDirectory;
            var relativePath = Path.Combine(basePath, "../../CA/emqxsl-ca.crt");
            var caPath = Path.GetFullPath(relativePath);


            var options = new MqttClientOptionsBuilder()
            .WithTcpServer(broker, port)
            .WithCredentials(username, password)
            .WithClientId(clientId)
            .WithCleanSession()
            .WithTlsOptions(o =>
            {
                o.WithCertificateValidationHandler(_ => true);

                o.WithSslProtocols(SslProtocols.Tls12);

                var certificate = new X509Certificate2(caPath, "");
                o.WithClientCertificates(new List<X509Certificate2> { certificate });

            })
            .Build();

            _mqttClient.ConnectAsync(options, CancellationToken.None).Wait();
        }

        public async Task<MqttClientPublishResult?> PublishMessageAsync(string topic, string payload)
        {
            var message = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(payload)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .WithRetainFlag()
            .Build();

            if (_mqttClient.IsConnected)
            {
                return await _mqttClient.PublishAsync(message);
            } else
            {
                return null;
            }
        }
    }
}
