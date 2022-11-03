#r "Newtonsoft.Json"

using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;


public static async Task<IActionResult> Run(HttpRequest req, ILogger log)
{ 
    // Extract inputs
    string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
    dynamic data = JsonConvert.DeserializeObject(requestBody);
    string managedIdentityType = data?.ManagedIdentityType; 
    string UAMIClientId = data?.UAMIClientId;
    string azureBlobUri = data?.AzureBlobUri;
    string s3BucketName = data?.S3BucketName;
    string AWSRoleArn = data?.AWSRoleArn;

    // Decide to use which credential, SAMI or UAMI
    DefaultAzureCredential azureCredential = null;
    switch(managedIdentityType) 
    {
        case "SAMI":
            azureCredential = new DefaultAzureCredential();
            break;
        case "UAMI":
            azureCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = UAMIClientId });
            break;
        default:
            throw new Exception("Bad request: Invalid ManagedIdentityType");
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
                log.LogInformation("Azure Blob name: {0}", blobItem.Name);
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

    // load oauth token (Jwt)
    log.LogInformation("Assuming AWS role...");
    var accessToken = azureCredential.GetToken(new TokenRequestContext(new[] { UAMIClientId }));
    String OAuthToken = accessToken.Token.ToString();
    // log.LogInformation(OAuthToken);

    // assume aws role via AssumeRoleWithWebIdentity http request
    var builder = new UriBuilder("https://sts.amazonaws.com/");
    var query = HttpUtility.ParseQueryString(builder.Query);
    query["Action"] = "AssumeRoleWithWebIdentity";
    query["Version"] = "2011-06-15";
    query["RoleArn"] = AWSRoleArn;
    query["RoleSessionName"] = "session1";
    query["WebIdentityToken"] = OAuthToken;
    builder.Query = query.ToString();
    string assumeRoleRequestUrl = builder.ToString();
    //log.LogInformation(assumeRoleRequestUrl);

    HttpClient hc = new HttpClient();
    using HttpResponseMessage response = await hc.GetAsync(assumeRoleRequestUrl);
    response.EnsureSuccessStatusCode();
    
    // extract temporary aws credential from response 
    var assumeRoleResponse = await response.Content.ReadAsStringAsync();

    XDocument xdoc = XDocument.Parse(assumeRoleResponse);
    XNamespace ns = "https://sts.amazonaws.com/doc/2011-06-15/";
    var root = xdoc.Root;
    var result = root.Element(ns + "AssumeRoleWithWebIdentityResult");

    var sessionToken = root.Descendants(ns + "SessionToken").Single().Value;
    var accessKeyId = root.Descendants(ns + "AccessKeyId").Single().Value;
    var secretAccessKey = root.Descendants(ns + "SecretAccessKey").Single().Value;

    // get blob items from s3
    var sessionCredentials = new SessionAWSCredentials(accessKeyId,secretAccessKey,sessionToken);

    // Create a client by providing temporary security credentials.
    log.LogInformation("Listing items from Amazon S3...");
    using (IAmazonS3 s3Client = new AmazonS3Client(sessionCredentials, RegionEndpoint.EUWest1))
    {
        var listObjectRequest = new ListObjectsRequest
        {
            BucketName = s3BucketName
        };
        // Send request to Amazon S3.
        ListObjectsResponse s3response = await s3Client.ListObjectsAsync(listObjectRequest);
        foreach (S3Object blobItem in s3response.S3Objects)
        {
            log.LogInformation("S3 Blob name: {0}", blobItem.Key);
        }

    }

 
    return new OkObjectResult(responseMessage);

 
}



