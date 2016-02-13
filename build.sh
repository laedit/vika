#!/bin/bash

mono ./tools/nuget.exe "install" "FAKE" "-OutputDirectory" "tools" "-ExcludeVersion"
mono ./tools/FAKE/tools/Fake.exe build.fsx "$@"