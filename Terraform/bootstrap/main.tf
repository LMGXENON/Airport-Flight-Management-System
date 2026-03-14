resource "aws_ecr_repository" "afms_repo" {
  name         = "afms-repo"

  image_tag_mutability = "MUTABLE"

  image_scanning_configuration {
    scan_on_push = true
  }
}
