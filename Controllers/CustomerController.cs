using CLDV7112_Project1.Models;
using CLDV7112_Project1.Services;
using Microsoft.AspNetCore.Mvc;

namespace CLDV7112_Project1.Controllers
{
    public class CustomerController : Controller
    {
        private readonly AzureStorageService _storage;

        public CustomerController(AzureStorageService storage)
        {
            _storage = storage;
        }

        public async Task<IActionResult> Index()
        {
            var customers =
                await _storage.GetCustomersAsync();

            return View(customers);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            customer.PartitionKey = "Customers";
            customer.RowKey = Guid.NewGuid().ToString();

            await _storage.AddCustomerAsync(customer);

            return RedirectToAction(nameof(Index));
        }
    }
}
