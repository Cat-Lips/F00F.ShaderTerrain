using Godot;

namespace F00F.ShaderTerrain.Tests
{
    public partial class Main
    {
        private class MyInput : F00F.MyInput
        {
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
            public static readonly StringName ToggleTerrain;
            public static readonly StringName Quit;

            public static readonly StringName Fast1;
            public static readonly StringName Fast2;
            public static readonly StringName Fast3;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
            internal static class Defaults
            {
                public static readonly Key ToggleTerrain = Key.F12;
                public static readonly Key Quit = Key.End;

                public static readonly Key Fast1 = Key.Alt;
                public static readonly Key Fast2 = Key.Ctrl;
                public static readonly Key Fast3 = Key.Shift;
            }

            static MyInput() => Init<MyInput>();
            private MyInput() { }
        }
    }
}
