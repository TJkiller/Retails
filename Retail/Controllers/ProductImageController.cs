using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Retail.Controllers
{
    public class ProductImageController : Controller
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IConfiguration _configuration;

        public ProductImageController(BlobServiceClient blobServiceClient, IConfiguration configuration)
        {
            _blobServiceClient = blobServiceClient;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                // Handle the case where no file is provided
                return BadRequest("No file uploaded.");
            }

            var containerName = _configuration["AzureStorage:BlobContainer"];
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            // Create the container if it does not exist
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(file.FileName);
            await blobClient.UploadAsync(file.OpenReadStream(), true); // Overwrite if exists

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            var containerName = _configuration["AzureStorage:BlobContainer"];
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            // Retrieve the list of blobs in the container
            var blobItems = containerClient.GetBlobsAsync();
            var blobList = new List<BlobItem>();

            await foreach (var blobItem in blobItems)
            {
                blobList.Add(blobItem);
            }

            return View(blobList);
        }
        public async Task<IActionResult> Download(string name)
        {
            var containerName = _configuration["AzureStorage:BlobContainer"];
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(name);

            if (await blobClient.ExistsAsync())
            {
                var memoryStream = new MemoryStream();
                await blobClient.DownloadToAsync(memoryStream);
                memoryStream.Position = 0;

                return File(memoryStream, "application/octet-stream", name);
            }
            else
            {
                return NotFound();
            }
        }
        [HttpPost]
        public async Task<IActionResult> Delete(string name)
        {
            var containerName = _configuration["AzureStorage:BlobContainer"];
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(name);

            if (await blobClient.ExistsAsync())
            {
                await blobClient.DeleteAsync();
            }

            return RedirectToAction("Index");
        }
    }
}
