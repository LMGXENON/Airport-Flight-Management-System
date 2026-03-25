resource "aws_s3_bucket" "tf_state" {
  bucket = "afms-state"

  lifecycle {
    prevent_destroy = true
  }

}
resource "aws_s3_bucket_versioning" "tf_state_versioning" {
  bucket = aws_s3_bucket.tf_state.id

  versioning_configuration {
    status = "Enabled"
  }
}
resource "aws_s3_bucket_server_side_encryption_configuration" "tf_state_encryption" {
  bucket = aws_s3_bucket.tf_state.id

  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm = "AES256"
    }
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
output "afms_repo_url" {
  value = aws_ecr_repository.afms_repo.repository_url
}