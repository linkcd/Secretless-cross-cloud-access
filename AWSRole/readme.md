Follow https://blog.identitydigest.com/azuread-access-aws/ to setup AWS Azure federation with Oauth

Note:
When adding idp in AWS IAM, the provider URL (the secure OpenID Connect URL for authentication requests)idp should be "https://sts.windows.net/YOUR-AZURE-TENANT-ID/". It should end with "/". 

After created the idp, in AWS IAM Console, the provider should like "sts.windows.net/YOUR-AZURE-TENANT-ID/"

It JTW token issue by Azure is the following, note the "/" in the "idp".
```json
{
  "aud": "MANAGED-IDENTITY-CLIENT-ID",
  "iss": "https://sts.windows.net/YOUR-AZURE-TENANT-ID/",
  "iat": 1667483765,
  "nbf": 1667483765,
  "exp": 1667570465,
  "aio": "E2ZgYBD8Y961eLe/nH6njeUb0U9+AA==",
  "appid": "MANAGED-IDENTITY-CLIENT-ID",
  "appidacr": "2",
  "idp": "https://sts.windows.net/YOUR-AZURE-TENANT-ID/",
  "oid": "0c44bdb8-29f1-416b-a3c1-0293fe81ee59",
  "rh": "0.AAAAtA5QYkaxnkeBiBMuRtRp4ghhnb3lxHVImtvweYJNWmyUAAA.",
  "sub": "0c44bdb8-29f1-416b-a3c1-0293fe81ee59",
  "tid": "62500eb4-b146-479e-8188-132e46d469e2",
  "uti": "bHTMg6izF0SrSUXWZshlAA",
  "ver": "1.0"
}
```

The IAM role trusted relationship json
```json
{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Effect": "Allow",
            "Principal": {
                "Federated": "arn:aws:iam::YOUR-AWS-ACCOUNT-NUMBER:oidc-provider/sts.windows.net/YOUR-AZURE-TENANT-ID/"
            },
            "Action": "sts:AssumeRoleWithWebIdentity",
            "Condition": {
                "StringEquals": {
                    "sts.windows.net/YOUR-AZURE-TENANT-ID/:aud": "MANAGED-IDENTITY-CLIENT-ID"
                }
            }
        }
    ]
}
```
