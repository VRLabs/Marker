# Marker

[![Generic badge](https://img.shields.io/badge/Unity-2019.4.31f1-informational.svg)](https://unity3d.com/unity/whats-new/2019.4.31)
[![Generic badge](https://img.shields.io/badge/SDK-AvatarSDK3-informational.svg)](https://vrchat.com/home/download)
[![Generic badge](https://img.shields.io/badge/License-MIT-informational.svg)](https://github.com/VRLabs/Marker/blob/main/LICENSE)
[![Generic badge](https://img.shields.io/github/downloads/VRLabs/Marker/total?label=Downloads)](https://github.com/VRLabs/Marker/releases/latest)

A marker for drawing.

<div>
  <img src="https://raw.githubusercontent.com/VRLabs/Marker/main/Media/Main.jpeg" width="100%">
  <br/>
</div>

## How it works

The "Draw" particle system emits particles for drawing. The "Eraser" collider kills the particles. Particles are emitted in a custom simulation space so the drawing can be manipulated.

## Install guide

https://user-images.githubusercontent.com/45078979/148662754-c6b64c0f-690f-495f-b012-f7da803c393e.mp4

Drag the Marker.cs script onto your avatar. You can customize settings for installing the marker. Some settings have tooltips for explanation.

After generating the marker, the ink and eraser emit from MarkerTarget.

Adjust the MarkerTarget transform by entering playmode with the emulator and enabling T-Pose Calibration. Enable the marker via its submenu. For the index finger setup, make sure MarkerTarget is positioned on the tip of your index finger. For the handheld pen setup, move and rotate MarkerTarget so your hand correctly holds the pen. MarkerTarget may also be freely scaled, if needed.

When finished adjusting MarkerTarget, copy its transform component to paste its values outside of playmode.

Click "Finish Setup" to finalize your marker and remove the script from your avatar.

## Credit

[ksivl](https://github.com/ksivl)

## Downloads

You can grab the latest version of the Marker in [Releases](https://github.com/VRLabs/Marker/releases/latest).

## License

Marker is available as-is under MIT. For more information see [LICENSE](https://github.com/VRLabs/Marker/blob/main/LICENSE).

## Contact us

If you need help, our support channel is on [Discord](https://discord.vrlabs.dev).
