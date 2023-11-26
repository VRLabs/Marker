<div align="center">

# Marker

[![Generic badge](https://img.shields.io/github/downloads/VRLabs/Marker/total?label=Downloads)](https://github.com/VRLabs/Marker/releases/latest)
[![Generic badge](https://img.shields.io/badge/License-MIT-informational.svg)](https://github.com/VRLabs/Marker/blob/main/LICENSE)
[![Generic badge](https://img.shields.io/badge/Unity-2019.4.31f1-lightblue.svg)](https://unity3d.com/unity/whats-new/2019.4.31)
[![Generic badge](https://img.shields.io/badge/SDK-AvatarSDK3-lightblue.svg)](https://vrchat.com/home/download)

[![Generic badge](https://img.shields.io/discord/706913824607043605?color=%237289da&label=DISCORD&logo=Discord&style=for-the-badge)](https://discord.vrlabs.dev/)
[![Generic badge](https://img.shields.io/endpoint.svg?url=https%3A%2F%2Fshieldsio-patreon.vercel.app%2Fapi%3Fusername%3Dvrlabs%26type%3Dpatrons&style=for-the-badge)](https://patreon.vrlabs.dev/)

A marker for drawing

![Marker-Draw](https://github.com/VRLabs/Marker/assets/76777936/e64f034a-f2a5-467a-b698-b383164e5422)
![Marker-Remove](https://github.com/VRLabs/Marker/assets/76777936/a6c657d3-19e1-4e59-950b-bf6d63096fd6)

### ⬇️ [Download Latest Version](https://github.com/VRLabs/Marker/releases/latest)

<!-- 
### 📦 [Add to VRChat Creator Companion]() -->

</div>

---

## How it works

The "Draw" particle system emits particles for drawing. The "Eraser" collider kills the particles. Particles are emitted in a custom simulation space so the drawing can be manipulated.

## Install guide

https://user-images.githubusercontent.com/45078979/148662754-c6b64c0f-690f-495f-b012-f7da803c393e.mp4

Drag the Marker.cs script onto your avatar. You can customize settings for installing the marker. Some settings have tooltips for explanation.

After generating the marker, the ink and eraser emit from MarkerTarget.

Adjust the MarkerTarget transform by entering playmode with the emulator and enabling T-Pose Calibration. Enable the marker via its submenu. For the index finger setup, make sure MarkerTarget is positioned on the tip of your index finger. For the handheld pen setup, move and rotate MarkerTarget so your hand correctly holds the pen. MarkerTarget may also be freely scaled, if needed.

When finished adjusting MarkerTarget, copy its transform component to paste its values outside of playmode.

Click "Finish Setup" to finalize your marker and remove the script from your avatar.

## Contributors

[ksivl](https://github.com/ksivl)

## License

Marker is available as-is under MIT. For more information see [LICENSE](https://github.com/VRLabs/Marker/blob/main/LICENSE).

​

<div align="center">

[<img src="https://github.com/VRLabs/Resources/raw/main/Icons/VRLabs.png" width="50" height="50">](https://vrlabs.dev "VRLabs")
<img src="https://github.com/VRLabs/Resources/raw/main/Icons/Empty.png" width="10">
[<img src="https://github.com/VRLabs/Resources/raw/main/Icons/Discord.png" width="50" height="50">](https://discord.vrlabs.dev/ "VRLabs")
<img src="https://github.com/VRLabs/Resources/raw/main/Icons/Empty.png" width="10">
[<img src="https://github.com/VRLabs/Resources/raw/main/Icons/Patreon.png" width="50" height="50">](https://patreon.vrlabs.dev/ "VRLabs")
<img src="https://github.com/VRLabs/Resources/raw/main/Icons/Empty.png" width="10">
[<img src="https://github.com/VRLabs/Resources/raw/main/Icons/Twitter.png" width="50" height="50">](https://twitter.com/vrlabsdev "VRLabs")

</div>
