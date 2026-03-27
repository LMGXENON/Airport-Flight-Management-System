resource "aws_db_subnet_group" "afms_db_subnet_group" {
  name       = "afms-db-subnet-group"
  subnet_ids = var.private_subnet_ids

  tags = {
    Name = "afms-db-subnet-group"
  }
}

resource "aws_security_group" "rds_sg" {
  name   = "rds-afms"
  vpc_id = var.vpc_id

  tags = {
    Name = "rds-afms"
  }
}

resource "aws_security_group_rule" "rds_ingress_from_ecs" {
  type                     = "ingress"
  from_port                = 5432
  to_port                  = 5432
  protocol                 = "tcp"
  source_security_group_id = var.ecs_sg_id
  security_group_id        = aws_security_group.rds_sg.id
}

resource "aws_security_group_rule" "rds_egress_all" {
  type              = "egress"
  from_port         = 0
  to_port           = 0
  protocol          = "-1"
  cidr_blocks       = ["0.0.0.0/0"]
  security_group_id = aws_security_group.rds_sg.id
}

resource "aws_db_instance" "afms_db" {
  identifier        = "afms-db"
  engine            = "postgres"
  engine_version    = var.engine_version
  instance_class    = var.db_instance_class
  allocated_storage = var.allocated_storage
  storage_encrypted = true
  storage_type      = "gp3"

  db_name  = var.db_name
  username = var.db_username
  password = var.db_password

  db_subnet_group_name            = aws_db_subnet_group.afms_db_subnet_group.name
  vpc_security_group_ids          = [aws_security_group.rds_sg.id]
  parameter_group_name            = "default.postgres${split(".", var.engine_version)[0]}"
  publicly_accessible             = false
  skip_final_snapshot             = var.skip_final_snapshot
  final_snapshot_identifier       = var.skip_final_snapshot ? null : "afms-db-final-snapshot-${formatdate("YYYY-MM-DD-hhmm", timestamp())}"
  backup_retention_period         = var.backup_retention_period
  backup_window                   = "03:00-04:00"
  maintenance_window              = "sun:04:00-sun:05:00"
  multi_az                        = var.multi_az
  deletion_protection             = var.deletion_protection
  copy_tags_to_snapshot           = true
  enabled_cloudwatch_logs_exports = ["postgresql"]

  tags = {
    Name = "afms-database"
  }
}

output "rds_endpoint" {
  value = aws_db_instance.afms_db.endpoint
}

output "rds_db_name" {
  value = aws_db_instance.afms_db.db_name
}

output "rds_username" {
  value = aws_db_instance.afms_db.username
}

output "rds_port" {
  value = aws_db_instance.afms_db.port
}
