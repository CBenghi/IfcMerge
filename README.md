# IfcMerge

A simple command line application for merging IfcElements of multiple ifc files into one.

## Usage

The following command options are available:

```
  -i, --InputFile     Required. Any number of valid IFC models or a file containing their names. If
                      the inputfile has txt extension each line will be parsed as a single source
                      file to merge.

  -o, --OutputFile    Required. The IFC File to output, the extension chosen determines the format
                      (e.g. ifczip).

  --RetainOwner       retains OwnerHistory ifnormation from the original file, where possible.

  --RetainSpatial     retains space hierarchy objects from the original files, even if they share
                      the same name of others in the merged set.

  --help              Display this help screen.

  --version           Display version information.
  
```

## Warning

This tool is quite simple and assumes (withouut checking) that the project shares the same units. 
In case models don't share the same units the resulting file would present incorrect data.

## History

- v1.2 - Added feature to read files to merge from a file with `txt` extension.
- v1.1 - Initial public release


## Todo

- [ ] Managed project of different units
- [ ] Improve the management of geometric contexts; currently some contexts might not be correctly listed in the project.

## Contributing

PRs are welcome.
