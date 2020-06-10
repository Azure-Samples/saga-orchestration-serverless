terraform {
  backend "azurerm" {
    storage_account_name = "generastorageamin"
    container_name       = "testingterraform"
    key                  = "terraform.tfstate"
    sas_token            = "?sv=2019-10-10&ss=b&srt=sco&sp=rwdlacx&se=2021-05-07T07:28:54Z&st=2020-05-06T23:28:54Z&spr=https&sig=T8zeiOw0BIS0Z%2BxAFSuHjr78MeoPmT5OaGpbvLVaXNM%3D"
  }
}