# Sensor Update Dev
Test app for external sensor data update methods.

### Functionality
Continually produces buffer of random byte values representing continuously updated sensor data pushed to Hololens using DataProducer.

Displays buffer on 'pixel grid' of spherical GameObjects for testing purposes using DataDisplayer.

### Building on Local Machine
1. Import and HoloToolkit package [Unity project assets](https://github.com/VUSE-Hololens/assets/tree/master/SensorUpdateDev).
2. Apply Mixed Reality Toolkit Project and Scene settings.
3. Add following prefabs to scene:
	1. ProducerPrefab
	2. DisplayerPrefab
	3. LightingPrefab
4. Within ProducerPrefab:
	1. Drag in DataProducer.cs as a component (if not already there)
	2. Set desired values for StartingHeight and StartingWidth
5. Within DisplayerPrefab:
	1. Drag in DataDisplayer.cs as a component (if not already there)
	2. Set appropriate values for all inspector variables
6. Build and deploy.