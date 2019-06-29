param($installPath, $toolsPath, $package, $project)

$toolkit = $project.ProjectItems | where Name -eq "SqlServerSpatial.Toolkit"
if($toolkit)
{
    $vis = $toolkit.ProjectItems | where Name -eq "SqlServerSpatial.Toolkit.DebuggerVisualizer.VS2013.dll"
    if ($vis)
    {
		$vis.Delete();
    }
    $vis2015 = $toolkit.ProjectItems | where Name -eq "SqlServerSpatial.Toolkit.DebuggerVisualizer.VS2015.dll"
    if ($vis2015)
    {
		$vis2015.Delete();
    }
    
    $vis2017 = $toolkit.ProjectItems | where Name -eq "SqlServerSpatial.Toolkit.DebuggerVisualizer.VS2017.dll"
    if ($vis2017)
    {
        $vis2017.Delete();
    }
    
    $viewer = $toolkit.ProjectItems | where Name -eq "SqlServerSpatial.Toolkit.Viewer.exe"
    if ($viewer)
    {
		$viewer.Delete();
    }

    if($toolkit.ProjectItems.Count -eq 0)
    {
        $toolkit.Delete()
    }
}