# External Feed Pathway Tester v1
Tester for EVP without external feed or projection. Tests SpatialMappingObserver, SpatialMappingManager, MeshManager, EFPDriver and VoxelGridManager.

### Functionality
EFPDriver pulls a list of all cached spatial mapping data from SpatialMappingObserver via SpatialMappingManager then MeshManager. EFP Driver then pushes all vertices to VoxelGridManager to be set in the voxel grid with a default value attached. TextControl pulls and displays process metadata.
The app renders a wireframe of visible meshes as well as a panel of process diagnostics.

![picture alt](../master/imgs/EFPTesterv1screenshot.jpg "screenshot")

### Building on Local Machine
1. Import [Unity project assets](https://github.com/VUSE-Hololens/assets/tree/master/EFP%20Tester%20v1) and HoloToolkit package.
2. Apply Mixed Reality Toolkit Project and Scene settings.
3. Add following prefabs to project:
	1. HoloToolkit/UX/Prefabs/3DTextPrefab
	2. EFP
	3. TextControlContainer
4. Add cube GameObject to serve as text background (if desired).
5. Within TextControlContainer/TextControl:
	1. Set Text Container to 3DTextPrefab.
	2. Set EFP Container to EFP.
6. Build and deploy.
