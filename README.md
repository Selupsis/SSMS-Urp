Screen Space Multiple Scattering for Unity's Universal Rendering Pipeline
=========================================================================
A port of OCASMS implementation of SSMS and their modified version of Unity's Global Fog.

Original Repo:



Unity thread with more info: https://forum.unity3d.com/threads/screen-space-multiple-scattering.446647/

Disclaimer: 
I am a beginning game developer, I don't yet have a clue how the URP or the SSMS/Global Fog really work - I merley adopted them (hrhr)
I don't know if everything works 100% (see list of known issues)

Setup (Note: I set up a demo project to test and verify the procedure, I don't know in which versions lower than the one I used it will work!)
--------------------------------------------------------------------------

* Create a new URP project in Unity (I used 2019.4.0f1, I think 2019.3.xx is needed as minimum)
>* You can delete all ExampleAssets if you want, or use them as a background 

* In the Package Manager, make sure the following packages have the right version:
>* Lightweight RP (7.3.1)

* Open the "SSMS-Urp.unitypackage" by doubleclicking onto it, and import at least the scripts and shaders!
>* The rest of the content is a fully set up demo scene, including a simple Camera Controller, Render Pipeline Settings and ForwardRenderer, which are set up as described below!

* Under "Edit->Project Settings->Graphics":
>* Make sure under **Scriptable Render Pipeline Settings" an UniversalRenderPipelineAsset is linked (it is normally created by the URP-template)
>* Double-Click the item to jump to the according settings of the asset

* In the asset, make sure there is a ForwardRenderData in the "Renderer List"
>* Next, check "Depth Texture" in the "General" Settings! This is a very important step, as the SSMS- and Globalfog-effect both need the depth-information for rendering their effects correctly!

* Open the settings of the forward renderer (either by double-clicking onto it in the "Renderer List", or by finding it in the "Asset"-Folder of the project
>* Click onto "Add Renderer Feature" and pick "SSMS Render Feature"
>* This adds the effect, and should immediatley show a blurred image in the editor
>* Under it's settings you can start to play around with the values
>* Next, click onto "Add Renderer Feature" again and pick the "SSMS Global Fog Renderer Feature"

* Now, here it gets a bit weird - if you just add the effect, everything will just turn black in your scene (in the editor and player)
* To get the fog effect to work, you **once** have to set the standard unity fog-settings to something that looks good to you, then you can deactivate them again. Only then the fog works...
>* Go to "Window->Rendering->Lighting Settings"
>* Navigate to "Other settings"
>* Check the "Fog"-checkbox
>* Pick a color (e.g. white)
>*  Set the mode e.g. to "Exponential" and the density to "0.01" - the fog effect will suddenly work!

Known Issues
-------------------------------------------------------------------------------

* Both the fog and SSMS might have one of the following issues in the **editor window**:
>* The fog does not render at all. I have not found out why yet, if you start playback in the editor or build and play, it works. I think the camera used in the editor has some internally different settings and thus has trouble rendering the fog...
>>* Perhaps it is also some sideeffect of using some other shaders/components, as it happens in my personal game project, but not in the sample-scene I set up for this manual, so who knows...
>>* As a workaround, I provided a setting for the fog called "Editor Render Pass Event". It defines, where in the rendering pipeline the effect is rendered for the editor, and if set to something like "Before rendering opaques" might not render the fog at all. But for my personal game project it means that the rest of my game is not black anymore...

License
-------

Copyright (C) 2015, 2016 Keijiro Takahashi, OCASM

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

[KinoBloom]: https://github.com/keijiro/KinoBloom
[CAVE]: http://www.cs.columbia.edu/CAVE/projects/ptping_media/

[ImageA1]: http://i.imgur.com/UbIjTt7.jpg
[ImageA2]: http://i.imgur.com/hgJ4Beo.png
[ImageA3]: http://i.imgur.com/6ykjXOI.png
[ImageA4]: http://i.imgur.com/fPkvPFQ.png

[ImageB1]: http://i.imgur.com/AFxiAIG.png
[ImageB2]: http://i.imgur.com/qcZCJpF.png
[ImageB3]: http://i.imgur.com/nEIF2B3.png
