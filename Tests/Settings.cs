using System;
using Godot;

namespace F00F.ShaderTerrain.Tests
{
    [Tool]
    public partial class Settings : DataView
    {
        private TerrainData _data;
        public event Action DataSet;
        public TerrainData Data { get => _data; set => this.Set(ref _data, value, DataSet); }

        private Button ResetNoise => GetNode<Button>("%ResetNoise");
        private Button ResetAll => GetNode<Button>("%ResetAll");

        public override void _Ready()
        {
            InitData();
            InitGrid();
            InitReset();

            void InitData()
                => Data ??= new();

            void InitGrid()
            {
                Grid.Init(Data.GetEditControls(out var SetData));
                DataSet += () => SetData(Data);
            }

            void InitReset()
            {
                this.ResetNoise.Pressed += ResetNoise;
                this.ResetAll.Pressed += ResetAll;

                void ResetNoise()
                    => Data.Noise = TerrainData.Default().Noise;

                void ResetAll()
                    => Data = TerrainData.Default();
            }
        }
    }
}
