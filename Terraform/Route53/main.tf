resource "aws_route53_record" "alb_alias" {
  count   = var.create_alias_record ? 1 : 0
  zone_id = var.hosted_zone_id
  type    = "A"
  name    = var.record_name

  alias {
    name                   = var.alb_dns_name
    zone_id                = var.alb_zone_id
    evaluate_target_health = false
  }
}

resource "aws_acm_certificate" "acm_cert" {
  count             = var.create_certificate ? 1 : 0
  domain_name       = var.domain_name
  validation_method = "DNS"
}

resource "aws_route53_record" "acm_cert_validation" {
  for_each = var.create_certificate ? {
    for dvo in aws_acm_certificate.acm_cert[0].domain_validation_options :
    dvo.domain_name => {
      name   = dvo.resource_record_name
      record = dvo.resource_record_value
      type   = dvo.resource_record_type
    }
  } : {}

  zone_id = var.hosted_zone_id
  name    = each.value.name
  type    = each.value.type
  ttl     = 60
  records = [each.value.record]
}

resource "aws_acm_certificate_validation" "acm_validate" {
  count                   = var.create_certificate ? 1 : 0
  certificate_arn         = aws_acm_certificate.acm_cert[0].arn
  validation_record_fqdns = [for r in aws_route53_record.acm_cert_validation : r.fqdn]
}