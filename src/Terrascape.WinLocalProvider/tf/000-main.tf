terraform {
    required_version = ">= 0.11.13"
}

provider "winlo" {

    ## Maybe in the future, "target" a remote system
    ## over WinRM or SSH on which to apply the changes?
    ## BUT that would start to overlap with the native
    ## "provisioner" feature of TF, so maybe not...
}

# data "winlo_registry_key" "sample1" {
#     root = "HKCU"
#     path = "Terrascape\\SampleKey1"
# }

# output "sample1_reg_key" {
#     value = {
#         key_names   = data.winlo_registry_key.sample1.key_names
#         value_names = data.winlo_registry_key.sample1.value_names
#         entries     = data.winlo_registry_key.sample1.entries
#     }
# }

# resource "winlo_registry_key" "sample2" {
#     root = "HKCU"
#     # path = "Terrascape\\SampleKey2"
#     # path = "Terrascape\\SampleKey3"
#     path = "Terrascape\\SampleKey4"

#     ## By default needed to create the TerrascapeTests
#     ## intermediate key and assign perms to allow any
#     ## user full control, to make this test work
#     # root = "HKLM"
#     # path = "SOFTWARE\\TerrascapeTests\\SampleKey1"

#     force_on_delete = true

#     entry "SampleString1" {
#        #condition    = "absent" # | "present"
#         type         = "string" # | "binary" | "dword" | "qword" | "multi-string" | "expandable-multi-string"
#         value        = "SampleString1 Value"
#     }

#     # entry "SampleBinary1" {
#     #     type = "binary"
#     #     value_base64 = "AAECAwQFBgcICQoLDA0ODxA="
#     # }

#     entry "SampleMulti1" {
#         type   = "multi"
#         values = [
#             "Foo",
#             "Bar",
#             "Non",
#             "Hello",
#             "World",
#             "!",
#         ]
#     }

#     entry "SampleDWord1" {
#         type  = "dword"
#         value = 199
#     }

#     ## TODO: literal hexadecimals appear to be broken in HCL2
#     # # # entry "SampleDWord2" {
#     # # #     value = 0x22
#     # # #     type  = "dword"
#     # # # }

#     entry "SampleDWord3" {
#         type  = "dword"
#         value = "999"
#     }

#     entry "SampleChange1" {
#         type = "binary"
#         value_base64 = "AAECAwQFBgcICQoLDA0ODxA="
#         # type         = "string"
#         # value        = "SampleString"
#     }
# }
