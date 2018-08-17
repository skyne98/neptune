# Neptune
Cross-platform 🎮 game engine for .NET Core

## Current status
As for the moment, the rendering part of the engine is nearly done. However, during the development it was discovered that the technique I am using right now for 2d rendering doesn't allow you to have semi-transparent sprites with arbitrary z-indexes. That means that I will have to ditch some of very important optimizations, such as instancing, grouping and etc, in order to remove that restriction.

### What are the possible solutions to this?
First of all, as I have just mentioned, the rendering pipeline can go back to rendering sprites one-by-one, based on the z-ordered list. Some new optimizations can be implemented to mitigate some newly-introduced nasty problems, such as constantly rebinding the textures, however, not being able to use instancing will hurt performance a lot.
On the other hand, there is an alternative -- linked-lists of transparencies stored on the GPU for each pixel, using some advanced modern interlocking techniques implemented by the current standards. This solution will allow to save all the current optimizations with the cost of some interlocking action and some GPU memory. Although this solution sounds very promising, I don't have enough experience in writing complex GPU programs to efficiently use that technique.

### Why do you care about performance so much?
My current goal is to have 1,000,000 constantly transforming small sprites being rendered at 60 fps on the average mobile dedicated graphics card such as GTX 850M. I know this is possible and I have already achieved it using the optimizations mentioned above, however, I you could have already understood, they will have to be dramatically overhauled and performance might be lost.
Back to the question: why do I care? I care because I want this engine/library to be a proof that C#/.NET Core can be a fast, portable and cross-platform solution for future games.

### How can I help?
If you want to help, there are a bunch of things that have to be discussed, modified or even completely reimplemented. You can contact me at [ahalahan@gmail.com](mailto:ahalahan@gmail.com).
Some the main tasks right now are:
* Solve the transparency problem
* Chose how coordinate systems should behave
* Check the support for different image formats
* Improve rendering API to be somewhere as accessible as PIXI JS APIs

### How can I motivate you to work on this?
You can always support me on [![](https://img.shields.io/badge/patreon-donate-green.svg)](https://www.patreon.com/magentafox) to give me some motivation to work on this project in my free time after work (writing proprietary software).
