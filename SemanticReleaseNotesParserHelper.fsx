#r "./tools/FAKE/tools/FakeLib.dll"

open Fake
open System
open System.Diagnostics
open System.Text
open Microsoft.FSharp.Reflection

/// Containes tasks which allow to call [SemanticReleaseNotesParser](https://github.com/laedit/SemanticReleaseNotesParser)
module SemanticReleaseNotesParser =

    /// Type of the output
    type OutputType =
        /// Generates a file with the name from OutputPath or ReleaseNotes.html by default
        | File
        /// Set an environment variable with the name SemanticReleaseNotes and value of the formatted release notes; support [build servers](https://github.com/laedit/SemanticReleaseNotesParser/wiki/Build-Servers-Support).
        | Environment
        /// Make both of the above
        | FileAndEnvironment

    // Format of the output
    type OutputFormat =
        | HTML
        | Markdown

    // Group by
    type GroupByType =
        | Sections
        | Categories

    // Include style
    type IncludeStyleType =
        | No
        | Yes
        | Custom of string

    type SemanticReleaseNotesParserParams = {
        /// Active the debug category on logger, useful for debugging. Default `false`.
        /// Equivalent to the `--debug` option.
        Debug: bool
        /// The path to SemanticReleaseNotesParser. Default search in the "tools" folder.
        ToolPath: string
        /// Maximum time to allow SemanticReleaseNotesParser to run before being killed.
        TimeOut: TimeSpan
        /// Release notes file path to parse (default: ReleaseNotes.md).
        /// Equivalent to the `-r=<filename>` option.
        ReleaseNotesPath: string
        /// Path of the resulting file (default: ReleaseNotes.html).
        /// Equivalent to the `-o=<filename>` option.
        OutputPath: string
        /// Type of output (default: File).
        /// Equivalent to the `-t=<file|environment|fileandenvironment>` option.
        OutputType: OutputType
        /// Format of the resulting file (default: HTML).
        /// Equivalent to the `f=<html|markdown>` option.
        OutputFormat: OutputFormat
        /// Defines the grouping of items (default: Sections).
        /// Equivalent to the `g=<sections|categories>` option.
        GroupBy: GroupByType
        /// Path of the [liquid template](https://github.com/laedit/SemanticReleaseNotesParser/wiki/Format-templating) file to format the result ; Overrides type, format and groupby of output.
        /// Equivalent to the `--template` option.
        TemplatePath: string
        /// Pluralize categories title; works only with GroupBy = Categories (default: false).
        /// Equivalent to the `--pluralizecategoriestitle` option.
        PluralizeCategoriesTitle: bool
        /// Include style in the html output; if no custom style is provided, the default is used (default: No).
        /// Equivalent to the `--includestyle[=<custom style>]` option.
        IncludeStyle: IncludeStyleType
    }

    /// The default option set given to SemanticReleaseNotesParser.
    let SemanticReleaseNotesParserDefaults = {
        Debug = false
        ToolPath = ""
        TimeOut = TimeSpan.FromMinutes 5.
        ReleaseNotesPath = ""
        OutputPath = ""
        OutputType = File
        OutputFormat = HTML
        GroupBy = Sections
        TemplatePath = ""
        PluralizeCategoriesTitle = false
        IncludeStyle = No
    }

    let private findExe toolPath =
        if toolPath |> isNullOrEmpty then
            let found = [
                        Seq.singleton ("tools" @@ "SemanticReleaseNotesParser")
                        pathDirectories
                        ]
                        |> Seq.concat
                        |> Seq.map (fun directory -> directory @@ "SemanticReleaseNotesParser.exe")
                        |> Seq.tryFind fileExists
            if found <> None then found.Value else failwith "Cannot find the SemanticReleaseNotesParser executable"
        else toolPath

    let private toString (x:'a) = 
        match FSharpValue.GetUnionFields(x, typeof<'a>) with
        | case, _ -> case.Name

    /// Runs SemanticReleaseNotesParser.
    ///
    /// ## Parameters
    ///
    ///  - `setParams` - Function used to manipulate the default `SemanticReleaseNotesParserParams` value.
    ///
    /// ## Sample usage
    ///
    ///     Target "BuildReleaseNoets" (fun _ ->
    ///         SemanticReleaseNotesParser.ParseReport (fun p -> { p with OutputFormat = Markdown })
    ///     )
    let Convert setParams =
        traceStartTask "SemanticReleaseNotesParser" String.Empty

        let parameters = setParams SemanticReleaseNotesParserDefaults

        let args = new StringBuilder()
                |> appendIfTrueWithoutQuotes parameters.Debug "--debug"
                |> appendIfNotNullOrEmpty parameters.ReleaseNotesPath "-r="
                |> appendIfNotNullOrEmpty parameters.OutputPath "-o="
                |> appendWithoutQuotes ("-t=" +  (toString parameters.OutputType).ToLower())
                |> appendWithoutQuotes ("-f=" + (toString parameters.OutputFormat).ToLower())
                |> appendWithoutQuotes ("-g=" + (toString parameters.GroupBy).ToLower())
                |> appendIfNotNullOrEmpty parameters.TemplatePath "--template "
                |> appendIfTrueWithoutQuotes parameters.PluralizeCategoriesTitle "--pluralizecategoriestitle"
                |> appendWithoutQuotes (match parameters.IncludeStyle with
                                        | No -> String.Empty
                                        | Yes -> "--includestyle"
                                        | Custom s -> s)
                |> toText

        let result =
            ExecProcess (fun info ->
                info.FileName <- findExe parameters.ToolPath
                info.Arguments <- args) parameters.TimeOut
        if result <> 0 then failwithf "SemanticReleaseNotesParser failed with exit code %i." result
        traceEndTask "SemanticReleaseNotesParser" String.Empty