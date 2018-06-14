# Intersector Tester
Application for testing Intersector class - dependency on Unity library prevents testing on local machine.

### Tests
The application runs the following tests and displays their output:
* Test 1: basic vector creation.
* Test 2: converting vector from world coordinates to local coordinates. 
* Test 3: creating 'view vector' from position vector in local coordinates
* Test 4: intersection on unit vectors with limited view-field, no occlusion.
* Test 5: large scale intersection calculation for speed estimates.
* Test 6: calulating size of grid required for occlusion approximation.
* Test 7: basic occlusion test with known occluded and non-occluded points.
* Test 8: large scale occlusion for approximation accuracy estimates.

![picture alt](../master/imgs/IntersectorTesterScreenshot.jpg "screenshot")

### Building on Local Machine
1. Import [Unity project assets](https://github.com/VUSE-Hololens/assets/tree/master/IntersectorTester) and HoloToolkit package.
2. Apply Mixed Reality Toolkit Project and Scene settings.
3. Add following prefabs to project:
	1. TextControlContainer
	2. IntersectorContainer
	3. Diagnostics
	4. VisualizerContainer
	5. CoordinateSystems
4. Within TextControlContainer/TextControl set:
	1. TextContainer to Diagnostics
	2. IntersectorContainer (field) to IntersectorContainer (prefab)
5. Build and deploy.