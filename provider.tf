provider "azurerm" {
  version         = "= 2.1.0"
  subscription_id = var.sub
  client_id       = var.client_id
  client_secret   = var.client_secret
  tenant_id       = var.tenant_id
}

provider "random" {
  version = "~> 2.2"
}