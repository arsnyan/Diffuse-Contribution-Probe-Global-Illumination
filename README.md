# FakeIrradianceGI Unity
This project is about creating fake simulation of global illumination, which doesn't actually use physically-accurate processes for calculation but made for artistic purposes.

Previews and gifs will be added later

### How it works?
It gets all the color information from it's nearest surfaces or `IrradiancePoint`s and blends them together, also it can be affected by shadows.
Further away algorithm will be changed to more precise, but it's not gonna be main solution for GI.

### Can I use it outside of cube scope?
Well, you will be able, but I think that it would be better to firstly implement something aside from raycasts for color detection.

I will implement shader logic only after getting sure that it's optimised for different scenarios. For now I understand that there's many cases this system would be to unrealistic and do things not how it supposed to be.

### Any way to easily place points even now when the shaders ain't ready?
I will implement very simple grid-like solution, which is already existing but needs modifications.

It will be easier as this solution might require tons of `IrradiancePoint`s and the best solution I could think of is projecting points or just using grid pattern.

### What about performance?
It's just a test now, don't expect magical things from it.

I'm not sure how it will work, but overall I'm going to make sure it's optimised enough, as this was the main reason I didn't want to use standard Lightmapper. It's slow.

I think it will be better to get it work statically for big unmovable objects and dynamically on very low-res. You might be thinking it doesn't make sense this way, but this "engine" handling calculations faster as they're easier, so it's a matter of seconds, unlike original Lightmapper.
