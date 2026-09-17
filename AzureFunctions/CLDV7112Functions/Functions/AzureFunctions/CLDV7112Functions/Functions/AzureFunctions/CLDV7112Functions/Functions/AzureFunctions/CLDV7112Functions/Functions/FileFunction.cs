using Azure.Storage.Files.Shares;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace CLDV7112Functions.Functions
{
    public class FileFunction
    {
        private readonly string _connectionString;

        public FileFunction()
        {
            _connectionString =
                Environment.GetEnvironmentVariable("AzureWebJobsStorage")
                ?? throw new InvalidOperationException(
                    "AzureWebJobsStorage is not configured.");
        }

        [Function("FileFunction")]
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

                var shareClient =
                    new ShareClient(
                        _connectionString,
                        "logs");

                await shareClient.CreateIfNotExistsAsync();

                var directoryClient =
                    shareClient.GetRootDirectoryClient();

                var fileClient =
                    directoryClient.GetFileClient(fileName);

                using var memoryStream =
                    new MemoryStream();

                await req.Body.CopyToAsync(memoryStream);

                memoryStream.Position = 0;

                await fileClient.CreateAsync(
                    memoryStream.Length);

                memoryStream.Position = 0;

                await fileClient.UploadRangeAsync(
                    new Azure.HttpRange(
                        0,
                        memoryStream.Length),
                    memoryStream);

                var response =
                    req.CreateResponse(HttpStatusCode.OK);

                await response.WriteAsJsonAsync(new
                {
                    message = "File uploaded successfully to Azure Files.",
                    share = "logs",
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
