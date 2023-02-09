# Secretless access Azure and AWS resources with Azure managed identity

## User Case
As a security best practice of developing cloud native applications, developers utilize the cloud-native IAM (identity and access management) mechanism to manage resource access, rather than managing credentials such as secrets and keys themselves. The cloud-native mechanism in AWS is AWS IAM and in Azure it is often Azure Managed Identity. 

AWS IAM and Azure Managed Identity works great in their own platform, but there are cross cloud cases: A workload in one cloud need to access resources in another cloud. For example, an Azure workload such as Azure Function or App Service that store data to Azure storage account but also save the data to an AWS S3 bucket as across cloud backup solution.

The Azure function is utilizing manage identity for access azure storage account. For accessing s3, the developer could create an IAM user and store the IAM user credentials, but there is a better way to implement a secretless access both Azure and AWS resource, with the very same azure managed identity.

## This Project
This project demonstrates a secretless approach to use one Azure managed identity (either User-Assigned managed identity (UAMI) or System-Assigned Managed Identity (SAMI)), for reading the objects from both Azure storage account and AWS S3 bucket. The same managed identity works in both cloud, without managing any secrets. 

## Architecture 
![Architecture](./Doc/Secretless-cross-cloud-access-diagram.png?raw=true "Architecture")

## Screenshot
![Screenshot](./Doc/screenshot-azurefunction-access-azure-and-aws.png?raw=true "Screenshot")

## Steps:

1. Create 2 Azure Functions (.net)
    1. *CrossCloudAccessFunction-SAMI*
    2. *CrossCloudAccessFunction-UAMI*
2. Create 1 UAMI: *UAMI-CrossCloudAccess-Identity*
3. Assign a managed identity to the function
    1. CrossCloudAccessFunction-UAMI: using User-Assigned managed identity (UAMI) - Do both following steps
        1. Assign UAMI to Azure Function, note the UAMI client Id
        2. In the azure function code, use var myCredential = new ManagedIdentityCredential("UAMI_CLIENT_ID");
    2. CrossCloudAccessFunction-SAMI: using System-Assigned Managed Identity (SAMI) - Do both following steps
        1. Enable SAMI in Azure Function
        2. In the azure function code, use var var myCredential = new DefaultAzureCredential(); OR var myCredential = new ManagedIdentityCredential();
    3. Have both (multiple) UAMI and SAMI assigned to the Azure Function at the same time (Need to double check)
        1. it is possilble to have multiple UAMI for one Azure Function. Depends on which crediental (UAMI) you are using in the code, you will have different permission to access different resources. 
        Depends on what credential you create in the code.
        2. Note: Using DefaultAzureCredential class implies using the order
            1. [EnvironmentCredential](https://learn.microsoft.com/en-us/dotnet/api/azure.identity.environmentcredential?view=azure-dotnet)
            2. [ManagedIdentityCredential](https://learn.microsoft.com/en-us/dotnet/api/azure.identity.managedidentitycredential?view=azure-dotnet)
            3. [SharedTokenCacheCredential](https://learn.microsoft.com/en-us/dotnet/api/azure.identity.sharedtokencachecredential?view=azure-dotnet)
            4. [VisualStudioCredential](https://learn.microsoft.com/en-us/dotnet/api/azure.identity.visualstudiocredential?view=azure-dotnet)
            5. [VisualStudioCodeCredential](https://learn.microsoft.com/en-us/dotnet/api/azure.identity.visualstudiocodecredential?view=azure-dotnet)
            6. [AzureCliCredential](https://learn.microsoft.com/en-us/dotnet/api/azure.identity.azureclicredential?view=azure-dotnet)
            7. [AzurePowerShellCredential](https://learn.microsoft.com/en-us/dotnet/api/azure.identity.azurepowershellcredential?view=azure-dotnet)
            8. [InteractiveBrowserCredential](https://learn.microsoft.com/en-us/dotnet/api/azure.identity.interactivebrowsercredential?view=azure-dotnet)
4. Create storage account *crosscloudazurestorage* blob storage and some test file
5. In storage account IAM, grant UAMI and/or SAMI “Storage Blob Data Contributor/Owner” Role (Note, the simple “Owner” role of storage account top level is NOT enough for read/write blobs in container), see doc (https://learn.microsoft.com/en-us/azure/storage/common/storage-auth-aad-app?tabs=dotnet)
6. list blob storage with Azure function
    1. add dependencies to the function: Blob, Identity
7. create an S3 bucket in AWS
8. create OIDC connection to allow Azure function to access S3
9. list S3 objects with Azure Function

## Ref
https://docs.aws.amazon.com/sdkfornet/v3/apidocs/items/SecurityToken/MSecurityTokenServiceAssumeRoleWithWebIdentityAssumeRoleWithWebIdentityRequest.html
https://learn.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/how-to-use-vm-token#get-a-token-using-the-azure-identity-client-library
https://docs.aws.amazon.com/IAM/latest/UserGuide/id_roles_create_for-idp_oidc.html
https://docs.aws.amazon.com/code-samples/latest/catalog/dotnetv3-STS-AssumeRole-AssumeRoleExample-AssumeRole.cs.html
https://github.com/aws/aws-sdk-net/issues/1699
https://docs.aws.amazon.com/IAM/latest/UserGuide/id_credentials_temp_request.html

## Todo
Next step: How to allow above 3 identities to assume IAM role? https://docs.aws.amazon.com/IAM/latest/UserGuide/id_roles_providers_saml.html https://blog.identitydigest.com/azuread-access-aws/ https://aws.amazon.com/blogs/apn/securing-aws-accounts-with-azure-active-directory-federation/ (For end user access aws console) https://devblogs.microsoft.com/azure-sdk/secretless-azure-functions-dev-with-the-new-azure-identity-libraries/