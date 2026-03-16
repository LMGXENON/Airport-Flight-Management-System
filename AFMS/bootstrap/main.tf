resource "aws_s3_bucket" "terraform_state" {
  bucket = "afms-tfstate"

  lifecycle {
    prevent_destroy = true
  }

}
resource "aws_ecr_repository" "afms_repo" {
  name         = "afms-repo"

  image_tag_mutability = "MUTABLE"

  image_scanning_configuration {
    scan_on_push = true
  }
}
resource "aws_ecr_lifecycle_policy" "repo_policy" {
  repository = aws_ecr_repository.afms_repo.name

  policy = jsonencode({
    rules = [{
      rulePriority = 1
      description  = "Expire old images"
      selection = {
        tagStatus   = "any"
        countType   = "imageCountMoreThan"
        countNumber = 10
      }
      action = {
        type = "expire"
      }
    }]
  })
}
