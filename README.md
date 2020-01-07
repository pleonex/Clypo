# Clypo ![GitHub Workflow Status](https://img.shields.io/github/workflow/status/pleonex/clypo/Build) ![GitHub](https://img.shields.io/github/license/pleonex/Clypo)

**Clypo** is a tool to export and import the content of the Nintendo 3DS layout
files: BCLYT (_Binary CTR LaYouT_).

The tool runs in Windows, Linux and MacOS. You can get the latest version
from the [GitHub release page](https://github.com/pleonex/Clypo/releases/latest).

## Usage

### Export

```plain
Clypo.Console.exe export input.bclyt output.yml output.po
```

### Export directory recursively

```plain
Clypo.Console.exe export-dir input output
```

### Import

```plain
Clypo.Console.exe import input.yml input.po original.bclyt output.bclyt
```

### Import directory recursively

```plain
Clypo.Console.exe import-dir original input output
```

## License

This software is license under the [MIT](https://choosealicense.com/licenses/mit/) license.

The specification of the BCLYT is based on assembly research in the game
_Attack of the Friday Monsters_ and information from [3dbrew](https://www.3dbrew.org/wiki/CLYT_format)
