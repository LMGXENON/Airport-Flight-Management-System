variable "rds_password" {
  description = "RDS master database password"
  type        = string
  sensitive   = true
}

variable "route53_hosted_zone_id" {
  description = "Route53 hosted zone ID used for ACM validation and ALB alias"
  type        = string
  default     = "Z058936924T76L0WJJPGE"
}

variable "route53_domain_name" {
  description = "Domain name for ACM certificate"
  type        = string
  default     = "afmscorp.net"
}

variable "route53_record_name" {
  description = "Route53 record name for ALB alias (use afmscorp.net for apex/root)"
  type        = string
  default     = "afmscorp.net"
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

variable "aerodatabox_api_key" {
  description = "AeroDataBox API key for live flight data"
  type        = string
  sensitive   = true
  default     = ""
}

variable "auth_admin_username" {
  description = "Admin username for AFMS login"
  type        = string
  default     = "afms"
}

variable "auth_admin_password" {
  description = "Admin password for AFMS login"
  type        = string
  sensitive   = true
  default     = ""
}

variable "auth_jwt_secret" {
  description = "JWT signing secret for AFMS auth"
  type        = string
  sensitive   = true
  default     = ""
}

variable "auth_issuer" {
  description = "JWT issuer for AFMS auth"
  type        = string
  default     = "AFMS"
}

variable "auth_audience" {
  description = "JWT audience for AFMS auth"
  type        = string
  default     = "AFMS.Users"
}

variable "auth_token_expiry_hours" {
  description = "JWT token expiry in hours"
  type        = number
  default     = 8
}
