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
            : base(StaticName, null, SubmenuNames.Render, EffectFlags.Configurable)
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
            Horizontal,
            Vertical,
            Diagonal1,
            Diagonal2,
            Conical,
            Square
        }

        int Size;
        GradientType Type;
        bool Reflected;
        ColorBgra Color1, Color2;
        int Alpha1, Alpha2;
        bool Lines;
        ColorBgra LineColor;

        protected override PropertyCollection OnCreatePropertyCollection()
        {
            List<Property> props = new List<Property>();

            props.Add(new Int32Property(Properties.Size, 100, 2, 1000));
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

            return new PropertyCollection(props);
        }

        protected override void OnCustomizeConfigUIWindowProperties(PropertyCollection props)
        {
            props[ControlInfoPropertyNames.WindowTitle].Value = StaticDialogName;
        }

        protected override ControlInfo OnCreateConfigUI(PropertyCollection props)
        {
            ControlInfo configUI = CreateDefaultConfigUI(props);

            configUI.SetPropertyControlType(Properties.Color1, PropertyControlType.ColorWheel);
            configUI.SetPropertyControlValue(Properties.Color1, ControlInfoPropertyNames.DisplayName, "");
            configUI.SetPropertyControlValue(Properties.Alpha1, ControlInfoPropertyNames.DisplayName, "Alpha");
            configUI.SetPropertyControlType(Properties.Color2, PropertyControlType.ColorWheel);
            configUI.SetPropertyControlValue(Properties.Color2, ControlInfoPropertyNames.DisplayName, "");
            configUI.SetPropertyControlValue(Properties.Alpha2, ControlInfoPropertyNames.DisplayName, "Alpha");
            configUI.SetPropertyControlType(Properties.LineColor, PropertyControlType.ColorWheel);
            configUI.SetPropertyControlValue(Properties.LineColor, ControlInfoPropertyNames.DisplayName, "");

            return configUI;
        }

        protected override void OnSetRenderInfo(PropertyBasedEffectConfigToken newToken, RenderArgs dstArgs, RenderArgs srcArgs)
        {
            Size = newToken.GetProperty<Int32Property>(Properties.Size).Value;
            Type = (GradientType)Enum.Parse(typeof(GradientType), (string)newToken.GetProperty(Properties.Type).Value);
            Reflected = newToken.GetProperty<BooleanProperty>(Properties.Reflected).Value;
            Color1 = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(Properties.Color1).Value);
            Alpha1 = (byte)newToken.GetProperty<Int32Property>(Properties.Alpha1).Value;
            Color2 = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(Properties.Color2).Value);
            Alpha2 = (byte)newToken.GetProperty<Int32Property>(Properties.Alpha2).Value;
            Lines = newToken.GetProperty<BooleanProperty>(Properties.Lines).Value;
            LineColor = ColorBgra.FromOpaqueInt32(newToken.GetProperty<Int32Property>(Properties.LineColor).Value);
            
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

                            float frac = 0;

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

                            if (Reflected)
                                frac = Math.Abs(-2 * frac + 1);

                            CurrentPixel = ColorBgra.Lerp(Color1.NewAlpha((byte)Alpha1), Color2.NewAlpha((byte)Alpha2), frac);

                        }
                        DstArgs.Surface[x, y] = CurrentPixel;
                    }
                }
            }
        }
    }
}