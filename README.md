# TGPublic
A reverse-engineered copy of the public code for the current build of The Genesis Project, as of 16/4/23. 
For easier modding purposes.

=== TUTORIAL ===

So you wanna also extract the code for yourself, right? Here's a basic rundown:
You're gonna need:
- SDK6.0
- ILSpy (https://github.com/icsharpcode/ILSpy/releases)

First, open ILSpy and press the folder button.

![image](https://user-images.githubusercontent.com/130933884/232343520-d7dd9075-54ee-425f-8730-310b3d8d993d.png)

Then, navigate to your Genesis Project folder in Steam, and find a folder called "The Genesis Project_Data." In it, navigate to Managed...

![image](https://user-images.githubusercontent.com/130933884/232343600-04f85b90-1ea9-4c70-a555-57af2862cd2a.png)

...and simply open the file called Assembly-CSharp.dll. It should be all the way at the top.
Boom, there you have it! The code is now accesible. You can right-click the Assembly file and save it in a folder to store it somewhere else, and for easier editing with other programs.

Happy modding, folks!

=========================================


COPY OF THE ORIGINAL LICENSE:
"The levels, 3d assets, sound assets, textures, and other assets INCLUDING particle systems, game object blueprints, and materials but NOT INCLUDING shaders are Copyright 2016 The Genesis Project Team if created expressly for the Genesis Project. If not created expressly for the Genesis Project, they remain copyright of the contributors, who by inclusion of their work in The Genesis Project are granting The Genesis Project a license to non-commercially distribute said work as part of The Genesis Project. If your work has been included in The Genesis Project and you do not wish to grant a license, you may request removal by opening an issue on this repo.

All code including shaders and C# scripts is released under the GPLv2.

All build artifacts are Copyright 2016 The Genesis Project, all rights reserved. They may not be distributed without explicit permission from the project."
