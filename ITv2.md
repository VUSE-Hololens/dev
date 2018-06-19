# Intersector Tester v2
Further tests on Intersector Class. Does not test occlusion.

### Changes Since v1
* Dynamic tests: Intersection tests is more comprehensive, interactive and insightful

### Functionality
Displays a box with fixed randomly generated points as well as a targeting box that visualizes a frustum originating at the user's eyes and extending in the direction of sight. Points within the frustum are painted differently than points outside of it. A panel displays counts of the number of points inside and outside the frustum.

![picture alt](../master/imgs/ITv2screenshot.jpg "screenshot")

### Building on Local Machine
1. Import [Unity project assets](https://github.com/VUSE-Hololens/assets/tree/master/ITv2) and HoloToolkit package.
2. Apply Mixed Reality Toolkit Project and Scene settings.
3. Add following prefabs to project:
	1. IT
	2. DiagnosticsText
	3. DiagnosticsBackground
4. Within IT/VertexDriver:
	1. Drag appropriate materials from HoloToolkit/UX/Materials to In View, Out View and Line Materials.
	2. Drag DiagnosticsText to VVLabelText
	3. Drag VVLabelBackground (prefab) to VVLabelBackground (field)
5. Within IT/DiagnosticsDriver:
	1. Drag DiagnosticsText (prefab) to DiagnosticsText (field)
	2. Drag DiagnosticsBackground (prefab) to DiagnosticsBackground (field)
6. Build and deploy.