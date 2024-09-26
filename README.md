# Secretless access Azure and AWS resources with Azure managed identity and AWS IAM

This is the source code repo of my blog [How to secretless access Azure and AWS resources with Azure managed identity and AWS IAM](https://feng.lu/2024/09/18/How-to-secretless-access-Azure-and-AWS-resources-with-Azure-managed-identity-and-AWS-IAM/)

## User case
Nowadays, it is common for companies to operate in multi-cloud environments, such as Azure and AWS. They often use Microsoft Entra ID (formerly Azure Active Directory) as their centralized identity provider (IdP), managing identities for both **human users** and **applications**. They would like to use the Entra ID identities to access resources in AWS.

Establishing human user identity access across Azure and AWS is straightforward. The IT department can use [AWS IAM Identity Center](https://aws.amazon.com/iam/identity-center/) to allow users from Microsoft Entra ID to sign-in to the AWS Management Console with Single Sign-On (SSO) via their browser. This integration simplifies authentication, offering a seamless and secure user experience across both Azure and AWS environments. For more information, you can read [this document](https://docs.aws.amazon.com/singlesignon/latest/userguide/idp-microsoft-entra.html).

However, the browser-based SSO approach for human users does not apply to applications.

For applications, developers follow security best practices by using cloud-native IAM (Identity and Access Management) mechanisms to manage resource access. In AWS, this mechanism is [AWS IAM](https://aws.amazon.com/iam/), while in Azure, it is typically [Azure Managed Identity](https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/overview). For example, by leveraging Azure Managed Identity, developers can build applications in Azure without the need to manage secrets or keys.

This approach is known as **secretless access** to cloud resources. 

AWS IAM and Azure Managed Identity work well within their respective platforms, but there are cross-cloud scenarios where a workload in one cloud needs to access resources in another. For instance, an Azure Function might need to save data to both an Azure Storage account and an AWS S3 bucket for cross-cloud backup. The Azure Function uses Managed Identity to access the Azure Storage account. For accessing S3, the developer could create an IAM user and store the IAM user credentials. However, there is a better way to achieve secretless access to both Azure and AWS resources using the same Azure Managed Identity.

![Screenshot](./Doc/problem.png?raw=true "problem")

## Solution
In AWS, there are multiple ways to request temporary, limited-privilege credentials by using [AWS Security Token Service (AWS STS)](https://docs.aws.amazon.com/STS/latest/APIReference/welcome.html), such as [AssumeRoleWithSAML](https://docs.aws.amazon.com/STS/latest/APIReference/API_AssumeRoleWithSAML.html) and [AssumeRoleWithWebIdentity](https://docs.aws.amazon.com/STS/latest/APIReference/API_AssumeRoleWithWebIdentity.html).

The solution is to use [AssumeRoleWithWebIdentity](https://docs.aws.amazon.com/STS/latest/APIReference/API_AssumeRoleWithWebIdentity.html.) and [IAM Web Identity Role](https://docs.aws.amazon.com/IAM/latest/UserGuide/id_roles_create_for-idp_oidc.html) to extend the permissions of the same Azure Managed Identity to also access AWS resources.  

![Screenshot](./Doc/architecture.png?raw=true "architecture")

## This Project
This project demonstrates a secretless approach to use one Azure managed identity (either User-Assigned managed identity (UAMI) or System-Assigned Managed Identity (SAMI)), for reading the objects from both Azure storage account and AWS S3 bucket. The same managed identity works in both cloud, without managing any secrets. 

## Screenshot
An azure function that can load the objects(files) from both Azure Storage account and AWS S3 buckets, without managing any secrets such as AWS IAM user secrets/keys.
![Screenshot](./Doc/screenshot.png?raw=true "Screenshot")

## Ref.
- [AWS Doc: Create a role for OpenID Connect federation](https://docs.aws.amazon.com/IAM/latest/UserGuide/id_roles_create_for-idp_oidc.html)
- [AWS Doc: Request temporary security credentials](https://docs.aws.amazon.com/IAM/latest/UserGuide/id_credentials_temp_request.html)
- [AWS Doc: SAML 2.0 federation](https://docs.aws.amazon.com/IAM/latest/UserGuide/id_roles_providers_saml.html)
- [AWS Doc: AmazonSecurityTokenServiceClient.AssumeRoleWithWebIdentity](https://docs.aws.amazon.com/sdkfornet/v3/apidocs/items/SecurityToken/MSecurityTokenServiceAssumeRoleWithWebIdentityAssumeRoleWithWebIdentityRequest.html)
- [AWS Doc: Code examples for AWS SDK for .NET](https://docs.aws.amazon.com/code-library/latest/ug/csharp_3_code_examples.html)
- [Azure Doc: Get a token using the Azure identity client library](https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/how-to-use-vm-token#get-a-token-using-the-azure-identity-client-library)
- [Azure Blog: Secretless Azure Functions dev with the new Azure Identity Libraries ](https://devblogs.microsoft.com/azure-sdk/secretless-azure-functions-dev-with-the-new-azure-identity-libraries/)
