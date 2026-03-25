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