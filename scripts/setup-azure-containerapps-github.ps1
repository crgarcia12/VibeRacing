[CmdletBinding()]
param(
    [string]$Repository,
    [string]$Branch = "main",
    [string]$Location = "eastus",
    [string]$ResourceGroup = "dustracing2d-aca-rg",
    [string]$ContainerAppsEnvironment = "dustracing2d-env",
    [string]$ContainerRegistry,
    [string]$BackendAppName = "dustracing2d-backend",
    [string]$FrontendAppName = "dustracing2d-frontend",
    [string]$IdentityName = "dustracing2d-gha"
)

$ErrorActionPreference = "Stop"

function Require-Command([string]$Name) {
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command '$Name' was not found in PATH."
    }
}

function Get-OriginRepository() {
    $remote = git config --get remote.origin.url
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($remote)) {
        throw "Unable to determine the origin remote URL."
    }

    $remote = $remote.Trim()
    if ($remote -match 'github\.com[:/](?<owner>[^/]+)/(?<repo>[^/]+?)(?:\.git)?$') {
        return "$($Matches.owner)/$($Matches.repo)"
    }

    throw "Origin remote '$remote' is not a GitHub repository URL."
}

function Get-OriginDefaultBranch() {
    $branch = git symbolic-ref --short refs/remotes/origin/HEAD 2>$null
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($branch)) {
        throw "Unable to determine the origin default branch."
    }

    return ($branch -replace '^origin/', '').Trim()
}

function Get-DefaultAcrName([string]$RepoName) {
    $candidate = (($RepoName -replace '[^a-zA-Z0-9]', '') + "acr").ToLowerInvariant()
    if ($candidate.Length -lt 5) {
        $candidate = ($candidate + "dust").Substring(0, 5)
    }

    if ($candidate.Length -gt 50) {
        $candidate = $candidate.Substring(0, 50)
    }

    return $candidate
}

function Set-RepoVariable([string]$Repo, [string]$Name, [string]$Value) {
    gh variable set $Name --repo $Repo --body $Value | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to set GitHub variable $Name."
    }
}

Require-Command az
Require-Command gh
Require-Command git

if ([string]::IsNullOrWhiteSpace($Repository)) {
    $Repository = Get-OriginRepository
}

if ([string]::IsNullOrWhiteSpace($Branch)) {
    $Branch = Get-OriginDefaultBranch
}

$repoParts = $Repository.Split('/', 2)
if ($repoParts.Length -ne 2) {
    throw "Repository must be in OWNER/REPO format."
}

if ([string]::IsNullOrWhiteSpace($ContainerRegistry)) {
    $ContainerRegistry = Get-DefaultAcrName $repoParts[1]
}

gh auth status | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw "GitHub CLI is not authenticated. Run 'gh auth login' first."
}

$account = az account show --output json | ConvertFrom-Json
if ($LASTEXITCODE -ne 0) {
    throw "Azure CLI is not authenticated. Run 'az login' first."
}

az group create --name $ResourceGroup --location $Location --tags app=DustRacing2D managed-by=github-actions | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw "Failed to create or update resource group '$ResourceGroup'."
}

$identityExists = az identity show --name $IdentityName --resource-group $ResourceGroup --query id --output tsv 2>$null
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($identityExists)) {
    az identity create --name $IdentityName --resource-group $ResourceGroup --location $Location | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to create user-assigned managed identity '$IdentityName'."
    }
}

$identity = az identity show --name $IdentityName --resource-group $ResourceGroup --output json | ConvertFrom-Json
if ($LASTEXITCODE -ne 0) {
    throw "Failed to load managed identity '$IdentityName'."
}

$scope = "/subscriptions/$($account.id)/resourceGroups/$ResourceGroup"
$existingRoleAssignment = az role assignment list `
    --assignee-object-id $identity.principalId `
    --scope $scope `
    --role Contributor `
    --query "[0].id" `
    --output tsv

if ($LASTEXITCODE -ne 0) {
    throw "Failed to query role assignments for '$IdentityName'."
}

if ([string]::IsNullOrWhiteSpace($existingRoleAssignment)) {
    az role assignment create `
        --assignee-object-id $identity.principalId `
        --assignee-principal-type ServicePrincipal `
        --role Contributor `
        --scope $scope `
        | Out-Null

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to assign Contributor on '$scope' to '$IdentityName'."
    }
}

$issuer = "https://token.actions.githubusercontent.com"
$subject = "repo:$($Repository):ref:refs/heads/$($Branch)"
$federatedCredentialName = ("github-actions-" + ($Branch -replace '[^a-zA-Z0-9-]', '-')).ToLowerInvariant()
$existingFederatedCredential = az identity federated-credential show `
    --name $federatedCredentialName `
    --identity-name $IdentityName `
    --resource-group $ResourceGroup `
    --output json 2>$null | ConvertFrom-Json

if ($LASTEXITCODE -ne 0) {
    $existingFederatedCredential = $null
}

if ($existingFederatedCredential -and $existingFederatedCredential.subject -ne $subject) {
    az identity federated-credential delete `
        --name $federatedCredentialName `
        --identity-name $IdentityName `
        --resource-group $ResourceGroup `
        | Out-Null

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to delete stale federated credential '$federatedCredentialName'."
    }

    $existingFederatedCredential = $null
}

if (-not $existingFederatedCredential) {
    az identity federated-credential create `
        --name $federatedCredentialName `
        --identity-name $IdentityName `
        --resource-group $ResourceGroup `
        --issuer $issuer `
        --subject $subject `
        --audiences "api://AzureADTokenExchange" `
        | Out-Null

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to create federated credential '$federatedCredentialName'."
    }
}

Set-RepoVariable -Repo $Repository -Name "AZURE_CLIENT_ID" -Value $identity.clientId
Set-RepoVariable -Repo $Repository -Name "AZURE_TENANT_ID" -Value $account.tenantId
Set-RepoVariable -Repo $Repository -Name "AZURE_SUBSCRIPTION_ID" -Value $account.id
Set-RepoVariable -Repo $Repository -Name "AZURE_LOCATION" -Value $Location
Set-RepoVariable -Repo $Repository -Name "AZURE_RESOURCE_GROUP" -Value $ResourceGroup
Set-RepoVariable -Repo $Repository -Name "AZURE_CONTAINERAPPS_ENVIRONMENT" -Value $ContainerAppsEnvironment
Set-RepoVariable -Repo $Repository -Name "AZURE_CONTAINER_REGISTRY" -Value $ContainerRegistry
Set-RepoVariable -Repo $Repository -Name "AZURE_BACKEND_APP_NAME" -Value $BackendAppName
Set-RepoVariable -Repo $Repository -Name "AZURE_FRONTEND_APP_NAME" -Value $FrontendAppName

Write-Host "Configured GitHub Actions OIDC and repository variables for $Repository." -ForegroundColor Green
Write-Host "Default branch subject : refs/heads/$Branch" -ForegroundColor Yellow
Write-Host "Resource group         : $ResourceGroup" -ForegroundColor Yellow
Write-Host "Container Apps env     : $ContainerAppsEnvironment" -ForegroundColor Yellow
Write-Host "Container registry     : $ContainerRegistry" -ForegroundColor Yellow
Write-Host "Backend app            : $BackendAppName" -ForegroundColor Yellow
Write-Host "Frontend app           : $FrontendAppName" -ForegroundColor Yellow
