# Proximity Sensor
Expansion of SIdev using proximity as simulated sensor data.

### Functionality
Mesh is colored according to proximity. See SIdev for more details.

![picture alt](../master/imgs/PS_ColoredMesh2.jpg "screenshot")

### Building on Local Machine
1. Import [Unity project assets](https://github.com/VUSE-Hololens/assets/tree/master/EFP%20Tester%20v2) and HoloToolkit package.
2. Apply Mixed Reality Toolkit Project and Scene settings.
3. Add following prefabs to project:
	1. EFP
	2. Diagnostics
	4. Menu
	3. Lighting
4. Within both EFP/EFPDriver and EFP/MeshManager:
	1. Set DefaultMaterial to Unity's built-in default material.
	2. Assign all other inspector variables as desired.
5. Within Diagnostics/DiagnosticsControlContainer/DiagnosticsControl:
	1. Set DiagnosticsText (field) to DiagnosticsText (GameObject)
	2. Set EFPContainer (field) to EFP (GameObject)
6. Build and deploy.
