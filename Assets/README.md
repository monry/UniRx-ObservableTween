# UniRx Observable Tween

## Installation

```bash
upm add package dev.monry.unirx-observabletween
```

## Usages

### Basic

Changes an arbitrary struct value from 1st argument to 2nd argument over seconds specified by 3rd argument.

```csharp
ObservableTween
    .Tween(0.0f, 10.0f, 1.0f)
    .Subscribe(x => transform.position = new Vector3(0.0f, x, 0.0f));
```

### Easing

You can specify easing to 4th arugment from listed below.

* Linear
* InQuadratic
* OutQuadratic
* InOutQuadratic
* InCubic
* OutCubic
* InOutCubic
* InQuartic
* OutQuartic
* InOutQuartic
* InQuintic
* OutQuintic
* InOutQuintic
* InSinusoidal
* OutSinusoidal
* InOutSinusoidal
* InExponential
* OutExponential
* InOutExponential
* InCircular
* OutCircular
* InOutCircular
* InBack
* OutBack
* InOutBack
* InBounce
* OutBounce
* InOutBounce
* InElastic
* OutElastic
* InOutElastic

### Loop

You can specify how to loop to 5th argument from listed below.

* No loop
* Repeat
    * Repeat every loop from 1st argument to 2nd argument
* Ping-Pong
    * Every odd loops changes value from 1st argument to 2nd argument, every even loops changes value from 2nd argument to 1st argument.
* Mirror
    * Every odd loops changes value from 1st argument to 2nd argument, every even loops changes value from 2nd argument to 1st argument.
    * For all even loops, select easing opposite to odd loops.

### Determine the value lazily

You can pass `System.Func<T>` for 1st, 2nd and 3rd arguments to determine the value lazily.

```csharp
ObservableTween
    .Tween(
        () => Mathf.Random(0.0f, 1.0f),
        () => Mathf.Random(100.0f, 200.0f)
        () => Time.frameCount % 2 == 0 ? 1.0f : 2.0f
    )
    .Subscribe(/* Do something */);
```

### Supported value types

ObservableTween supported types listed below

* `int`
* `float`
* `UnityEngine.Vector2`
    * Changes `x` and `y`
* `UnityEngine.Vector3`
    * Changes `x`, `y` and `x`
