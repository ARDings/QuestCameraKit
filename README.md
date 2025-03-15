QuestCameraKit is a collection of template and reference projects demonstrating how to use Meta Quest’s new **Passthrough Camera API** `(PCA)` for advanced AR/VR vision, tracking, and shader effects.

[![Support on Patreon](https://img.shields.io/badge/Become%20a%20Patron-orange?logo=patreon&style=flat-square)](https://www.patreon.com/c/blackwhalestudio)
[![Follow on X](https://img.shields.io/twitter/follow/xrdevrob?style=social)](https://x.com/xrdevrob)

[![Watch the video](https://img.youtube.com/vi/1z3pcMJbnRA/0.jpg)](https://www.youtube.com/watch?v=1z3pcMJbnRA)


Overview
========

1. 🎨 Color Picker
   --------------------------------
- **Purpose:** Convert a 3D point in space to its corresponding 2D image pixel.
- **Description:** This sample shows the mapping between 3D space and 2D image coordinates using the Passthrough Camera API. We use MRUK's EnvironmentRaycastManager to determine a 3D point in our environment and map it to the location on our WebcamTexture. We then extract the pixel on that point, to determine the color of a real world object.

2. 🍎 Object Detection with Unity Sentis
   --------------------------------
- **Purpose:** Convert 2D screen coordinates into their corresponding 3D points in space.
- **Description:** Use the Unity Sentis framework to infer different ML models to detect and track objects. Learn how to convert detected image coordinates (e.g. bounding boxes) back into 3D points for dynamic interaction within your scenes. In this sample you will also see how to filter labels. This means e.g. you can only detect humans and pets, to create a more safe play-area for your VR game. The sample video below is filtered to monitor, person and laptop. The sample is running at around `60 fps`.

| 1. 🎨 Color Picker                          | 2. 🍎 Object Detection                      |
|---------------------------------------------|---------------------------------------------|
| ![CPE](Media/ColorPicker_Environment.gif)   | ![OBJD](Media/ObjectDetection.gif)          |

3. 📱 QR Code Tracking with ZXing
   --------------------------------
- **Purpose:** Detect and track QR codes in real time. Open webviews or log-in to 3rd party services with ease.
- **Description:** Similarly to the object detection sample, get QR code coordinated and projects them into 3D space. Detect QR codes and call their URLs. You can select between a multiple or single QR code mode. The sample is running at around `70 fps` for multiple QR codes and a stable `72 fps` for a single code.

4. 🪟 Frosted Glass Shader
   --------------------------------
- **Purpose:** Apply a custom frosted glass shader effect to virtual surfaces.
- **Description:** A shader which takes our camera feed as input to blur the content behind it.
- **`Todo`**: We have a shader that correctly maps the camera texture onto a quad, and we have one vertical blur shader and one horizontal blur shader. Ideally we would combine all of these into one shader effect to be able to easily apply it to meshes or UI elements.

| 3. 📱 QR Code Tracking                | 4. 🪟 Frosted Glass                   |
|---------------------------------------|---------------------------------------|
| ![QR Code](Media/QRCodeTracking.gif)  | ![Frosted](Media/FrostedGlass.gif)    |

5. 🧠 OpenAI vision model
   --------------------------------
- **Purpose:** Ask OpenAI's vision model (or any other multi-modal LLM) for context of your current scene.
- **Description:** We use a the OpenAI Speech to text API to create a coommand. We then send this command together with a screenshot to the Vision model. Lastly, we get the response back and use the Text to speech API to turn the response text into an audio file in Unity to speak the response. The user can select different speakers, models, and speed. For the command we can add additional instructions for the model, as well as select an image, image & text, or just a text mode. The whole loop takes anywhere from `2-6 seconds`, depending on the internet connection.

https://github.com/user-attachments/assets/a4cfbfc2-0306-40dc-a9a3-cdccffa7afea

Getting Started with PCA
===============

| **Information**        | **Details**                                                                                                                                                                                             |
|------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Device Requirements**| - Only for Meta `Quest 3` and `3s`<br>- `HorizonOS v74` or later                                                                                                                                              |
| **Unity WebcamTexture**| - Access through Unity’s WebcamTexture<br>- Only one camera at a time (left or right), a Unity limitation                                                                                               |
| **Android Camera2 API**| - Unobstructed forward-facing RGB cameras<br>- Provides camera intrinsics (`camera ID`, `height`, `width`, `lens translation & rotation`)<br>- Android Manifest: `horizonos.permission.HEADSET_CAMERA`          |
| **Public Experimental**| Apps using PCA are not allowed to be submitted to the Meta Horizon Store yet.                                                                                                                           |
| **Specifications**     | - Frame Rate: `30fps`<br>- Image latency: `40-60ms`<br>- Available resolutions per eye: `320x240`, `640x480`, `800x600`, `1280x960`                                                                         |

Prerequisites
--------------
- **Meta Quest Device:** Ensure you are runnning on a `Quest 3` or `Quest 3s` and your device is updated to `HorizonOS v74` or later.
- **Unity:** Recommended is `Unity 6`. Also runs on Unity `2022.3. LTS`.
- **Camera Passthrough API does not work in the Editor or XR Simulator.**
- Get more information from the [Meta Quest Developer Documentation](https://developers.meta.com/horizon/documentation/unity/unity-pca-documentation)

> [!CAUTION]
> Every feature involving accessing the camera has significant impact on your application's performance. Be aware of this and ask yourself if the feature you are trying to implement can be done any other way besides using cameras.

Installation
-------------
1. **Clone the Repository:**
   ```
   git clone https://github.com/yourusername/QuestVisionKit.git
   ```

2. **Open the Project in Unity:**
Launch Unity and open the cloned project folder.

3. **Configure Dependencies:**
Follow the instructions in the section below to run one of the samples.

Running the Samples
===================

1. **[Color Picker](https://github.com/xrdevrob/QuestCameraKit/edit/main/README.md#-color-picker)**
- Open the `ColorPicker` scene.
- Build the scene and run the APK on your headset.
- Aim the ray onto a surface in your real space and press the A button or pinch your fingers to observe the cube changing it's color to the color in your real environment.

2. **[Object Detection with Unity Sentis](https://github.com/xrdevrob/QuestCameraKit/edit/main/README.md#-color-picker)**
- Open the `ObjectDetection` scene.
- You will need [Unity Sentis](https://docs.unity3d.com/Packages/com.unity.sentis@2.1/manual/get-started.html) for this project to run (com.unity.sentis@2.1.2).
- Select the labels you would like to track. No label means all objects will be tracked.
- Build the scene and run the APK on your headset. Look around your room and see how tracked objects receive a bounding box in accurate 3D space.

Below you can see all the labels that are provided:

<table>
  <tr>
    <td>person</td>
    <td>bicycle</td>
    <td>car</td>
    <td>motorbike</td>
    <td>aeroplane</td>
    <td>bus</td>
    <td>train</td>
    <td>truck</td>
  </tr>
  <tr>
    <td>boat</td>
    <td>traffic light</td>
    <td>fire hydrant</td>
    <td>stop sign</td>
    <td>parking meter</td>
    <td>bench</td>
    <td>bird</td>
    <td>cat</td>
  </tr>
  <tr>
    <td>dog</td>
    <td>horse</td>
    <td>sheep</td>
    <td>cow</td>
    <td>elephant</td>
    <td>bear</td>
    <td>zebra</td>
    <td>giraffe</td>
  </tr>
  <tr>
    <td>backpack</td>
    <td>umbrella</td>
    <td>handbag</td>
    <td>tie</td>
    <td>suitcase</td>
    <td>frisbee</td>
    <td>skis</td>
    <td>snowboard</td>
  </tr>
  <tr>
    <td>sports ball</td>
    <td>kite</td>
    <td>baseball bat</td>
    <td>baseball glove</td>
    <td>skateboard</td>
    <td>surfboard</td>
    <td>tennis racket</td>
    <td>bottle</td>
  </tr>
  <tr>
    <td>wine glass</td>
    <td>cup</td>
    <td>fork</td>
    <td>knife</td>
    <td>spoon</td>
    <td>bowl</td>
    <td>banana</td>
    <td>apple</td>
  </tr>
  <tr>
    <td>sandwich</td>
    <td>orange</td>
    <td>broccoli</td>
    <td>carrot</td>
    <td>hot dog</td>
    <td>pizza</td>
    <td>donut</td>
    <td>cake</td>
  </tr>
  <tr>
    <td>chair</td>
    <td>sofa</td>
    <td>pottedplant</td>
    <td>bed</td>
    <td>diningtable</td>
    <td>toilet</td>
    <td>tvmonitor</td>
    <td>laptop</td>
  </tr>
  <tr>
    <td>mouse</td>
    <td>remote</td>
    <td>keyboard</td>
    <td>cell phone</td>
    <td>microwave</td>
    <td>oven</td>
    <td>toaster</td>
    <td>sink</td>
  </tr>
  <tr>
    <td>refrigerator</td>
    <td>book</td>
    <td>clock</td>
    <td>vase</td>
    <td>scissors</td>
    <td>teddy bear</td>
    <td>hair drier</td>
    <td>toothbrush</td>
  </tr>
</table>

3. **[QR Code Tracking](https://github.com/xrdevrob/QuestCameraKit/edit/main/README.md#-color-picker)**
- Open the `QRCodeTracking` scene to test real-time QR code detection and tracking.
- You will need to install [NuGet for Unity](https://github.com/GlitchEnzo/NuGetForUnity)
- After installing NuGet for Unity you will have a new Menu `NuGet`. Click on it and then on `Manage NuGet Packages`. Search for the [ZXing.Net package](https://github.com/micjahn/ZXing.Net/) from Michael Jahn and install it.
- Build the scene and run the APK on your headset. Look at a QR code to see the marker in 3D space and URL of the QR code.
  
**Troubleshooting**: If you ever get the error below, make sure in your `Player Settings` under `Scripting Define Symbols` you see `ZXING_ENABLED`.
  ```
  The type or namespace name 'ZXing' could not be found (are you missing a using directive or an assembly reference?)
  ```

4. **[Frosted Glass Shader](https://github.com/xrdevrob/QuestCameraKit/edit/main/README.md#-color-picker)**
- Open the `FrostedGlass` scene.
- Build the scene and run the APK on your headset.
- Look at the panel from different angles and observe how objects behind it are blurred.

**Troubleshooting**: If you cannot see the blur effect, make sure in your render asset the `Opaque Texture` check-box is checked. 

> [!WARNING]  
> The Meta Project Setup Tool (PST) will show a warning and tell you to uncheck it, so do not fix this warning.

5. **[OpenAI vision model & voice commands](https://github.com/xrdevrob/QuestCameraKit/edit/main/README.md#-color-picker)**
- Open the `ImageLLM` scene.
- Make sure to create an [API key](https://platform.openai.com/api-keys) and enter it in the `OpenAI Manager prefab`.
- Select your desired model and optionally give the LLM some instructions.
- Make sure your headset is connected to the internet (the faster the better).
- Build the scene and run the APK on your headset.

> [!NOTE]  
> File uploads are currently limited to `25 MB` and the following input file types are supported: `mp3`, `mp4`, `mpeg`, `mpga`, `m4a`, `wav`, and `webm`.

Below you can see all supported languages. You can send commands and receive results in any of these languages:
<table>
  <tr>
    <td>Afrikaans</td>
    <td>Arabic</td>
    <td>Armenian</td>
    <td>Azerbaijani</td>
    <td>Belarusian</td>
    <td>Bosnian</td>
    <td>Bulgarian</td>
    <td>Catalan</td>
    <td>Chinese</td>
  </tr>
  <tr>
    <td>Croatian</td>
    <td>Czech</td>
    <td>Danish</td>
    <td>Dutch</td>
    <td>English</td>
    <td>Estonian</td>
    <td>Finnish</td>
    <td>French</td>
    <td>Galician</td>
  </tr>
  <tr>
    <td>German</td>
    <td>Greek</td>
    <td>Hebrew</td>
    <td>Hindi</td>
    <td>Hungarian</td>
    <td>Icelandic</td>
    <td>Indonesian</td>
    <td>Italian</td>
    <td>Japanese</td>
  </tr>
  <tr>
    <td>Kannada</td>
    <td>Kazakh</td>
    <td>Korean</td>
    <td>Latvian</td>
    <td>Lithuanian</td>
    <td>Macedonian</td>
    <td>Malay</td>
    <td>Marathi</td>
    <td>Maori</td>
  </tr>
  <tr>
    <td>Nepali</td>
    <td>Norwegian</td>
    <td>Persian</td>
    <td>Polish</td>
    <td>Portuguese</td>
    <td>Romanian</td>
    <td>Russian</td>
    <td>Serbian</td>
    <td>Slovak</td>
  </tr>
  <tr>
    <td>Slovenian</td>
    <td>Spanish</td>
    <td>Swahili</td>
    <td>Swedish</td>
    <td>Tagalog</td>
    <td>Tamil</td>
    <td>Thai</td>
    <td>Turkish</td>
    <td>Ukrainian</td>
  </tr>
  <tr>
    <td>Urdu</td>
    <td>Vietnamese</td>
    <td>Welsh</td>
    <td></td>
    <td></td>
    <td></td>
    <td></td>
    <td></td>
    <td></td>
  </tr>
</table>

License
=======

This project is licensed under the MIT License. See the LICENSE file for details. Feel free to use the samples for your own projects, though I would appreciate if you would leave some credits to this repo in your work ❤️

Contact
=======

For questions, suggestions, or feedback, please open an issue in the repository or contact me on [X](https://x.com/xrdevrob), [LinkedIn](https://www.linkedin.com/in/robertocoviello/), or at [roberto@blackwhale.dev](mailto:roberto@blackwhale.dev). Find all my info [here](https://bento.me/xrdevrob) or join our growing XR developer community on [Discord](https://discord.gg/KkstGGwueN).

Acknowledgements & Credits
================

- **Meta** For the Passthrough Camera API and [**Passthrough Camera API Samples**](https://github.com/oculus-samples/Unity-PassthroughCameraApiSamples/).
- Thanks to shader wizard [Daniel Ilett](https://www.youtube.com/@danielilett) for helping me set up the `FrostedGlass` sample.
- Thanks to **[Michael Jahn](https://github.com/micjahn/ZXing.Net/)** for the XZing.Net library used for the QR code tracking samples.
- Thanks to **[Julian Triveri](https://github.com/trev3d/QuestDisplayAccessDemo)** for constantly pushing the boundaries with what is possible with Meta Quest hardware and software.

--------------------------------------------------------------------------------
Happy coding and enjoy exploring the possibilities with QuestCameraKit!
--------------------------------------------------------------------------------
