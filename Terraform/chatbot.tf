resource "random_id" "uniqueness_string" {
  byte_length = 4
}

resource "azurerm_search_service" "custom_question_answering" {
  name                = var.chatbot_name
  resource_group_name = azurerm_resource_group.chatbot.name
  location            = azurerm_resource_group.chatbot.location

  #### begin free-tier -
  sku                 = "free"
  #### - end free-tier


  tags = {
    Application = "Chatbot"
    Owner       = "GlauxServices"
  }

  ## For standard deployments, disable public access
  #### begin standard -
  # sku = "standard"
  # allowed_ips = var.allowed_ip_addresses
  # public_network_access_enabled = false
  #
  # identity {
  #   type = "SystemAssigned"
  # }
  # depends_on = [azurerm_virtual_network.default]
  #### - end standard

}

resource "azurerm_cognitive_account" "chatbot_ai_services" {
  custom_subdomain_name = "${var.chatbot_name}-${random_id.uniqueness_string.dec}"
  name                  = "${title(var.chatbot_name)}-${random_id.uniqueness_string.dec}"
  resource_group_name   = azurerm_resource_group.chatbot.name
  location              = azurerm_resource_group.chatbot.location
  kind                  = "TextAnalytics"

  ## Free-tier only allows public endpoints. Public endpoints allow automation of the search service assignment
  #### begin free-tier -
  sku_name              = "F0"

  custom_question_answering_search_service_id = azurerm_search_service.custom_question_answering.id
  custom_question_answering_search_service_key = azurerm_search_service.custom_question_answering.primary_key
  #### - end free-tier

  ## For standard deployments, use a VPC for private endpoints. Private endpoints require an IAM assignment for the
  ## Cognitive Service account to access the Search Service, which is a circular dependency. Thus, I have left it as
  ## a manual step
  #### start standard -
  # sku_name = "S"
  # network_acls {
  #   default_action = "Deny"
  #   ip_rules = var.allowed_ip_addresses
  #   virtual_network_rules {
  #     subnet_id = [for subnet in azurerm_virtual_network.default.subnet: subnet.id][0]
  #   }
  # }
  #
  # identity {
  #   type = "SystemAssigned"
  # }
  #
  # depends_on = [azurerm_search_service.custom_question_answering, azurerm_virtual_network.default]
  #### - end standard

  tags = {
    Application = "Chatbot"
    Owner       = "GlauxServices"
  }

  timeouts {
    create = "120m"
  }
}
