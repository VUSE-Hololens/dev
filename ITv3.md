# Intersector Tester v3
Further tests on Intersector Class

### Changes Since v2
* Occlusion: Occlusion approximation is calcuated and visualized.

### Functionality
Generates a cube of random points. A frustum originating at the user's eyes is projected onto the points, and points are colored differently if they are in the frustum and non-occluded, in the fruthum but occluded or out of the frustum.
A target helps visualized the frustum FOV as well as occlusion grid resolution. A panel displays counts of points in each category.

![picture alt](../master/imgs/ITv3screenshot.jpg "screenshot")

### Building on Local Machine
1. Import [Unity project assets](https://github.com/VUSE-Hololens/assets/tree/master/ITv3) and HoloToolkit package.
2. Apply Mixed Reality Toolkit Project and Scene settings.
3. Add following prefabs to project:
	1. IT
	2. DiagnosticsText
	3. DiagnosticsBackground
4. Within IT/VertexDriver:
	1. Drag appropriate materials from HoloToolkit/UX/Materials to In View, Occluded, Out View and Line Materials.
	2. Drag DiagnosticsText to VVLabelText
	3. Drag VVLabelBackground (prefab) to VVLabelBackground (field)
5. Within IT/DiagnosticsDriver:
	1. Drag DiagnosticsText (prefab) to DiagnosticsText (field)
	2. Drag DiagnosticsBackground (prefab) to DiagnosticsBackground (field)
6. Build and deploy.