param($installPath, $toolsPath, $package, $project)

$packagePath = (New-Object system.IO.DirectoryInfo $toolsPath).Parent.FullName
$corePath           = Join-Path $packagePath "lib\net45\SqlServerSpatial.Toolkit.dll"
$coreXmlDocPath     = Join-Path $packagePath "lib\net45\SqlServerSpatial.Toolkit.xml"
$debugVis2013Path   = Join-Path $packagePath "toolkit\SqlServerSpatial.Toolkit.DebuggerVisualizer.VS2013.dll"
$debugVis2015Path   = Join-Path $packagePath "toolkit\SqlServerSpatial.Toolkit.DebuggerVisualizer.VS2015.dll"
$debugVis2017Path   = Join-Path $packagePath "toolkit\SqlServerSpatial.Toolkit.DebuggerVisualizer.VS2017.dll"
$viewerPath         = Join-Path $packagePath "toolkit\SqlServerSpatial.Toolkit.Viewer.exe"

$toolkit = $project.ProjectItems.Item("SqlServerSpatial.Toolkit")

if (!$toolkit)
{
    $toolkit = $project.ProjectItems.AddFolder("SqlServerSpatial.Toolkit")
}

$toolkitVis = $toolkit.ProjectItems | where Name -eq "SqlServerSpatial.Toolkit.DebuggerVisualizer.VS2013.dll"
if (!$toolkitVis)
{
    $toolkitVisLink = $toolkit.ProjectItems.AddFromFile($debugVis2013Path)
}
$toolkitVis2015 = $toolkit.ProjectItems | where Name -eq "SqlServerSpatial.Toolkit.DebuggerVisualizer.VS2015.dll"
if (!$toolkitVis2015)
{
    $toolkitVis2015Link = $toolkit.ProjectItems.AddFromFile($debugVis2015Path)
}

$toolkitVis2017 = $toolkit.ProjectItems | where Name -eq "SqlServerSpatial.Toolkit.DebuggerVisualizer.VS2017.dll"
if (!$toolkitVis2017)
{
    $toolkitVis2017Link = $toolkit.ProjectItems.AddFromFile($debugVis2017Path)
}

$toolkitViewer = $toolkit.ProjectItems | where Name -eq "SqlServerSpatial.Toolkit.Viewer.exe"
if (!$toolkitViewer)
{
    $toolkitViewerLink = $toolkit.ProjectItems.AddFromFile($viewerPath)
}

$readmefile = Join-Path (Split-Path $project.FileName) "SqlServerSpatial.Toolkit\readme.htm"
$dte.ItemOperations.Navigate($readmefile)