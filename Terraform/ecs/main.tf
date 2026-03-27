resource "aws_ecs_cluster" "afms_cluster" {
  name = "afms-cluster"


}
resource "aws_cloudwatch_log_group" "afms" {
  name              = "afms_ecs_cloudwatch"
  retention_in_days = 7
}

resource "aws_efs_file_system" "afms_dp_keys" {
  creation_token = "afms-data-protection-keys"
  encrypted      = true
}

resource "aws_security_group" "efs_sg" {
  name   = "afms-efs-dpkeys"
  vpc_id = var.vpc_id
}

resource "aws_security_group_rule" "efs_in_from_ecs" {
  type                     = "ingress"
  security_group_id        = aws_security_group.efs_sg.id
  source_security_group_id = aws_security_group.ecs_sg.id
  from_port                = 2049
  to_port                  = 2049
  protocol                 = "tcp"
  description              = "Allow NFS from ECS tasks"
}

resource "aws_security_group_rule" "efs_out_all" {
  type              = "egress"
  security_group_id = aws_security_group.efs_sg.id
  from_port         = 0
  to_port           = 0
  protocol          = "-1"
  cidr_blocks       = ["0.0.0.0/0"]
}

resource "aws_efs_mount_target" "afms_dp_keys" {
  for_each        = toset(var.private_subnet_ids)
  file_system_id  = aws_efs_file_system.afms_dp_keys.id
  subnet_id       = each.value
  security_groups = [aws_security_group.efs_sg.id]
}

resource "aws_efs_access_point" "afms_dp_keys" {
  file_system_id = aws_efs_file_system.afms_dp_keys.id

  posix_user {
    uid = 1000
    gid = 1000
  }

  root_directory {
    path = "/afms-dpkeys"

    creation_info {
      owner_uid   = 1000
      owner_gid   = 1000
      permissions = "0770"
    }
  }
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
        },
        {
          name  = "DEEPSEEK_API_KEY"
          value = var.deepseek_api_key
        },
        {
          name  = "DEEPSEEK_API_ENDPOINT"
          value = var.deepseek_api_endpoint
        },
        {
          name  = "DEEPSEEK_MODEL"
          value = var.deepseek_model
        },
        {
          name  = "DEEPSEEK_TIMEOUT_SECONDS"
          value = tostring(var.deepseek_timeout_seconds)
        },
        {
          name  = "DEEPSEEK_MAX_REQUESTS_PER_MINUTE"
          value = tostring(var.deepseek_max_requests_per_minute)
        },
        {
          name  = "DEEPSEEK_PROMPT_FILE"
          value = var.deepseek_prompt_file
        },
        {
          name  = "AERODATABOX_API_KEY"
          value = var.aerodatabox_api_key
        },
        {
          name  = "AERODATABOX_API_HOST"
          value = var.aerodatabox_api_host
        },
        {
          name  = "DEFAULT_AIRPORT"
          value = var.default_airport
        },
        {
          name  = "DATA_PROTECTION_KEYS_PATH"
          value = var.data_protection_keys_path
        }
      ]

      mountPoints = [
        {
          sourceVolume  = "data-protection-keys"
          containerPath = var.data_protection_keys_path
          readOnly      = false
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

  volume {
    name = "data-protection-keys"

    efs_volume_configuration {
      file_system_id     = aws_efs_file_system.afms_dp_keys.id
      root_directory     = "/"
      transit_encryption = "ENABLED"

      authorization_config {
        access_point_id = aws_efs_access_point.afms_dp_keys.id
        iam             = "DISABLED"
      }
    }
  }



}
resource "aws_ecs_service" "afms-service" {
  name             = "afms-service"
  cluster          = aws_ecs_cluster.afms_cluster.id
  task_definition  = aws_ecs_task_definition.afms-task.arn
  desired_count    = 2
  launch_type      = "FARGATE"
  platform_version = "LATEST"
  propagate_tags   = "SERVICE"

  health_check_grace_period_seconds = 180

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

  depends_on = [aws_efs_mount_target.afms_dp_keys]


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

resource "aws_security_group_rule" "ecs_out_to_efs" {
  type              = "egress"
  security_group_id = aws_security_group.ecs_sg.id
  from_port         = 2049
  to_port           = 2049
  protocol          = "tcp"
  cidr_blocks       = ["10.0.0.0/16"]
  description       = "Allow NFS to EFS mount targets"
}

resource "aws_security_group_rule" "ecs_out_dns_udp" {
  type              = "egress"
  security_group_id = aws_security_group.ecs_sg.id
  from_port         = 53
  to_port           = 53
  protocol          = "udp"
  cidr_blocks       = ["0.0.0.0/0"]
  description       = "Allow DNS UDP queries"
}

resource "aws_security_group_rule" "ecs_out_dns_tcp" {
  type              = "egress"
  security_group_id = aws_security_group.ecs_sg.id
  from_port         = 53
  to_port           = 53
  protocol          = "tcp"
  cidr_blocks       = ["0.0.0.0/0"]
  description       = "Allow DNS TCP queries"
}

output "aws_ecs_task_definition_arn" {
  value = aws_ecs_task_definition.afms-task.arn

}
output "ecs_sg_id" {
  value = aws_security_group.ecs_sg.id
}

output "data_protection_efs_id" {
  value = aws_efs_file_system.afms_dp_keys.id
}