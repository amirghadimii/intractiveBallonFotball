using UnityEngine;

public class RemapPoints
{
    public static Vector2 RemapPointsMain(float x, float y, float xmin, float xmax, float ymin, float ymax)
    {
        float width = 1280f;
        float height = 720f;

        // Remap x coordinate
        float x_remap =ReMapvalue(0,x,width,xmin,xmax,0);
        float y_remap = ReMapvalue(0,y,height,ymin,ymax,0);


        // Remap y coordinate
   

        return new Vector2(x_remap, y_remap);
    }

    private static float ReMapvalue(float MinIpc, float Input, float MaxIpc, float MinImg, float MaxImg,
        float offset)
    {
        //  return (int) (0 + (MinIpc - Zero) * (MaxImg - minImg) / (MaxIpc - Zero));

        return (float)(((((MaxImg - MinImg) / (MaxIpc - MinIpc)) * Input) + MinImg) - offset);
    }
}