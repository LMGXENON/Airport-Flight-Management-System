variable "vpc_id" {
  description = "vpc_id"
}
variable "public_subnet_ids" {

  type = list(string)

}

variable "alb_sg_id" {
  type = string
}

variable "certificate_arn" {
  type = string

}
variable "vpc_cidr_block" {
  type = string
}
variable "cidr_blocks_all" {
  default = ["0.0.0.0/0"]

}
variable "gatus_port" {
  default = 8080

}