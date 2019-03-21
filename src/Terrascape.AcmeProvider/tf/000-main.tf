terraform {
    required_version = ">= 0.11.10"
}

# provider "aws" {
#     ## During the 0.12 preview, do not specify a version constraing to
#     ## automatically default to the one included with the preview package
#     version = ">= 1.57.0"

#     profile = "${var.aws_profile}"
#     region  = "${var.aws_region}"
# }

# provider "lo" {}

# data "lo_sys_info" "local1" {
#     inp1 = "FOO"
#     inp2 = false
#     inp3 = 3
# }

# output "os_name" {
#     value = data.lo_sys_info.local1.name
# }
# output "os_version" {
#     value = data.lo_sys_info.local1.version
# }

# resource "lo_directory" "temp_dir" {
#     path = "${path.module}/temp/test-terrascape"
# }

# resource "lo_file" "temp_file" {
#     depends_on = ["lo_directory.temp_dir"]

#     path = "c:/temp/test-terrascape-file.txt"
# }

provider "acmelo" {
    server_url = "https://example.com/"
}

# resource "acmelo_account" "account" {
#     email_address = "FOO"
# }


resource "acmelo_file" "temp1" {
    path = "${path.module}/test1.txt"

    compute_checksum = "md5"
    # compute_checksum = "sha1"
    # compute_checksum = "sha256"
    # compute_checksum = "sc_hash"

    # content          = "THIS IS A TEST FROM TF"
    content_path = "./source1.txt"
}

# resource "acmelo_file" "temp2" {
#     path = "${path.module}/test2.txt"
# }
