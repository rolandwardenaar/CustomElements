<#
.SYNOPSIS
    Builds and publishes Blazor WebAssembly custom elements without using MSBuild targets.

.DESCRIPTION
    This script publishes the Calculator.Wasm and DrawingClient.Wasm projects and copies
    the resulting static files to the appropriate Server project wwwroot directories,
    creating stable-name copies of fingerprinted framework JS files.

    This is the PowerShell equivalent of the MSBuild targets in:
    - CalcApp.Server\CalcApp.Server.csproj (PublishCalculatorWasm)
    - DemoApp.Server\DemoApp.Server.csproj (PublishDrawingClientWasm)

.PARAMETER Project
    Which WASM project to build. Valid values: 'Calculator', 'DrawingClient', 'All'.
    Default: 'All'

.PARAMETER Configuration
    Build configuration. Default: 'Release'

.PARAMETER Verbose
    Show detailed output during the build process.

.EXAMPLE
    .\Build-CustomElements.ps1
    # Builds all custom elements

.EXAMPLE
    .\Build-CustomElements.ps1 -Project Calculator
    # Builds only the Calculator custom element

.EXAMPLE
    .\Build-CustomElements.ps1 -Project DrawingClient -Verbose
    # Builds only the DrawingClient custom element with verbose output
#>

[CmdletBinding()]
param(
    [Parameter()]
    [ValidateSet('Calculator', 'DrawingClient', 'All')]
    [string]$Project = 'All',

    [Parameter()]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$ScriptDir = $PSScriptRoot

function Write-Step {
    param([string]$Message)
    Write-Host "▶ $Message" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "✓ $Message" -ForegroundColor Green
}

function Write-Failure {
    param([string]$Message)
    Write-Host "✗ $Message" -ForegroundColor Red
}

function Remove-StaleFiles {
    param(
        [string]$Path,
        [string]$Pattern = '*'
    )
    
    if (Test-Path $Path) {
        $files = Get-ChildItem -Path $Path -Filter $Pattern -File -ErrorAction SilentlyContinue
        foreach ($file in $files) {
            Remove-Item $file.FullName -Force
            Write-Verbose "Removed stale file: $($file.Name)"
        }
    }
}

function Copy-WasmStaticFiles {
    param(
        [string]$SourceWwwroot,
        [string]$DestinationDir
    )
    
    # Ensure destination exists
    if (-not (Test-Path $DestinationDir)) {
        New-Item -Path $DestinationDir -ItemType Directory -Force | Out-Null
    }
    
    # Get all files, excluding .br and .gz pre-compressed variants
    $files = Get-ChildItem -Path $SourceWwwroot -Recurse -File |
        Where-Object { $_.Extension -notin '.br', '.gz' }
    
    foreach ($file in $files) {
        $relativePath = $file.FullName.Substring($SourceWwwroot.Length).TrimStart('\', '/')
        $destPath = Join-Path $DestinationDir $relativePath
        $destDir = Split-Path $destPath -Parent
        
        if (-not (Test-Path $destDir)) {
            New-Item -Path $destDir -ItemType Directory -Force | Out-Null
        }
        
        Copy-Item -Path $file.FullName -Destination $destPath -Force
        Write-Verbose "Copied: $relativePath"
    }
}

function Copy-FingerprintedToStable {
    <#
    .SYNOPSIS
        Creates stable-name copies of fingerprinted framework JS files.
    
    .DESCRIPTION
        blazor.webassembly.js internally does dynamic ES module imports using stable names
        (e.g., import('./dotnet.js')). Since the host app cannot use the WASM project's
        import map, we create stable-name copies of each fingerprinted file.
        
        File mapping:
          blazor.webassembly.HASH.js  →  blazor.webassembly.js
          dotnet.HASH.js              →  dotnet.js
          dotnet.native.HASH.js       →  dotnet.native.js
          dotnet.runtime.HASH.js      →  dotnet.runtime.js
    #>
    param(
        [string]$FrameworkDir
    )
    
    $mappings = @(
        @{ Pattern = 'blazor.webassembly.*.js'; StableName = 'blazor.webassembly.js'; ExcludePattern = $null }
        @{ Pattern = 'dotnet.*.js'; StableName = 'dotnet.js'; ExcludePattern = 'dotnet.native.*|dotnet.runtime.*' }
        @{ Pattern = 'dotnet.native.*.js'; StableName = 'dotnet.native.js'; ExcludePattern = $null }
        @{ Pattern = 'dotnet.runtime.*.js'; StableName = 'dotnet.runtime.js'; ExcludePattern = $null }
    )
    
    foreach ($mapping in $mappings) {
        $files = Get-ChildItem -Path $FrameworkDir -Filter $mapping.Pattern -File -ErrorAction SilentlyContinue |
            Where-Object { $_.Extension -notin '.br', '.gz' }
        
        if ($mapping.ExcludePattern) {
            $files = $files | Where-Object { $_.Name -notmatch $mapping.ExcludePattern }
        }
        
        if ($files -and $files.Count -gt 0) {
            $sourceFile = $files | Select-Object -First 1
            $destPath = Join-Path $FrameworkDir $mapping.StableName
            Copy-Item -Path $sourceFile.FullName -Destination $destPath -Force
            Write-Verbose "Created stable copy: $($mapping.StableName) <- $($sourceFile.Name)"
        }
    }
}

function Publish-CalculatorWasm {
    <#
    .SYNOPSIS
        Publishes Calculator.Wasm and copies files to CalcApp.Server/wwwroot/calculator/
    #>
    
    Write-Step "Publishing Calculator.Wasm custom element..."
    
    $wasmProjectPath = Join-Path $ScriptDir 'Calculator.Wasm\Calculator.Wasm.csproj'
    $serverDir = Join-Path $ScriptDir 'CalcApp.Server'
    $publishDir = Join-Path $serverDir 'obj\wasm-publish'
    $destDir = Join-Path $serverDir 'wwwroot\calculator'
    $frameworkDir = Join-Path $destDir '_framework'
    $serverContentDir = Join-Path $serverDir 'wwwroot\_content'
    
    # 1. Clean publish directory
    if (Test-Path $publishDir) {
        Remove-Item $publishDir -Recurse -Force
        Write-Verbose "Cleaned publish directory: $publishDir"
    }
    
    # 2. Publish WASM project
    Write-Verbose "Running: dotnet publish $wasmProjectPath -c $Configuration"
    $publishResult = & dotnet publish $wasmProjectPath -c $Configuration -o $publishDir --nologo -v quiet 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Failure "Failed to publish Calculator.Wasm"
        Write-Host $publishResult -ForegroundColor Red
        return $false
    }
    
    # 3. Remove stale framework files
    Remove-StaleFiles -Path $frameworkDir
    
    # 4. Copy static files (excluding .br and .gz)
    $sourceWwwroot = Join-Path $publishDir 'wwwroot'
    Copy-WasmStaticFiles -SourceWwwroot $sourceWwwroot -DestinationDir $destDir
    
    # 5. Create stable-name copies of fingerprinted JS files
    Copy-FingerprintedToStable -FrameworkDir $frameworkDir
    
    # 6. Copy _content/ to server's wwwroot root
    $sourceContentDir = Join-Path $sourceWwwroot '_content'
    if (Test-Path $sourceContentDir) {
        Copy-WasmStaticFiles -SourceWwwroot $sourceContentDir -DestinationDir $serverContentDir
    }
    
    Write-Success "Calculator.Wasm published to wwwroot\calculator\ + wwwroot\_content\"
    return $true
}

function Publish-DrawingClientWasm {
    <#
    .SYNOPSIS
        Publishes DrawingClient.Wasm and copies files to DemoApp.Server/wwwroot/drawing-client/
    #>
    
    Write-Step "Publishing DrawingClient.Wasm custom element..."
    
    $wasmProjectPath = Join-Path $ScriptDir 'DrawingClient.Wasm\DrawingClient.Wasm.csproj'
    $serverDir = Join-Path $ScriptDir 'DemoApp.Server'
    $publishDir = Join-Path $serverDir 'obj\wasm-publish'
    $destDir = Join-Path $serverDir 'wwwroot\drawing-client'
    $frameworkDir = Join-Path $destDir '_framework'
    $serverContentDir = Join-Path $serverDir 'wwwroot\_content'
    
    # 1. Clean publish directory
    if (Test-Path $publishDir) {
        Remove-Item $publishDir -Recurse -Force
        Write-Verbose "Cleaned publish directory: $publishDir"
    }
    
    # 2. Publish WASM project
    Write-Verbose "Running: dotnet publish $wasmProjectPath -c $Configuration"
    $publishResult = & dotnet publish $wasmProjectPath -c $Configuration -o $publishDir --nologo -v quiet 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Failure "Failed to publish DrawingClient.Wasm"
        Write-Host $publishResult -ForegroundColor Red
        return $false
    }
    
    # 3. Remove stale framework files
    Remove-StaleFiles -Path $frameworkDir
    
    # 4. Copy static files (excluding .br and .gz)
    $sourceWwwroot = Join-Path $publishDir 'wwwroot'
    Copy-WasmStaticFiles -SourceWwwroot $sourceWwwroot -DestinationDir $destDir
    
    # 5. Create stable-name copies of fingerprinted JS files
    Copy-FingerprintedToStable -FrameworkDir $frameworkDir
    
    # 6. Copy _content/ to server's wwwroot root
    $sourceContentDir = Join-Path $sourceWwwroot '_content'
    if (Test-Path $sourceContentDir) {
        Copy-WasmStaticFiles -SourceWwwroot $sourceContentDir -DestinationDir $serverContentDir
    }
    
    Write-Success "DrawingClient.Wasm published to wwwroot\drawing-client\ + wwwroot\_content\"
    return $true
}

# Main execution
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor DarkCyan
Write-Host " Blazor WebAssembly Custom Elements Builder" -ForegroundColor White
Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor DarkCyan
Write-Host ""

$success = $true

switch ($Project) {
    'Calculator' {
        $success = Publish-CalculatorWasm
    }
    'DrawingClient' {
        $success = Publish-DrawingClientWasm
    }
    'All' {
        $calcSuccess = Publish-CalculatorWasm
        Write-Host ""
        $drawSuccess = Publish-DrawingClientWasm
        $success = $calcSuccess -and $drawSuccess
    }
}

Write-Host ""
if ($success) {
    Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor DarkGreen
    Write-Host " Build completed successfully!" -ForegroundColor Green
    Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor DarkGreen
    exit 0
} else {
    Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor DarkRed
    Write-Host " Build failed!" -ForegroundColor Red
    Write-Host "═══════════════════════════════════════════════════════════════════" -ForegroundColor DarkRed
    exit 1
}
