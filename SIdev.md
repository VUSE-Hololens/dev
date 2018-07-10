# Sensor Integrator Dev
Development app for Sensor Integrator platform. Integrates a simulated sensor.

### Functionality
Simulated data from a dummy external sensor positioned at the user's head is projected onto the mesh, with intersected vertices and their corresponding external feed values stored in a voxel grid. 
The mesh is rendered, colored according to each vertice's value in the voxel grid.
Optionally intersected vertices can be rendered as spheres colored according to their projected value. A diagnostic panel can also be displayed. Bounding boxes can optionally be visualized around individual mesh objects.
The mesh rendering can be switched to uncolored wireframe. A menu with sliders allows dynamic adjustment of mesh density and refresh rate, occlusion resolution and voxel grid resolution.

![picture alt](../master/imgs/SIdev.jpg "screenshot")

The app is driven by the Unity GameObject EFP, which has the following components: EFPDriver, SpatialMappingObserver, SpatialMappingManager, and MeshManager.
Each cycle EFPDriver calls MeshManager to update which cached meshes in within the sensor's FOV, which it does using an Intersector object. Mesh caching is handled by SpatialMappingObserver.
EFPDriver then uses its own Intersector object to determine which mesh vertices are actually visible and non-occluded, and then calculates their projected values from the simulated external sensor.
Next EFPDriver maintains a voxel grid of those points and values using a VoxelGridManager object, which internally stores them in an Octree.
Finally EFPDriver calls on a Visualizer object to a) render mesh vertices according to their project values or b) color the mesh object itself with the same scheme (depends on rendering settings). Optionally, MeshManager can also use its own Visualizer object to render bounding boxes around each individual mesh.

The GameObject Menu drives the menu. It controls rendering options, mesh density and refresh rate, occlusion and voxel grid resolution.

The Unity GameObject Diagnostics drives the diagnostics panel. The DiagnosticsControl component polls various EFP components each cycle for metadata then displays them on the diagnostics panel.

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
