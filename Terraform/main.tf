
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
}