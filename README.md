# CategoryTool
Category Tool is a Unity Editor tool to create Categories in the Hierarchy. The Categories work as dividers between GameObjects.

![alt text](https://github.com/Demkeys/CategoryTool/blob/master/CategoryToolPic01.png "Category Tool")

### Features
This tool gives you the option to create __Category__ gameobjects. __Category__ gameobjects are just regular gameobjects, but they are assigned the "EditorOnly" tag and "Category" layer. __Categories__ are shown in the __Hierarchy__ differently compared to other gameobjects, which makes them ideal for use dividers.
You can control the appearence of the __Category__ gameobjects from __Edit > Preferences > Category Tool__.
Additionally, the __Category Tool__ gives you the option to display __Enable/Disable__ and __Delete__ options right in the __Hierarchy__. Those options can be enabled or disabled from __Edit > Preferences > Category Tool__.

__NOTE:__ You need to have a layer named Category in your project, otherwise the __Category Tool__ won't work. If you don't have a layer named Category in your project, create it, and then you can use the __Category Tool__.

### Instructions
* Place this script in the __Editor__ folder in your project.
* Click __Create > Create Other > Category__ to create a new __Category__.

### Known Bugs:
* When renaming a __Category__ through the __Hierarchy__ the name will look mangled until you hit Enter. To avoid this issue, rename __Categories__ from the __Inspector__ instead.
