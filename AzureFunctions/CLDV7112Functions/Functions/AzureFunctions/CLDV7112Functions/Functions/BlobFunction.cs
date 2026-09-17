using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace CLDV7112Functions.Functions
{
    public class BlobFunction
    {
        private readonly string _connectionString;

        public BlobFunction()
        {
            _connectionString =
                Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                ?? throw new InvalidOperationException(
                    "AzureWebJobsStorage is not configured.");
        }

        [Function("BlobFunction")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")]
            HttpRequestData req)
        {
            try
            {
                string? fileName =
                    System.Web.HttpUtility.ParseQueryString(
                        req.Url.Query)["fileName"];

                if (string.IsNullOrWhiteSpace(fileName))
                {
                    var badResponse =
                        req.CreateResponse(HttpStatusCode.BadRequest);

                    await badResponse.WriteStringAsync(
                        "Please provide a fileName query parameter.");

                    return badResponse;
                }

                var containerClient =
                    new BlobContainerClient(
                        _connectionString,
                        "product-images");

                await containerClient.CreateIfNotExistsAsync();

                var blobClient =
                    containerClient.GetBlobClient(fileName);

                await blobClient.UploadAsync(
                    req.Body,
                    overwrite: true);

                var response =
                    req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(new
                {
                    message = "File uploaded successfully to Azure Blob Storage.",
                    container = "product-images",
                    fileName = fileName
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
    }
}
