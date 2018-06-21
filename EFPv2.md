# External Feed Pathway Tester v2
Tester for EVP simulated external sensor. Tests SpatialMappingObserver, SpatialMappingManager, MeshManager, EFPDriver, Intersector and VoxelGridManager and Visualizer classes.

### Changes Since v1
The following changes were made since EFP Tester v1:
* Mesh/sensor-projection intersection: only non-occluded vertices are updated in voxel grid, previously all cached vertices were updated.
* Simulated external sensor: external feed input data simulated, previously updates were set to default value.

### Functionality
Simulated data from a dummy external sensor positioned at the user's head is projected onto the mesh, with intersected vertices and their corresponding external feed values stored in a voxel grid. 
Optionally intersected vertices can be rendered and colored according to their projected value. A diagnostic panel is also displayed.
Eventually a real external sensor will be used and rendering will be done via the mesh texture itself.

![picture alt](../master/imgs/EFPTesterv2screenshot2.jpg "screenshot")

The app is driven by the Unity GameObject EFP, which has the following components: EFPDriver, SpatialMappingObserver, SpatialMappingManager, and MeshManager.
Each cycle EFPDriver calls MeshManager to update a list of mesh vertices, which it does by polling SpatialMappingObserver and compiling all vertices from meshes it estimates as visible using an Intersector object.
EFPDriver then uses its own Intersector object to determine which mesh vertices are actually visible and non-occluded, and then calculate their projected values from the simulated external sensor.
Next EFPDriver maintains a voxel grid of those points and values using a VoxelGridManager object, which internally stores them in an Octree.
Finally EFPDriver calls on a Visualizer object to render mesh vertices according to their project values. Optionally, MeshManager can also use its own Visualizer object to render bounding boxes around each individual mesh.

The Unity GameObject Diagnostics drives the diagnostics panel. The DiagnosticsControl component polls various EFP components each cycle for metadata then displays them on the diagnostics panel.

### Building on Local Machine
1. Import [Unity project assets](https://github.com/VUSE-Hololens/assets/tree/master/EFP%20Tester%20v2) and HoloToolkit package.
2. Apply Mixed Reality Toolkit Project and Scene settings.
3. Add following prefabs to project:
	1. EFP
	2. Diagnostics
	3. Lighting
4. Within both EFP/EFPDriver and EFP/MeshManager:
	1. Set DefaultMaterial to Unity's built-in default material.
	2. Assign all other inspector variables as desired.
5. Within Diagnostics/DiagnosticsControlContainer/DiagnosticsControl:
	1. Set DiagnosticsText (field) to DiagnosticsText (GameObject)
	2. Set EFPContainer (field) to EFP (GameObject)
6. Build and deploy.
