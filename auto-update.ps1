<#
this script updates all packages in a solution and works around the issue of 
https://github.com/NuGet/Home/issues/1798 where NuGet.exe doesn't really support the -Id option.
This will be resolved in NuGet 3.4

nuget.exe also doesn't contain a -Version option yet, which means you cannot use this script to update to a specific version of the framework.
TODO: We could maybe do this by using the mirror command to put it on some local place first.
#> 

param($solutionName, $nugetsource)

function Get-CSharpProjectFiles()
{
    Get-Content $solutionName |
        Select-String 'Project\(' |
            ForEach-Object {
                $projectParts = $_ -Split '[,=]' | ForEach-Object { $_.Trim('[ "{}]') };
                $projectInfo = New-Object PSObject -Property @{
                    Name = $projectParts[1];
                    File = $projectParts[2];
                    Guid = $projectParts[3]
                }
                
                # get all csproj files from the solution
                $ext = [io.path]::GetExtension($projectInfo.File)
                if($ext -eq ".csproj")
                {
                    $projectInfo.File
                }
            }
}

function Get-PackageVersion ($packages, $packageName)
{
    $packages | ?{$_.id -eq $packageName} | %{$_.version}
}

function Invoke-UninstallScript($packageId, $packageVersion, $csprojPath)
{
    $packageFolder = "$packageId.$packageVersion"
    $toolsPath = "$solutionDir\packages\$packageFolder\tools"
    $uninstallScriptPath = "$toolsPath\Uninstall_via_exe.ps1"
    if (Test-Path $uninstallScriptPath)
    {
        Write-Host "Calling Uninstall_via_exe.ps1 to uninstall $packageFolder. csproj = $csprojPath"
        Invoke-Expression "$uninstallScriptPath $csprojPath $toolsPath $packageId $packageVersion"
    }
    else
    {
        Write-Host "No uninstall script found in $uninstallScriptPath" -foreground "yellow"
    }
}

function Invoke-InstallScript($packageId, $packageVersion, $csprojPath)
{
    $packageFolder = "$packageId.$packageVersion"
    $toolsPath = "$solutionDir\packages\$packageFolder\tools"
    $installScriptPath = "$toolsPath\Install_via_exe.ps1"
    if (Test-Path $installScriptPath)
    {
        Write-Host "Calling Install_via_exe.ps1 to install $packageFolder csproj = $csprojPath"
        Invoke-Expression "$installScriptPath $csprojPath $toolsPath $packageId $packageVersion"
    }
    else
    {
        Write-Host "no install script found in $installScriptPath" -foreground "yellow"
    }
}

function Update-Package($packageId, $packageVersion, $fullcsprojPath, $solutionDir, $nugetsource)
{
    $nugetexe = "$solutionDir\.nuget\nuget.exe"
    
    # run the uninstall script. If the package was not updated, it just runs the install script again.
    Invoke-UninstallScript $packageId $packageVersion $fullcsprojPath
    
    # update the package via nuget.exe
    Write-Host "Invoking nuget update for $fullcsprojPath on $packageId"
    Invoke-Expression "$nugetexe update packages.config -Id $packageId -Prerelease -FileConflictAction Overwrite -ConfigFile $solutionDir\NuGet.config -Source `"$nugetsource`""
    
    [xml]$packagesFile = Get-Content "packages.config"
    $packages = $packagesFile.packages.package
    
    $installedVersion = Get-PackageVersion $packages $packageId
    
    #run the install script
    Invoke-InstallScript $packageId $installedVersion $fullcsprojPath    
}
    
# script starts here
$DSAppPlugin = "DeltaShell.ApplicationPlugin"
$DSTestProject = "DeltaShell.TestProject"
$DSFramework = "DeltaShell.Framework"

start-transcript -path "nuget-update.log"
$solutionDir = Get-Location

try
{
    # restore all missing packages in the solution
    
    $nugetexe = "$solutionDir\.nuget\nuget.exe"

    Invoke-Expression "$nugetexe restore $solutionName"

    # get all csproj files in the solution
    $csprojPaths = Get-CSharpProjectFiles

    # go to that csproj folder and run a nuget update command for DeltaShell.ApplicationPlugin
    ForEach($csprojPath in $csprojPaths)
    {    
        pushd
        
        Write-Host "Updating packages for $csprojPath" -foreground "magenta"
        
        # go to the directory of the csproj, so we can call the packages.config file
        $projectDir = [io.path]::GetDirectoryName($csprojPath)
        cd $projectDir
        
        $fullcsprojPath = "$solutionDir\$csprojPath"
        
        # get all ids of the packages. This is the workaround for https://github.com/NuGet/Home/issues/1798
        # you could normally do "-Id DeltaShell.ApplicationPlugin -Id DeltaShell.TestProject",
        # but it then installs DeltaShell.ApplicationPlugin into the test-project and vice-versa
        if(Test-Path "packages.config")
        {
            [xml]$packagesFile = Get-Content "packages.config"
            $packages = $packagesFile.packages.package
            
            $packageVersion = Get-PackageVersion $packages $DSAppPlugin
            if($packageVersion)
            {
                Update-Package $DSAppPlugin $packageVersion $fullcsprojPath $solutionDir $nugetsource
            }
            $packageVersion = $Null
            $packageVersion = Get-PackageVersion $packages $DSTestProject
            
            if($packageVersion)
            {
                Update-Package $DSTestProject $packageVersion $fullcsprojPath $solutionDir $nugetsource
            }
            $packageVersion = $Null
            $packageVersion = Get-PackageVersion $packages $DSFramework
            if($packageVersion)
            {
                Update-Package $DSFramework $packageVersion $fullcsprojPath $solutionDir $nugetsource
            }
        }
        
        popd
    }
}
catch
{
    Write-Error "An exception occured, exiting with code 1"
    exit 1
}
finally
{
    cd $solutionDir
    stop-transcript
}