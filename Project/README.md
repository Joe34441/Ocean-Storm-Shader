Diorama scene and all files can be found in:   Assets --> Scenes --> Main

Errors in console are an engine error (see link below).
Though fixed in version 2022.1.22f1 and later, I remained on the same version as is used in the park campus labs.

https://issuetracker.unity3d.com/issues/worker0-texture-creation-failed-and-worker0-nullreferenceexception-errors-appear-when-creating-urp-lit-shader-graph-if-opaque-texture-is-enabled



Tessellation implementation note:
- Can be interacted with when both in and out of play mode
- Must be toggled before shader activates
- Click on the glass parent object and enable "active" boolean, then click somewhere on the glass pane to begin effect
- Can also be enabled through "CTRL + T" during play mode
- DISABLE "active" BOOLEAN AFTER USE, ELSE UNDESIRED OUTCOMES MAY OCCUR


![alt text](https://i.imgur.com/b1WaZgJ.png)