# vika
Visual Interpreter of Kooky Analysis.
Also means 'bug' in Finnish.

[![Build status](https://ci.appveyor.com/api/projects/status/3rd6pj5qqk1349ne?svg=true)](https://ci.appveyor.com/project/laedit/vika)

## What it is
Right now it's just a tiny tool which parse analysis reports and send messages to the build server, or in console if it's not executed on a build server.

You can use it like this: `NVika buildserver "inspectcodereport.xml"`

### additional params:
 - --debug: active the debug category on logger, useful for debugging
 - --includesource: include the build server name in messages

## Analysis tools
### Supported
 - [InspectCode](https://chocolatey.org/packages/resharper-clt): example of usage `inspectcode /o="inspectcodereport.xml" "Vika.sln"`
 
### To come
 - FxCop
 - CodeCracker
 - StyleCop
 - NDepend
 - DupFinder (if someone wants it reaaaally bad)
 
## Build servers
### Supported
  - [AppVeyor](http://appveyor.com)
![AppVeyor example](AppVeyor.png)
  
### To come
 - TeamCity?
 - ContinuaCI?
 - MyGet?

I really wondering if there is any value to supporting these three, because there doesn't support to add build message like AppVeyor but only log message.
And they support custom HTML report, so an xsl transformation is enough.
 
## What it will be
A website will be added for displaying a nice and shiny aggregated report from all source to a dedicated page for each GitHub project.

There also will be a solution to upload a temporary report stored for a week.

And the client may push reports through the website public API.