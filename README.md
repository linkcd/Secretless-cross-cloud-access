# Secretless Cross Cloud Access
Secretless access Azure and AWS resources (API) with Azure managed identity 

Next step: How to allow above 3 identities to assume IAM role?
https://docs.aws.amazon.com/IAM/latest/UserGuide/id_roles_providers_saml.html
https://blog.identitydigest.com/azuread-access-aws/
https://aws.amazon.com/blogs/apn/securing-aws-accounts-with-azure-active-directory-federation/ (For end user access aws console)
https://devblogs.microsoft.com/azure-sdk/secretless-azure-functions-dev-with-the-new-azure-identity-libraries/


Test Setup with Azure Function and AWS S3 Bucket:
Use case: Function with .Net to list storage account and S3 bucket objects

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
            1. EnvironmentCredential (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.environmentcredential?view=azure-dotnet)
            2. ManagedIdentityCredential (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.managedidentitycredential?view=azure-dotnet)
            3. SharedTokenCacheCredential (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.sharedtokencachecredential?view=azure-dotnet)
            4. VisualStudioCredential (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.visualstudiocredential?view=azure-dotnet)
            5. VisualStudioCodeCredential (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.visualstudiocodecredential?view=azure-dotnet)
            6. AzureCliCredential (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.azureclicredential?view=azure-dotnet)
            7. AzurePowerShellCredential (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.azurepowershellcredential?view=azure-dotnet)
            8. InteractiveBrowserCredential (https://learn.microsoft.com/en-us/dotnet/api/azure.identity.interactivebrowsercredential?view=azure-dotnet)
4. Create storage account *crosscloudazurestorage* blob storage and some test file
5. In storage account IAM, grant UAMI and/or SAMI “Storage Blob Data Contributor/Owner” Role (Note, the simple “Owner” role of storage account top level is NOT enough for read/write blobs in container), see doc (https://learn.microsoft.com/en-us/azure/storage/common/storage-auth-aad-app?tabs=dotnet)
6. list blob storage with Azure function
    1. add dependencies to the function: Blob, Identity
7. create an S3 bucket in AWS
8. create OIDC connection to allow Azure function to access S3
9. list S3 objects with Azure Function

https://docs.aws.amazon.com/sdkfornet/v3/apidocs/items/SecurityToken/MSecurityTokenServiceAssumeRoleWithWebIdentityAssumeRoleWithWebIdentityRequest.html
https://learn.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/how-to-use-vm-token#get-a-token-using-the-azure-identity-client-library
https://docs.aws.amazon.com/IAM/latest/UserGuide/id_roles_create_for-idp_oidc.html
https://docs.aws.amazon.com/code-samples/latest/catalog/dotnetv3-STS-AssumeRole-AssumeRoleExample-AssumeRole.cs.html
https://github.com/aws/aws-sdk-net/issues/1699
https://docs.aws.amazon.com/IAM/latest/UserGuide/id_credentials_temp_request.html