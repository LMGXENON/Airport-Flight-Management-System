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
