resource "local_file" "appsettings_file" {
  filename = "${terraform.workspace}/../../ChatBot-Demo/ChatBot-Demo/appsettings.json"
  content = <<-EOT
{
  "Configuration": {
    "AzureCognitiveServicesEndpoint": "${azurerm_cognitive_account.chatbot_ai_services.endpoint}",
    "AzureCognitiveServicesKey": "${azurerm_cognitive_account.chatbot_ai_services.primary_access_key}",
    "DeploymentName": "${azurerm_cognitive_account.chatbot_ai_services.name}"
  }
}
EOT
}