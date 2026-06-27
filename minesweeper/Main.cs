using Godot;
using System;

public partial class Main : Control
{
    public override void _Ready()
    {
        foreach ( MyButton button in GetNode<VBoxContainer>( "VBoxContainer/VBoxContainer2" ).GetChildren() ) {
            button.Pressed += () => {
                Button( button.ButtonType );
            };
        };
    }
    private void Button( MyButton.ButtonTypeEnum ButtonType ) {
        Minesweeper minesweeper = GetNode<Minesweeper>( "Minesweeper" );

        switch ( ButtonType ) {
            case MyButton.ButtonTypeEnum.Easy:
                minesweeper.SetupGame( 50 );
                break;
            case MyButton.ButtonTypeEnum.Normal:
                minesweeper.SetupGame( 65 );
                break;
            case MyButton.ButtonTypeEnum.Hard:
                minesweeper.SetupGame( 80 );
                break;
        }

        GetNode<VBoxContainer>( "VBoxContainer" ).Visible = false;
    }
}


