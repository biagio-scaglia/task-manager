<#
.SYNOPSIS
Adds the current user to the docker-users Windows group to allow managing Docker without requiring elevated UAC privileges.

.DESCRIPTION
This script must be run as Administrator once. It modifies the local group membership.
After running this script, the user MUST log out and log back in (or reboot) for changes to take effect.
#>

if (!([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Write-Warning "[SECURITY CHECK] Please run this script as Administrator to configure Docker access."
    Exit
}

try {
    # Check if docker-users group exists
    $group = Get-LocalGroup -Name 'docker-users' -ErrorAction SilentlyContinue
    if (!$group) {
        Write-Warning "Docker Desktop might not be installed natively or the 'docker-users' group doesn't exist."
        Exit 1
    }

    $user = $env:USERNAME
    
    # Check if user is already in group
    $members = Get-LocalGroupMember -Group 'docker-users' | Select-Object -ExpandProperty Name
    $isMember = $false
    foreach ($m in $members) {
        if ($m -match $user) {
            $isMember = $true
            break
        }
    }

    if ($isMember) {
        Write-Host "User '$user' is already a member of the 'docker-users' group." -ForegroundColor Cyan
    } else {
        Add-LocalGroupMember -Group 'docker-users' -Member $user -ErrorAction Stop
        Write-Host "Successfully added '$user' to the 'docker-users' group." -ForegroundColor Green
        Write-Host "NOTE: You must REBOOT or LOG OFF and LOG ON again for Windows to apply the group membership." -ForegroundColor Yellow
    }
} catch {
    Write-Error "An error occurred: $_"
}
