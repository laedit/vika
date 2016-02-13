#!/bin/bash

mono ./tools/nuget/nuget.exe "install" "FAKE" "-OutputDirectory" "tools" "-ExcludeVersion"
mono ./tools/FAKE.exe "$@"