variable "vpc_id" {
  type = string
}
variable "private_subnet_ids" {
  type = list(string)
}

variable "target_group_arn" {
  description = "target group arn for afms-alb"
  type        = string

}


variable "afms_image" {
  type = string
}

variable "afms_image_tag" {
  type = string

}
variable "afms_port" {
  default = 8080

}

variable "region" {
  default = "eu-west-2"
}

variable "execution_role_arn" {
  type = string
}

variable "alb_sg_id" {
  description = "Security group ID of the ALB"
  type        = string
}