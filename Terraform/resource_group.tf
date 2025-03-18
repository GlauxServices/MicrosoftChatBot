resource "azurerm_resource_group" "chatbot" {
  name     = "Chatbot"
  location = "eastus"

  tags = {
    Application = "Chatbot"
    Owner       = "GlauxServices"

  }
}