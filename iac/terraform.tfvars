#this is a file with recommended variable names
prefix          = "test"
environment     = "dev"
location        = "canadaeast"
partition_count = "2"
#failover location MUST be different than location, if same Terraform won't be able to create a Cosmos DB instance
failover_location    = "canadacentral"
storage_account_name = "gjstorageadmin"
azure_function_app   = "gjsaga-functions"
