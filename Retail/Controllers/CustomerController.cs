using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using Retail.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Retail.Controllers
{
    public class CustomerController : Controller
    {
        private readonly TableServiceClient _tableServiceClient;

        public CustomerController(TableServiceClient tableServiceClient)
        {
            _tableServiceClient = tableServiceClient;
        }

        public async Task<IActionResult> Index()
        {
            var tableClient = _tableServiceClient.GetTableClient("CustomerProfiles");

            // Use async enumeration for querying
            var entities = new List<CustomerProfile>();
            await foreach (var entity in tableClient.QueryAsync<CustomerProfile>())
            {
                entities.Add(entity);
            }

            return View(entities);
        }

        // GET: /Customer/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Customer/Create
        [HttpPost]
        public async Task<IActionResult> Create(CustomerProfile customer)
        {
            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            var tableClient = _tableServiceClient.GetTableClient("CustomerProfiles");

            try
            {
                // Ensure PartitionKey and RowKey are set
                customer.PartitionKey = "CustomerPartition"; // Or another appropriate value
                customer.RowKey = Guid.NewGuid().ToString();  // Unique identifier for each entity

                await tableClient.AddEntityAsync(customer);
            }
            catch (Exception ex)
            {
                // Log the exception and show an error message
                ModelState.AddModelError("", $"Error creating customer: {ex.Message}");
                return View(customer);
            }

            return RedirectToAction("Index");
        }

        // GET: /Customer/Edit/{partitionKey}/{rowKey}
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var tableClient = _tableServiceClient.GetTableClient("CustomerProfiles");

            try
            {
                var entity = await tableClient.GetEntityAsync<CustomerProfile>(partitionKey, rowKey);
                return View(entity.Value);
            }
            catch (Exception ex)
            {
                // Log the exception and show an error message
                ModelState.AddModelError("", $"Error retrieving customer: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        // POST: /Customer/Edit
        [HttpPost]
        // POST: /Customer/Edit
        [HttpPost]
        public async Task<IActionResult> Edit(CustomerProfile customer)
        {
            if (!ModelState.IsValid)
            {
                return View(customer);
            }

            var tableClient = _tableServiceClient.GetTableClient("CustomerProfiles");

            try
            {
                // Ensure ETag is not default for concurrency checks
                if (customer.ETag == default)
                {
                    ModelState.AddModelError("", "Customer ETag is required for updating.");
                    return View(customer);
                }

                // Update entity
                await tableClient.UpdateEntityAsync(customer, customer.ETag, TableUpdateMode.Replace);
            }
            catch (Exception ex)
            {
                // Log the exception and show an error message
                ModelState.AddModelError("", $"Error updating customer: {ex.Message}");
                return View(customer);
            }

            return RedirectToAction("Index");
        }


        // GET: /Customer/Delete/{partitionKey}/{rowKey}
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            var tableClient = _tableServiceClient.GetTableClient("CustomerProfiles");

            try
            {
                // Delete the entity
                await tableClient.DeleteEntityAsync(partitionKey, rowKey);
            }
            catch (Exception ex)
            {
                // Log the exception and show an error message
                ModelState.AddModelError("", $"Error deleting customer: {ex.Message}");
            }

            return RedirectToAction("Index");
        }
    }
}
