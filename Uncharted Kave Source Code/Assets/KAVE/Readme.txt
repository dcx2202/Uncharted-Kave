For more information about this package and its development repository visit https://bitbucket.org/neurorehablab/kave/overview
for information about how it works please consult the paper at https://doi.org/10.1145/3229092


This package requires a Kinect v2 and its adapter for windows.

The Kinect SDK must be installed, "Kinect for Windows SDK 2.0" download at:
	https://www.microsoft.com/en-us/download/details.aspx?id=44561
	https://www.microsoft.com/en-us/download/confirmation.aspx?id=44561	(direct link)

The official Kinect2 unity package must also be added to the project,
extract "Kinect.2.0.1410.19000.unitypackage" from "Unity Pro packages", download at:
	https://developer.microsoft.com/en-us/windows/kinect	(Kinect for Windows homepage, download Unity Pro packages)
	https://go.microsoft.com/fwlink/p/?LinkId=513177	(direct link)

Use this software to create a calibration file (instructions included in the archive):
	https://bitbucket.org/neurorehablab/kave/downloads/KAVECalibrator.rar


How to use this package
	You should use the calibration software to generate a file describing your setup. The file "Calibration.xml" will need to be placed at the "StreamingAssets" folder of your project.

	1. Add the KAVE package to your project
	2. Add the VRManager prefab to the scene
	3. Game cameras should be disabled as VRManager will create its own
	4. Place the "Calibration.xml" that the calibration software generated in the "StreamingAssets" folder of your project.

