# Image Synthesis for Machine Learning #
This project's aim is to help Machine Learning and Computer Vision researchers with generation of annotated training sets in Unity.

### Rationale ###

One of the main challenges in Machine Learning is the task of getting large amounts of training data in the right format. Deep learning, and machine learning more generally, needs huge training sets to work properly.

Virtual worlds can provide a wealth of training data. However it must consist of more than just the final image: object categorization, optical flow, etc

### What does it do? ###

This repository contains code that is easy to add to any existing Unity project. It allows to save the following data as .png images with minimal intrusion:
* Image segmentation - color encoded unique object identifier (InstanceID)
* Object categorization - color encoded object Layer or Tag
* Optical flow - based on Motion Vectors
* Depth
* ... and more in the future

Process of capturing data does not require any content modification. 

Data can be previewed in the Editor (in Play mode) via Display drop down in Game view. 'Display 1' provides the final image as usual, while 'Display 2' to 'Display 5' - displays the additional data like image segmentation, optical flow, etc.

### How to use ###

* Copy contents (C# scripts and shaders) of the repository into your Unity project
* Call GetComponent<ImageSynthesis>().Save(framename) or use ExampleUI to save data for single frame
* Optional: call GetComponent<ImageSynthesis>().OnSceneChange, if new objects were added at runtime
* No further changes to content are necessary

### How does it work ###


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
* twitter: @__ReJ__