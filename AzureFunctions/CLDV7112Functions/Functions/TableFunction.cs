using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Text.Json;

namespace CLDV7112Functions.Functions
{
    public class TableFunction
    {
        private readonly string _connectionString;

        public TableFunction()
        {
            _connectionString =
                Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                ?? throw new InvalidOperationException(
                    "AzureWebJobsStorage is not configured.");
        }

        [Function("TableFunction")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")]
            HttpRequestData req)
        {
            try
            {
                string requestBody =
                    await new StreamReader(req.Body).ReadToEndAsync();

                var product =
                    JsonSerializer.Deserialize<ProductRequest>(
                        requestBody,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                if (product == null ||
                    string.IsNullOrWhiteSpace(product.ProductName))
                {
                    var badResponse =
                        req.CreateResponse(HttpStatusCode.BadRequest);

                    await badResponse.WriteStringAsync(
                        "Please provide ProductName, Price and StockQuantity.");

                    return badResponse;
                }

                var tableClient =
                    new TableClient(
                        _connectionString,
                        "Products");

                await tableClient.CreateIfNotExistsAsync();

                var entity = new TableEntity
                {
                    PartitionKey = "Products",
                    RowKey = Guid.NewGuid().ToString(),
                    ["ProductName"] = product.ProductName,
                    ["Price"] = product.Price,
                    ["StockQuantity"] = product.StockQuantity
                };

                await tableClient.AddEntityAsync(entity);

                var response =
                    req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(new
                {
                    message = "Product stored successfully in Azure Table Storage.",
                    table = "Products",
                    product = product.ProductName
                });

                return response;
            }
            catch (Exception ex)
            {
                var response =
                    req.CreateResponse(HttpStatusCode.InternalServerError);

                await response.WriteStringAsync(
                    "Error: " + ex.Message);

                return response;
            }
        }

        public class ProductRequest
        {
            public string ProductName { get; set; } = "";
            public double Price { get; set; }
            public int StockQuantity { get; set; }
        }
    }
}
