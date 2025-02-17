# SpaceMaths
An AR space strategy game that trains your maths, with voice control powered by IBM Watson.

## Build Instructions
Requirements:
- [Unity Editor 2020.3.x](https://unity3d.com/get-unity/download) with Android Build Support modules.
- [ARCore supported devices](https://developers.google.com/ar/devices).

This project is built with Unity Editor 2020.3.35f1. To build this project:
1. Clone this repo to a local folder and open with Unity Editor.
2. Navigate to File/Build Setting, change the platform to Android and click "Switch Platform".
3. Click "Build" and specify a path for your .apk file.
4. Install the apk to your android phone and enjoy the game!

## Run the game inside the Unity Editor
This game can be run without any AR support inside the Unity Editor. What you need to do is open `StartingScene` under "Assets/Scenes/" and click the "play" button.

To move the camera when playing without AR:
- Navigate to "Debug Camera" in the scene hierarchy.
- Enable the "Active" option in the "Free Fly Camera" component.

*Warning: Interaction via clicking is not available when the fly camera is enabled, if you want to click any button, you should disable the fly camera first*


## Team Members
- Arthur Ng([arthurng](https://github.com/arthurtng))
- Crystal Poon([pcypoon](https://github.com/pcypoon))
- Fangxu Chu([Jimmy-True](https://github.com/Jimmy-True))
- Junting Lai([Travisljt](https://github.com/Travisljt))
- Mingzhe Deng([Desmond121](https://github.com/Desmond121))

## Credits
- Plain Spaceship models from TechLevel (https://sketchfab.com/3d-models/spaceships-69d7d84573be4e358d92d63e9cee4e50)
- Gameboard assets used from Kenney (https://www.kenney.nl/assets/survival-kit)
- Background music from Pix (https://www.youtube.com/watch?v=G2nmOULeOBQ&list=LL&index=26)
- Voxel Mechas Collection by maxparata (https://maxparata.itch.io/voxel-mechas)
- Question Panel assets from Unity assert store (https://assetstore.unity.com/)
- Sc-Fi Music from Theo Prior Music (https://assetstore.unity.com/publishers/58542)
- Dark Magic from MGWSoundDesign (https://assetstore.unity.com/publishers/27115)
- Retro Noisy Explosion Sound Pack Lite from hiro (https://assetstore.unity.com/publishers/13951)
- Shooting Sound from B.G.M (https://assetstore.unity.com/publishers/9381)
- UI Samples from Unity Technologies (unity3d.com)
