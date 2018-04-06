# JankyUI
Unity Immediate Mode GUI Wrapper

---
JankyUI is a Unity Immediate Mode GUI Wrapper for quickly writing User Interfaces from code.

### Why?

* Unity's new UI System is great, but creating Test UIs from Code with it is not.
* Unity's Immediate Mode GUI is great for Code-Side UI, but writing the code to use it is not.

JankyUI Provides a Way of Writing Immediate Mode GUI without touching any of the GUI/GUILayout Functions.

Without worrying about UI Callbacks/Code,

#### Create This:
![window](https://user-images.githubusercontent.com/12700106/38439183-12171fe0-39b3-11e8-8e3f-118bd1c4b8c7.png)

#### From This:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<JankyUI xmlns='janky://Schema/v1'>
  <Window width='400' height='300' title='Window Title' on-mouse-over='@MouseState'>
    <Group type='vertical'>
      <Group type='horizontal'>
        <Group type='vertical' stretch='none'>
          <Label text='A Label' />
          <Label text='Another Label' />
        </Group>
        <Group type='vertical' stretch='horizontal'>
          <Textbox text='@BoundTextbox' stretch='horizontal' />
          <Textbox text='@AnotherBoundTextbox' stretch='horizontal' />
        </Group>
      </Group>
      <Label text='A Big Text Area:' stretch='horizontal' />
      <Textbox type='multiline' stretch='both' text='@ApiToken' />
      <Group type='horizontal' stretch='horizontal' >
        <Space />
        <Button name='Button1' text='A Button' on-click='@Click' />
        <Button name='Button2' text='Another Button' on-click='@Click' />
      </Group>
    </Group>
  </Window>
</JankyUI>
```

Check the [Wiki](https://github.com/usagirei/JankyUI/wiki) for Documentation and samples
