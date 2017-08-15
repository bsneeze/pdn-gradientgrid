using PaintDotNet;
using PaintDotNet.Effects;
using PaintDotNet.IndirectUI;
using PaintDotNet.PropertySystem;
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
            Start,
            End,
            Type,
            Reflected,
            Color1,
            Alpha1,
            Color2,
            Alpha2,
            Lines,
            LineColor,
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
        bool Reflected, Lines;
        ColorBgra Color1, Color2, LineColor;
        double Start, End;

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();
            List<PropertyCollectionRule> rules = new List<PropertyCollectionRule>();

            props.Add(new Int32Property(Properties.Size, 100, 2, 1000));
            props.Add(new DoubleProperty(Properties.Start, 0, 0, 1));
            props.Add(new DoubleProperty(Properties.End, 1, 0, 1));
            props.Add(new StaticListChoiceProperty(Properties.Type, Enum.GetNames(typeof(GradientType))));
            props.Add(new BooleanProperty(Properties.Reflected, false));
            props.Add(new Int32Property(Properties.Color1,
                ColorBgra.ToOpaqueInt32(EnvironmentParameters.PrimaryColor.NewAlpha(255)), 0, 0xFFFFFF));
            props.Add(new Int32Property(Properties.Alpha1, EnvironmentParameters.PrimaryColor.A, 0, 255));
            props.Add(new Int32Property(Properties.Color2,
                ColorBgra.ToOpaqueInt32(EnvironmentParameters.SecondaryColor.NewAlpha(255)), 0, 0xFFFFFF));
            props.Add(new Int32Property(Properties.Alpha2, EnvironmentParameters.SecondaryColor.A, 0, 255));
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
            
            configUI.SetPropertyControlType(Properties.Color1, PropertyControlType.ColorWheel);
            configUI.SetPropertyControlValue(Properties.Color1, ControlInfoPropertyNames.DisplayName, "Color");
            configUI.SetPropertyControlValue(Properties.Alpha1, ControlInfoPropertyNames.DisplayName, "Alpha");
            configUI.SetPropertyControlType(Properties.Color2, PropertyControlType.ColorWheel);
            configUI.SetPropertyControlValue(Properties.Color2, ControlInfoPropertyNames.DisplayName, "");
            configUI.SetPropertyControlValue(Properties.Alpha2, ControlInfoPropertyNames.DisplayName, "Alpha");
            configUI.SetPropertyControlType(Properties.LineColor, PropertyControlType.ColorWheel);
            configUI.SetPropertyControlValue(Properties.LineColor, ControlInfoPropertyNames.DisplayName, "");
            configUI.SetPropertyControlValue(Properties.Reflected, ControlInfoPropertyNames.DisplayName, "");
            configUI.SetPropertyControlValue(Properties.Reflected, ControlInfoPropertyNames.Description, "Reflected");
            configUI.SetPropertyControlValue(Properties.Lines, ControlInfoPropertyNames.DisplayName, "");
            configUI.SetPropertyControlValue(Properties.Lines, ControlInfoPropertyNames.Description, "Lines");

            return configUI;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            Size = newToken.GetProperty<Int32Property>(Properties.Size).Value;
            Type = (GradientType)Enum.Parse(typeof(GradientType), (string)newToken.GetProperty(Properties.Type).Value);
            Reflected = newToken.GetProperty<BooleanProperty>(Properties.Reflected).Value;
            Color1 = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(Properties.Color1).Value);
            Color1.A = (byte)newToken.GetProperty<Int32Property>(Properties.Alpha1).Value;
            Color2 = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(Properties.Color2).Value);
            Color2.A = (byte)newToken.GetProperty<Int32Property>(Properties.Alpha2).Value;
            Lines = newToken.GetProperty<BooleanProperty>(Properties.Lines).Value;
            LineColor = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(Properties.LineColor).Value);
            Start = newToken.GetProperty<DoubleProperty>(Properties.Start).Value;
            End = newToken.GetProperty<DoubleProperty>(Properties.End).Value;

            base.OnSetRenderInfo(newToken, dstArgs, srcArgs);
        }

        protected unsafe override void OnRender(Rectangle[] renderRects, int startIndex, int length)
        {
            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Rectangle rect = renderRects[i];
                float cellrad = (float)Math.Sqrt(Size / 2 * Size / 2 + Size / 2 * Size / 2);
                ColorBgra CurrentPixel;
                for (int y = rect.Top; y < rect.Bottom; y++)
                {
                    for (int x = rect.Left; x < rect.Right; x++)
                    {
                        int dx = x % Size; //distance into a grid cell
                        int dy = y % Size;

                        if (Lines && (dx == 0 || dy == 0))
                        {
                            CurrentPixel = LineColor;
                        }
                        else
                        {
                            int tlx = x / Size * Size; //top left corner of a grid cell
                            int tly = y / Size * Size;
                            int cx = tlx + Size / 2; //center of a grid cell
                            int cy = tly + Size / 2;

                            int sx = x - cx; //distance from center of a grid cell
                            int sy = y - cy;

                            double frac = 0;

                            switch (Type)
                            {
                                case GradientType.Radial:
                                    frac = (float)Math.Sqrt(sx * sx + sy * sy) / cellrad;
                                    break;
                                case GradientType.Horizontal:
                                    frac = dx / (float)Size;
                                    break;
                                case GradientType.Vertical:
                                    frac = dy / (float)Size;
                                    break;
                                case GradientType.Diagonal1:
                                    frac = (dx + dy) / (float)Size / 2;
                                    break;
                                case GradientType.Diagonal2:
                                    frac = (dx - dy + Size) / (float)Size / 2;
                                    break;
                                case GradientType.Conical:
                                    frac = (float)(Math.Atan2(Math.Abs(sy), sx) / (Math.PI));
                                    break;
                                case GradientType.Square:
                                    frac = 1 - 2 * Math.Min(1 - Math.Max(dx, dy) / (float)Size, Math.Min(dx, dy) / (float)Size);
                                    break;
                            }

                            frac = (frac - Start) / (End - Start);

                            if (Reflected)
                                frac = Math.Abs(-2 * frac + 1);

                            CurrentPixel = ColorBgra.Lerp(Color1, Color2, frac);

                        }
                        DstArgs.Surface[x, y] = CurrentPixel;
                    }
                }
            }
        }
    }
}