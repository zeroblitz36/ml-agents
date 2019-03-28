# Creating custom protobuf messages

Unity and Python communicate by sending protobuf messages to and from each other. You can create custom protobuf messages if you want to exchange structured data beyond what is included by default. 

Assume the ml-agents repository is checked out to a folder named $MLAGENTS_ROOT. Whenever you change the fields of a custom message, you must run `$MLAGENTS_ROOT/protobuf-definitions/make.bat` to create C# and Python files corresponding to the new message. Follow the directions in [that file](../protobuf-definitions/README.md) for guidance. After running it, reinstall the Python package by running `pip install $MLAGENTS_ROOT/ml-agents` and make sure your Unity project is using the newly-generated version of `$MLAGENTS_ROOT/UnitySDK`.

## Custom message types

There are three custom message types currently supported, described below. In each case, `env` is an instance of a `UnityEnvironment` in Python. `CustomAction` is described most thoroughly; usage of the  other custom messages follows a similar template. 

### Custom actions

By default, the Python API sends actions to Unity in the form of a floating-point list per agent and an optional string-valued text action. 

You can define a custom action type to replace or augment this by adding fields to the `CustomAction` message, which you can do by editing the file `protobuf-definitions/proto/mlagents/envs/communicator_objects/custom_action.proto`. 

Instances of custom actions are set via the `custom_action` parameter of `env.step`. An agent receives a custom action by defining a method with the signature

```csharp
public virtual void AgentAction(float[] vectorAction, string textAction, CommunicatorObjects.CustomAction customAction)
```

Here is an example of creating a custom action that instructs an agent to choose a cardinal direction to walk in and how far to walk. 

`custom_action.proto` will look like 

```protobuf
syntax = "proto3";

option csharp_namespace = "MLAgents.CommunicatorObjects";
package communicator_objects;

message CustomAction {
    enum Direction {
        NORTH=0;
        SOUTH=1;
        EAST=2;
        WEST=3;
    }
    float walkAmount = 1;    
    Direction direction = 2;
}
```

In your Python file, create an instance of a custom action:

```python
from mlagents.envs.communicator_objects import CustomAction
env = mlagents.envs.UnityEnvironment(...)
...
action = CustomAction(direction=CustomAction.NORTH, walkAmount=2.0)
env.step(custom_action=action)
```

Then in your agent,

```csharp
...
using MLAgents;
using MLAgents.CommunicatorObjects;

class MyAgent : Agent {
    ...
    override public void AgentAction(float[] vectorAction, string textAction, CustomAction customAction) {
        switch(customAction.Direction) {
            case CustomAction.Types.Direction.North:
                transform.Translate(0, 0, customAction.WalkAmount);
                break;
            ...
        }
    }
}
```

Note that the protobuffer compiler automatically configures the capitalization scheme of the C# version of the custom field names you defined in the `CustomAction` message to match C# conventions - "NORTH" becomes "North", "walkAmount" becomes "WalkAmount", etc.

### Custom reset parameters

By default, you can configure an environment `env ` in the Python API by specifying a `config` parameter that is a dictionary mapping strings to floats. 

You can also configure an environment using a custom protobuf message. To do so, add fields to the `CustomResetParameters` protobuf message in `custom_reset_parameters.proto`, analogously to `CustomAction` above. Then pass an instance of the message to `env.reset` via the `custom_reset_parameters` keyword parameter.

In Unity, you can then access the `customResetParameters` field of your academy to accesss the values set in your Python script.

In this example, an academy is setting the initial position of a box based on custom reset parameters that looks like 

```protobuf
message CustomResetParameters {
    message Position {
        float x = 1;
        float y = 2;
        float z = 3;
    }
    message Color {
        float r = 1;
        float g = 2;
        float b = 3;
    }
    Position initialPos = 1;
    Color color = 2;
}
```

In your academy, you'd have something like

```csharp
public class MyAcademy : Academy
{
    public GameObject box;  // This would be connected to a game object in your scene in the Unity editor.

    override public void AcademyReset()
    {
        var boxParams = customResetParameters;
        if (boxParams != null)
        {
            var pos = boxParams.InitialPos;
            var color = boxParams.Color;
            box.transform.position = new Vector3(pos.X, pos.Y, pos.Z);
            box.GetComponent<Renderer>().material.color = new Color(color.R, color.G, color.B);
        }
    }
}
```

Then in Python, when setting up your scene, you might write

```python
from mlagents.envs.communicator_objects import CustomResetParameters
env = ...
pos = CustomResetParameters.Position(x=1, y=1, z=2)
color = CustomResetParameters.Color(r=.5, g=.1, b=1.0)
params = CustomResetParameters(initialPos=pos, color=color)
env.reset(custom_reset_parameters=params)
```

### Custom observations

By default, Unity returns observations to Python in the form of a floating-point vector. 

You can define a custom observation message to supplement that. To do so, add fields to the `CustomObservation` protobuf message in `custom_observation.proto`. 

Then in your agent, create an instance of a custom observation via `new CommunicatorObjects.CustomObservation`. Then in `CollectObservations`, call `SetCustomObservation` with the custom observation instance as the parameter.

In Python, the custom observation can be accessed by calling `env.step` or `env.reset` and accessing the `custom_observations` property of the return value. It will contain a list with one `CustomObservation` instance per agent.

For example, if you have added a field called `customField` to the `CustomObservation` message, you would program your agent like


```csharp
class MyAgent : Agent {
    override public void CollectObservations() {
        var obs = new CustomObservation();
        obs.CustomField = 1.0;
        SetCustomObservation(obs);
    }    
}
```

Then in Python, the custom field would be accessed like 

```python
...
result = env.step(...)
result[brain_name].custom_observations[0].customField
```

where `brain_name` is the name of the brain attached to the agent.