terraform {
  required_providers {

    aws = {
      source  = "hashicorp/aws"
      version = "6.31.0"
    }

  }
  backend "s3" {
    bucket  = "afms-state"
    key     = "terraform.tfstate"
    region  = "eu-west-2"
    encrypt = true

  }

}

provider "aws" {
  region = "eu-west-2"
}

