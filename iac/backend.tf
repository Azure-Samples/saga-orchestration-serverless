terraform {
  backend "azurerm" {
    storage_account_name = "patrykgaterraformstate"
    container_name       = "testingterraform"
    key                  = "terraform.tfstate"
  }
}
