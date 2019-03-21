terraform {
    required_version = ">= 0.11.13"
}

provider "lo" {
}

# resource "acmelo_account" "account" {
#     email_address = "FOO"
# }


resource "lo_file" "temp1" {
    path = "${path.module}/test1.txt"

    compute_checksum = "md5"
    # compute_checksum = "sha1"
    # compute_checksum = "sha256"
    # compute_checksum = "sc_hash"

    # content          = "This is content from inline TF config."
    content_path = "./source1.txt"
}

# resource "acmelo_file" "temp2" {
#     path = "${path.module}/test2.txt"
# }
