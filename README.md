# PersistencyManager

Abstract: this will be a library that can be included by other mods to be able to improve newtwork peristency in a easy and unified way.

Usage: All the using mod has to do include and intialize the persistency manager and to implement MoveItIntergration https://github.com/Quboid/CS-MoveIt/blob/master/MoveItIntegration/Integration.cs. many mods already implement move it integration so that is a good start :). only the following methods are needed from MoveItIntegration: Copy/Paste Encode/Decode

Details: 
This mod will persist data in the following scenarios all in one place:

- [ ] when move it copies stuff to new place (obviously!)

- [ ] when user loads/saves game

- [ ] when user loads/saves intersections in asset editor

- [ ] when user splits segments or moves node using the road tool.

- [ ] when user splits/merges segments using 3rd party mod (e.g. NMT or Roundabout Builder)

This will be a stand alone library. the using mod need to include this library in their own code and Enable/Disable it when their own mod is enabled/disabled.

**particular features of this mod can be disabled.**
