output "certificate_arn" {
  value = try(aws_acm_certificate_validation.acm_validate[0].certificate_arn, null)
}