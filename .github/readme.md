# OWL

This repository will eventually contain the overall CLI for the OWL project.
However right now, it includes the entire compiler.

## Examples

It can't do much right now, but it can interpret the main example, which is the
[`hoot_hoot.owl`](../src/examples/hoot_hoot.owl) file.

If you'd like to do so, you can run the following in the root of this repository:
```sh
dotnet run --project src/cli -- run src/examples/hoot_hoot.owl
```
This will run the example in debug mode, if you'd like to check the performance
then you should run it in release mode instead, which you can do by running:
 ```sh
dotnet run -c Release --project src/cli -- run src/examples/hoot_hoot.owl
```

The output won't be much, just the prints from the example source code.

But, here's a nicer preview from some debug output:

![preview](res/preview.png)

## Versions

If you'd like to keep track of the compiler's progress and versions, you can
do it with the `version` **command** *(not the flag, since I can't customise
the output of that as nicely yet).*

```sh
dotnet run --project src/cli -- version
```

Which will look something like this:

![version](res/version.png)
