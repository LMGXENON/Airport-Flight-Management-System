resource "aws_ecs_cluster" "afms_cluster" {
  name = "afms-cluster"


}
resource "aws_cloudwatch_log_group" "afms" {
  name              = "afms_ecs_cloudwatch"
  retention_in_days = 7
}
resource "aws_ecs_task_definition" "afms-task" {
  family                   = "afms_task"
  requires_compatibilities = ["FARGATE"]
  network_mode             = "awsvpc"
  cpu                      = "256"
  memory                   = "512"
  execution_role_arn       = var.execution_role_arn
  container_definitions = jsonencode([
    {
      name  = "afms"
      image = "${var.afms_image}:${var.afms_image_tag}"

      portMappings = [{
        containerPort = 8080
        protocol      = "tcp"
      }]

      logConfiguration = {
        logDriver = "awslogs"
        options = {
          awslogs-group         = aws_cloudwatch_log_group.afms.name
          awslogs-stream-prefix = "ecs"
          awslogs-region        = var.region
        }
      }
    }
  ])



}