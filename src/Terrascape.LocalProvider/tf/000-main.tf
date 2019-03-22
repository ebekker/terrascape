terraform {
    required_version = ">= 0.11.13"
}

provider "lo" {
}

# resource "acmelo_account" "account" {
#     email_address = "FOO"
# }

data "lo_sys_info" "local" {
}

output "local_sys_info" {
    value = {
        process_architecture  = data.lo_sys_info.local.process_architecture
        os_architecture       = data.lo_sys_info.local.os_architecture
        os_platform           = data.lo_sys_info.local.os_platform
        os_description        = data.lo_sys_info.local.os_description
        os_version_string     = data.lo_sys_info.local.os_version_string
        framework_description = data.lo_sys_info.local.framework_description
    }
}

data "lo_file_info" "temp1" {
    path = lo_file.temp1.path
}

output "temp1_file_info" {
    value = {
        path       = data.lo_file_info.temp1.path
        full_path  = data.lo_file_info.temp1.full_path
        attributes = data.lo_file_info.temp1.attributes

        name             = data.lo_file_info.temp1.name
        extension        = data.lo_file_info.temp1.extension
        creation_time    = data.lo_file_info.temp1.creation_time
        last_access_time = data.lo_file_info.temp1.last_access_time
        last_write_time  = data.lo_file_info.temp1.last_write_time
        length           = data.lo_file_info.temp1.length
    }
}

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
