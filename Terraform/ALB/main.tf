resource "aws_lb" "afms_alb" {
  name               = "afms-alb"
  internal           = false
  load_balancer_type = "application"
  security_groups    = [aws_security_group.alb_sg.id]
  subnets            = aws_subnet.public[*].id

}
resource "aws_security_group" "alb_sg" {
  name   = "alb"
  vpc_id = aws_vpc.afms_vpc.id


}
resource "aws_lb_target_group" "afms_tg" {
  name        = "afms-alb-tg"
  port        = 8080
  protocol    = "HTTP"
  target_type = "ip"
  vpc_id      = var.vpc_id

  health_check {
    protocol            = "HTTP"
    path                = "/"
    matcher             = "200-399"
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
