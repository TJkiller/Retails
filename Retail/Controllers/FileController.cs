using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

public class FileController : Controller
{
    private readonly ShareServiceClient _shareServiceClient;
    private readonly IConfiguration _configuration;

    public FileController(ShareServiceClient shareServiceClient, IConfiguration configuration)
    {
        _shareServiceClient = shareServiceClient;
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        var shareName = _configuration["AzureStorage:FileShareName"];
        var shareClient = _shareServiceClient.GetShareClient(shareName);
        var directoryClient = shareClient.GetRootDirectoryClient();

        var fileClient = directoryClient.GetFileClient(file.FileName);
        using (var stream = file.OpenReadStream())
        {
            await fileClient.CreateAsync(stream.Length);
            await fileClient.UploadAsync(stream);
        }

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Index()
    {
        var shareName = _configuration["AzureStorage:FileShareName"];
        var shareClient = _shareServiceClient.GetShareClient(shareName);
        var directoryClient = shareClient.GetRootDirectoryClient();

        var fileItems = new List<ShareFileItem>();

        // Retrieve the list of files and directories
        await foreach (var item in directoryClient.GetFilesAndDirectoriesAsync())
        {
            fileItems.Add(item);
        }

        return View(fileItems);
    }
    [HttpPost]
    public async Task<IActionResult> Delete(string fileName)
    {
        var shareName = _configuration["AzureStorage:FileShareName"];
        var shareClient = _shareServiceClient.GetShareClient(shareName);
        var directoryClient = shareClient.GetRootDirectoryClient();
        var fileClient = directoryClient.GetFileClient(fileName);

        if (await fileClient.ExistsAsync())
        {
            await fileClient.DeleteAsync();
        }

        return RedirectToAction("Index");
    }
}
