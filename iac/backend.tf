terraform {
  backend "azurerm" {
    storage_account_name = "generastorageamin"
    container_name       = "testingterraform"
    key                  = "terraform.tfstate"
  }
}