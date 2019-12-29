provider "lo" {

}

data "lo_env" "path" {
    name = "PATH"
}

output "path-env" {
    value = data.lo_env.path
}