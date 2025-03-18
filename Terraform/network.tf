## The chatbot only needs a VPC when using standard or above instances. For free-tier, only public endpoints are supported
#### begin standard -
# resource "azurerm_virtual_network" "default" {
#   address_space = var.virtual_network_address_space
#   name                = "chatbot-network"
#   resource_group_name = azurerm_resource_group.chatbot.name
#   location            = azurerm_resource_group.chatbot.location
#
#   subnet {
#     address_prefixes     = var.chatbot_subnet_address_space
#     name                 = "chatbot-subnet"
#     service_endpoints = ["Microsoft.CognitiveServices"]
#   }
#   tags = {
#     Application = "Chatbot"
#     Owner       = "GlauxServices"
#   }
#
#   depends_on = [azurerm_resource_group.chatbot]
# }
#### - end standard