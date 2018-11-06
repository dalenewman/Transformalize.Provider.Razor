nuget pack Transformalize.Provider.Razor.nuspec -OutputDirectory "c:\temp\modules"
nuget pack Transformalize.Provider.Razor.Autofac.nuspec -OutputDirectory "c:\temp\modules"
nuget pack Transformalize.Transform.Razor.nuspec -OutputDirectory "c:\temp\modules"
nuget pack Transformalize.Transform.Razor.Autofac.nuspec -OutputDirectory "c:\temp\modules"

nuget push "c:\temp\modules\Transformalize.Provider.Razor.0.3.12-beta.nupkg" -source https://api.nuget.org/v3/index.json
nuget push "c:\temp\modules\Transformalize.Provider.Razor.Autofac.0.3.12-beta.nupkg" -source https://api.nuget.org/v3/index.json
nuget push "c:\temp\modules\Transformalize.Transform.Razor.0.3.12-beta.nupkg" -source https://api.nuget.org/v3/index.json
nuget push "c:\temp\modules\Transformalize.Transform.Razor.Autofac.0.3.12-beta.nupkg" -source https://api.nuget.org/v3/index.json