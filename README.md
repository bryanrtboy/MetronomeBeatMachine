# MetronomeBeatMachine
A metronome based on the [Unity DSP Time Example](https://docs.unity3d.com/ScriptReference/AudioSettings-dspTime.html). The main difference to the example is an option to turn on/off the metronome and I've added delegates and events so that notes instantiated into the scene will start listening for a beat or downbeat to play.

There are three scenes, the basic Metronome, a random chime scene and a step sequencer.  The random scene has a few controls to alter the randomness of the sounds. The step sequences is an implentation for the Editor using UIElements and includes the ability to save sequence patterns to a .json file including the sound file that should be triggered. 

The step sequencer is a fairly complex example of how to build things with UIElements, attaching callbacks and events in the Editor. Also, how to read/write .json files that include Prefabs assigned in the Editor. Future goals would be to implement a similar interface for in game saving and playing.  

The animation shows the new .json files that are created. They are stored using the [Application.persistentDataPath](https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html), which is different for each OS. Note – the Library folder where this file is stored is normally hidden on a Mac. In the Finder, select Finder>Go and hold down the option key to see the Library folder.

Demo animation for how the step sequencer works in the Editor.

![alt text](stepSequenceDemo.gif "Demo of how the beat step sequencer")
