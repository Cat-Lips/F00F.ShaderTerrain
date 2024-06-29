using Godot;

namespace F00F.ShaderTerrain.Tests
{
    [Tool]
    public partial class Main : Game3D
    {
        private ulong? ms;

        public Camera Camera => GetNode<Camera>("Camera");
        public Terrain Terrain => GetNode<Terrain>("Terrain");
        public Options Options => GetNode<Options>("Options");
        public Settings Settings => GetNode<Settings>("Settings");

        #region Godot

        public override void _Ready()
        {
            GD.Print("HI");
            InitSettings(); ShowSettings();
            Settings.DataSet += InitTerrain;
            Terrain.ConfigSet += InitSettings;
            Camera.SelectModeSet += ShowSettings;
            if (Engine.IsEditorHint()) Settings.Visible = true;

            void InitTerrain()
                => Terrain.Config = Settings.Data;

            void InitSettings()
                => Settings.Data = Terrain.Config;

            void ShowSettings()
            {
                Settings.Visible = Camera.SelectMode;
                Options.EnableOptions(Camera.SelectMode);
            }
        }

        public override void _UnhandledInput(InputEvent e)
        {
            if (e is InputEventMouseButton mouse)
            {
                switch (mouse.ButtonIndex)
                {
                    case MouseButton.Left:
                        if (mouse.Pressed)
                            StartTimer();
                        else
                            CreateBody();
                        break;
                }
            }

            void StartTimer()
                => ms = Time.GetTicksMsec();

            void CreateBody()
            {
                if (ms is null) return;
                var time = Time.GetTicksMsec() - ms.Value;
                ms = null;

                var body = TestBody.Instantiate();
                PlaceBodyInFrontOfCamera();
                PropelBodyForwardFromCamera();
                Terrain.AddChild(body, forceReadableName: true);

                void PlaceBodyInFrontOfCamera()
                    => body.Transform = Camera.Transform.TranslatedLocal(Vector3.Forward * TestBody.Config.ShapeSize);

                void PropelBodyForwardFromCamera()
                {
                    var force = BaseForce() * ForceMultiplier();
                    body.ApplyCentralImpulse(Camera.Forward() * force);

                    float BaseForce()
                        => time / body.Mass;

                    float ForceMultiplier()
                    {
                        var x = 1;
                        if (Input.IsActionPressed(MyInput.Fast1)) x *= 10;
                        if (Input.IsActionPressed(MyInput.Fast2)) x *= 10;
                        if (Input.IsActionPressed(MyInput.Fast3)) x *= 10;
                        return x;
                    }
                }
            }
        }

        public override void _UnhandledKeyInput(InputEvent e)
        {
            if (this.Handle(Input.IsActionJustPressed(MyInput.Quit), Quit)) return;
            if (this.Handle(Input.IsActionJustPressed(MyInput.ToggleTerrain), ToggleTerrain)) return;

            void Quit()
            {
                GetTree().Quit();
                GD.Print("BYE");
            }

            void ToggleTerrain()
                => Terrain.Mesh.Visible = !Terrain.Mesh.Visible;
        }

        #endregion
    }
}
