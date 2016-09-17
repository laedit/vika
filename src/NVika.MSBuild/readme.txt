Thanks for using NVika.MSBuild!

This add a Target which is invoked automatically after all other targets of the project.
It call the nvika command line with some parameters:

 - reports to analyze: provided by <NVikaReports>;
                       Automatically populated for FxCop and Roslyn Analyzers if they are activated and NVikaReports is empty;
                       if there are no reports, the command line is not called
 - includesource: added if NVikaIncludeSource is 'true'
 - warningaserror: added if TreatWarningsAsErrors is 'true'


 Example of configuration in an MSBuild project file:

 <PropertyGroup>
    <RunCodeAnalysis>true</RunCodeAnalysis> <!-- FxCop is activated-->
    <ErrorLog>analysisresult.sarif.json</ErrorLog> <!-- generates output file for Roslyn Analyzers -->
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors> <!-- all warnings during build are considered as errors, including those reported by NVika -->
    <NVikaIncludeSource>true</NVikaIncludeSource> <!-- add source in NVika messages -->
 </PropertyGroup>

 Add file manually:
 
 <ItemGroup>
    <NVikaReports Include="$(ErrorLog)" />
    <NVikaReports Include="$(OutputPath)MyProject.exe.CodeAnalysisLog.xml" />
    <NVikaReports Include="./reports/GendarmeReport.xml" />
 </ItemGroup>