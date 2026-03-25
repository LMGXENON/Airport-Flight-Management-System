resource "aws_route53_record" "alb_alias" {
  zone_id = "Z0404500D627T0ENSBZN"
  type    = "A"
  name    = "tm"
  alias {
    name                   = var.alb_dns_name
    zone_id                = var.alb_zone_id
    evaluate_target_health = false
  }
}
resource "aws_acm_certificate" "cert" {
  domain_name       = "AFMS.com"
  validation_method = "DNS"
}
resource "aws_route53_record" "acm_cert_validation" {
  for_each = {
    for dvo in aws_acm_certificate.acm_cert.domain_validation_options :
    dvo.domain_name => {
      name   = dvo.resource_record_name
      record = dvo.resource_record_value
      type   = dvo.resource_record_type
    }

  }
  zone_id = "Z0404500D627T0ENSBZN"
  name    = each.value.name
  type    = each.value.type
  ttl     = 60
  records = [each.value.record]
}