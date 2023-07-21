# IdentityService
This is a simple service that provides Azure VM Instance Metadata Service like functionality on your local machine. Only the Identity token part of the REST API is provided.

# How to Use

Update the appsetting.json file to include the TenantId, ClientId and either the ClientSecret or the Certificate information for your Service Principal.

On Windows, you can place your certificate into the Certificate Store and specify the store information and the subject name of the certificate.

On Linux, you should be able to specify the path to the certificate.

# Get a Token

### CURL
``` Bash
curl 'http://127.0.0.1:50342/metadata/identity/oauth2/token?resource=https%3A%2F%2Fmanagement.azure.com%2F'
```

### PowerShell
``` PowerShell
irm "http://127.0.0.1:50342/metadata/identity/oauth2/token?resource=https%3A%2F%2Fmanagement.azure.com%2F"
```

In addition to the `/metadata/identity/oauth2/token` endpoint, I have also added a shorter `/oauth2/token` endpoint if you do not need full IMDS compatibility.

# Setup as a Service

On Windows, you can easily set this up as a Windows Service:

``` cmd
sc.exe create "IndentityService" binpath="C:\path\to\file\Identity.exe"
```