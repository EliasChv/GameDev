using Godot;
using System;

public partial class Cat : Node3D
{
    private Node3D CatNode;
    [ Export ] private Texture2D[] CatTexture = [];
    private int CurrentSkin = 0;
    public override void _Ready()
    {
        CatNode = GetNode<Node3D>("Cat");

        GetNode<Button>( "Control/HBoxContainer/right" ).Pressed += () => { ChangeTexture( 1 ); };
        GetNode<Button>( "Control/HBoxContainer/left" ).Pressed += () => { ChangeTexture( -1 ); };
    }

    public override void _Process( double delta )
    {
        CatNode.RotateY( - ( float ) Math.PI * 2 * ( float ) delta );
    }

    public void ChangeTexture( int number ) {
        CurrentSkin = Math.Clamp( CurrentSkin + number, 0, CatTexture.Length - 1 );
        CatNode.GetNode<MeshInstance3D>( "body" ).Mesh.SurfaceGetMaterial( 0 ).Set( "albedo_texture", CatTexture[ CurrentSkin ] );
    }
}