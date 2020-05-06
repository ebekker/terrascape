terraform {
    required_version = ">= 0.11.13"
}

provider "lo" {

    ## Sample for how to manage various platforms,
    ## borrowed from ideas in Puppet

    # default_package_system = "choco" ## or "yum" or "apt" or "brew" ...
    # default_service_system = "win32" ## or "systemd" or "init" or "launchd"
}

# data "lo_sys_info" "local" {
# }

# output "local_sys_info" {
#     value = {
#         process_architecture  = data.lo_sys_info.local.process_architecture
#         os_architecture       = data.lo_sys_info.local.os_architecture
#         os_platform           = data.lo_sys_info.local.os_platform
#         os_description        = data.lo_sys_info.local.os_description
#         os_version_string     = data.lo_sys_info.local.os_version_string
#         framework_description = data.lo_sys_info.local.framework_description
#     }
# }




# data "lo_file_info" "source1" {
#     path = "./source1.txt"
# }

# output "source1_file_info" {
#     value = {
#         path       = data.lo_file_info.source1.path
#         full_path  = data.lo_file_info.source1.full_path
#         attributes = data.lo_file_info.source1.attributes

#         name             = data.lo_file_info.source1.name
#         exists           = data.lo_file_info.source1.exists
#         extension        = data.lo_file_info.source1.extension
#         creation_time    = data.lo_file_info.source1.creation_time
#         last_access_time = data.lo_file_info.source1.last_access_time
#         last_write_time  = data.lo_file_info.source1.last_write_time
#         length           = data.lo_file_info.source1.length
#     }
# }





# data "lo_file_info" "temp1" {
#     path = lo_file.temp1.path
# }

# output "temp1_file_info" {
#     value = {
#         path       = data.lo_file_info.temp1.path
#         full_path  = data.lo_file_info.temp1.full_path
#         attributes = data.lo_file_info.temp1.attributes

#         name             = data.lo_file_info.temp1.name
#         extension        = data.lo_file_info.temp1.extension
#         creation_time    = data.lo_file_info.temp1.creation_time
#         last_access_time = data.lo_file_info.temp1.last_access_time
#         last_write_time  = data.lo_file_info.temp1.last_write_time
#         length           = data.lo_file_info.temp1.length
#     }
# }

variable "temp1_checksum" {
    default = "md5"
    # default = "sha1"
    # default = "sha256"
    # default = "sc_hash"
}

resource "lo_file" "temp1" {
    path = "${path.module}/test1.txt"

    compute_checksum = var.temp1_checksum

    content          = "This is content from inline TF config."
    # content_base64   = ""
    # content_path     = "./source1.txt"
    # content_uri      = "https://api.ipify.org?format=json"

    append {
        content = "\n * appending my public IP:\n"
    }
    append {
        content_url = "https://api.ipify.org?format=json"
    }
    append {
        content = "\n * appending from another file:\n"
    }
    append {
        content_path = "./source1.txt"
    }
}

output "temp1_checksum" {
    value = lo_file.temp1.checksum
}


resource "winlo_registry_key" "sample1" {
    root = "HKCU" # | "HKLM"
    path = "HKEY_CURRENT_USER\\Software\\Cygwin\\Installations"

    value {
        type = "string" # | "binary" | "dword" | "qword" | "multi-string" | "expandable-multi-string"
        name = "foobar"
        content = ""
        content_base64 = ""
        content_multi = [
            
        ]
    }
}


# resource "acmelo_file" "temp2" {
#     path = "${path.module}/test2.txt"
# }


# Sample for how this could work...

# resource "lo_package" "nginx" {
#     name = "nginx"
# }

# resource "lo_service" "nginx" {
#     name  = "nginx"
#     state = "running"
# }
