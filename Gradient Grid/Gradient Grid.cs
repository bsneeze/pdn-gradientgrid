using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
using pyrochild.effects.common;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace pyrochild.effects.gradientgrid
{
    [PluginSupportInfo(typeof(PluginSupportInfo))]
    class GradientGrid : PropertyBasedEffect
    {
        public GradientGrid()
            : base(StaticName, new Bitmap(typeof(GradientGrid), "icon.png"), SubmenuNames.Render, EffectFlags.Configurable)
        {
        }

        public static string StaticDialogName
        {
            get
            {
                return StaticName + " by pyrochild";
            }
        }

        public static string StaticName
        {
            get
            {
                string name = "Gradient Grid";
#if DEBUG
                name += " BETA";
#endif
                return name;
            }
        }

        public enum Properties
        {
            Size,
            Range,
            Type,
            Reflected,
            Color1,
            Color2,
            Alphas,
            Lines,
            LineColor,
            GammaAdjust,
            Offset
        }

        public enum GradientType
        {
            Radial,
            Diagonal1,
            Diagonal2,
            Conical,
            Square,
            Horizontal,
            Vertical,
        }

        int Size;
        GradientType Type;
        bool Reflected, Lines, GammaAdjust;
        ColorBgra Color1, Color2, LineColor;
        double Start, End;
        Pair<double, double> Offset;

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();
            List<PropertyCollectionRule> rules = new List<PropertyCollectionRule>();

            props.Add(new Int32Property(Properties.Size, 100, 2, 1000));
            props.Add(new StaticListChoiceProperty(Properties.Type, Enum.GetNames(typeof(GradientType))));
            props.Add(new DoubleVectorProperty(Properties.Range,
                Pair.Create(0.0, 1.0), Pair.Create(0.0, 0.0), Pair.Create(1.0, 1.0)));

            props.Add(new BooleanProperty(Properties.Reflected, false));
            props.Add(new Int32Property(Properties.Color1,
                ColorBgra.ToOpaqueInt32(EnvironmentParameters.PrimaryColor.NewAlpha(255)), 0, 0xFFFFFF));
            
            props.Add(new Int32Property(Properties.Color2,
                ColorBgra.ToOpaqueInt32(EnvironmentParameters.SecondaryColor.NewAlpha(255)), 0, 0xFFFFFF));

            props.Add(new DoubleVectorProperty(Properties.Alphas,
                Pair.Create(
                    EnvironmentParameters.PrimaryColor.A / 255.0,
                    EnvironmentParameters.SecondaryColor.A / 255.0),
                Pair.Create(0.0, 0.0), Pair.Create(1.0, 1.0)));

            props.Add(new BooleanProperty(Properties.GammaAdjust, false));
            props.Add(new DoubleVectorProperty(Properties.Offset,
                Pair.Create(0.0, 0.0), Pair.Create(-1.0, -1.0), Pair.Create(1.0, 1.0)));

            props.Add(new BooleanProperty(Properties.Lines, false));
            props.Add(new Int32Property(Properties.LineColor,
                ColorBgra.ToOpaqueInt32(EnvironmentParameters.PrimaryColor.NewAlpha(255)), 0, 0xFFFFFF));

            rules.Add(new ReadOnlyBoundToBooleanRule(Properties.LineColor, Properties.Lines, true));

            return new PropertyCollection(props, rules);
        }

        protected override void OnCustomizeConfigUIWindowProperties(PropertyCollection props)
        {
            props[ControlInfoPropertyNames.WindowTitle].Value = StaticDialogName;
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlValue(Properties.Size, ControlInfoPropertyNames.SliderLargeChange, 10);
            configUI.SetPropertyControlValue(Properties.Size, ControlInfoPropertyNames.SliderSmallChange, 5);
            configUI.SetPropertyControlValue(Properties.Type, ControlInfoPropertyNames.DisplayName, "");
            configUI.SetPropertyControlType(Properties.Range, PropertyControlType.Slider);
            configUI.SetPropertyControlValue(Properties.Range, ControlInfoPropertyNames.DisplayName, "Gradient range");
            configUI.SetPropertyControlValue(Properties.Range, ControlInfoPropertyNames.SliderLargeChangeX, 0.1);
            configUI.SetPropertyControlValue(Properties.Range, ControlInfoPropertyNames.SliderSmallChangeX, 0.05);
            configUI.SetPropertyControlValue(Properties.Range, ControlInfoPropertyNames.UpDownIncrementX, 0.01);
            configUI.SetPropertyControlValue(Properties.Range, ControlInfoPropertyNames.SliderLargeChangeY, 0.1);
            configUI.SetPropertyControlValue(Properties.Range, ControlInfoPropertyNames.SliderSmallChangeY, 0.05);
            configUI.SetPropertyControlValue(Properties.Range, ControlInfoPropertyNames.UpDownIncrementY, 0.01);
            configUI.SetPropertyControlType(Properties.Color1, PropertyControlType.ColorWheel);
            configUI.SetPropertyControlValue(Properties.Color1, ControlInfoPropertyNames.DisplayName, "Color");
            configUI.SetPropertyControlType(Properties.Color2, PropertyControlType.ColorWheel);
            configUI.SetPropertyControlValue(Properties.Color2, ControlInfoPropertyNames.DisplayName, "");
            configUI.SetPropertyControlType(Properties.Alphas, PropertyControlType.Slider);
            configUI.SetPropertyControlValue(Properties.Alphas, ControlInfoPropertyNames.DisplayName, "Color alphas (transparency)");
            configUI.SetPropertyControlValue(Properties.Alphas, ControlInfoPropertyNames.SliderLargeChangeX, 0.1);
            configUI.SetPropertyControlValue(Properties.Alphas, ControlInfoPropertyNames.SliderSmallChangeX, 0.05);
            configUI.SetPropertyControlValue(Properties.Alphas, ControlInfoPropertyNames.UpDownIncrementX, 0.01);
            configUI.SetPropertyControlValue(Properties.Alphas, ControlInfoPropertyNames.SliderLargeChangeY, 0.1);
            configUI.SetPropertyControlValue(Properties.Alphas, ControlInfoPropertyNames.SliderSmallChangeY, 0.05);
            configUI.SetPropertyControlValue(Properties.Alphas, ControlInfoPropertyNames.UpDownIncrementY, 0.01);
            configUI.SetPropertyControlType(Properties.LineColor, PropertyControlType.ColorWheel);
            configUI.SetPropertyControlValue(Properties.LineColor, ControlInfoPropertyNames.DisplayName, "");
            configUI.SetPropertyControlValue(Properties.Reflected, ControlInfoPropertyNames.DisplayName, "");
            configUI.SetPropertyControlValue(Properties.Reflected, ControlInfoPropertyNames.Description, "Reflected");
            configUI.SetPropertyControlValue(Properties.Lines, ControlInfoPropertyNames.DisplayName, "");
            configUI.SetPropertyControlValue(Properties.Lines, ControlInfoPropertyNames.Description, "Lines");
            configUI.SetPropertyControlValue(Properties.GammaAdjust, ControlInfoPropertyNames.DisplayName, "");
            configUI.SetPropertyControlValue(Properties.GammaAdjust, ControlInfoPropertyNames.Description, "Gamma-adjusted color blend");
            configUI.SetPropertyControlValue(Properties.Offset, ControlInfoPropertyNames.SliderLargeChangeX, 0.1);
            configUI.SetPropertyControlValue(Properties.Offset, ControlInfoPropertyNames.SliderSmallChangeX, 0.05);
            configUI.SetPropertyControlValue(Properties.Offset, ControlInfoPropertyNames.UpDownIncrementX, 0.01);
            configUI.SetPropertyControlValue(Properties.Offset, ControlInfoPropertyNames.SliderLargeChangeY, 0.1);
            configUI.SetPropertyControlValue(Properties.Offset, ControlInfoPropertyNames.SliderSmallChangeY, 0.05);
            configUI.SetPropertyControlValue(Properties.Offset, ControlInfoPropertyNames.UpDownIncrementY, 0.01);

            return configUI;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            Size = newToken.GetProperty<Int32Property>(Properties.Size).Value;
            Type = (GradientType)Enum.Parse(typeof(GradientType), (string)newToken.GetProperty(Properties.Type).Value);
            Reflected = newToken.GetProperty<BooleanProperty>(Properties.Reflected).Value;
            Color1 = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(Properties.Color1).Value);
            Color2 = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(Properties.Color2).Value);
            Pair<double, double> Alphas = newToken.GetProperty<DoubleVectorProperty>(Properties.Alphas).Value;
            Color1.A = (byte)(Alphas.First * 255);
            Color2.A = (byte)(Alphas.Second * 255);
            Lines = newToken.GetProperty<BooleanProperty>(Properties.Lines).Value;
            LineColor = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(Properties.LineColor).Value);
            Pair<double, double> Range = newToken.GetProperty<DoubleVectorProperty>(Properties.Range).Value;
            Start = Range.First;
            End = Range.Second;
            GammaAdjust = newToken.GetProperty<BooleanProperty>(Properties.GammaAdjust).Value;
            Offset = newToken.GetProperty<DoubleVectorProperty>(Properties.Offset).Value;

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected unsafe override void OnRender(Rectangle[] renderRects, int startIndex, int length)
        {
            double cellrad = Math.Sqrt(Size / 2 * Size / 2 + Size / 2 * Size / 2);
            ColorBgra CurrentPixel;

            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Rectangle rect = renderRects[i];
                for (int y = rect.Top; y < rect.Bottom; y++)
                {
                    for (int x = rect.Left; x < rect.Right; x++)
                    {
                        int cell_x = x % Size; //distance into a grid cell
                        int cell_y = y % Size;

                        if (Lines && (cell_x == 0 || cell_y == 0))
                        {
                            CurrentPixel = LineColor;
                        }
                        else
                        {
                            cell_x -= (int)(Offset.First * Size);
                            cell_y -= (int)(Offset.Second * Size);

                            int cell_l = x / Size * Size; //top left corner of a grid cell
                            int cell_t = y / Size * Size;

                            int cell_cx = x - cell_l - Size / 2 - (int)(Offset.First * Size); //distance from center of a grid cell
                            int cell_cy = y - cell_t - Size / 2 - (int)(Offset.Second * Size);

                            double frac = 0;

                            switch (Type)
                            {
                                case GradientType.Radial:
                                    frac = Math.Sqrt(cell_cx * cell_cx + cell_cy * cell_cy) / cellrad;
                                    break;
                                case GradientType.Horizontal:
                                    frac = cell_x / (double)Size;
                                    break;
                                case GradientType.Vertical:
                                    frac = cell_y / (double)Size;
                                    break;
                                case GradientType.Diagonal1:
                                    frac = (cell_x + cell_y) / (double)Size / 2;
                                    break;
                                case GradientType.Diagonal2:
                                    frac = (cell_x - cell_y + Size) / (double)Size / 2;
                                    break;
                                case GradientType.Conical:
                                    frac = (Math.Atan2(Math.Abs(cell_cy), cell_cx) / (Math.PI));
                                    break;
                                case GradientType.Square:
                                    frac = 1 - 2 * Math.Min(
                                        1 - Math.Max(cell_x, cell_y) / (double)Size,
                                        Math.Min(cell_x, cell_y) / (double)Size);
                                    break;
                            }

                            if (Start == End)
                            {
                                frac = frac < Start ? 0 : 1;
                            }
                            else
                            {
                                frac = (frac - Start) / (End - Start);
                            }

                            if (Reflected)
                                frac = Math.Abs(-2 * frac + 1);

                            if (GammaAdjust)
                            {
                                CurrentPixel = ColorBgraBlender.Blend(Color1, Color2, frac);
                            }
                            else
                            {
                                CurrentPixel = ColorBgra.Lerp(Color1, Color2, frac);
                            }

                        }
                        DstArgs.Surface[x, y] = CurrentPixel;
                    }
                }
            }
        }
    }
}