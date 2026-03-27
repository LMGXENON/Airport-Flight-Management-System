variable "hosted_zone_id" {
  description = "Route53 hosted zone ID where records will be managed"
  type        = string
}

variable "domain_name" {
  description = "Domain name for ACM certificate"
  type        = string
  default     = "afmscorp.net"
}

variable "record_name" {
  description = "Record name for ALB alias"
  type        = string
  default     = "afmscorp.net"
}

variable "alb_dns_name" {
  description = "ALB DNS name for alias record"
  type        = string
  default     = null
}

variable "alb_zone_id" {
  description = "ALB hosted zone ID for alias record"
  type        = string
  default     = null
}

variable "create_certificate" {
  description = "Whether to create and validate ACM certificate"
  type        = bool
  default     = true
}

variable "create_alias_record" {
  description = "Whether to create ALB alias Route53 record"
  type        = bool
  default     = false
}
