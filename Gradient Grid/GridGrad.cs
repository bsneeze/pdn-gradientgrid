// Submenu: Render
// Name: GridGrad
// Title: GridGrad by pyrochild
// Author: Zach Walker, a.k.a. pyrochild
// URL: http://forums.getpaint.net/index.php?/topic/7291-

#region UICode
int Amount1 = 100; // [0,1000] Size
byte Amount2 = 0; // Gradient Type|Radial|Linear H|Linear V|Linear D1|Linear D2|Conical|Square
bool Amount3 = false; // [0,1] Reflected
ColorBgra Amount4 = ColorBgra.FromBgr(0,0,0); // Color 1
int Amount5 = 255; // [0,255] Color 1 Alpha
ColorBgra Amount6 = ColorBgra.FromBgr(0,0,0); // Color 2
int Amount7 = 255; // [0,255] Color 2 Alpha
bool Amount8 = true; // [0,1] Gridlines
ColorBgra Amount9 = ColorBgra.FromBgr(0,0,0); // Gridline Color
#endregion

void Render(Surface dst, Surface src, Rectangle rect)
{
    float cellrad = (float)Math.Sqrt(Amount1/2*Amount1/2+Amount1/2*Amount1/2);
    ColorBgra CurrentPixel;
    for (int y = rect.Top; y < rect.Bottom; y++)
    {
        for (int x = rect.Left; x < rect.Right; x++)
        {
            CurrentPixel = src[x,y];
            int dx = x%Amount1; //distance into a grid cell
            int dy = y%Amount1;

            if(Amount8 && (dx==0 || dy==0))
            {
                CurrentPixel=Amount9;
            }
            else
            {
                int tlx=x/Amount1*Amount1; //top left corner of a grid cell
                int tly=y/Amount1*Amount1;
                int cx = tlx+Amount1/2; //center of a grid cell
                int cy = tly+Amount1/2;
                
                int sx=x-cx; //distance from center of a grid cell
                int sy=y-cy;
                
                float frac=0;
                
                switch(Amount2)
                {
                    case 0: //radial
                        frac=(float)Math.Sqrt(sx*sx+sy*sy)/cellrad;
                        break;
                    case 1: //linear h
                        frac=dx/(float)Amount1;
                        break;
                    case 2: //linear v
                        frac=dy/(float)Amount1;
                        break;
                    case 3: //linear d1
                        frac=(dx+dy)/(float)Amount1/2;
                        break;
                    case 4: //linear d2
                        frac=(dx-dy+Amount1)/(float)Amount1/2;
                        break;
                    case 5: //conical
                        frac=(float)(Math.Atan2(Math.Abs(sy),sx)/(Math.PI));
                        break;
                    case 6: //square
                        frac=1-2*Math.Min(1-Math.Max(dx,dy)/(float)Amount1,Math.Min(dx,dy)/(float)Amount1);
                        break;
                }
                
                if(Amount3)
                    frac=Math.Abs(-2*frac+1);
                
                CurrentPixel=ColorBgra.Lerp(Amount4.NewAlpha((byte)Amount5),Amount6.NewAlpha((byte)Amount7),frac);
                
            }
            dst[x,y] = CurrentPixel;
        }
    }
}
