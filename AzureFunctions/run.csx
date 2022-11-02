#r "Newtonsoft.Json"

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;


public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{ 
    // Extract inputs
    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
    dynamic data = JsonConvert.DeserializeObject(requestBody);
    string managedIdentityType = data?.ManagedIdentityType; 
    string azureBlobUri = data?.AzureBlobUri;

    // Decide to use which credential, SAMI or UAMI
    TokenCredential azureCredential = null;
    switch(managedIdentityType) 
    {
        case "SAMI":
            azureCredential = new ManagedIdentityCredential();
            break;
        case "UAMI":
            string UAMIClientId = data?.UAMIClientId;
            azureCredential = new ManagedIdentityCredential(UAMIClientId);
            break;
        default:
            throw new Exception("Bad request: Invalid ManagedIdentityType");
            break;
    }


    // Load blob items from Azure Storage account 
    log.LogInformation("Listing items from Azure...");

    //var azureCredential = new ManagedIdentityCredential();
    var blobContainerClient = new BlobContainerClient(new Uri(azureBlobUri), azureCredential); 

    string responseMessage = "";
    try
    {
        // Call the listing operation and return pages of the specified size.
        var resultSegment = blobContainerClient.GetBlobsAsync()
            .AsPages(default, 10); 

        // Enumerate the blobs returned for each page.
        await foreach (Azure.Page<BlobItem> blobPage in resultSegment)
        {
            foreach (BlobItem blobItem in blobPage.Values)
            {
                log.LogInformation("Blob name: {0}", blobItem.Name);
            }

            log.LogInformation("");
        }
    }
    catch (RequestFailedException e)
    {
        log.LogInformation(e.Message);
        log.LogInformation("");
        throw;
    }

    // Load blob items from AWS S3

    return new OkObjectResult(responseMessage);

 
}



