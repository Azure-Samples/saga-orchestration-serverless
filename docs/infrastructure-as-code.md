# Create Infrastructure as Code
This project is implemented using 3 main services in Azure: Functions, Event Hub and CosmosDB using Terraform to provision the required environment in Azure for this project.

The flow is triggered and controlled by a [Github Action](https://help.github.com/es/actions). The action contains a set of tasks that are organized logically to evaluate Terraform scripts and to provision the infrastructure on Azure.

## Environment Resources

The infrastructure provisioned by Terraform includes:

| Service | Description |
|---|---| 
| Resource Group | Contains all the resources for the solution |
| Event Hub | To ingest and distribute data to the functions. The script will create four event hubs - (`validator`, `receipt`, `transfer`, `saga-reply`)|
| Functions | Serverless compute services. This includes Durable functions |
| CosmosDB | multi-model database service for operational and analytics workloads. The script will create five collectiosn - `validator`, `receipt`, `orchestrator`, `transfer`, `saga`|

Prerequisites:

* [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest)
* A pre existing Azure Storage Account
* A Service Principal - [Azure doc](https://docs.microsoft.com/en-us/cli/azure/create-an-azure-service-principal-azure-cli?view=azure-cli-latest) | [Terraform doc](https://www.terraform.io/docs/providers/azurerm/guides/service_principal_client_secret.html)

### 1. Create the Service Principal

1. By using Azure CLI, in case you need it, you need to create an Azure Service Principal

```bash
az ad sp create-for-rbac --name "myApp" --role contributor --scopes /subscriptions/your-subscription-id --sdk-auth
```

![Service Principal](/img/servicePrincipal.png)

You'll need from here the **clientId**, **clientSecret**
### 2. Review Terraform scripts

There are four Ferraform (`.tf`) scripts to create this environment:

| Script    | Description |
|---|---|
| [main.tf](infrastructure/main.tf) | main file with the environment definition |
| [variables.tf](infrastructure/variables.tf) | variables used in the main script | 
| [backend.tf](infrastructure/backend.tf) | backend used by Terraform |
| [version.tf](infrastructure/version.tf)| minimun Terraform version required |

The only script that may need a change is `variables.tf` as it is used to customize the infrastructure names.
