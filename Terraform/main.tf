
module "vpc" {
  source = "./vpc"
  
}

module "ecs" {
  source = "./ecs"
 
}

module "RDS" {
  source = "./RDS"
}

module "ALB" {
  source = "./ALB"
  vpc_id = module.vpc.vpc_id
  public_subnet_ids = module.vpc.public_subnet_ids
  alb_sg_id         = module.security_groups.alb_sg_id
  certificate_arn   = module.Route53.certificate_arn

}