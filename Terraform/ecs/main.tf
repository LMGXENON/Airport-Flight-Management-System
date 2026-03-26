resource "aws_ecs_cluster" "afms_cluster" {
  name = "afms-cluster"


}
resource "aws_cloudwatch_log_group" "afms" {
  name              = "afms_ecs_cloudwatch"
  retention_in_days = 7
}
resource "aws_ecs_task_definition" "afms-task" {
  family                   = "afms_task"
  requires_compatibilities = ["FARGATE"]
  network_mode             = "awsvpc"
  cpu                      = "256"
  memory                   = "512"
  execution_role_arn       = var.execution_role_arn
  container_definitions = jsonencode([
    {
      name  = "afms"
      image = "${var.afms_image}:${var.afms_image_tag}"

      portMappings = [{
        containerPort = 8080
        protocol      = "tcp"
      }]

      environment = [
        {
          name  = "ConnectionStrings__DefaultConnection"
          value = "Host=${split(":", var.rds_endpoint)[0]};Port=${var.rds_port};Database=${var.rds_db_name};Username=${var.rds_username};Password=${var.rds_password};SSL Mode=Require;"
        }
      ]

      logConfiguration = {
        logDriver = "awslogs"
        options = {
          awslogs-group         = aws_cloudwatch_log_group.afms.name
          awslogs-stream-prefix = "ecs"
          awslogs-region        = var.region
        }
      }
    }
  ])



}
resource "aws_ecs_service" "afms-service" {
  name             = "afms-service"
  cluster          = aws_ecs_cluster.afms_cluster.id
  task_definition  = aws_ecs_task_definition.afms-task.arn
  desired_count    = 2
  launch_type      = "FARGATE"
  platform_version = "LATEST"
  propagate_tags   = "SERVICE"

  health_check_grace_period_seconds = 60

  load_balancer {
    target_group_arn = var.target_group_arn
    container_name   = "afms"
    container_port   = 8080
  }

  network_configuration {
    assign_public_ip = false
    subnets          = var.private_subnet_ids
    security_groups  = [aws_security_group.ecs_sg.id]
  }

  deployment_circuit_breaker {
    enable   = true
    rollback = true
  }


}
resource "aws_security_group" "ecs_sg" {
  name   = "ecs"
  vpc_id = var.vpc_id

}
resource "aws_security_group_rule" "ecs_in_from_alb" {
  type                     = "ingress"
  security_group_id        = aws_security_group.ecs_sg.id
  source_security_group_id = var.alb_sg_id
  from_port                = var.afms_port
  to_port                  = var.afms_port
  protocol                 = "tcp"
}

resource "aws_security_group_rule" "ecs_out_to_ecr" {
  type              = "egress"
  security_group_id = aws_security_group.ecs_sg.id
  from_port         = 443
  to_port           = 443
  protocol          = "tcp"
  cidr_blocks       = ["0.0.0.0/0"]
  description       = "Allow HTTPS to ECR and other services"
}

resource "aws_security_group_rule" "ecs_out_to_rds" {
  type              = "egress"
  security_group_id = aws_security_group.ecs_sg.id
  from_port         = 5432
  to_port           = 5432
  protocol          = "tcp"
  cidr_blocks       = ["10.0.0.0/16"]
  description       = "Allow PostgreSQL to RDS"
}

output "aws_ecs_task_definition_arn" {
  value = aws_ecs_task_definition.afms-task.arn

}
output "ecs_sg_id" {
  value = aws_security_group.ecs_sg.id
}