resource "aws_vpc" "afms_vpc" {
  cidr_block = "10.0.0.0/16"
  tags = {

    Name = "afms_vpc"
  }

}

resource "aws_subnet" "public" {
  count             = 2
  vpc_id            = aws_vpc.afms_vpc.id
  cidr_block        = var.public_cidrs[count.index]
  availability_zone = var.azs[count.index]
}
resource "aws_subnet" "private" {
  count             = 2
  vpc_id            = aws_vpc.afms_vpc.id
  cidr_block        = var.private_cidrs[count.index]
  availability_zone = var.azs[count.index]

}
resource "aws_route_table" "public" {
  vpc_id = aws_vpc.afms_vpc.id
  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.igw.id
  }
}
resource "aws_route_table_association" "pub-rt" {
  count          = 2
  subnet_id      = aws_subnet.public[count.index].id
  route_table_id = aws_route_table.public.id
}

resource "aws_route_table" "private" {
  count  = 2
  vpc_id = aws_vpc.afms_vpc.id
  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_nat_gateway.ngw[count.index].id
  }
}
resource "aws_route_table_association" "priv-rt" {
  count          = 2
  subnet_id      = aws_subnet.private[count.index].id
  route_table_id = aws_route_table.private[count.index].id
}
resource "aws_internet_gateway" "igw" {
  vpc_id = aws_vpc.afms_vpc.id
}



resource "aws_eip" "ngw-eip" {
  count  = 2
  domain = "vpc"
}
resource "aws_nat_gateway" "ngw" {
  count         = 2
  allocation_id = aws_eip.ngw-eip[count.index].id
  subnet_id     = aws_subnet.public[count.index].id
  depends_on    = [aws_internet_gateway.igw]

}

