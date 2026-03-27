variable "vpc_id" {
  type = string
}
variable "private_subnet_ids" {
  type = list(string)
}

variable "target_group_arn" {
  description = "target group arn for afms-alb"
  type        = string

}


variable "afms_image" {
  type = string
}

variable "afms_image_tag" {
  type = string

}
variable "afms_port" {
  default = 8080

}

variable "region" {
  default = "eu-west-2"
}

variable "execution_role_arn" {
  type = string
}

variable "alb_sg_id" {
  description = "Security group ID of the ALB"
  type        = string
}

variable "rds_endpoint" {
  description = "RDS database endpoint"
  type        = string
}

variable "rds_db_name" {
  description = "RDS database name"
  type        = string
}

variable "rds_username" {
  description = "RDS database username"
  type        = string
}

variable "rds_password" {
  description = "RDS database password"
  type        = string
  sensitive   = true
}

variable "rds_port" {
  description = "RDS database port"
  type        = number
}

variable "deepseek_api_key" {
  description = "DeepSeek API key for AI search functionality"
  type        = string
  sensitive   = true
  default     = ""
}

variable "deepseek_api_endpoint" {
  description = "DeepSeek API endpoint"
  type        = string
  default     = "https://api.deepseek.com/v1"
}

variable "deepseek_model" {
  description = "DeepSeek model name"
  type        = string
  default     = "deepseek-chat"
}

variable "deepseek_timeout_seconds" {
  description = "DeepSeek API timeout in seconds"
  type        = number
  default     = 15
}

variable "deepseek_max_requests_per_minute" {
  description = "Maximum number of DeepSeek requests per minute per client"
  type        = number
  default     = 5
}

variable "deepseek_prompt_file" {
  description = "DeepSeek prompt file path"
  type        = string
  default     = "Prompts/DeepSeekFlightSearchPrompt.txt"
}

variable "aerodatabox_api_key" {
  description = "AeroDataBox API key"
  type        = string
  sensitive   = true
  default     = ""
}

variable "aerodatabox_api_host" {
  description = "AeroDataBox API host"
  type        = string
  default     = "aerodatabox.p.rapidapi.com"
}

variable "default_airport" {
  description = "Default airport ICAO code"
  type        = string
  default     = "EGLL"
}