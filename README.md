# Stomp
Experimental image glitcher in C#.

This is not really release-ready software, so don't expect user friendliness.

[![comparison preview](https://raw.githubusercontent.com/hexafluoride/Stomp/master/comparison-small.jpg)](https://raw.githubusercontent.com/hexafluoride/Stomp/master/comparison.png)
click to see full size

## Building Stomp
Either run
```
xbuild
```
or
```
mdtool build
```
at the repo root. Output binary will be located at Stomp/bin/`<Config>`/Stomp.exe where `<Config>` is the build configuration(Debug or Release, Debug by default).

## Running Stomp
```
./Stomp.exe image.png
```

It's that simple(for now).

## Writing filter chains
### The hard-coded way:

Stomp works with `FilterChain` objects, which as the name suggests, contain a list of `IFilter` objects. An example FilterChain is shown below:

```cs
var chain = new FilterChain
(
      new BitDepth() { BitsPerChannel = 3 },
      new ScanLines() { PreserveBrightness = true },
      new ChromaShift() { RedShift = -10, GreenShift = 10, BlueShift = 30 },
      new RandomGaps() { GapCount = 30, Behavior = GapBehavior.Random, MinGapLength = -100, MaxGapLength = 100 },
      new Saturation() { Intensity = 2 },
      new PngFiltered(new FilterChain(
          new RandomBytes() { Rate = 0.0001 }
      )) { Behavior = FilterTypeGen.Constant, ConstantType = FilterType.Paeth }
);
```

`BitDepth`, `ScanLines`, `ChromaShift` and so on are classes that implement `IFilter`.

When this chain gets executed via `chain.Apply(image)`, all of these filters will be executed sequentially.

Here's the output from the above filter chain, which is pretty self-explanatory:

```
[+] Executed filter bit-depth in 0.14 seconds.
[+] Executed filter scanlines in 0.36 seconds.
[+] Executed filter chroma-shift in 0.43 seconds.
[+] Executed filter random-gaps in 0.04 seconds.
[+] Executed filter saturate in 0.71 seconds.
[=] Entering context png-filter-context
  [+] Encoded in 0.468 seconds.
    [+] Executed filter random-bytes in 0.00 seconds.
  [+] Decoded in 0.341 seconds.
[=] Exiting context png-filter-context
```

### The scripted way
Stomp now has a scripting engine, and can parse filter chains from text.
Filters are defined as:
```
filter {
      property-1 = 0
      property-2 = true
}
```
This will add the filter called "filter" into the root filter chain with properties "property-1" and "property-2".

Contexts can also take other filters as properties:
```
context {
      filter {
            property-1 = 1
      }
      filter {
            property-1 = 2
      }
      property-0 = true
}
```

Here's a real working example, equivalent to the hard-coded example above:
```
bit-depth {
      bits-per-channel = 3
}
scanlines {
      preserve-brightness = true
}
chroma-shift {
      red-shift = -10
      green-shift = 10
      blue-shift = 30
}
random-gaps {
      gap-count = 30
      behavior = random
      min-gap-length = -100
      max-gap-length = 100
}
saturate {
      intensity = 2
}
png-filter-context {
    random-bytes {
        rate = 0.0001
    }
    png-filter-gen = constant
    constant-filter = paeth
}
```

The script aliases of the names and properties are denoted with a ScriptAlias attribute before each property in the implementations. Check the source files out if you want to know what name to use for a filter property inside a script.

## Contexts?!?!
You may have noticed something interesting going on in the above example. The last filter in that chain is a `PngFiltered` filter, which takes another `FilterChain` in its constructor. What?

These classes are called _contexts_. Contexts take the image, perform some transformations on it, and then execute an inner filter chain on the transformed image data.

In that example, the `PngFiltered` context encodes the image using PNG pre-filtering with the Paeth subtype, and then executes its own `FilterChain` on the encoded image data, which can lead to some pretty interesting effects. 

## Filter parameters
Each filter takes a bunch of parameters for configurations, which will eventually be documented somewhere. Not today, though. At the moment, most filters are pretty simple and the parameter names are self-explanatory. <sup><sub>_the code IS the documentation_</sup></sub>
