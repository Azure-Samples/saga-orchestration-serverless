#Azure account variables
variable "sub" {
  type        = string
  description = "Subscription that will host the infrastructure"
}

variable "client_secret" {
  type        = string
  description = "Service principal client secret"
}

variable "client_id" {
  type        = string
  description = "Service principal client ID"
}

variable "tenant_id" {
  type        = string
  description = "Service principal tenant_id"
}

variable "prefix" {
  description = "The Prefix used for all resources"
}

variable "environment" {
  description = "Environment being deployed"
}

variable "location" {
  description = "The Azure Region in which all resources should be created."
}

variable "partition_count" {
  description = "Number of partitions to scale Azure Event Hubs"
  default     = 2
}

variable "eventhub" {
  description = "Event Hubs needed in the solution"
  default     = ["validator", "receipt", "transfer", "saga-reply"]
}

variable "ehcg" {
  description = "Event Hub Consumer Group name"
  default     = "saga-ehcg"
}

variable "failover_location" {
  description = "The Azure Region for CosmosDB failover."
}

variable "collections" {
  description = "Create Tables in SQL"
  type        = map(string)
  default     = { "validator" = "id", "receipt" = "transactionId", "orchestrator" = "id", "transfer" = "transactionId", "saga" = "transactionId" }
}

variable "storage_account_name" {
  description = "Unique global identifier for an Azure Storage Account"
}

variable "azure_function_app" {
  description = "Unique global identifier for an Azure Functions App"
}


