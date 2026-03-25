variable "private_subnet_ids" {
  type = list(string)
}

variable "target_group_arn" {
  description = "target group arn for afms-alb"
  type        = string

}

variable "ecs_sg_id" {
  description = "security group for alb"
  type        = string
}

variable "afms_image" {
  type = string
}

variable "afms_image_tag" {
  type = string

}

variable "region" {
  default = "eu-west-2"

}
variable "execution_role_arn" {
  type = string

}