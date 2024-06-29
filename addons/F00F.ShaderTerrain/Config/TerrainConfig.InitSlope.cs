using Godot;

namespace F00F;

using static CurveTexture;

public partial class TerrainConfig
{
    private CurveTexture HeightCurve;

    private void InitSlope()
    {
        AutoAction GradientChanged = null;
        GradientPreview.SafeConnect(Resource.SignalName.Changed, OnGradientChanged);

        void OnGradientChanged()
        {
            (GradientChanged ??= new(ResetHeightCurve)).Run();

            void ResetHeightCurve()
            {
                HeightCurve = new CurveTexture { Curve = GradientPreview, TextureMode = TextureModeEnum.Red };
                SetShaderHeightCurve();
                OnShapeChanged.Run();
            }
        }
    }

    private AutoAction _ResetGradientPreview;
    private void ResetGradientPreview()
    {
        (_ResetGradientPreview ??= new(ResetGradientPreview)).Run();

        void ResetGradientPreview()
        {
            GradientPreview.ClearPoints();
            Regions.ForEach(AddPoint);
            AddFinalPoint();

            void AddPoint(TerrainType region, int idx, float step)
            {
                if (UseSmoothing)
                    GradientPreview.AddPoint(new(idx * step, region.Gradient), region.LowerSmooth, region.UpperSmooth);
                else
                    GradientPreview.AddPoint(new(idx * step, region.Gradient), leftMode: Curve.TangentMode.Linear, rightMode: Curve.TangentMode.Linear);
            }

            void AddFinalPoint()
                => GradientPreview.AddPoint(Vector2.One);
        }
    }
}
