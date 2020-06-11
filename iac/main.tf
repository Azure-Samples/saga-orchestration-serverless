resource "azurerm_resource_group" "sagalogic-resource-group" {
  name     = "${var.prefix}-resources"
  location = var.location

  tags = {
    environment = var.environment
  }
}

resource "azurerm_eventhub_namespace" "sagalogic-namespace" {
  name                = "${var.prefix}-ehnamespace"
  location            = azurerm_resource_group.sagalogic-resource-group.location
  resource_group_name = azurerm_resource_group.sagalogic-resource-group.name
  sku                 = "Standard"
  capacity            = 2

  tags = {
    environment = var.environment
  }
}

resource "azurerm_eventhub_namespace_authorization_rule" "sagalogic-namespace-auth" {
  name                = "${var.prefix}-nsauth-rule"
  namespace_name      = azurerm_eventhub_namespace.sagalogic-namespace.name
  resource_group_name = azurerm_resource_group.sagalogic-resource-group.name

  listen = true
  send   = true
  manage = false
}

resource "azurerm_eventhub" "sagalogic-eventhub" {
  for_each            = toset(var.eventhub)
  name                = each.value
  namespace_name      = azurerm_eventhub_namespace.sagalogic-namespace.name
  resource_group_name = azurerm_resource_group.sagalogic-resource-group.name

  partition_count   = var.partition_count
  message_retention = 1
}



resource "azurerm_eventhub_consumer_group" "sagalogic-eventhub-cons-grp" {
  for_each            = toset(var.eventhub)
  name                = var.ehcg
  namespace_name      = azurerm_eventhub_namespace.sagalogic-namespace.name
  eventhub_name       = azurerm_eventhub.sagalogic-eventhub[each.key].name
  resource_group_name = azurerm_resource_group.sagalogic-resource-group.name
}

resource "azurerm_storage_account" "sagalogic-storage-account" {
  name                     = "${var.prefix}${var.storage_account_name}"
  resource_group_name      = azurerm_resource_group.sagalogic-resource-group.name
  location                 = azurerm_resource_group.sagalogic-resource-group.location
  account_tier             = "Standard"
  account_replication_type = "LRS"

  tags = {
    environment = var.environment
  }
}

resource "azurerm_app_service_plan" "sagalogic-app-service-plan" {
  name                = "${var.prefix}-service-plan"
  resource_group_name = azurerm_resource_group.sagalogic-resource-group.name
  location            = azurerm_resource_group.sagalogic-resource-group.location

  sku {
    tier = "Standard"
    size = "S1"
  }

  tags = {
    environment = var.environment
  }
}

resource "azurerm_function_app" "sagalogic-function" {
  name                      = "${var.prefix}-${var.azure_function_app}"
  resource_group_name       = azurerm_resource_group.sagalogic-resource-group.name
  location                  = azurerm_resource_group.sagalogic-resource-group.location
  app_service_plan_id       = azurerm_app_service_plan.sagalogic-app-service-plan.id
  storage_connection_string = azurerm_storage_account.sagalogic-storage-account.primary_connection_string
  version                   = "~3"

  app_settings = {
    "CosmosDbConnectionString"           = "AccountEndpoint=${azurerm_cosmosdb_account.sagalogic-db-account.endpoint};AccountKey=${azurerm_cosmosdb_account.sagalogic-db-account.primary_master_key};",
    "CosmosDbDatabaseName"               = azurerm_cosmosdb_sql_database.sagalogic-sql-database.name,
    "CosmosDbOrchestratorCollectionName" = azurerm_cosmosdb_sql_container.sagalogic-sql-container["orchestrator"].name,
    "CosmosDbValidatorCollectionName"    = azurerm_cosmosdb_sql_container.sagalogic-sql-container["validator"].name,
    "CosmosDbTransferCollectionName"     = azurerm_cosmosdb_sql_container.sagalogic-sql-container["transfer"].name,
    "CosmosDbReceiptCollectionName"      = azurerm_cosmosdb_sql_container.sagalogic-sql-container["receipt"].name,
    "CosmosDbSagaCollectionName"         = azurerm_cosmosdb_sql_container.sagalogic-sql-container["saga"].name,

    "EventHubsNamespaceConnection" = azurerm_eventhub_namespace.sagalogic-namespace.default_primary_connection_string,
    "ValidatorEventHubName"        = azurerm_eventhub.sagalogic-eventhub["validator"].name,
    "ReceiptEventHubName"          = azurerm_eventhub.sagalogic-eventhub["receipt"].name,
    "TransferEventHubName"         = azurerm_eventhub.sagalogic-eventhub["transfer"].name,
    "ReplyEventHubName"            = azurerm_eventhub.sagalogic-eventhub["saga-reply"].name,

    "EventHubsProducerMaxRetryAttempts" = 3,
    "ActivityMaxRetryAttempts"          = 3,
    "ActivityRetryInterval"             = 5,

    "EventHubsProducerExceptionsAllowedBeforeBreaking" = 2,
    "EventHubsProducerBreakDuration"                   = 10,

    "OrchestratorActivityTimeoutSeconds" = 60,
    "ValidatorTimeoutSeconds"            = 60,
    "TransferTimeoutSeconds"             = 60,
    "ReceiptTimeoutSeconds"              = 60,

    "CreateRandomReceiptResult" = true,
    "ReceiptSuccessProbability" = 0,

    "FUNCTIONS_WORKER_RUNTIME" = "dotnet",

    "APPINSIGHTS_INSTRUMENTATIONKEY" = azurerm_application_insights.sagalogic-application-insights.instrumentation_key,
  }

  tags = {
    environment = var.environment
  }
}

resource "random_integer" "sagalogic-ri" {
  min = 10000
  max = 99999
}

resource "azurerm_cosmosdb_account" "sagalogic-db-account" {
  name                = "${var.prefix}-cosmos-sagalogic-db-${random_integer.sagalogic-ri.result}"
  resource_group_name = azurerm_resource_group.sagalogic-resource-group.name
  location            = azurerm_resource_group.sagalogic-resource-group.location
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB"

  consistency_policy {
    consistency_level       = "Session"
    max_interval_in_seconds = 5
    max_staleness_prefix    = 100
  }

  enable_automatic_failover = true

  geo_location {
    location          = var.failover_location
    failover_priority = 1
  }

  geo_location {
    prefix            = "${var.prefix}-cosmos-db-${random_integer.sagalogic-ri.result}-customid"
    location          = azurerm_resource_group.sagalogic-resource-group.location
    failover_priority = 0
  }

  tags = {
    environment = var.environment
  }
}

resource "azurerm_cosmosdb_sql_database" "sagalogic-sql-database" {
  name                = "${var.prefix}-cosmos-sql-db"
  resource_group_name = azurerm_resource_group.sagalogic-resource-group.name
  account_name        = azurerm_cosmosdb_account.sagalogic-db-account.name
  throughput          = 500
}

resource "azurerm_cosmosdb_sql_container" "sagalogic-sql-container" {
  for_each            = tomap(var.collections)
  name                = each.key
  resource_group_name = azurerm_cosmosdb_account.sagalogic-db-account.resource_group_name
  account_name        = azurerm_cosmosdb_account.sagalogic-db-account.name
  database_name       = azurerm_cosmosdb_sql_database.sagalogic-sql-database.name
  partition_key_path  = "/${each.value}"
}

resource "azurerm_application_insights" "sagalogic-application-insights" {
  name                = "${var.prefix}-app-insights"
  location            = var.location
  resource_group_name = azurerm_resource_group.sagalogic-resource-group.name
  application_type    = "web"
}