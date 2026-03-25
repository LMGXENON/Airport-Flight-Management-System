output "db_instance_endpoint" {
  description = "RDS instance endpoint (address:port)"
  value       = aws_db_instance.afms_db.endpoint
}

output "db_instance_address" {
  description = "RDS instance address only"
  value       = aws_db_instance.afms_db.address
}

output "db_instance_port" {
  description = "RDS instance port"
  value       = aws_db_instance.afms_db.port
}

output "db_name" {
  description = "Name of the database"
  value       = aws_db_instance.afms_db.db_name
}

output "db_username" {
  description = "Master username for database"
  value       = aws_db_instance.afms_db.username
  sensitive   = true
}

output "db_password" {
  description = "Master password for database (store securely)"
  value       = var.db_password
  sensitive   = true
}

output "rds_security_group_id" {
  description = "RDS security group ID"
  value       = aws_security_group.rds_sg.id
}

output "connection_string_dotnet" {
  description = ".NET EntityFramework connection string (PostgreSQL)"
  value       = "Server=${aws_db_instance.afms_db.address};Port=${aws_db_instance.afms_db.port};Database=${aws_db_instance.afms_db.db_name};User Id=${aws_db_instance.afms_db.username};Password=${var.db_password};SSL Mode=Disable;"
  sensitive   = true
}
