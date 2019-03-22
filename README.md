# Terrascape

On-host configuration management powered by [Terraform](https://github.com/hashicorp/terraform).

Terraform is a great tool to manage infrastructure across multiple cloud platform and services providers using a declarative notation.  But what if you could use that same notation and tooling to manage your on-host configuration?

Terraform is essentially a state management and differencing engine, with the different provider plugins doing the actual heaving lifting of interacting with different service providers to implement changes in the infrastructure in the form of resources.

That same model could work just as well to manage the different resources that represent different assets and services within the host.  This is akin to existing tools like Chef, Puppet and DSC, but using the language (HCL2) and state model specific to Terraform.

So for example...

```hcl

## A "local" system provider to manage local resources
provider "lo" {
    ## Perhaps not much to configure
    ## at the provider level...
}

variable "epel_rpm_source" {
    default = "https://dl.fedoraproject.org/pub/epel/epel-release-latest-7.noarch.rpm"
}


resource "lo_packages" "epel" {
    ## Install the EPEL repo package
    names  = ["epel-release"]

    ## Optionally specify an explicit source
    source = var.epel_rpm_source
}

resource "lo_packages" "myapp_deps" {
    ## Install a few packages from the default repo
    names = [
        "mariadb",
        "tomcat",
        "certbot",
    ]
}

resource "lo_user" "tomcat" {
    name   = "tomcat_user"
    groups = ["myapp"]
}

resource "lo_file" "myapp_war" {
    depends_on = [
        "lo_packages.myapp_deps",
    ]

    path         = "/var/lib/tomcat/webapps/myapp.war"
    symlink_path = "/usr/share/guacamole/myapp.war"
}

resource "lo_service" "tomcat" {
    depends_on = [
        "lo_file.myapp_war",
        "lo_packages.myapp_deps",
    ]

    enabled = true
    state   = "running"
}

resource "lo_file" "nginx_config" {
    path    = "/etc/nginx/conf.d/https.conf"
    content = <<CONTENT
server {
    listen       443 ssl http2 default_server;
    listen       [::]:443 ssl http2 default_server;
    server_name  _;
    root         /usr/share/nginx/html;

    ssl_certificate "/etc/pki/cert.pem";
    ssl_certificate_key "/etc/pki/key.pem";
    ssl_session_cache shared:SSL:1m;
    ssl_session_timeout  10m;
    ssl_ciphers HIGH:!aNULL:!MD5;
    ssl_prefer_server_ciphers on;

    # Load configuration files for the default server block.
    include /etc/nginx/default.d/*.conf;

    location / {
        return 301 https://\$host\$request_uri/guac;
    }

    error_page 404 /404.html;
        location = /40x.html {
    }

    error_page 500 502 503 504 /50x.html;
        location = /50x.html {
    }
}
CONTENT
}

resource "lo_service" "tomcat" {
    depends_on = [
        "lo_file.nginx_config",
        "lo_service.tomcat",
    ]

    enabled = true
    state   = "running"
}

```