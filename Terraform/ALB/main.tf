resource "aws_lb" "afms_alb" {
  name               = "afms-alb"
  internal           = false
  load_balancer_type = "application"
  security_groups    = [aws_security_group.alb_sg.id]
  subnets            = var.public_subnet_ids

}
resource "aws_security_group" "alb_sg" {
  name   = "alb"
  vpc_id = var.vpc_id


}
resource "aws_lb_target_group" "afms_tg" {
  name        = "afms-alb-tg"
  port        = 8080
  protocol    = "HTTP"
  target_type = "ip"
  vpc_id      = var.vpc_id

  health_check {
    protocol            = "HTTP"
    path                = "/health"
    matcher             = "200"
    healthy_threshold   = 2
    unhealthy_threshold = 2
    interval            = 30
    timeout             = 5


  }

}
resource "aws_lb_listener" "HTTP" {
  load_balancer_arn = aws_lb.afms_alb.arn
  port              = "80"
  protocol          = "HTTP"

  default_action {
    type = "redirect"

    redirect {
      port        = "443"
      protocol    = "HTTPS"
      status_code = "HTTP_301"
    }
  }
}
resource "aws_lb_listener" "HTTPS" {
  load_balancer_arn = aws_lb.afms_alb.arn
  port              = "443"
  protocol          = "HTTPS"
  ssl_policy        = "ELBSecurityPolicy-TLS13-1-2-2021-06"
  certificate_arn   = var.certificate_arn
  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.afms_tg.arn
  }

}
resource "aws_security_group_rule" "allow_all_http_in_alb" {
  type              = "ingress"
  security_group_id = aws_security_group.alb_sg.id
  cidr_blocks       = var.cidr_blocks_all
  from_port         = 80
  to_port           = 80
  protocol          = "tcp"

}
resource "aws_security_group_rule" "allow_all_https_in_alb" {
  type              = "ingress"
  security_group_id = aws_security_group.alb_sg.id
  cidr_blocks       = var.cidr_blocks_all
  from_port         = 443
  to_port           = 443
  protocol          = "tcp"

}
output "alb_sg_id" {
  value = aws_security_group.alb_sg.id
}

output "target_group_arn" {
  value = aws_lb_target_group.afms_tg.arn
}

output "alb_dns_name" {
  value = aws_lb.afms_alb.dns_name
}

output "alb_zone_id" {
  value = aws_lb.afms_alb.zone_id
}

