# Included filter chains
Stomp already has a few included filter chains for you to use without any configuration whatsoever. All included chains are detailed below.

The sample original image is this:

![le funny blonde politician](https://raw.githubusercontent.com/hexafluoride/Stomp/master/misc/trump.png)

## default-filter
default-filter imitates "real" PNG datamoshing by randomly flipping bytes inside a PngFiltered context.

Output:

![default-filter output](https://raw.githubusercontent.com/hexafluoride/Stomp/master/misc/default.png)

## fake-glitches
fake-glitches is, as its name suggests, mostly not "real" datamoshing, but instead it applies a bunch of deterministic algorithms and a random-gaps filter for variety.

Output:

![fake-glitches output](https://raw.githubusercontent.com/hexafluoride/Stomp/master/misc/fake.png)
