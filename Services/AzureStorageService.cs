using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Files.Shares;
using CLDV7112_Project1.Models;

namespace CLDV7112_Project1.Services
{
    public class AzureStorageService
    {
        private readonly string _connectionString;

        private readonly TableClient _customerTable;
        private readonly TableClient _productTable;

        private readonly BlobContainerClient _blobContainer;
        private readonly QueueClient _queueClient;
        private readonly ShareClient _shareClient;

        public AzureStorageService(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("AzureStorage")
                ?? "";

            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                throw new InvalidOperationException(
                    "Azure Storage connection string is not configured.");
            }

            _customerTable =
                new TableClient(
                    _connectionString,
                    "Customers");

            _productTable =
                new TableClient(
                    _connectionString,
                    "Products");

            _blobContainer =
                new BlobContainerClient(
                    _connectionString,
                    "product-images");

            _queueClient =
                new QueueClient(
                    _connectionString,
                    "order-processing");

            _shareClient =
                new ShareClient(
                    _connectionString,
                    "logs");
        }

        // ==========================================
        // CUSTOMER TABLE STORAGE
        // ==========================================

        public async Task AddCustomerAsync(Customer customer)
        {
            await _customerTable.CreateIfNotExistsAsync();

            await _customerTable.AddEntityAsync(customer);
        }

        public async Task<List<Customer>> GetCustomersAsync()
        {
            await _customerTable.CreateIfNotExistsAsync();

            var customers = new List<Customer>();

            await foreach (
                Customer customer
                in _customerTable.QueryAsync<Customer>())
            {
                customers.Add(customer);
            }

            return customers;
        }

        // ==========================================
        // PRODUCT TABLE STORAGE
        // ==========================================

        public async Task AddProductAsync(Product product)
        {
            await _productTable.CreateIfNotExistsAsync();

            await _productTable.AddEntityAsync(product);
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            await _productTable.CreateIfNotExistsAsync();

            var products = new List<Product>();

            await foreach (
                Product product
                in _productTable.QueryAsync<Product>())
            {
                products.Add(product);
            }

            return products;
        }

        // ==========================================
        // BLOB STORAGE
        // ==========================================

        public async Task UploadBlobAsync(
            Stream stream,
            string fileName)
        {
            await _blobContainer.CreateIfNotExistsAsync();

            var blobClient =
                _blobContainer.GetBlobClient(fileName);

            await blobClient.UploadAsync(
                stream,
                overwrite: true);
        }

        public async Task<List<string>> GetBlobNamesAsync()
        {
            await _blobContainer.CreateIfNotExistsAsync();

            var files = new List<string>();

            await foreach (
                var blobItem
                in _blobContainer.GetBlobsAsync())
            {
                files.Add(blobItem.Name);
            }

            return files;
        }

        // ==========================================
        // QUEUE STORAGE
        // ==========================================

        public async Task AddQueueMessageAsync(
            string message)
        {
            await _queueClient.CreateIfNotExistsAsync();

            await _queueClient.SendMessageAsync(message);
        }

        public async Task<List<string>> GetQueueMessagesAsync()
        {
            await _queueClient.CreateIfNotExistsAsync();

            var messages = new List<string>();

            var response =
                await _queueClient.PeekMessagesAsync(32);

            foreach (var message in response.Value)
            {
                messages.Add(message.MessageText);
            }

            return messages;
        }

        // ==========================================
        // AZURE FILE STORAGE
        // ==========================================

        public async Task UploadFileAsync(
            Stream stream,
            string fileName)
        {
            await _shareClient.CreateIfNotExistsAsync();

            var directory =
                _shareClient.GetRootDirectoryClient();

            var file =
                directory.GetFileClient(fileName);

            await file.CreateAsync(stream.Length);

            await file.UploadAsync(stream);
        }

        public async Task<List<string>> GetFileNamesAsync()
        {
            await _shareClient.CreateIfNotExistsAsync();

            var directory =
                _shareClient.GetRootDirectoryClient();

            var files = new List<string>();

            await foreach (
                var item
                in directory.GetFilesAndDirectoriesAsync())
            {
                if (!item.IsDirectory)
                {
                    files.Add(item.Name);
                }
            }

            return files;
        }
    }
}

