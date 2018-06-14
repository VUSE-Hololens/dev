# External Feed Pathway Tester v2
Tester for EVP without actual external feed. Tests SpatialMappingObserver, SpatialMappingManager, MeshManager, EFPDriver, Intersector and VoxelGridManager.

### Changes Since v1
The following changes were made since EFP Tester v1:
* Mesh/sensor-projection intersection: Now only non-occluded vertices are updated in voxel grid, previously all cached vertices were updated.
* Simulated external sensor: external feed input data simulated, previously updates were set to default value.
* Better diagnostics: more accurate octree memory estimate, more detailed speed information.
* Easier local build: more comphrehensive prefabs included in assets.

### Functionality
EFPDriver pulls a list of all cached spatial mapping data from SpatialMappingObserver via SpatialMappingManager then MeshManager. EFPDriver then pushes spatial mapping data to Intersector, which calculates non-occluded mesh vertices and their projected sensor value. EFPDriver then pushes non-occlused vertex-value pairs to VoxelGridManager to be set in the voxel grid. TextControl pulls and displays process metadata.
The app renders a wireframe of visible meshes as well as a panel of process diagnostics.

![picture alt](../master/imgs/EFPTesterv2Screenshot.jpg "screenshot")

### Building on Local Machine
1. Import [Unity project assets](https://github.com/VUSE-Hololens/assets/tree/master/EFP%20Tester%20v2).
2. Apply Mixed Reality Toolkit Project and Scene settings.
3. Add following prefabs to project:
	1. EFP
	2. Diagnostics
	3. TextControlContainer
4. Within TextControlContainer/TextControl:
	1. Set Text Container to Diagnostics/DiagnosticsText.
	2. Set EFP Container to EFP.
5. Build and deploy.
