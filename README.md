# Image Synthesis for Machine Learning #
Aim is to help Machine Learning and Computer Vision researchers to generate annotated training sets in Unity.

### Rationale ###

One of the main challenges in Machine Learning is the task of getting large amounts of training data in the right format. Deep learning, and machine learning more generally, needs huge training sets to work properly.

Virtual worlds can provide a wealth of training data. However it must consist of more than just the final image: object categorization, optical flow, etc

### What does it do? ###

This repository contains code that is easy to add to any existing Unity project. It allows to save the following data as .png images with minimal intrusion:

* __Image segmentation__ - color encoded unique object identifier ('InstanceID')
* __Object categorization__ - color encoded object Layer or Tag
* __Optical flow__ - based on Motion Vectors
* __Depth__
* *... and more in the future*

Process of capturing data does not require any content modification. 

Data can be previewed in the Editor (in Play mode) via 'Display' drop down in the Game View. 'Display 1' provides the final image as usual, while 'Display 2' to 'Display 5' - displays the additional data like image segmentation, optical flow, etc.

### How to use ###

1. Copy contents (C# scripts and shaders) of the repository into your Unity project
2. Add `ImageSynthesis` component to your camera(s)
2. Call `GetComponent<ImageSynthesis>().Save(framename)` or attach `ExampleUI` component to save data for a single frame
3. Optional: call `GetComponent<ImageSynthesis>().OnSceneChange`, if new objects were added at runtime
4. No further changes to content should be necessary

### How does it work ###

First of all `ImageSynthesis.OnSceneChange()` calls `ColorEncoding` class to encode unique object idenitifier and layer as RGB color. These colors are stored in `MaterialPropertyBlock` for each object and are automatically passed into the shaders when rendering.

Upon start `ImageSynthesis` component creates hidden camera for every single pass of output data (image segmentation, optical flow, depth, etc). These cameras allow to override usual rendering of the scene and instead use custom shaders to generate the output. These cameras are attached to different displays using `Camera.targetDisplay` property - handy for preview in the Editor.

For __Image segmentation__ and __Object categorization__ pass special replacement shader is set with `Camera.SetReplacementShader()`. It overrides shaders that would be otherwise used for rendering and instead outputs encoded object id or layer.

__Optical flow__ and __Depth__ pass cameras request additional data to be rendered with `DepthTextureMode.Depth` and `DepthTextureMode.MotionVectors` flags. Rendering of these  cameras is followed by drawing full screen quad `CommandBuffer.Blit()` with custom shaders that convert 24/16bit-per-channel data into the 8-bit RGB encoding.

Finally images are readback with `Texture.ReadPixels()` from GPU, compressed with `Texture.EncodePNG()` to PNG format and stored on disk.

Related documentation: [Replacement Shaders](https://docs.unity3d.com/Manual/SL-ShaderReplacement.html), [Command Buffers](https://docs.unity3d.com/Manual/GraphicsCommandBuffers.html), [Depth and Motion Vectors](https://docs.unity3d.com/Manual/SL-CameraDepthTexture.html), [MaterialPropertyBlock](https://docs.unity3d.com/ScriptReference/MaterialPropertyBlock.html)

### To Do ###

* Better example scene(s)
* Script for deterministic playback
* Control from Python
* Figure out if more data like "Albedo", "Normals", "Lighting" or object bounding boxes would be beneficial
* Support custom encoding for "Depth" and "Optical Flow"
* Example how to simulated image defects common to real-world camera (noise)
* Tests

### Who do I talk to? ###

* email: rej@unity3d.com
* twitter: `@__ReJ__`