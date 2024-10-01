using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Retail.Controllers
{
    public class OrderController : Controller
    {
        private readonly QueueServiceClient _queueServiceClient;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;
        private readonly string _queueName;

        public OrderController(QueueServiceClient queueServiceClient, BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            _queueServiceClient = queueServiceClient;
            _blobServiceClient = blobServiceClient;
            _configuration = configuration;
            _queueName = _configuration["AzureStorage:QueueName"];
        }

        [HttpPost]
        public async Task<IActionResult> ProcessOrder(string orderId, IFormFile image)
        {
            var queueClient = _queueServiceClient.GetQueueClient(_queueName);
            var containerClient = _blobServiceClient.GetBlobContainerClient(_configuration["AzureStorage:BlobContainer"]);

            if (!string.IsNullOrEmpty(orderId))
            {
                await queueClient.SendMessageAsync($"Processing order: {orderId}");
            }

            if (image != null && image.Length > 0)
            {
                var imageName = Path.GetFileName(image.FileName);
                var blobClient = containerClient.GetBlobClient(imageName);

                using (var stream = image.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, true);  // Overwrite if the image exists
                }

                await queueClient.SendMessageAsync($"Uploaded image: {imageName}, Added to products");
            }

            TempData["Message"] = "Order processed and image added to products.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            var messages = await GetQueueMessages();
            ViewBag.StatusMessage = TempData["Message"];
            return View(messages);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string messageId, string popReceipt)
        {
            var queueClient = _queueServiceClient.GetQueueClient(_queueName);

            // Delete the message from the queue using messageId and popReceipt
            await queueClient.DeleteMessageAsync(messageId, popReceipt);

            // TempData to show success message after redirect
            TempData["Message"] = "Message deleted successfully.";

            // Redirect back to Index after deleting the message
            return RedirectToAction("Index");
        }

        private async Task<List<QueueMessage>> GetQueueMessages()
        {
            var queueClient = _queueServiceClient.GetQueueClient(_queueName);
            var response = await queueClient.ReceiveMessagesAsync(maxMessages: 10);
            return new List<QueueMessage>(response.Value);
        }
    }
}
