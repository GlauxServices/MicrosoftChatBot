variable "subscription_id" {
  description = "Subscription ID for Azure account"
  type = string
}

variable "allowed_ip_addresses" {
  description = "List of allowed external IP addresses"
  type = list(string)
  default = null
}

variable "virtual_network_address_space" {
  description = "Address space of virtual network"
  type = list(string)
  default = ["10.0.0.0/16"]
}

variable "chatbot_subnet_address_space" {
  description = "Address space of virtual network"
  type = list(string)
  default = ["10.0.1.0/24"]
}

variable "chatbot_name" {
  description = "Base name for the chatbot"
  type = string
  default = "glaux-services-chatbot"
}