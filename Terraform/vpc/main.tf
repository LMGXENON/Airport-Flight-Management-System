resource "aws_vpc" "gatus_vpc" {
  cidr_block = "10.0.0.0/16"
  tags = {

    Name = "afms_vpc"
  }

}

resource "aws_subnet" "public" {
  count             = 2
  vpc_id            = aws_vpc.gatus_vpc.id
  cidr_block        = var.public_cidrs[count.index]
  availability_zone = var.azs[count.index]
}