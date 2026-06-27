using Godot;
using System;

public partial class MyButton : Godot.Button
{
    public enum ButtonTypeEnum { Easy, Normal, Hard, ZoomIn, ZoomOut, Reset }
    [ Export ] public ButtonTypeEnum ButtonType = ButtonTypeEnum.Easy;
}
