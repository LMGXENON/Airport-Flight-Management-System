resource "aws_vpc" "gatus_vpc" {
  cidr_block = "10.0.0.0/16"
  tags = {

    Name = "afms_vpc"
  }

}
