#this is a file with recommended variable names
prefix          = "patrykga"
environment     = "dev"
location        = "northeurope"
partition_count = "2"
#failover location MUST be different than location, if same Terraform won't be able to create a Cosmos DB instance
failover_location    = "westeurope"
storage_account_name = "aminesstorage"
azure_function_app   = "aminazure-functions"
