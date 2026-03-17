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