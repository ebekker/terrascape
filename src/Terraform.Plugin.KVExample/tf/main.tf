
provider "kv" {
    path = "my_kv.json"
}



# data "kv_info" "info" {

# }

# data "kv_get" "foo" {
#     name = "string"
# }



resource "kv_put" "string" {
    name = "string"
    value = "string value"
}

# resource "kv_put" "int" {
#     // Declare a dependency to serialize the call,
#     // otherwise we have competing file lock issue
#     depends_on = [kv_put.foo]

#     name = "int"
#     value = 3
# }



# output "info-details" {
#     value = data.kv_info.info
# }

# output "foo-value" {
#     value = data.kv_get.foo
# }
