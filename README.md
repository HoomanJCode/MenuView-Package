# MenuView-Package
This is my simple UnityGameEngine menu management tool and I want to share it with my friends.
You can Create some Menus and switch between them and manage them at multiple layers.
### to do that:
0. Import Package from Unity Package Manager by selecting ``` Add package from git URL... ```
1. Create your Menu Script as Monobehaviour
2. Change Monobehaviour with ```MenuViews.MenuView```
3. Assign that to your UI GameObject.
4. Manage your menu by some commands like ChangeCurrentView<MenuClass>()

# Example
```csharp
using MenuViews;

//MainGameMenu.cs file
public class MainGameMenu : MenuView
{
    protected override void Init()
    {
        //It is MonoBehaviour, init buttons, texts, events, and more...
        throw new System.NotImplementedException();
    }
    //you have these commands in public and static
    //ChangeCurrentView<AuthMenu>();
    //ChangeToLastView();
    //LastView {get;}
    //GetCurrentView();
    //And more ...
}

//AuthMenu.cs file
public class AuthMenu : MenuView
{
    protected override void Init()
    {
        throw new System.NotImplementedException();
    }
}

```
