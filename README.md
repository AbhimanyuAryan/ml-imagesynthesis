# Image Synthesis for Machine Learning #
Aim is to help Machine Learning and Computer Vision researchers to generate annotated training sets in Unity.

### Rationale ###

One of the main challenges in Machine Learning is the task of getting large amounts of training data in the right format. Deep learning, and machine learning more generally, needs huge training sets to work properly.

Virtual worlds can provide a wealth of training data. However it must consist of more than just the final image: object categorization, optical flow, etc

### What does it do? ###

This repository contains code that is easy to add to any existing Unity project. It allows to capture image depth, segmentation, optical flow, etc as .png images with minimal intrusion:

* __Image segmentation__ - each object in the scene gets unique color
* __Object categorization__ - objects are assigned color based on their category
* __Optical flow__ - pixels are colored according to their motion in the relation to camera
* __Depth__ - pixels are colored according to their distance from the camera
* __Normals__ - coming soon!
* *... and more in the future*

![Readme_DescriptionMainImage.png](https://bitbucket.org/repo/5KgjLE/images/1913953980-Readme_DescriptionMainImage.png)
![Readme_Description4Passes.png](https://bitbucket.org/repo/5KgjLE/images/3420646604-Readme_Description4Passes.png)


### Who do I talk to? ###

* email: rej@unity3d.com
* twitter: `@__ReJ__`

### License ###
This repository is covered under the [MIT/X11](LICENSE.TXT) license.