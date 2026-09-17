using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Text.Json;

namespace CLDV7112Functions.Functions
{
    public class QueueFunction
    {
        private readonly string _connectionString;

        public QueueFunction()
        {
            _connectionString =
                Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                ?? throw new InvalidOperationException(
                    "AzureWebJobsStorage is not configured.");
        }

        [Function("QueueFunction")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(
                AuthorizationLevel.Function,
                "get",
                "post")]
            HttpRequestData req)
        {
            var queueClient =
                new QueueClient(
                    _connectionString,
                    "order-processing");

            await queueClient.CreateIfNotExistsAsync();

            if (req.Method.Equals(
                "POST",
                StringComparison.OrdinalIgnoreCase))
            {
                return await SendMessage(req, queueClient);
            }

            return await ReadMessages(req, queueClient);
        }

        private async Task<HttpResponseData> SendMessage(
            HttpRequestData req,
            QueueClient queueClient)
        {
            string requestBody =
                await new StreamReader(req.Body).ReadToEndAsync();

            var messageRequest =
                JsonSerializer.Deserialize<MessageRequest>(
                    requestBody,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (messageRequest == null ||
                string.IsNullOrWhiteSpace(messageRequest.Message))
            {
                var badResponse =
                    req.CreateResponse(HttpStatusCode.BadRequest);

                await badResponse.WriteStringAsync(
                    "Please provide a Message.");

                return badResponse;
            }

            await queueClient.SendMessageAsync(
                messageRequest.Message);

            var response =
                req.CreateResponse(HttpStatusCode.OK);

            await response.WriteAsJsonAsync(new
            {
                message = "Message added successfully to Azure Queue Storage.",
                queue = "order-processing",
                content = messageRequest.Message
            });

            return response;
        }

        private async Task<HttpResponseData> ReadMessages(
            HttpRequestData req,
            QueueClient queueClient)
        {
            var messages =
                await queueClient.PeekMessagesAsync(32);

            var results =
                messages.Value
                    .Select(message => message.MessageText)
                    .ToList();

            var response =
                req.CreateResponse(HttpStatusCode.OK);

            await response.WriteAsJsonAsync(new
            {
                message = "Queue messages retrieved successfully.",
                queue = "order-processing",
                messages = results
            });

            return response;
        }

        private class MessageRequest
        {
            public string Message { get; set; } = "";
        }
    }
}
