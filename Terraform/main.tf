
module "vpc" {
  source = "./vpc"
}

module "ecs" {
  source = "./ecs"
 
}

module "RDS" {
  source = "./RDS"
}
