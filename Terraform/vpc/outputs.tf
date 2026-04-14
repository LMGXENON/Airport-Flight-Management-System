output "vpc_id" {
  value = aws_vpc.afms_vpc.id
}

output "public_subnet_ids" {
  value = aws_subnet.public[*].id
}
output "private_subnet_ids" {
  value = aws_subnet.private[*].id
}

output "private_subnet_ids_by_az" {
  value = {
    for idx, az in var.azs : az => aws_subnet.private[idx].id
  }
}

output "route_table_public_id" {
  value = aws_route_table.public.id
}
output "nat_gateway_id" {
  value = aws_nat_gateway.ngw[*].id
}
output "vpc_cidr_block" {
  value = aws_vpc.afms_vpc.cidr_block
}