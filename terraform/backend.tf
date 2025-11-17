terraform {
  backend "s3" {
    bucket       = "terraform-secertnick25-bucket"
    key          = "terraform.tfstate"
    region       = "eu-central-1"
    use_lockfile = true
    encrypt      = true
  }
}
