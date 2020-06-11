# Create Infrastructure as Code
This project is implemented using 3 main services in Azure: Functions, Event Hub and CosmosDB using Terraform to provision the required infrastructure on Azure.

The flow is triggered and controlled by an [Github Action](https://help.github.com/es/actions). The action contains a set of tasks that are organized logically in `Continous Integration` (`CI`)- to evaluate the Terraform scripts - and `Continous Development` (`CD`) - to provision the infrastructure on Azure.

## Environment Resources

The infrastructure provisioned by Terraform includes:

| Service | Description |
|---|---| 
| Resource Group | Contains all the resources for the solution |
| Event Hub | To ingest and distribute data to the functions. The script will create four event hubs - (`validator`, `receipt`, `transfer`, `saga-reply`)|
| Functions | Serverless compute services. This includes Durable functions |
| CosmosDB | multi-model database service for operational and analytics workloads. The script will create five collectiosn - `validator`, `receipt`, `orchestrator`, `transfer`, `saga`|

Repository structure relevant to this document:

----Put here the order of the files----

Prerequisites:

* [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest)
* [Azure DevOps CLI](https://docs.microsoft.com/en-us/azure/devops/cli/?view=azure-devops)
* Service Principal - [Azure doc](https://docs.microsoft.com/en-us/cli/azure/create-an-azure-service-principal-azure-cli?view=azure-cli-latest) | [Terraform doc](https://www.terraform.io/docs/providers/azurerm/guides/service_principal_client_secret.html)
* Shell
* [jq](https://stedolan.github.io/jq/download/)

### 1. Create the variable groups

1. Edit [create_variable_groups.sh](scripts/create_variable_groups.sh) to include the values such as your Azure Devops `Organization` and `Project` and Azure parameters.

```bash
...
# fill the correct values of the variables below to create the variable group needed for Terraform Pipelines
GROUP_NAME="INFRASTRUCTURE_VARIABLES" # name of the variable group used in the solution
ARM_SUBSCRIPTION_ID=
ARM_TENANT_ID=
AZURE_SUBSCRIPTION= # format SUBSCRIPTION_NAME (SUBSCRIPTION)
CONTAINER_NAME=
STATE_RES_GRP=
STORAGE_ACCOUNT_NAME=
...
ARM_ACCESS_KEY=
ARM_CLIENT_ID=
ARM_CLIENT_SECRET=
...
```

Some variables are used by Terraform `remote_state`. You can learn more about it [here](https://www.terraform.io/docs/backends/types/azurerm.html)

2. Then run the script:
```shell
./scripts/create_variable_groups.sh
```

### 2. Review Terraform scripts

There are four Ferraform (`.tf`) scripts to create this environment:

| Script    | Description |
|---|---|
| [main.tf](infrastructure/main.tf) | main file with the environment definition |
| [variables.tf](infrastructure/variables.tf) | variables used in the main script | 
| [backend.tf](infrastructure/backend.tf) | backend used by Terraform |
| [version.tf](infrastructure/version.tf)| minimun Terraform version required |

The only script that may need a change is `variables.tf` as it is used to customize the infrastructure names.
