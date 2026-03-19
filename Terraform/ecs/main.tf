resource "aws_ecs_cluster" "afms_cluster" {
  name = "afms-cluster"


}
resource "aws_cloudwatch_log_group" "afms" {
  name              = "afms_ecs_cloudwatch"
  retention_in_days = 7
}