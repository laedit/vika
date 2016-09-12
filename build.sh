#!/bin/bash

mono ./tools/nuget.exe "install" "FAKE" "-OutputDirectory" "tools" "-ExcludeVersion"
mono ./tools/FAKE/tools/FAKE.exe build/build.fsx "$@"