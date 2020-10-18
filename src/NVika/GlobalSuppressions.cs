// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CC0021:Use nameof", Justification = "Not from the type 'Issue'", Scope = "member", Target = "~M:NVika.Parsers.InspectCodeParser.Parse(System.Xml.Linq.XDocument)~System.Collections.Generic.IEnumerable{NVika.Parsers.Issue}")]
[assembly: SuppressMessage("Design", "CC0021:Use nameof", Justification = "Not from the property 'Name'", Scope = "member", Target = "~M:NVika.Parsers.FxCopParser.Parse(System.Xml.Linq.XDocument)~System.Collections.Generic.IEnumerable{NVika.Parsers.Issue}")]
[assembly: SuppressMessage("Design", "CC0021:Use nameof", Justification = "Not from the property 'Name'", Scope = "member", Target = "~M:NVika.Parsers.GendarmeParser.Parse(System.Xml.Linq.XDocument)~System.Collections.Generic.IEnumerable{NVika.Parsers.Issue}")]
[assembly: SuppressMessage("Design", "CC0021:Use nameof", Justification = "Not from the property 'Name'", Scope = "member", Target = "~M:NVika.Parsers.GendarmeParser.GetCategory(System.Collections.Generic.IEnumerable{System.Xml.Linq.XElement},System.String)~System.String")]
