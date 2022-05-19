using UnityEngine;

public interface IColorable
{
    public Color colorAt(float evaluateVal);
}

public class HeightMapColor : IColorable
{
    public Gradient heightGradient;

    public Color colorAt(float evaluateVal)
    {
        return heightGradient.Evaluate(evaluateVal);
    }

    public HeightMapColor(Gradient heightGradient)
    {
        this.heightGradient = heightGradient;
    }
}