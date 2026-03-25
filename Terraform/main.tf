
module "vpc" {
  source = "./vpc"
  
}

module "ecs" {
  source = "./ecs"
  vpc_id = module.vpc.vpc_id
  private_subnet_ids = module.vpc.private_subnet_ids
  target_group_arn = module.ALB.target_group_arn
  ecs_sg_id = module.security_groups.ecs_sg_id
  afms_image = "afms"
  afms_image_tag = "latest"
  region = "eu-west-2"
  execution_role_arn = module.ecs_task_execution_role.execution_role_arn
 
}

module "RDS" {
  source = "./RDS"
}

module "ALB" {
  source = "./ALB"
  vpc_id = module.vpc.vpc_id
  vpc_cidr_block = module.vpc.vpc_cidr_block
  public_subnet_ids = module.vpc.public_subnet_ids
  alb_sg_id         = module.security_groups.alb_sg_id
  certificate_arn   = module.Route53.certificate_arn


}

module "Route53" {
  source = "./Route53"
  alb_dns_name = module.ALB.alb_dns_name
  alb_zone_id = module.ALB.alb_zone_id

}