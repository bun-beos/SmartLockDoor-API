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

            var options = new MqttClientOptionsBuilder()
            .WithTcpServer(broker, port)
            .WithCredentials(username, password)
            .WithClientId(clientId)
            .WithCleanSession()
            .Build();

            _mqttClient.ConnectAsync(options, CancellationToken.None).Wait();
        }

        public async Task<MqttClientPublishResult?> PublishMessageAsync(MqttPublishRequest mqttPublishRequest)
        {
            var message = new MqttApplicationMessageBuilder()
                    .WithTopic(mqttPublishRequest.Topic)
                    .WithPayload(mqttPublishRequest.Payload)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                    .WithRetainFlag()
                    .Build();

            if (_mqttClient.IsConnected)
            {
                return await _mqttClient.PublishAsync(message);
            }
            else
            {
                return null;
            }
        }

        public async Task<Boolean> PublishManyAsync(List<MqttPublishRequest> mqttPublishRequests)
        {
            foreach (var mqttPublishRequest in mqttPublishRequests)
            {
                var message = new MqttApplicationMessageBuilder()
                    .WithTopic(mqttPublishRequest.Topic)
                    .WithPayload(mqttPublishRequest.Payload)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                    .WithRetainFlag()
                    .Build();

                await _mqttClient.PublishAsync(message);
            }
            if (_mqttClient.IsConnected)
            {
                return true;
            }
            else return false;
        }
    }
}
