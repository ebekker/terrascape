
provider "pwsh" {

}

# data "pwsh_script" "s1" {

#     script = <<-SCRIPT
#         $context.outputs['hello'] = 'world'
#         $context.outputs['foo'] = 1
#         $context.outputs['bar'] = "BAR"
#         SCRIPT
# }

# output "s1" {
#     value = data.pwsh_script.s1.outputs
# }



# data "pwsh_script" "s2" {

#     script = <<-SCRIPT
#         $psver = ($PSVersionTable | Out-String)
#         $context.outputs['os']    = $PSVersionTable.OS
#         $context.outputs['psver'] = $psver.ToString()#.Substring(0, 255)
#         $context.outputs['test']  = "test"
#         SCRIPT
# }

# output "s2" {
#     value = {
#         os    = data.pwsh_script.s2.outputs["os"]
#         psver = data.pwsh_script.s2.outputs["psver"]
#         test  = data.pwsh_script.s2.outputs["test"]
#     }
# }

# locals {
#     some_random = "Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String 0123456789 Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String 0123456789"
#     upper_psver = upper(data.pwsh_script.s2.outputs["psver"])
#     upper_os    = upper(data.pwsh_script.s2.outputs["os"])
# }


# data "pwsh_script" "s3" {

#     inputs = {
#         foo   = "F00"
#         #psver = "Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String 0123456789 Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String Some Random String 0123456789"
#         psver = upper(data.pwsh_script.s2.outputs["psver"])
#         #psver = local.some_random
#         #psver = local.upper_os
#     }

#     script = <<-SCRIPT
#         $foo   = $context.inputs['foo']
#         $psver = $context.inputs['psver']

#         $foobar   = "$($FOO)B@R"
#         $psverlen = $psver.Length

#         $context.outputs['foobar']   = $foobar
#         $context.outputs['psverlen'] = $psverlen

#         SCRIPT
# }

# output "s3" {
#     value = {
#         foobar   = data.pwsh_script.s3.outputs["foobar"]
#         psverlen = data.pwsh_script.s3.outputs["psverlen"]
#     }
# }

# data "pwsh_dsc_resource" "file" {
#     module_name    = "PSDesiredStateConfiguration"
#     module_version = "0.0"
#     type_name      = "MSFT_FileDirectoryConfiguration"

#     properties = {
#         DestinationPath = "C:\\local\\prj\\bek\\zyborg\\terrascape\\src\\Terrascape.PwshProvider\\obj\\dsctest\\_IGNORE\\DirectAccess.txt"
#         Contents        = "This file is create by Invoke-DscResource"
#         # Contents        = "This file is create by Invoke-DscResource by way of Terraform"
#         Force           = false
#         Attributes      = "[\"Archive\"]"
#     }
# }

# output "dsc-file-ds" {
#     value = {
#         props            = data.pwsh_dsc_resource.file.properties
#         results          = data.pwsh_dsc_resource.file.results
#         in_desired_state = data.pwsh_dsc_resource.file.in_desired_state
#     }
# }

locals {
    dsctest = "C:\\local\\prj\\bek\\zyborg\\terrascape\\src\\Terrascape.PwshProvider\\obj\\dsctest"
}

data "pwsh_script" "dow" {
    script = <<-SCRIPT
        $context.outputs['dow_short'] = [datetime]::Now.ToString('ddd')
        $context.outputs['dow_long'] = [datetime]::Now.ToString('dddd')
        $context.outputs['hour'] = [datetime]::Now.ToString('HH')
        $context.outputs['min'] = [datetime]::Now.ToString('MM')
        SCRIPT
}


resource "pwsh_dsc_resource" "file1" {
    module_name    = "PSDesiredStateConfiguration"
    module_version = "0.0"
    type_name      = "MSFT_FileDirectoryConfiguration"

    properties = {
        DestinationPath = "${local.dsctest}\\_IGNORE\\DirectAccess.txt"
        # Contents        = "This file is create by Invoke-DscResource"
        Contents        = <<-CONTENTS
            This file is create by Invoke-DscResource...
            ...by way of Terraform!

            Today...
            ...the dow is: ${data.pwsh_script.dow.outputs["dow_short"]}
            CONTENTS
        Attributes      = jsonencode([
            "Archive"
        ])
    }
}

resource "pwsh_dsc_resource" "file2" {
    module_name    = "PSDesiredStateConfiguration"
    module_version = "0.0"
    type_name      = "MSFT_FileDirectoryConfiguration"

    properties = {
        DestinationPath = "${local.dsctest}\\_IGNORE\\SecondFile.txt"
        # Contents        = "This file is create by Invoke-DscResource"
        Contents        = <<-CONTENTS
            Today...
            ...the dow is: ${data.pwsh_script.dow.outputs["dow_short"]}
            This file is create by Invoke-DscResource...
            ...by way of Terraform!
            CONTENTS
    }
}

# resource "pwsh_dsc_resource" "file3" {
#     module_name    = "PSDesiredStateConfiguration"
#     module_version = "0.0"
#     type_name      = "MSFT_FileDirectoryConfiguration"

#     properties = {
#         DestinationPath = "${local.dsctest}\\_IGNORE\\File3.txt"
#         Contents        = <<-CONTENTS
#             The second file is at ${pwsh_dsc_resource.file2.results.DestinationPath}
#             It was updated at ${pwsh_dsc_resource.file2.results.ModifiedDate}
#             CONTENTS
#     }
# }

output "dsc-file-res" {
    value = {
        props            = pwsh_dsc_resource.file1.properties
        results          = pwsh_dsc_resource.file1.results
        required_reboot  = pwsh_dsc_resource.file1.required_reboot
        all_creds        = pwsh_dsc_resource.file1.all_creds
    }
}

resource "pwsh_dsc_resource" "many_files" {
    count = 3

    module_name    = "PSDesiredStateConfiguration"
    module_version = "0.0"
    type_name      = "MSFT_FileDirectoryConfiguration"

    properties = {
        DestinationPath = "${local.dsctest}\\_IGNORE\\many-${count.index}.txt"
        Contents        = <<-CONTENTS
            This is file #${count.index}

            This file is create by Invoke-DscResource...
            ...by way of Terraform!
 
            Today...
            ...the dow is: ${data.pwsh_script.dow.outputs["dow_short"]}
            CONTENTS
    }
}
