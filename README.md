# Clypo ![GitHub Workflow Status](https://img.shields.io/github/workflow/status/pleonex/clypo/Build) ![GitHub](https://img.shields.io/github/license/pleonex/Clypo)

**Clypo** is a tool to export and import the content of the Nintendo 3DS layout
files: BCLYT.

## Usage

### Export

```plain
clypo.exe export input.bclyt output.yml output.po
```

### Export directory recursively

```plain
clypo.exe export-dir input output
```

### Import

```plain
clypo.exe import input.yml input.po original.bclyt output.bclyt
```

### Import directory recursively

```plain
clypo.exe import-dir original input output
```

## License

This software is license under the [MIT](https://choosealicense.com/licenses/mit/) license.

The specification of the BCLYT is based on assembly research in the game
_Attack of the Friday Monsters_ and information from [3dbrew](https://www.3dbrew.org/wiki/CLYT_format)
