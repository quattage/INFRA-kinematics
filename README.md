## INFRA Kinematics 
This repository contains a bunch of code I used for an old unity project. 

I've decided to make this public since I don't intend on using it for anything and I think it feels pretty good. Kinematic character controllers don't get much love (in the mainstream unity tutorial space) and the ones that do exist need lots of fiddling to not feel like ass.

Here's a list of stuff this library supports:

- Kinematic collision detection
- Crouching, jumping, sliding, bhopping, etc.
- Very accurate Titanfall 2 wallruns (leaving wall surfaces needs improvement)
- Swappable movement modes
- "Posession" system that could be extended to allow for the player to control vehicles or enemies
- A somewhat stupid input wrapper system
- Developer console that doesn't look like shit
- Extensible console variable/command system already hooked into the movement stuff (airaccel, friction, sensitivity, slope)

Nothing here was written explicitly to support any particular networking library but doing so should be pretty simple.

You won't find any unity packages here because I've moved to s&box and seriously just do not care to bundle this properly. Implement your own code based on the examples provided here - you'll have a hard time copying the entire repo into your unity project without something breaking.