data "aws_caller_identity" "current" {}

locals {
  afms_ecr_image = "${data.aws_caller_identity.current.account_id}.dkr.ecr.eu-west-2.amazonaws.com/afms-repo"
}

module "vpc" {
  source = "./vpc"
}

module "ecs" {
  source                           = "./ecs"
  vpc_id                           = module.vpc.vpc_id
  private_subnet_ids               = module.vpc.private_subnet_ids
  target_group_arn                 = module.ALB.target_group_arn
  alb_sg_id                        = module.ALB.alb_sg_id
  afms_image                       = local.afms_ecr_image
  afms_image_tag                   = "latest"
  region                           = "eu-west-2"
  execution_role_arn               = aws_iam_role.ecs_task_execution_role.arn
  rds_endpoint                     = module.RDS.rds_endpoint
  rds_db_name                      = module.RDS.rds_db_name
  rds_username                     = module.RDS.rds_username
  rds_password                     = var.rds_password
  rds_port                         = module.RDS.rds_port
  deepseek_api_key                 = var.deepseek_api_key
  deepseek_api_endpoint            = var.deepseek_api_endpoint
  deepseek_model                   = var.deepseek_model
  deepseek_timeout_seconds         = var.deepseek_timeout_seconds
  deepseek_max_requests_per_minute = var.deepseek_max_requests_per_minute
}

module "RDS" {
  source             = "./RDS"
  vpc_id             = module.vpc.vpc_id
  private_subnet_ids = module.vpc.private_subnet_ids
  ecs_sg_id          = module.ecs.ecs_sg_id
  db_password        = var.rds_password
}

module "ALB" {
  source            = "./ALB"
  vpc_id            = module.vpc.vpc_id
  public_subnet_ids = module.vpc.public_subnet_ids
  certificate_arn   = module.route53_cert.certificate_arn
}

resource "aws_iam_role" "ecs_task_execution_role" {
  name = "ecs-task-execution-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action = "sts:AssumeRole"
      Effect = "Allow"
      Principal = {
        Service = "ecs-tasks.amazonaws.com"
      }
    }]
  })
}

resource "aws_iam_role_policy_attachment" "ecs_task_execution_role_policy" {
  role       = aws_iam_role.ecs_task_execution_role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
}

resource "aws_iam_role_policy" "ecs_task_execution_role_ecr" {
  name = "ecs-task-execution-role-ecr"
  role = aws_iam_role.ecs_task_execution_role.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "ecr:GetAuthorizationToken"
        ]
        Resource = "*"
      },
      {
        Effect = "Allow"
        Action = [
          "ecr:BatchGetImage",
          "ecr:GetDownloadUrlForLayer"
        ]
        Resource = "arn:aws:ecr:eu-west-2:${data.aws_caller_identity.current.account_id}:repository/afms-repo"
      }
    ]
  })
}

module "route53_cert" {
  source              = "./Route53"
  hosted_zone_id      = var.route53_hosted_zone_id
  domain_name         = var.route53_domain_name
  create_certificate  = true
  create_alias_record = false
}

module "route53_alias" {
  source              = "./Route53"
  hosted_zone_id      = var.route53_hosted_zone_id
  record_name         = var.route53_record_name
  alb_dns_name        = module.ALB.alb_dns_name
  alb_zone_id         = module.ALB.alb_zone_id
  create_certificate  = false
  create_alias_record = true
}
