These XAML Viewbox resources originated from the [Visual Studio 2022 Image Library](https://www.microsoft.com/en-us/download/details.aspx?id=35825). However, each file had to be modified to use a `<ResourceDictionary>` root element instead of a `<Viewbox>` root. I also had to:

* Add x:Key to each viewbox since it's in a resource dictionary that gets merged into App.xaml.
* Add x:Shared="false" to each viewbox so the icon could be used in multiple places (e.g., on a menu and a toolbar button).
* Manually edit AboutBox.xaml to make the 'v' letter visible. That file's last GeometryDrawing didn't have a visible brush as delivered from Microsoft (even though the 'v' was visible in the .png and .svg).