Thanks for using NVika.MSBuild!

/!\ Warning for existing users, this version has breaking changes:
 - netcore 3.1 is necessary
 - SARIF version has passed to 2.1, old version isn't supported anymore, please upgrade your ErrorLog to version 2 like in sample below
-------------------------------------------------------------------

This add a Target which is invoked automatically after all other targets of the project.
It call the nvika command line with some parameters:

 - reports to analyze: provided by <NVikaReports>;
                       Automatically populated for FxCop and Roslyn Analyzers if they are activated and NVikaReports is empty;
                       if there are no reports, the command line is not called
 - includesource: added if NVikaIncludeSource is 'true'
 - treatwarningsaserrors: added if TreatWarningsAsErrors is 'true'


 Example of configuration in an MSBuild project file:

 <PropertyGroup>
    <RunCodeAnalysis>true</RunCodeAnalysis> <!-- FxCop is activated-->
    <ErrorLog>analysisresult.sarif.json;version=2</ErrorLog> <!-- generates output file for Roslyn Analyzers -->
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors> <!-- all warnings during build are considered as errors, including those reported by NVika -->
    <NVikaIncludeSource>true</NVikaIncludeSource> <!-- add source in NVika messages -->
 </PropertyGroup>

 Add file manually:
 
 <ItemGroup>
    <NVikaReports Include="$(ErrorLog)" />
    <NVikaReports Include="$(OutputPath)MyProject.exe.CodeAnalysisLog.xml" />
    <NVikaReports Include="./reports/GendarmeReport.xml" />
 </ItemGroup>