terraform {
  backend "azurerm" {
    storage_account_name = "patrykgaterraformstate"
    container_name       = "tfstate"
    key                  = "terraform.tfstate"
  }
}
