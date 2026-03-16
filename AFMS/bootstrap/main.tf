resource "aws_s3_bucket" "tf-state" {
  bucket = "afms-state"

  lifecycle {
    prevent_destroy = true
  }

}
resource "aws_s3_bucket_versioning" "tf_state_versioning" {
  bucket = aws_s3_bucket.tf-state.id

  versioning_configuration {
    status = "Enabled"
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
